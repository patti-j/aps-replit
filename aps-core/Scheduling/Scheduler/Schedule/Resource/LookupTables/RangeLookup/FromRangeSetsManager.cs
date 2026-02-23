using PT.APSCommon;
using PT.Common.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

//********************************************************************************************************************************************************************************************************
// At some point you might want to try getting rid of the inheritance from BaseIdManager. 
// You can replace it with an instance of BaseIdGenerator. But decimal check on the way the BaseId is used.
// It might be indexed into an array.
//
// Serialize your instance of BaseIdGenerator().
// 
// ++++++++++++++++++++++++++++++++++++++++++++
//
// 2015.1.7 AT: Removing inheritence from BaseIdManager as described above
//
//
//********************************************************************************************************************************************************************************************************

namespace PT.Scheduler.RangeLookup;

/// <summary>
/// Contains multiple FromRangeSets. Manages the creation of FromSetupRangeSets.
/// </summary>
public class FromRangeSetsManager : IPTSerializable
{
    #region IPTSerializable Members
    public FromRangeSetsManager(IReader a_reader, BaseIdGenerator a_generator)
    {
        // this is to ensure old versions deserialize correctly. See notes at the top of this file.
        if (a_reader.VersionNumber < 503)
        {
            BaseId temp = new (a_reader);
        }

        m_idGen = a_generator;

        if (a_reader.VersionNumber >= 1)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                FromRangeSets ma;
                ma = new FromRangeSets(a_reader);
                m_fromSetupRangeSets.Add(ma);
            }
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(m_fromSetupRangeSets.Count);
        for (int i = 0; i < m_fromSetupRangeSets.Count; ++i)
        {
            m_fromSetupRangeSets[i].Serialize(a_writer);
        }
    }

    private const int UNIQUE_ID = -1;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public FromRangeSetsManager(BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
    }

    private readonly BaseIdGenerator m_idGen;

    private readonly List<FromRangeSets> m_fromSetupRangeSets = new ();

    internal FromRangeSets Add(string a_name, string a_description)
    {
        FromRangeSets c = new (m_idGen.NextID(), a_name, a_description);
        m_fromSetupRangeSets.Add(c);
        return c;
    }

    public int Count => m_fromSetupRangeSets.Count;

    public FromRangeSets this[int a_idx] => m_fromSetupRangeSets[a_idx];

    public FromRangeSets Find(BaseId a_id)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].Id == a_id)
            {
                return this[i];
            }
        }

        return null;
    }

    public FromRangeSets Find(string a_aName)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].Name == a_aName)
            {
                return this[i];
            }
        }

        return null;
    }

    public override string ToString()
    {
        return string.Format("Contains {0} FromRangeSetCollections", Count);
    }

    private int FindIdxById(BaseId a_aId)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].Id == a_aId)
            {
                return i;
            }
        }

        return -1;
    }

    internal void Receive(SetupRangeBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is SetupRangeCopyT)
        {
            Copy((SetupRangeCopyT)a_t, a_dataChanges);
        }
        else
        {
            throw new PTValidationException("2870", new object[] { GetType().ToString(), a_t.GetType().ToString() });
        }
    }

    internal List<FromRangeSets> Receive(ScenarioDetail a_sd, ERPTransmissions.LookupAttributeNumberRangeT a_t, out List<ResourceKeyList> a_resKeyLists, out HashSet<BaseId> a_tableIdsAffected, IScenarioDataChanges a_dataChanges)
    {
        List<FromRangeSets> rangeSetList = new ();
        a_resKeyLists = new List<ResourceKeyList>();
        a_tableIdsAffected = new HashSet<BaseId>(); //keep track for auto-delete
        for (int tableI = 0; tableI < a_t.Count; tableI++)
        {
            ERPTransmissions.LookupAttributeNumberRangeT.LookupAttributeNumberRangeTable tTable = a_t.tableList[tableI];
            rangeSetList.Add(UpdateFromRangeSet(tTable.Id, tTable.Name, tTable.Description, tTable.SetupRangeAttributeUpdates, a_sd.AttributeManager, a_dataChanges));

            a_tableIdsAffected.AddIfNew(tTable.Id);

            //Copy in the Resources
            ResourceKeyList resKeyList = new ();
            a_resKeyLists.Add(resKeyList);
            for (int resI = 0; resI < tTable.AssignedResources.Count; resI++)
            {
                ResourceKeyExternal keyExt = tTable.AssignedResources[resI];

                Plant plant = a_sd.PlantManager.GetByExternalId(keyExt.PlantExternalId);
                if (plant == null)
                {
                    throw new PTValidationException("2750", new object[] { keyExt.PlantExternalId, a_sd._scenario.Id.ToString() });
                }

                Department dept = plant.Departments.GetByExternalId(keyExt.DepartmentExternalId);
                if (dept == null)
                {
                    throw new PTValidationException("2751", new object[] { keyExt.DepartmentExternalId, keyExt.PlantExternalId, a_sd._scenario.Id.ToString() });
                }

                Resource res = dept.Resources.GetByExternalId(keyExt.ResourceExternalId);
                if (res == null)
                {
                    throw new PTValidationException("2752", new object[] { keyExt.ResourceExternalId, keyExt.DepartmentExternalId, keyExt.PlantExternalId, a_sd._scenario.Id.ToString() });
                }

                resKeyList.Add(new ResourceKey(plant.Id, dept.Id, res.Id));
            }
        }


        return rangeSetList;
    }

    internal FromRangeSets Receive(SetupRangeUpdateT a_t, AttributeManager a_attributesManager, IScenarioDataChanges a_dataChanges)
    {
        return UpdateFromRangeSet(a_t.FromToRangeId, a_t.tableName, a_t.tableDescription, a_t.setupRangeAttributeUpdates, a_attributesManager, a_dataChanges);
    }

    private FromRangeSets UpdateFromRangeSet(BaseId a_id, string a_tableName, string a_description, List<SetupRangeAttributeUpdate> a_setupRangeAttributeUpdates, AttributeManager a_attributeManager, IScenarioDataChanges a_dataChanges)
    {
        if (a_id == BaseId.NULL_ID)
        {
            a_id = m_idGen.NextID();
        }

        int idx = FindIdxById(a_id);

        FromRangeSets attributeFromRangeSetList = new (a_id, a_tableName, a_description);

        try
        {
            a_attributeManager.InitFastLookupByExternalId();

            for (int attributeI = 0; attributeI < a_setupRangeAttributeUpdates.Count; ++attributeI)
            {
                SetupRangeAttributeUpdate rangeUpdate = a_setupRangeAttributeUpdates[attributeI];

                if (a_attributeManager.GetByExternalId(rangeUpdate.m_ptAttributeExternalId) == null)
                {
                    throw new PTValidationException($"Setup Range Attribute referenced an Attribute that does not exist with ExternalId {rangeUpdate.m_ptAttributeExternalId}"); //TODO: Convert to new error code
                }

                attributeFromRangeSetList.Add(rangeUpdate);
            }
        }
        finally
        {
            a_attributeManager.DeInitFastLookupByExternalId();
        }

        attributeFromRangeSetList.DataChangesCompleted();
        attributeFromRangeSetList.Validate();

        if (idx >= 0)
        {
            m_fromSetupRangeSets[idx] = attributeFromRangeSetList;
        }
        else
        {
            m_fromSetupRangeSets.Add(attributeFromRangeSetList);
        }

        DataChangesCompleted();

        if (idx >= 0)
        {
            a_dataChanges.FromRangeSetChanges.UpdatedObject(attributeFromRangeSetList.Id);
        }
        else
        {
            a_dataChanges.FromRangeSetChanges.AddedObject(attributeFromRangeSetList.Id);
        }

        return attributeFromRangeSetList;
    }

    internal FromRangeSets Receive(SetupRangeDeleteT a_dT, IScenarioDataChanges a_dataChanges)
    {
        return Delete(a_dT.FromToRangeId, a_dataChanges);
    }

    internal FromRangeSets Delete(BaseId a_fromToRangeId, IScenarioDataChanges a_dataChanges)
    {
        int idx = FindIdxById(a_fromToRangeId);

        if (idx >= 0)
        {
            FromRangeSets attributeFromRangeSetList = this[idx];
            m_fromSetupRangeSets.RemoveAt(idx);
            a_dataChanges.FromRangeSetChanges.DeletedObject(attributeFromRangeSetList.Id);
            return attributeFromRangeSetList;
        }

        throw new PTValidationException("2277");
    }

    private void Copy(SetupRangeCopyT a_t, IScenarioDataChanges a_dataChanges)
    {
        int idx = FindIdxById(a_t.FromToRangeId);

        if (idx < 0)
        {
            throw new PTValidationException("2277");
        }

        FromRangeSets attributeFromRangeSetList = new (m_idGen.NextID(), this[idx]);
        m_fromSetupRangeSets.Add(attributeFromRangeSetList);
        a_dataChanges.FromRangeSetChanges.AddedObject(attributeFromRangeSetList.Id);
    }

    internal void DeleteAll(IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < Count; ++i)
        {
            a_dataChanges.FromRangeSetChanges.DeletedObject(this[i].Id);
        }

        m_fromSetupRangeSets.Clear();
    }

    private void DataChangesCompleted()
    {
        m_fromSetupRangeSets.Sort(Compare);
    }

    private int Compare(FromRangeSets a_a, FromRangeSets a_b)
    {
        return string.Compare(a_a.Name, a_b.Name, true);
    }
}