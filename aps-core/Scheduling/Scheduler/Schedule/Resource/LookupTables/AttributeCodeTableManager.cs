using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of AttributeCode Tables.
/// </summary>
public class AttributeCodeTableManager : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 570;

    public AttributeCodeTableManager(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_nextId = new BaseId(reader);

            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Add(new AttributeCodeTable(reader));
            }
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        m_nextId.Serialize(writer);

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public AttributeCodeTableManager()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    #region list maintenance
    private readonly SortedList<BaseId, AttributeCodeTable> m_tables = new ();

    public int Count => m_tables.Count;

    public AttributeCodeTable this[int index] => m_tables.Values[index];

    private void Add(AttributeCodeTable table)
    {
        m_tables.Add(table.Id, table);
    }

    private void Delete(ScenarioDetail sd, AttributeCodeTable table, IScenarioDataChanges a_dataChanges)
    {
        table.UpdateResourcesForDelete(sd, a_dataChanges);
        m_tables.Remove(table.Id);
    }

    private AttributeCodeTable Copy(AttributeCodeTable a_sourceTable)
    {
        AttributeCodeTable newTable = new (NextId(), a_sourceTable);
        return newTable;
    }

    public AttributeCodeTable Find(BaseId a_tableId)
    {
        if (m_tables.TryGetValue(a_tableId, out AttributeCodeTable table))
        {
            return table;
        }

        return null;
    }

    private BaseId m_nextId = new (0);

    private BaseId NextId()
    {
        BaseId newId = m_nextId;
        m_nextId = new BaseId(newId.ToBaseType() + 1);
        return newId;
    }
    #endregion

    #region Transmissions
    public void Receive(AttributeCodeTableBaseT t, ScenarioDetail sd, AttributeManager a_attributeManager, IScenarioDataChanges a_dataChanges)
    {
        if (t is AttributeCodeTableIdBaseT)
        {
            AttributeCodeTableIdBaseT idT = (AttributeCodeTableIdBaseT)t;
            AttributeCodeTable table = Find(idT.Id);
            if (table == null)
            {
                throw new PTValidationException("2276", new object[] { idT.Id });
            }

            if (t is AttributeCodeTableUpdateT)
            {
                table.Update(sd, ((AttributeCodeTableUpdateT)idT).AttributeCodeTable, a_attributeManager, a_dataChanges);
                a_dataChanges.LookupAttributeCodeTablesUpdated.UpdatedObject(table.Id);
            }
            else if (t is AttributeCodeTableDeleteT)
            {
                Delete(sd, table, a_dataChanges);
                a_dataChanges.LookupAttributeCodeTablesUpdated.DeletedObject(table.Id);
            }
            else if (t is AttributeCodeTableCopyT)
            {
                AttributeCodeTable copyTable = Copy(table);
                Add(copyTable);
                a_dataChanges.LookupAttributeCodeTablesUpdated.AddedObject(copyTable.Id);
            }
        }
        else if (t is AttributeCodeTableNewT)
        {
            AttributeCodeTable newTable = AddNew(sd, ((AttributeCodeTableNewT)t).AttributeCodeTable, a_attributeManager, a_dataChanges);
            a_dataChanges.LookupAttributeCodeTablesUpdated.AddedObject(newTable.Id);
        }
    }

    /// <summary>
    /// ERP Transmission processing
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_s"></param>
    /// <param name="a_sd"></param>
    public void Receive(ERPTransmissions.LookupAttributeCodeTableT a_t, Scenario a_s, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        List<PostProcessingAction> actions = new ();
        ApplicationExceptionList errList = new ();

        HashSet<string> tablesInT = new ();

        try
        {
            for (int i = 0; i < a_t.AttributeCodeTableList.Count; i++)
            {
                try
                {
                    PT.Transmissions.AttributeCodeTable attributeCodeTable = a_t.AttributeCodeTableList[i];
                    if (!tablesInT.Contains(attributeCodeTable.Name))
                    {
                        tablesInT.Add(attributeCodeTable.Name);
                    }

                    // Move ResourceExternalidList to ResourceKeyList
                    ResourceKeyList resourceKeyList = new ();
                    ArrayList resourceKeyExternalidList = attributeCodeTable.AssignedResourcesExternalId.CreateArrayListShallowCopy();
                    for (int j = 0; j < resourceKeyExternalidList.Count; j++)
                    {
                        ResourceKeyExternal resourceKeyExternalId = (ResourceKeyExternal)resourceKeyExternalidList[j];
                        Resource resource = a_sd.PlantManager.GetResource(resourceKeyExternalId.PlantExternalId, resourceKeyExternalId.DepartmentExternalId, resourceKeyExternalId.ResourceExternalId);
                        if (resource != null)
                        {
                            ResourceKey resourceKey = new (resource.PlantId, resource.DepartmentId, resource.Id);
                            resourceKeyList.Add(resourceKey);
                        }
                    }

                    attributeCodeTable.AssignedResources = resourceKeyList;
                    AttributeCodeTable table = FindByName(attributeCodeTable.Name);
                    // New Table, add
                    if (table == null)
                    {
                        AttributeCodeTable newTable = AddNew(a_sd, attributeCodeTable, a_sd.AttributeManager, a_dataChanges);
                        a_dataChanges.LookupAttributeCodeTablesUpdated.AddedObject(newTable.Id);
                    }
                    // Existing table, remove and add
                    else
                    {
                        Delete(a_sd, table, a_dataChanges);
                        AttributeCodeTable newTable = AddNew(a_sd, attributeCodeTable, a_sd.AttributeManager, a_dataChanges);
                        a_dataChanges.LookupAttributeCodeTablesUpdated.UpdatedObject(newTable.Id);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            // remove for omitted records
            if (a_t.AutoDeleteMode)
            {
                // Determine deleted records, can't delete here because enumerator can't be modified
                AttributeCodeTable table;
                IEnumerator<KeyValuePair<BaseId, AttributeCodeTable>> de = m_tables.GetEnumerator();
                while (de.MoveNext())
                {
                    table = de.Current.Value;
                    if (!tablesInT.Contains(table.Name))
                    {
                        a_dataChanges.LookupAttributeCodeTablesUpdated.DeletedObject(table.Id);
                    }
                }

                if (a_dataChanges.LookupAttributeCodeTablesUpdated.TotalDeletedObjects > 0)
                {
                    ScenarioExceptionInfo sei = new ();
                    sei.Create(a_sd);
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            try
                            {
                                //Now delete
                                foreach (BaseId id in a_dataChanges.LookupAttributeCodeTablesUpdated.Deleted)
                                {
                                    AttributeCodeTable tbl = Find(id);
                                    Delete(a_sd, tbl, a_dataChanges);
                                }
                            }
                            catch (PTHandleableException err)
                            {
                                m_errorReporter.LogException(err, a_t, sei, ELogClassification.PtInterface, false);
                            }
                        }));
                }
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            a_sd.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface,false);
            }
        }
    }

    /// <summary>
    /// Return the attribute code table if it already exists
    /// </summary>
    /// <param name="a_attributeCodeTable"></param>
    /// <returns></returns>
    internal AttributeCodeTable FindByName(string a_attributeCodeTableName)
    {
        AttributeCodeTable table;
        IEnumerator<KeyValuePair<BaseId, AttributeCodeTable>> de = m_tables.GetEnumerator();
        while (de.MoveNext())
        {
            table = de.Current.Value;
            if (table.Name.ToUpperInvariant() == a_attributeCodeTableName.ToUpperInvariant())
            {
                return table;
            }
        }

        return null;
    }

    /// <summary>
    /// Delete all tables and fire ScenarioEvents update event.
    /// </summary>
    internal void DeleteAll(ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            AttributeCodeTable delTable = this[i];
            Delete(sd, delTable, a_dataChanges);
            a_dataChanges.LookupAttributeCodeTablesUpdated.DeletedObject(delTable.Id);
        }
    }

    private AttributeCodeTable AddNew(ScenarioDetail sd, PT.Transmissions.AttributeCodeTable t, AttributeManager a_attributeManager, IScenarioDataChanges a_dataChanges)
    {
        AttributeCodeTable newTable = new (NextId(), t, sd, a_attributeManager, a_dataChanges);
        Add(newTable);
        return newTable;
    }
    #endregion

    private ISystemLogger m_errorReporter;

    public void RestoreReferences(ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;
    }

    public void ConvertSetupCodeTableForBackwardsCompatibility(SetupCodeTable a_setupCodeTable, AttributeManager a_attributeManager)
    {
        AttributeCodeTable convertedTable = new AttributeCodeTable(NextId(), a_setupCodeTable, a_attributeManager);
        Add(convertedTable);
    }
}