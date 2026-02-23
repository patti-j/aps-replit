using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Common.Exceptions;
using PT.Database;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of MaterialRequirements.
/// </summary>
public partial class MaterialRequirementsCollection : ICopyTable, IPTSerializable, IEnumerable<MaterialRequirement>, AfterRestoreReferences.IAfterRestoreReferences
{
    private readonly BaseIdGenerator m_idGen;
    public const int UNIQUE_ID = 303;

    #region IPTSerializable Members
    public MaterialRequirementsCollection(IReader a_reader, BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
        if (a_reader.VersionNumber >= 12409)
        {
            m_materialRequirementList = new MaterialRequirementList(a_reader, a_idGen);
        }
        else if (a_reader.VersionNumber >= 410)
        {
            a_reader.Read(out long m_lastId);
            m_materialRequirementList = new MaterialRequirementList(a_reader, a_idGen);

            //For backwards compatibility. All MRs used to start from ID 0 in the operation. 
            //This was not compatible with lot sourcing and alternate paths, where MR demands had duplicate MR Ids for the same MO.
            List<MaterialRequirement> oldMrs = new (m_materialRequirementList.Count);

            //Get each MR, remove it from the list, change the ID, then add it back to the list.
            //We need to remove and re-add to keep the collection keys updated with the new Ids.
            foreach (MaterialRequirement mr in m_materialRequirementList)
            {
                oldMrs.Add(mr);
            }

            m_materialRequirementList.Clear();

            foreach (MaterialRequirement mr in oldMrs)
            {
                mr.Id = a_idGen.NextID();
                Add(mr);
            }
        }
    }

    internal void RestoreReferences(WarehouseManager aWarehouses, ItemManager aItems, BaseOperation a_op)
    {
        for (int i = 0; i < m_materialRequirementList.Count; ++i)
        {
            m_materialRequirementList[i].RestoreReferences(aWarehouses, aItems, (InternalOperation)a_op);
        }
    }

    #region IAfterRestoreReferences
    public void AfterRestoreReferences_1(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
        HashSet<MaterialRequirement> mrHash = new (m_materialRequirementList.Count);
        foreach (MaterialRequirement mr in m_materialRequirementList)
        {
            mrHash.Add(mr);
        }

        m_materialRequirementList.Clear();
        foreach (MaterialRequirement mr in mrHash)
        {
            mr.Id = m_idGen.NextID();
            m_materialRequirementList.Add(mr);
        }
    }

    public void AfterRestoreReferences_2(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
    }
    #endregion


    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        
        m_materialRequirementList.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    private readonly MaterialRequirementList m_materialRequirementList = new ();

    public class MaterialRequirementsCollectionException : PTException
    {
        public MaterialRequirementsCollectionException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    public MaterialRequirementsCollection(BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
    }
    #endregion

    #region Properties and Methods

    public Type ElementType => typeof(MaterialRequirement);

    internal void Add(MaterialRequirement a_material)
    {
        m_materialRequirementList.Add(a_material);
    }

    internal void Clear()
    {
        //m_materialRequirementList.Clear(); We need to delete each one individually to clean up references.
        while (m_materialRequirementList.Count > 0)
        {
            MaterialRequirement mr = m_materialRequirementList.GetByIndex(0);
            Remove(mr);
        }
    }

    public int Count => m_materialRequirementList.Count;

    /// <summary>
    /// Remove the requirement for a specific item.
    /// </summary>
    internal void Remove(MaterialRequirement a_mr)
    {
        a_mr.Deleting();
        m_materialRequirementList.RemoveObject(a_mr);
    }

    public MaterialRequirement this[int a_index] => m_materialRequirementList[a_index];

    public object GetRow(int a_index)
    {
        return m_materialRequirementList.GetByIndex(a_index);
    }

    /// <summary>
    /// Returns the first Material Requirement found that uses an Item in the specified ItemGroup (case sensitive).
    /// </summary>
    /// <param name="group">The ItemGroup to search Items for.</param>
    /// <returns>A Material Requirement or null if none is found.</returns>
    public MaterialRequirement FindFirstMaterialForItemWithGroup(string itemGroup)
    {
        IEnumerator<MaterialRequirement> enumerator = m_materialRequirementList.GetEnumerator();

        while (enumerator.MoveNext())
        {
            MaterialRequirement mr = enumerator.Current;
            if (mr.Item != null && mr.Item.ItemGroup == itemGroup)
            {
                return mr;
            }
        }

        return null;
    }

    public MaterialRequirement FindByExternalId(string a_externalid)
    {
        IEnumerator<MaterialRequirement> enumerator = m_materialRequirementList.GetEnumerator();

        while (enumerator.MoveNext())
        {
            MaterialRequirement mr = enumerator.Current;
            if (mr.Item != null && mr.ExternalId == a_externalid)
            {
                return mr;
            }
        }

        return null;
    }

    public MaterialRequirement FindByBaseId(BaseId a_MrIdKey)
    {
        return m_materialRequirementList.GetValue(a_MrIdKey);
    }
    #endregion

    #region PT Database
    internal void PopulateJobDataSet(ref JobDataSet dataSet, BaseOperation operation)
    {
        //Set sort to scheduled start and the Id (in case of it being unscheduled)
        dataSet.MaterialRequirement.DefaultView.Sort = string.Format("{0} DESC,{1} ASC", dataSet.MaterialRequirement.LatestSourceDateTimeColumn.ColumnName, dataSet.MaterialRequirement.MaterialNameColumn.ColumnName);

        for (int i = 0; i < m_materialRequirementList.Count; i++)
        {
            m_materialRequirementList[i].PopulateJobDataSet(ref dataSet, operation);
        }
    }

    public void PtDbPopulate(ref PtDbDataSet dataSet, BaseOperation op, PtDbDataSet.JobOperationsRow jobOpRow, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < m_materialRequirementList.Count; i++)
        {
            m_materialRequirementList[i].PtDbPopulate(ref dataSet, op, jobOpRow, a_dbHelper);
        }
    }
    #endregion

    #region Update
    internal bool Update(MaterialRequirementsCollection a_tMrCollection,
                                 BaseOperation a_operation,
                                 long a_clock,
                                 bool a_erpUpdate,
                                 WarehouseManager a_warehouseManager,
                                 IScenarioDataChanges a_dataChanges)
    {
        //lastId = tMRC.lastId; TODO: Why would we use this? lastId is serialized, we should keep increasing to avoid collisions.
        //We must update existing material requirements even if there are new or deleted ones. Clearing and re-adding all MRs will lose manual updates not being imported.

        Dictionary<string, MaterialRequirement> newRequirements = new ();
        Dictionary<string, MaterialRequirement> updatedRequirements = new ();
        bool updated = false;
        for (int requirementI = 0; requirementI < a_tMrCollection.Count; requirementI++)
        {
            MaterialRequirement newMr = a_tMrCollection[requirementI];
            MaterialRequirement mr = GetByExternalId(newMr.ExternalId);

            if (mr != null)
            {
                //Update
                updatedRequirements.Add(newMr.ExternalId, newMr);
            }
            else
            {
                //New
                newRequirements.Add(newMr.ExternalId, newMr);
            }
        }

        List<MaterialRequirement> mrItemsToRemove = new ();
        //Remove all requirements that have been deleted
        foreach (MaterialRequirement mr in this)
        {
            if (!updatedRequirements.ContainsKey(mr.ExternalId))
            {
                //This one has been deleted
                mrItemsToRemove.Add(mr);
            }
        }

        foreach (MaterialRequirement mr in mrItemsToRemove)
        {
            Remove(mr);
            updated = true;
            a_dataChanges.AuditEntry(new AuditEntry(mr.Id, mr.Operation.Id, mr), false, true);
        }

        //Update existing requirements
        foreach (MaterialRequirement mr in this)
        {
            AuditEntry mrAuditEntry = new AuditEntry(mr.Id, mr.Operation.Id, mr);
            updated |= mr.Update(updatedRequirements[mr.ExternalId], a_clock, a_erpUpdate, a_operation, a_warehouseManager, a_dataChanges);
            a_dataChanges.AuditEntry(mrAuditEntry);
        }

        try
        {
            //Now Add new requirements
            if (newRequirements.Values.Count > 0 && a_operation.Scheduled)
            {
                //we are adding new MRs so we need to flag a constraint change
                a_dataChanges.FlagConstraintChanges(a_operation.Job.Id);
            }

            foreach (MaterialRequirement newMr in newRequirements.Values)
            {
                MaterialRequirement copyOfRR = new (m_idGen.NextID(), newMr.ExternalId, newMr, m_idGen);
                Add(copyOfRR);
                updated = true;
                a_dataChanges.AuditEntry(new AuditEntry(newMr.Id, newMr.Operation.Id, newMr), true);
            }
        }
        catch (Exception e)
        {
            //why does VS not break on this error????
        }

        return updated;
    }

    /// <summary>
    /// O(n).
    /// Get a MaterialRequirement by external id.
    /// </summary>
    /// <param name="externalId">The external id of the MaterialRequirement that you want.</param>
    /// <returns>The MaterialRequirement that you want or null if it's not here.</returns>
    private MaterialRequirement GetByExternalId(string externalId)
    {
        for (int materialRequirementI = 0; materialRequirementI < Count; ++materialRequirementI)
        {
            MaterialRequirement rr = this[materialRequirementI];
            if (rr.ExternalId == externalId)
            {
                return rr;
            }
        }

        return null;
    }
    #endregion

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long clockAdvanceTicks)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].AdjustDemoDataForClockAdvance(clockAdvanceTicks);
        }
    }
    #endregion

    /// <summary>
    /// This is a wrapper class for MaterialRequirement list.
    /// </summary>
    private class MaterialRequirementList : CustomSortedDictionary<BaseId, MaterialRequirement>
    {
        public MaterialRequirementList()
            : base() { }

        public MaterialRequirementList(IReader a_reader, BaseIdGenerator a_idGen)
            : base(a_reader, a_idGen) { }

        protected override MaterialRequirement CreateInstance(IReader a_reader)
        {
            //This is not used
            throw new NotImplementedException();
        }

        protected override MaterialRequirement CreateInstance(IReader a_reader, BaseIdGenerator a_baseIdGenerator)
        {
            return new MaterialRequirement(a_reader, a_baseIdGenerator);
        }

        public MaterialRequirement this[int index] => GetByIndex(index);
    }

    public override string ToString()
    {
        string s = string.Format("MaterialRequirements.Count={0}", m_materialRequirementList.Count);
        if (Count == 1)
        {
            s = string.Format("{0}; MatlReqt={1}", s, m_materialRequirementList[0]);
        }

        return s;
    }

    public IEnumerator<MaterialRequirement> GetEnumerator()
    {
        return m_materialRequirementList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    /// <summary>
    /// Removes all MRP maintained material requirements
    /// </summary>
    public void ClearMrpRequirements()
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            MaterialRequirement mr = this[i];
            if (mr.MaintenanceMethod == JobDefs.EMaintenanceMethod.MrpGenerated)
            {
                foreach (MaterialRequirement existingMr in StockMaterials)
                {
                    //Find the first non MRP material for this item
                    if (existingMr.Item == mr.Item && existingMr.MaintenanceMethod != JobDefs.EMaintenanceMethod.MrpGenerated)
                    {
                        existingMr.TotalRequiredQty += mr.TotalRequiredQty;
                        existingMr.TotalCost += mr.TotalCost;
                        break;
                    }
                }

                Remove(mr);
            }
        }
    }

    /// <summary>
    /// Returns all non BuyDirect material requirements
    /// </summary>
    public IEnumerable<MaterialRequirement> StockMaterials
    {
        get
        {
            foreach (MaterialRequirement mr in m_materialRequirementList)
            {
                if (mr.BuyDirect)
                {
                    continue;
                }

                yield return mr;
            }
        }
    }

    public IEnumerable<MaterialRequirement> BuyDirectMaterials
    {
        get
        {
            foreach (MaterialRequirement mr in m_materialRequirementList)
            {
                if (!mr.BuyDirect)
                {
                    continue;
                }

                yield return mr;
            }
        }
    }

    public MaterialRequirement SplitOffNewSupplyPeggingForMRP(BaseId a_sourceMrId, decimal a_qty)
    {
        if (FindByBaseId(a_sourceMrId) is MaterialRequirement sourceMr)
        {
            //Create a copy MR with new id and external id
            BaseId nextId = m_idGen.NextID();
            MaterialRequirement copyOfMR = new (nextId, ExternalBaseIdObject.MakeExternalId(nextId.Value), sourceMr, m_idGen);
            Add(copyOfMR);

            decimal percentOfCost = a_qty / sourceMr.TotalRequiredQty;
            decimal sourceMrTotalCost = sourceMr.TotalCost * percentOfCost;

            sourceMr.TotalRequiredQty -= a_qty;
            sourceMr.TotalCost -= sourceMrTotalCost;

            copyOfMR.TotalRequiredQty = a_qty;
            copyOfMR.TotalCost = sourceMrTotalCost;
            copyOfMR.MaintenanceMethod = JobDefs.EMaintenanceMethod.MrpGenerated;

            return copyOfMR;
        }

        throw new PTValidationException("MaterialRequirement not found by Id");
    }

    public MaterialRequirement GetById(BaseId a_materialId)
    {
        return m_materialRequirementList.GetValue(a_materialId);
    }
}