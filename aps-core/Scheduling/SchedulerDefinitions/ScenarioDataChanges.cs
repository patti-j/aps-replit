using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Extensions;

namespace PT.SchedulerDefinitions;

/// <summary>
/// The complete set of Ids of new, updated, and deleted objects
/// </summary>
public interface IScenarioDataChanges
{
    /// <summary>
    /// There is at least one object change
    /// </summary>
    bool HasChanges { get; }

    /// <summary>
    /// New, updated, and deleted jobs
    /// </summary>
    IDataObjectChanges JobChanges { get; }

    IDataObjectChanges TemplateChanges { get; }

    IDataObjectChanges CapacityIntervalChanges { get; }
    IDataObjectChanges RecurringCapacityIntervalChanges { get; }
    IDataObjectChanges PlantChanges { get; }
    IDataObjectChanges DepartmentChanges { get; }
    IDataObjectChanges MachineChanges { get; }
    IDataObjectChanges CapabilityChanges { get; }
    IDataObjectChanges PurchaseToStockChanges { get; }
    IDataObjectChanges WarehouseChanges { get; }
    IDataObjectChanges LookupCleanoutTriggerTablesUpdated { get; }
    IDataObjectChanges LookupItemCleanoutTablesUpdated { get; }
    IDataObjectChanges LookupAttributeCodeTablesUpdated { get; }
    IDataObjectChanges CellChanges { get; }
    IDataObjectChanges SalesOrderChanges { get; }
    IDataObjectChanges TransferOrderChanges { get; }
    IDataObjectChanges ItemChanges { get; }
    IDataObjectChanges InventoryChanges { get; }
    IDataObjectChanges StorageAreaChanges { get; }
    IDataObjectChanges StorageAreaConnectorChanges { get; }
    IDataObjectChanges UserChanges { get; }
    IDataObjectChanges UserWorkspaceChanges { get; }
    IDataObjectChanges SharedWorkspacesChanges { get; }
    IDataObjectChanges CustomerChanges { get; }
    IDataObjectChanges ChangeOrderChanges { get; }
    IDataObjectChanges FromRangeSetChanges { get; }
    IDataObjectChanges ShopViewResourceOptionsChanges { get; }
    IDataObjectChanges BalancedCompositeDispatcherDefinitionChanges { get; }
    IDataObjectChanges PermissionChanges { get; }
    IDataObjectChanges PlantPermissionChanges { get; }
    IDataObjectChanges PTAttributeChanges { get; }
    IDataObjectChanges ResourceConnectorChanges { get; }
    IDataObjectChanges ActivityChanges { get; }
    IDataObjectChanges CompatibilityCodeChanges { get; }
    IDataObjectChanges UserFieldDefinitionChanges { get; }
    IEnumerable<IAuditEntry> AuditEntries { get; }

    bool KPIChanges { get; set; }

    /// <summary>
    /// A simulation has been run after these changes happened
    /// </summary>
    bool SimulationProcessed { set; }

    /// <summary>
    /// There is at least one object change that has not been process by a simulation
    /// </summary>
    bool HasUnprocessedChanges { get; }

    /// <summary>
    /// Flag Eligibility changes by object Id or using BaseId.NULL_ID if the specific object is
    /// not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    void FlagEligibilityChanges(BaseId a_objectId);

    /// <summary>
    /// Flag Production changes by object Id or using BaseId.NULL_ID if the specific object is
    /// not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    void FlagProductionChanges(BaseId a_objectId);

    /// <summary>
    /// Flag Constraint changes by object Id or using BaseId.NULL_ID if the specific object is
    /// not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    void FlagConstraintChanges(BaseId a_objectId);
    
    /// <summary>
    /// Flag Visual changes by object Id or using BaseId.NULL_ID if the specific object is
    /// not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    void FlagVisualChanges(BaseId a_objectId);

    /// <summary>
    /// Flag Job has need date or other changes that would affect JIT calculations.
    /// </summary>
    /// <param name="a_objectId"></param>
    void FlagJitChanges(BaseId a_objectId);

    /// <summary>
    /// Check if there are Eligibility changes by object Id or by BaseId.NULL_ID if the specific object
    /// is not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <returns></returns>
    bool HasEligibilityChanges(BaseId a_objectId);

    /// <summary>
    /// Check if there are JIT changes by object Id or by BaseId.NULL_ID if the specific object
    /// is not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <returns></returns>
    bool HasJitChanges(BaseId a_objectId);

    /// <summary>
    /// Check if there are Production changes by object Id or by BaseId.NULL_ID if the specific object
    /// is not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <returns></returns>
    bool HasProductionChanges(BaseId a_objectId);

    /// <summary>
    /// Check if there are Constraint changes by object Id or by BaseId.NULL_ID if the specific object
    /// is not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <returns></returns>
    bool HasConstraintChanges(BaseId a_objectId);
    
    /// <summary>
    /// Check if there are Visual changes by object Id or by BaseId.NULL_ID if the specific object
    /// is not relevant
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <returns></returns>
    bool HasVisualChanges(BaseId a_objectId);

    // This function might not be needed. It seems like all the usages of IDataChange where the Id of the object being
    // changed are added directly to the underlying DataChange object so the current intention
    // is to just directly add change values to the dictionary on the corresponding DataChange instead
    // of trying to use this function and route the changes through a huge switch statement. 
    //void FlagFieldChange(string a_objectType, BaseId a_objectId, string a_fieldName, string a_newValue);

    void AuditEntry(IAuditEntry a_auditEntry, bool a_added = false, bool a_deleted = false);
}

/// <summary>
/// New, updated, and deleted objects
/// </summary>
public interface IDataObjectChanges
{
    /// <summary>
    /// There is at least one new, updated, or changed object
    /// </summary>
    bool HasChanges { get; }

    int TotalAddedObjects { get; }
    int TotalUpdatedObjects { get; }
    int TotalDeletedObjects { get; }
    IEnumerable<BaseId> Added { get; }
    IEnumerable<BaseId> Updated { get; }
    IEnumerable<BaseId> Deleted { get; }
    void AddedObject(BaseId a_objectId);
    void UpdatedObject(BaseId a_objectId);
    void DeletedObject(BaseId a_objectId);
    HashSet<BaseId> GetChangedIds();
}

public class ScenarioDataChanges : IScenarioDataChanges
{
    private readonly HashSet<BaseId> m_eligibilityChanges = new ();
    private readonly HashSet<BaseId> m_productionChanges = new ();
    private readonly HashSet<BaseId> m_constraintChanges = new ();
    private readonly HashSet<BaseId> m_visualChanges = new ();
    private readonly HashSet<BaseId> m_jitChanges = new ();
    private readonly List<IAuditEntry> m_auditEntries = new List<IAuditEntry>();

    public void FlagEligibilityChanges(BaseId a_objectId)
    {
        m_eligibilityChanges.AddIfNew(a_objectId);
    }

    public void FlagProductionChanges(BaseId a_objectId)
    {
        m_productionChanges.AddIfNew(a_objectId);
    }

    public void FlagConstraintChanges(BaseId a_objectId)
    {
        m_constraintChanges.AddIfNew(a_objectId);
    }

    public void FlagVisualChanges(BaseId a_objectId)
    {
        m_visualChanges.AddIfNew(a_objectId);
    }

    public void FlagJitChanges(BaseId a_objectId)
    {
        m_jitChanges.AddIfNew(a_objectId);
    }

    public bool HasEligibilityChanges(BaseId a_objectId)
    {
        if (a_objectId != BaseId.NULL_ID)
        {
            return m_eligibilityChanges.Contains(a_objectId);
        }

        return m_eligibilityChanges.Count > 0;
    }

    public bool HasProductionChanges(BaseId a_objectId)
    {
        if (a_objectId != BaseId.NULL_ID)
        {
            return m_productionChanges.Contains(a_objectId);
        }

        return m_productionChanges.Count > 0;
    }

    public bool HasConstraintChanges(BaseId a_objectId)
    {
        if (a_objectId != BaseId.NULL_ID)
        {
            return m_constraintChanges.Contains(a_objectId);
        }

        return m_constraintChanges.Count > 0;
    }

    public bool HasVisualChanges(BaseId a_objectId)
    {
        if (a_objectId != BaseId.NULL_ID)
        {
            return m_visualChanges.Contains(a_objectId);
        }

        return m_visualChanges.Count > 0;
    }

    public void AuditEntry(IAuditEntry a_auditEntry, bool a_added = false, bool a_deleted = false)
    {
        a_auditEntry.SetFlags(a_added, a_deleted);

        if (!a_deleted && !a_added)
        {
            a_auditEntry.Compare();
        }

        if (!a_auditEntry.Added && !a_auditEntry.Deleted && !a_auditEntry.Updated)
        {
            return;
        }

        m_auditEntries.Add(a_auditEntry);
    }

    public bool HasJitChanges(BaseId a_objectId)
    {
        if (a_objectId != BaseId.NULL_ID)
        {
            return m_jitChanges.Contains(a_objectId);
        }

        return m_jitChanges.Count > 0;
    }

    //public void FlagFieldChange(string a_objectType, BaseId a_objectId, string a_fieldName, string a_newValue)
    //{

    //}

    public ScenarioDataChanges()
    {
        JobChanges = new DataObjectChanges();
        TemplateChanges = new DataObjectChanges();
        CapacityIntervalChanges = new DataObjectChanges();
        RecurringCapacityIntervalChanges = new DataObjectChanges();
        PlantChanges = new DataObjectChanges();
        DepartmentChanges = new DataObjectChanges();
        MachineChanges = new DataObjectChanges();
        CapabilityChanges = new DataObjectChanges();
        PurchaseToStockChanges = new DataObjectChanges();
        WarehouseChanges = new DataObjectChanges();
        LookupSetupCodeTablesUpdated = new DataObjectChanges();
        LookupCleanoutTriggerTablesUpdated = new DataObjectChanges();
        LookupItemCleanoutTablesUpdated = new DataObjectChanges();
        LookupAttributeCodeTablesUpdated = new DataObjectChanges();
        CellChanges = new DataObjectChanges();
        SalesOrderChanges = new DataObjectChanges();
        TransferOrderChanges = new DataObjectChanges();
        ItemChanges = new DataObjectChanges();
        InventoryChanges = new DataObjectChanges();
        UserChanges = new DataObjectChanges();
        UserWorkspaceChanges = new DataObjectChanges();
        SharedWorkspacesChanges = new DataObjectChanges();
        CustomerChanges = new DataObjectChanges();
        ChangeOrderChanges = new DataObjectChanges();
        FromRangeSetChanges = new DataObjectChanges();
        ShopViewResourceOptionsChanges = new DataObjectChanges();
        BalancedCompositeDispatcherDefinitionChanges = new DataObjectChanges();
        PermissionChanges = new DataObjectChanges();
        PlantPermissionChanges = new DataObjectChanges();
        PTAttributeChanges = new DataObjectChanges();
        ResourceConnectorChanges = new DataObjectChanges();
        ActivityChanges = new DataObjectChanges();
        CompatibilityCodeChanges = new DataObjectChanges();
        UserFieldDefinitionChanges = new DataObjectChanges();
        StorageAreaChanges = new DataObjectChanges();
        StorageAreaConnectorChanges = new DataObjectChanges();
        KPIChanges = false;
        SystemSettingChanges = new DataObjectChanges();
    }

    public IDataObjectChanges JobChanges { get; }
    public IDataObjectChanges TemplateChanges { get; }
    public IDataObjectChanges CapacityIntervalChanges { get; }
    public IDataObjectChanges RecurringCapacityIntervalChanges { get; }
    public IDataObjectChanges PlantChanges { get; }
    public IDataObjectChanges DepartmentChanges { get; }
    public IDataObjectChanges MachineChanges { get; }
    public IDataObjectChanges CapabilityChanges { get; }
    public IDataObjectChanges PurchaseToStockChanges { get; }
    public IDataObjectChanges WarehouseChanges { get; }
    public IDataObjectChanges LookupSetupCodeTablesUpdated { get; }

    public IDataObjectChanges LookupCleanoutTriggerTablesUpdated { get; }
    public IDataObjectChanges LookupItemCleanoutTablesUpdated { get; }

    public IDataObjectChanges LookupAttributeCodeTablesUpdated { get; }
    public IDataObjectChanges CellChanges { get; }
    public IDataObjectChanges SalesOrderChanges { get; }
    public IDataObjectChanges TransferOrderChanges { get; }
    public IDataObjectChanges ItemChanges { get; }
    public IDataObjectChanges InventoryChanges { get; }
    public IDataObjectChanges StorageAreaChanges { get; }
    public IDataObjectChanges StorageAreaConnectorChanges { get; }
    public IDataObjectChanges UserChanges { get; }
    public IDataObjectChanges UserWorkspaceChanges { get; }
    public IDataObjectChanges SharedWorkspacesChanges { get; }
    public IDataObjectChanges CustomerChanges { get; }
    public IDataObjectChanges ChangeOrderChanges { get; }
    public IDataObjectChanges FromRangeSetChanges { get; }
    public IDataObjectChanges ShopViewResourceOptionsChanges { get; }
    public IDataObjectChanges BalancedCompositeDispatcherDefinitionChanges { get; }
    public IDataObjectChanges PermissionChanges { get; }
    public IDataObjectChanges PlantPermissionChanges { get; }
    public IDataObjectChanges PTAttributeChanges { get; }
    public IDataObjectChanges ResourceConnectorChanges { get; }
    public IDataObjectChanges ActivityChanges { get; }
    public IDataObjectChanges CompatibilityCodeChanges { get; }
    public IDataObjectChanges UserFieldDefinitionChanges { get; }
    public IEnumerable<IAuditEntry> AuditEntries => m_auditEntries;
    public IDataObjectChanges SystemSettingChanges { get; }

    public bool KPIChanges { get; set; }

    public bool SimulationProcessed { get; set; }

    public bool HasUnprocessedChanges => !SimulationProcessed && HasChanges;

    public bool HasChanges =>
        JobChanges.HasChanges || TemplateChanges.HasChanges || CapacityIntervalChanges.HasChanges || RecurringCapacityIntervalChanges.HasChanges || PlantChanges.HasChanges || DepartmentChanges.HasChanges || MachineChanges.HasChanges 
        || CapabilityChanges.HasChanges || PurchaseToStockChanges.HasChanges || WarehouseChanges.HasChanges || LookupSetupCodeTablesUpdated.HasChanges || LookupCleanoutTriggerTablesUpdated.HasChanges || LookupItemCleanoutTablesUpdated.HasChanges || LookupAttributeCodeTablesUpdated.HasChanges 
        || CellChanges.HasChanges || TransferOrderChanges.HasChanges || ItemChanges.HasChanges || InventoryChanges.HasChanges || SalesOrderChanges.HasChanges || UserChanges.HasChanges || UserWorkspaceChanges.HasChanges 
        || SharedWorkspacesChanges.HasChanges || CustomerChanges.HasChanges || ChangeOrderChanges.HasChanges || FromRangeSetChanges.HasChanges || ShopViewResourceOptionsChanges.HasChanges || BalancedCompositeDispatcherDefinitionChanges.HasChanges 
        || PermissionChanges.HasChanges || PlantPermissionChanges.HasChanges || PTAttributeChanges.HasChanges || ResourceConnectorChanges.HasChanges || KPIChanges || ActivityChanges.HasChanges || CompatibilityCodeChanges.HasChanges
        || UserFieldDefinitionChanges.HasChanges || StorageAreaChanges.HasChanges || StorageAreaConnectorChanges.HasChanges || SystemSettingChanges.HasChanges;
}

public class DataObjectChanges : IDataObjectChanges
{
    internal DataObjectChanges()
    {
    }


    public bool HasChanges => m_added.Count > 0 || m_updated.Count > 0 || m_deleted.Count > 0;

    public int TotalAddedObjects => m_added.Count;

    public int TotalUpdatedObjects => m_updated.Count;

    public int TotalDeletedObjects => m_deleted.Count;
    public IEnumerable<BaseId> Added => m_added;

    public IEnumerable<BaseId> Updated => m_updated;

    public IEnumerable<BaseId> Deleted => m_deleted;
    private readonly HashSet<BaseId> m_added = new();
    private readonly HashSet<BaseId> m_updated = new();
    private readonly HashSet<BaseId> m_deleted = new();

    public void AddedObject(BaseId a_objectId)
    {
        m_added.AddIfNew(a_objectId);
    }

    public void UpdatedObject(BaseId a_objectId)
    {
        #if DEBUG
        if (m_deleted.Contains(a_objectId))
        {
            throw new DebugException("An object being updated is also in the DataObjectChanges.m_deleted list.");
        }
        #endif

        m_updated.AddIfNew(a_objectId);
    }

    public void DeletedObject(BaseId a_objectId)
    {
        #if DEBUG
        if (m_updated.Contains(a_objectId))
        {
            //throw new DebugException("An object being deleted is also in the DataObjectChanges.m_updated list.");
        }
        #endif

        m_deleted.AddIfNew(a_objectId);
    }

    /// <summary>
    /// Returns all unique Ids from added, updated, and deleted objects
    /// </summary>
    /// <returns></returns>
    public HashSet<BaseId> GetChangedIds()
    {
        // Combine all unique IDs from added, updated, and deleted sets
        HashSet<BaseId> result = new (m_added);
        result.UnionWith(m_updated);
        result.UnionWith(m_deleted);
        return result;
    }
}