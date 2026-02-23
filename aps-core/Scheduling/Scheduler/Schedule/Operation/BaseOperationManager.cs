using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.Collections;
using static PT.ERPTransmissions.JobT;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of BaseOperation objects.
/// </summary>
public partial class BaseOperationManager : IPTSerializable, AfterRestoreReferences.IAfterRestoreReferences
{
    #region IPTSerializable Members
    internal BaseOperationManager(IReader a_reader, BaseIdGenerator a_idGen)
    {
        _idGen = a_idGen;

        if (a_reader.VersionNumber >= 12521)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                ResourceOperation op = new ResourceOperation(a_reader, a_idGen);
                Add(op);
            }
        }
        else if (a_reader.VersionNumber >= 406)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int opType);
                BaseOperation op;

                if (opType == ResourceOperation.UNIQUE_ID)
                {
                    op = new ResourceOperation(a_reader, a_idGen);
                }
                else if (opType == TankOperation.UNIQUE_ID)
                {
                    op = new TankOperation(a_reader, a_idGen);
                }
                else
                {
                    throw new PTException("Invalid Operation Type deserialized".Localize());
                }

                Add(op);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(_opExternalIdSortedList.Count);
        IDictionaryEnumerator ioEnumerator = OperationsHashInternal.GetEnumerator();
        while (ioEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)ioEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;

            op.Serialize(a_writer);
        }
    }

    public const int UNIQUE_ID = 6;

    public virtual int UniqueId => UNIQUE_ID;

    public void RestoreReferences(ManufacturingOrder a_mo, PlantManager a_plants, CapabilityManager a_capabilities, WarehouseManager a_warehouses, ItemManager a_items, ScenarioDetail a_sd)
    {
        m_manufacturingOrder = a_mo;

        IEnumerator opEnumerator = _opBaseIdSortedList.GetEnumerator();
        while (opEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)opEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;

            if (op is InternalOperation operation)
            {
                operation.RestoreReferences(this, a_mo, a_warehouses, a_items, a_sd.SalesOrderManager, a_sd.TransferOrderManager, a_sd.AttributeManager, a_sd.IdGen, a_plants, a_capabilities);
            }
            else
            {
                throw new PTException("Invalid Operation type encountered during deserialization of BaseOperationManager.".Localize());
            }
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        IEnumerator opEnumerator = _opBaseIdSortedList.GetEnumerator();
        while (opEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)opEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;

            op.RestoreReferences(a_udfManager);
        }
    }
    #endregion

    #region IAfterRestoreReferences
    public void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.SortedIdListHelperFor_AfterRestoreReferences_1(serializationVersionNbr, _idGen, _opBaseIdSortedList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    public void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.SortedIdListHelperFor_AfterRestoreReferences_2(serializationVersionNbr, _opBaseIdSortedList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }
    #endregion

    #region Declarations
    public class BaseOperationManagerException : PTException
    {
        public BaseOperationManagerException(string message)
            : base(message) { }
    }

    private readonly SortedList _opExternalIdSortedList = new ();

    /// <summary>
    /// For accessing operations by EXTERNAL id
    /// </summary>
    public SortedList OperationsHash => _opExternalIdSortedList;

    private readonly SortedList _opBaseIdSortedList = new ();

    /// <summary>
    /// For accessing operations by INTERNAL id
    /// </summary>
    public SortedList OperationsHashInternal => _opBaseIdSortedList;

    private void Add(BaseOperation bo)
    {
        _opExternalIdSortedList.Add(bo.ExternalId, bo);
        _opBaseIdSortedList.Add(bo.Id, bo);
    }

    internal void Remove(PlantManager a_plantManager, BaseOperation a_bo, IScenarioDataChanges a_dataChanges)
    {
        AuditEntry auditEntry = new AuditEntry(a_bo.Id, a_bo.ManufacturingOrder.Id, a_bo);
        
        a_bo.Deleting(a_plantManager, a_dataChanges);
        _opExternalIdSortedList.Remove(a_bo.ExternalId);
        _opBaseIdSortedList.Remove(a_bo.Id);

        a_dataChanges.AuditEntry(auditEntry, false, true );
    }
    #endregion

    #region Construction
    public BaseOperationManager(ManufacturingOrder manufacturingOrder, BaseIdGenerator idGen)
    {
        m_manufacturingOrder = manufacturingOrder;
        _idGen = idGen;
    }
    #endregion

    private readonly BaseIdGenerator _idGen;

    public BaseOperation this[string externalOpId] => (BaseOperation)_opExternalIdSortedList[externalOpId];

    public BaseOperation this[BaseId opId] => (BaseOperation)_opBaseIdSortedList[opId];

    /// <summary>
    /// Iterates the list to find the Operation.
    /// If not found, null is returned.
    /// </summary>
    public BaseOperation Find(string operationName)
    {
        for (int i = 0; i < Count; i++)
        {
            BaseOperation op = GetByIndex(i);
            if (op.Name == operationName)
            {
                return op;
            }
        }

        return null;
    }

    public bool Contains(BaseId opId)
    {
        return _opBaseIdSortedList.Contains(opId);
    }

    public int Count => OperationsHash.Count;

    public BaseOperation GetByIndex(int index)
    {
        return (BaseOperation)OperationsHashInternal.GetByIndex(index);
    }

    #region Properties
    private ManufacturingOrder m_manufacturingOrder;

    /// <summary>
    /// The number of Operations that are Finished.
    /// </summary>
    public int FinishedOperationCount
    {
        get
        {
            int finishedCount = 0;
            for (int i = 0; i < Count; i++)
            {
                if (GetByIndex(i).Finished)
                {
                    finishedCount++;
                }
            }

            return finishedCount;
        }
    }
    #endregion

    #region ERP Transmissions
    public void Receive(JobT.ManufacturingOrder tMO, CapabilityManager machineCapabilities, ScenarioDetail a_sd, bool a_isErpUpdate, IScenarioDataChanges a_dataChanges, UserFieldDefinitionManager a_udfManager, ISystemLogger a_errorReporter, bool a_createDefaultActivity, bool a_autoDeleteOperationAttributes, bool a_newMo = false)
    {
        HashSet<string> trackedOperation = new HashSet<string>();
        for (int opNbr = 0; opNbr < tMO.OperationCount; opNbr++)
        {
            JobT.BaseOperation jobTOp = tMO.GetOperation(opNbr);
            InternalOperation op;

            if (jobTOp is JobT.ResourceOperation ro)
            {
                op = new ResourceOperation(_idGen.NextID(), m_manufacturingOrder, ro, machineCapabilities, a_sd, a_isErpUpdate, a_dataChanges, a_errorReporter, a_createDefaultActivity, a_udfManager, a_autoDeleteOperationAttributes);
            }
            else if (jobTOp is JobT.BatchProcessorOperation)
            {
                throw new PTException("Batch operation types aren't supported yet.".Localize());
            }
            else
            {
                throw new PTException("None supported operation type.".Localize());
            }

            Add(op);
            

            if (!a_newMo)
            {
                ManufacturingOrder originalMo = m_manufacturingOrder.Job.ManufacturingOrders.GetByExternalId(m_manufacturingOrder.ExternalId);
                BaseOperation operation = originalMo.OperationManager[op.ExternalId];
                if (operation != null)
                {
                    AddMaterialRequirements(a_sd, a_dataChanges, operation.MaterialRequirements, jobTOp, op);
                    AddProducts(a_sd, a_dataChanges, operation.Products, jobTOp, op, (InternalOperation)operation);

                    a_dataChanges.AuditEntry(new AuditEntry(operation.Id, operation.ManufacturingOrder.Id, operation, op));

                    foreach (InternalActivity originalActivity in ((InternalOperation)operation).Activities)
                    {
                        InternalActivity newActivity = op.Activities.GetByExternalId(originalActivity.ExternalId);
                        if (newActivity == null)
                        {
                            //Deleted
                            a_dataChanges.AuditEntry(new AuditEntry(originalActivity.Id, operation.Id, originalActivity),false, true);
                        }
                        else
                        {
                            a_dataChanges.AuditEntry(new AuditEntry(originalActivity.Id, operation.Id, originalActivity, newActivity));
                        }

                    }
                }
                else
                {
                    AddMaterialRequirements(a_sd, a_dataChanges, null, jobTOp, op);
                    AddProducts(a_sd, a_dataChanges, null, jobTOp, op, null);
                    AuditOperation(a_dataChanges, op);
                }
            }
            else
            {
                AddMaterialRequirements(a_sd, a_dataChanges, null, jobTOp, op);
                AddProducts(a_sd, a_dataChanges, null, jobTOp, op, null);
                AuditOperation(a_dataChanges, op);
            }

            trackedOperation.Add(op.ExternalId);

                //Init production info for new activities so that JIT dates can be calculated correctly
            for (int i = 0; i < ((InternalOperation)op).Activities.Count; i++)
            {
                InternalActivity act = ((InternalOperation)op).Activities.GetByIndex(i);
                act.InitializeProductionInfoForResources(a_sd.PlantManager, a_sd.ProductRuleManager, a_sd.ExtensionController);
            }
        }

        if (!a_newMo)
        {
            ManufacturingOrder originalMo = m_manufacturingOrder.Job.ManufacturingOrders.GetByExternalId(m_manufacturingOrder.ExternalId);
            for (int i = 0; i < originalMo.OperationManager.Count; i++)
            {
                BaseOperation operation = originalMo.OperationManager.GetByIndex(i);
                if (!trackedOperation.Contains(operation.ExternalId))
                {
                    a_dataChanges.AuditEntry(new AuditEntry(operation.Id, originalMo.Id, operation), false, true);
                }

            }
        }
    }

    private static void AuditOperation(IScenarioDataChanges a_dataChanges, InternalOperation op)
    {
        foreach (InternalActivity internalActivity in op.Activities)
        {
            a_dataChanges.AuditEntry(new AuditEntry(internalActivity.Id, op.Id, internalActivity), true);
        }

        a_dataChanges.AuditEntry(new AuditEntry(op.Id, op.ManufacturingOrder.Id, op), true);
    }

    private void AddProducts(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, ProductsCollection? a_originalProductsCollection, JobT.BaseOperation jobTOp, InternalOperation op, InternalOperation a_originalOperation)
    {
        Dictionary<string,Product> trackedProductList = new Dictionary<string, Product>();
        //Add Products
        for (int i = 0; i < jobTOp.ProductCount; i++)
        {
            JobT.Product jobTProduct = jobTOp.GetProduct(i);
            Product product = new Product(_idGen.NextID(), jobTProduct, a_sd, a_sd.ItemManager, a_sd.ScenarioOptions);

            op.Products.Add(product);

            if (a_originalProductsCollection != null)
            {
                bool isNewProduct = true;
                foreach (Product originalProduct in a_originalProductsCollection)
                {
                    if (originalProduct.ExternalId != product.ExternalId)
                    {
                        continue;
                    }

                    a_dataChanges.AuditEntry(new AuditEntry(originalProduct.Id, a_originalOperation.Id, originalProduct, product));
                    trackedProductList.Add(originalProduct.ExternalId, originalProduct);
                    isNewProduct = false;
                }

                if (isNewProduct)
                {
                    a_dataChanges.AuditEntry(new AuditEntry(product.Id, product), true);
                }
            }
            else
            {
                a_dataChanges.AuditEntry(new AuditEntry(product.Id, product), true);
            }
        }

        if (a_originalProductsCollection != null)
        {
            foreach (Product product in a_originalProductsCollection)
            {
                if (!trackedProductList.TryGetValue(product.ExternalId, out Product deletedProduct))
                {
                    a_dataChanges.AuditEntry(new AuditEntry(product.Id, product), false, true);
                }
            }
        }
    }

    private void AddMaterialRequirements(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, MaterialRequirementsCollection? a_originalMaterialRequirementsCollection, JobT.BaseOperation jobTOp, InternalOperation op)
    {
        Dictionary<string, MaterialRequirement> trackedMrList = new Dictionary<string, MaterialRequirement>();
        //Add Material Requirements
        for (int materialI = 0; materialI < jobTOp.MaterialRequirementCount; materialI++)
        {
            MaterialRequirement material = new MaterialRequirement(_idGen.NextID(), jobTOp.GetMaterialRequirement(materialI), a_sd, a_sd.WarehouseManager, a_sd.ScenarioOptions, op);
            //Verify there are no buy direct materials with an availability date in the future if an activity is running
            if (material.BuyDirect && !material.IssuedComplete && material.NonConstraint && material.LatestSourceDate > a_sd.Clock)
            {
                for (int i = 0; i < (op).Activities.Count; i++)
                {
                    InternalActivity act = (op).Activities.GetByIndex(i);
                    if (act.ProductionStatus == InternalActivityDefs.productionStatuses.SettingUp || act.ProductionStatus == InternalActivityDefs.productionStatuses.Running)
                    {
                        throw new PTValidationException("2956", new object[] { op.Job.ExternalId, op.ManufacturingOrder.ExternalId, op.ExternalId, act.ExternalId, material.ExternalId });
                    }
                }
            }

            if (op.MaterialRequirements.FindByExternalId(material.ExternalId) != null)
            {
                throw new PTValidationException("3078", new object[] { op.ExternalId, material.ExternalId });
            }

            op.MaterialRequirements.Add(material);

            if (a_originalMaterialRequirementsCollection != null)
            {
                MaterialRequirement originalMaterialRequirement = a_originalMaterialRequirementsCollection.FindByExternalId(material.ExternalId);

                if (originalMaterialRequirement == null)
                {
                    a_dataChanges.AuditEntry(new AuditEntry(material.Id, op.Id), true);
                }
                else
                {
                    a_dataChanges.AuditEntry(new AuditEntry(originalMaterialRequirement.Id, originalMaterialRequirement.Operation.Id, material));
                    trackedMrList.Add(material.ExternalId, material);
                }
            }
            else
            {
                a_dataChanges.AuditEntry(new AuditEntry(material.Id, op.Id), true);
            }
        }

        if (a_originalMaterialRequirementsCollection != null)
        {
            foreach (MaterialRequirement materialRequirement in a_originalMaterialRequirementsCollection)
            {
                if (!trackedMrList.TryGetValue(materialRequirement.ExternalId, out MaterialRequirement mr))
                {
                    a_dataChanges.AuditEntry(new AuditEntry(materialRequirement.Id, materialRequirement), false, true);
                }
            }
        }
    }
    #endregion ERP Transmissions

    internal void PopulateJobDataSet(JobDataSet.ManufacturingOrderRow moRow, JobManager jobs, ref JobDataSet dataSet)
    {
        //Set sort to scheduled start and the Operation Id (in case of it being unscheduled)
        dataSet.ResourceOperation.DefaultView.Sort = string.Format("{0} ASC,{1} ASC", dataSet.ResourceOperation.ScheduledStartColumn.ColumnName, dataSet.ResourceOperation.IdColumn.ColumnName);

        IDictionaryEnumerator enumerator = _opExternalIdSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            if (op is ResourceOperation)
            {
                ResourceOperation resOp = (ResourceOperation)op;
                resOp.PopulateJobDataSet(moRow, jobs, ref dataSet);
            }
        }
    }

    /// <summary>
    /// Copy in the Operations and their Activities.
    /// </summary>
    internal void Copy(BaseIdGenerator idGen, BaseOperationManager sourceOperations, ManufacturingOrder sourceMo, ManufacturingOrder parentMo, IScenarioDataChanges a_dataChanges)
    {
        IDictionaryEnumerator enumerator = sourceOperations.OperationsHash.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation sourceOp = (BaseOperation)de.Value;
            BaseOperation newOp;

            if (sourceOp is ResourceOperation)
            {
                newOp = new ResourceOperation(idGen.NextID(), (ResourceOperation)sourceOp, parentMo, a_dataChanges, idGen);
                ResourceOperation.AdjustQuantitiesForTemplateCopy((ResourceOperation)sourceOp, (ResourceOperation)newOp, sourceMo.RequiredQty, parentMo.RequiredQty, sourceMo.GetPrimaryProduct(), sourceMo.ScenarioDetail.ScenarioOptions);
            }
            else
            {
                throw new PTValidationException("2232");
            }

            Add(newOp);
        }
    }

    internal void Commit(bool a_commit, DateTime a_clock, Dictionary<BaseId, BaseId> a_resourcesToInclude)
    {
        IDictionaryEnumerator enumerator = OperationsHash.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            ResourceOperation op = (ResourceOperation)de.Value;
            if (a_commit)
            {
                if (op.Scheduled)
                {
                    Resource resource = op.GetScheduledPrimaryResource();
                    if (a_resourcesToInclude != null && a_resourcesToInclude.ContainsKey(resource.Id))
                    {
                        op.SetCommitDates();
                    }
                }
            }
            else
            {
                op.ClearCommitDates();
            }
        }
    }

    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal void ResetERPStatusUpdateVariables()
    {
        IDictionaryEnumerator enumerator = OperationsHash.GetEnumerator();

        while (enumerator.MoveNext())
        {
            BaseOperation operation = (BaseOperation)enumerator.Value;
            operation.ResetERPStatusUpdateVariables();
        }
    }
    #endregion

    #region Operation updates
    /// <summary>
    /// Update the operations.
    /// </summary>
    /// <param name="a_udfManager"></param>
    /// <param name="a_plantManager"></param>
    /// <param name="a_tempOpManager"></param>
    /// <param name="a_moT"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_erpUpdate"></param>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    /// <param name="a_jobId"></param>
    internal bool Update(UserFieldDefinitionManager a_udfManager, PlantManager a_plantManager, BaseOperationManager a_tempOpManager, JobT.ManufacturingOrder a_moT, ScenarioDetail a_sd, bool a_erpUpdate, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        IDictionaryEnumerator operationEnum = OperationsHash.GetEnumerator();
        List<BaseOperation> deletionList = new ();
        List<BaseOperation> updateList = new ();
        while (operationEnum.MoveNext())
        {
            string externalId = (string)operationEnum.Key;
            BaseOperation tempOp = a_tempOpManager[externalId];
            if (tempOp != null)
            {
                updateList.Add(tempOp);
                if (tempOp is ResourceOperation)
                {
                    ResourceOperation op = (ResourceOperation)operationEnum.Value;
                    JobT.BaseOperation tOp = a_moT.GetOperation(op.ExternalId);
                    if (tOp is JobT.ResourceOperation resourceOperation)
                    {
                        AuditEntry auditEntry = new AuditEntry(op.Id, op.ManufacturingOrder.Id, op);
                        if (op.Update(tempOp, resourceOperation, a_sd.Clock, a_erpUpdate, a_sd.AttributeManager, a_sd.WarehouseManager, this, a_sd.ProductRuleManager, a_t, a_udfManager, a_dataChanges))
                        {
                            updated = true;
                            a_dataChanges.AuditEntry(auditEntry);
                        }
                    }
                }
            }
            else
            {
                deletionList.Add((BaseOperation)operationEnum.Value);
            }
        }

        //Delete non updated operations
        foreach (BaseOperation op in deletionList)
        {
            Remove(a_plantManager, op, a_dataChanges);
            updated = true;
        }

        //Add new Operations
        //Must create a new operation here because the tempMo operation has references to the temp MO.
        for (int i = 0; i < a_tempOpManager.Count; i++)
        {
            if (a_tempOpManager.GetByIndex(i) is ResourceOperation tempOp && !updateList.Contains(tempOp))
            {
                BaseOperation newOp = new ResourceOperation(_idGen.NextID(), tempOp, m_manufacturingOrder, a_dataChanges, _idGen);

                Add(newOp);
            }
        }

        return updated;
    }
    #endregion

    #region Delete Validation
    /// <summary>
    /// Checks to make sure the Warehouse is not in use.
    /// </summary>
    internal void ValidateWarehouseDelete(Warehouse warehouse)
    {
        IEnumerator enumerator = _opBaseIdSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            op.ValidateWarehouseDelete(warehouse);
        }
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        IEnumerator enumerator = _opBaseIdSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            op.ValidateStorageAreaDelete(a_storageArea, a_deleteProfile);
        }
    }

    /// <summary>
    /// Checks to make sure the Inventory is not in use.
    /// </summary>
    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        IEnumerator enumerator = _opBaseIdSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            op.ValidateInventoryDelete(a_deleteProfile);
        }
    }

    /// <summary>
    /// Checks to make sure the Item is not in use.
    /// </summary>
    internal void ValidateItemDelete(ItemDeleteProfile a_itemDeleteProfile)
    {
        IEnumerator enumerator = _opBaseIdSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            op.ValidateItemDelete(a_itemDeleteProfile);
        }
    } 
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        IEnumerator enumerator = _opBaseIdSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            op.ValidateItemStorageDelete(a_itemStorageDeleteProfile);
        }
    }
    #endregion

    /// <summary>
    /// If all of the Operations in total only make one Item as a Product then that Item is returned.
    /// Otherwise, null is returned.
    /// </summary>
    /// <returns></returns>
    internal Item GetOnlyProductMade()
    {
        IEnumerator enumerator = _opBaseIdSortedList.GetEnumerator();
        Item item = null;
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            Item nextItem = op.GetOnlyProductMade();
            if (nextItem != null && item != null && nextItem != item)
            {
                return null;
            }

            if (nextItem != null)
            {
                item = nextItem;
            }
        }

        return item;
    }

    /// <summary>
    /// Returns the Product at index 0 for the last Operation (by index) that has a Product, or null if no Operation has a Product.
    /// </summary>
    /// <returns></returns>
    internal Item GetFirstProductMadeByLatestOperation()
    {
        for (int i = _opBaseIdSortedList.Count - 1; i >= 0; i--)
        {
            BaseOperation op = (BaseOperation)_opBaseIdSortedList.GetByIndex(i);
            if (op.Products.Count > 0)
            {
                return op.Products.PrimaryProduct.Item;
            }
        }

        return null;
    }

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long a_clockAdvanceTicks)
    {
        IEnumerator enumerator = _opBaseIdSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            op.AdjustDemoDataForClockAdvance(a_clockAdvanceTicks);
        }
    }
    #endregion

    #region Quantity Adjustments
    /// <summary>
    /// Adjust the quantities of operations, activities, products, and stock material requirements
    /// by the ratio.
    /// </summary>
    /// <param name="newReqQty"></param>
    /// <returns> boolean for whether the job needs to unschedule if it is no longer able to schedule on the resource</returns>
    internal bool AdjustRequiredQty(decimal a_ratio, decimal a_newMORequiredQty, BaseOperation a_sourceOfChangeOp, InternalActivity a_sourceOfChangeAct, ProductRuleManager a_productRuleManager)
    {
        IDictionaryEnumerator ioEnumerator = OperationsHashInternal.GetEnumerator();
        bool needsToUnschedule = false;
        while (ioEnumerator.MoveNext())
        {
            ResourceOperation op = (ResourceOperation)ioEnumerator.Value;

            if (op == a_sourceOfChangeOp)
            {
                needsToUnschedule |= op.AdjustRequiredQty(a_ratio, a_newMORequiredQty, a_sourceOfChangeAct, m_manufacturingOrder.GetPrimaryProduct(), a_productRuleManager);
            }
            else
            {
                needsToUnschedule |= op.AdjustRequiredQty(a_ratio, a_newMORequiredQty, null, m_manufacturingOrder.GetPrimaryProduct(), a_productRuleManager);
            }

            op.SimulationInitializationOfActivities(op.Activities);
        }

        return needsToUnschedule;
    }

    /// <summary>
    /// Adjust the quantities of operations, activities, products, and stock material requirements
    /// by the ratio for Storage resize.
    /// </summary>
    /// <param name="a_ratio"></param>
    /// <param name="a_newMORequiredQty"></param>
    /// <param name="a_primaryRes"></param>
    /// <param name="a_productRuleManager"></param>
    /// <returns> boolean for whether the MO can be resized</returns>
    internal bool AdjustRequiredQtyForStorage(decimal a_ratio, decimal a_newMORequiredQty, Resource a_primaryRes, ProductRuleManager a_productRuleManager)
    {
        IDictionaryEnumerator ioEnumerator = OperationsHashInternal.GetEnumerator();
        bool adjustedAtLeastOneOp = false;
        while (ioEnumerator.MoveNext())
        {
            if (ioEnumerator.Value is ResourceOperation ro)
            {
                if (!ro.AdjustRequiredQtyForStorage(a_ratio, a_newMORequiredQty, a_primaryRes, m_manufacturingOrder.GetPrimaryProduct(), a_productRuleManager))
                {
                    continue;
                }

                adjustedAtLeastOneOp = true;
                ro.SimulationInitializationOfActivities(ro.Activities);
            }
        }

        return adjustedAtLeastOneOp;
    }
    #endregion
    
    internal IDictionaryEnumerator GetOpEnumerator()
    {
        return _opExternalIdSortedList.GetEnumerator();
    }

    internal void Deleting(PlantManager a_plantManager, IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < Count; ++i)
        {
            BaseOperation bo = GetByIndex(i);
            bo.Deleting(a_plantManager, a_dataChanges);
        }
    }

    internal bool DeleteUserFieldByExternalId(string a_udfExternalId)
    {
        bool removed = false;
        for (int i = 0; i < Count; ++i)
        {
            BaseOperation bo = GetByIndex(i);
            removed |= bo.UserFields.Remove(a_udfExternalId);
        }

        return removed;
    }
}