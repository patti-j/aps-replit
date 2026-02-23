
using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Extensions;
using PT.Common.PTMath;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

public partial class StorageAreaConnector : BaseObject, IPTSerializable
{
    private readonly HashSet<BaseId> m_resourceInIdList = new ();
    private readonly HashSet<BaseId> m_resourceOutIdList = new ();
    private readonly HashSet<BaseId> m_storageAreaInIdList = new ();
    private readonly HashSet<BaseId> m_storageAreaOutIdList = new ();

    public  HashSet<BaseId> ResourceInList => m_resourceInIdList;
    public  HashSet<BaseId> ResourceOutList => m_resourceOutIdList;
    public  HashSet<BaseId> StorageAreaInList => m_storageAreaInIdList;
    public HashSet<BaseId> StorageAreaOutList => m_storageAreaOutIdList;

    private BoolVector32 m_boolVector32;
    private const short c_counterFlowIdx = 0;

    /// <summary>
    /// Flag to indicate a material can flow in both directions at once.
    /// </summary>
    public bool CounterFlow
    {
        get => m_boolVector32[c_counterFlowIdx];
        set => m_boolVector32[c_counterFlowIdx] = value;
    }
    private int m_storageInFlowLimit;
    /// <summary>
    /// Indicates how many objects can store material into storage at the same time
    /// </summary>
    public int StorageInFlowLimit
    {
        get => m_storageInFlowLimit;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("3094", new object[] { ExternalId});
            }
            m_storageInFlowLimit = value;
        }
    }

    private int m_storageOutFlowLimit;
    /// <summary>
    /// Indicates how many objects can withdraw material from storage at the same time
    /// </summary>
    public int StorageOutFlowLimit
    {
        get => m_storageOutFlowLimit;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("3095", new object[] { ExternalId });
            }

            m_storageOutFlowLimit = value;
        }
    }

    private int m_counterFlowLimit;
    /// <summary>
    /// Indicates the total limit of storing and withdrawing
    /// </summary>
    public int CounterflowLimit
    {
        get => m_counterFlowLimit;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("3096", new object[] { ExternalId });
            }
            m_counterFlowLimit = value;
        }
    }

    public StorageAreaConnector(IReader a_reader) : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_boolVector32 = new BoolVector32(a_reader);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_resourceInIdList.Add(new BaseId(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_resourceOutIdList.Add(new BaseId(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_storageAreaInIdList.Add(new BaseId(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_storageAreaOutIdList.Add(new BaseId(a_reader));
            }

            a_reader.Read(out m_storageInFlowLimit);
            a_reader.Read(out m_storageOutFlowLimit);
            a_reader.Read(out m_counterFlowLimit);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_boolVector32.Serialize(a_writer);

        a_writer.Write(m_resourceInIdList.Count);
        foreach (BaseId baseId in m_resourceInIdList)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_resourceOutIdList.Count);
        foreach (BaseId baseId in m_resourceOutIdList)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_storageAreaInIdList.Count);
        foreach (BaseId baseId in m_storageAreaInIdList)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_storageAreaOutIdList.Count);
        foreach (BaseId baseId in m_storageAreaOutIdList)
        {
            baseId.Serialize(a_writer);
        }

        a_writer.Write(m_storageInFlowLimit);
        a_writer.Write(m_storageOutFlowLimit);
        a_writer.Write(m_counterFlowLimit);
    }

    internal StorageAreaConnector(BaseId a_id) : base(a_id) { }
    internal StorageAreaConnector(BaseId a_id, string a_externalId) : base(a_id, a_externalId) { }
    internal StorageAreaConnector(BaseId a_id, PTObjectBase a_ptObject) : base(a_id, a_ptObject) { }
    internal StorageAreaConnector(BaseId a_id, PTObjectIdBase a_ptObject) : base(a_id, a_ptObject) { }
    internal StorageAreaConnector(BaseId a_newId, BaseObject a_sourceBaseObject) : base(a_newId, a_sourceBaseObject) { }
    internal StorageAreaConnector(BaseId a_id, StorageAreaConnector a_sourceConnector)
        : base(a_id)
    {
        Copy(a_sourceConnector);
    }

    private void Copy(StorageAreaConnector a_sourceConnector)
    {
        Name = string.Format("Copy of {0}".Localize(), a_sourceConnector.Name);
        Description = a_sourceConnector.Description;

        a_sourceConnector.m_storageAreaInIdList.ShallowCopyTo(m_storageAreaInIdList);
        a_sourceConnector.m_storageAreaOutIdList.ShallowCopyTo(m_storageAreaOutIdList);
        a_sourceConnector.m_resourceInIdList.ShallowCopyTo(m_resourceInIdList);
        a_sourceConnector.m_resourceOutIdList.ShallowCopyTo(m_resourceOutIdList);
    }

    public override string DefaultNamePrefix => "Storage Area Connector";

    public override int UniqueId => 1121;

    public Warehouse Warehouse;

    internal void RestoreReferences(Warehouse a_warehouse)
    {
        Warehouse = a_warehouse;
    }

    internal void Deleting(Resource a_resourceToDelete)
    {
        m_resourceInIdList.Remove(a_resourceToDelete.Id);
        m_resourceOutIdList.Remove(a_resourceToDelete.Id);
    }

    internal void Deleting(StorageArea a_storageAreaToDelete)
    {
        m_storageAreaInIdList.Remove(a_storageAreaToDelete.Id);
        m_storageAreaOutIdList.Remove(a_storageAreaToDelete.Id);
    }

    internal void Update(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, WarehouseT.StorageAreaConnector a_storageAreaConnectorT, StorageAreaConnector a_storageAreaConnector, Warehouse a_warehouse, bool a_autoDeleteStorageAreaConnectorsIn, bool a_autoDeleteStorageAreaConnectorsOut, bool a_autoDeleteResourceStorageAreaConnectorsIn, bool a_autoDeleteResourceStorageAreaConnectorsOut)
    {
        base.Update(a_storageAreaConnector);
        base.UpdateUserFields(a_storageAreaConnectorT.UserFields, a_udfManager, UserField.EUDFObjectType.StorageAreaConnectors);

        Warehouse = a_warehouse;

        if (a_storageAreaConnectorT.CounterFlowSet)
        {
            CounterFlow = a_storageAreaConnectorT.CounterFlow;
        }

        StorageInFlowLimit = a_storageAreaConnectorT.StorageInFlowLimit;
        StorageOutFlowLimit = a_storageAreaConnectorT.StorageOutFlowLimit;
        CounterflowLimit = a_storageAreaConnectorT.CounterFlowLimit;

        #region Convert In/Out Connector External Id collection to Base Id collections
        List<BaseId> storageAreaInCollection = new List<BaseId>();
        foreach (string storageAreaExternalId in a_storageAreaConnectorT.StorageAreaConnectorIn)
        {
            StorageArea storageArea = a_warehouse.StorageAreas.GetByExternalId(storageAreaExternalId);
            if (storageArea == null)
            {
                throw new PTValidationException("3084", new object[] { storageAreaExternalId });
            }
            storageAreaInCollection.Add(storageArea.Id);
        }

        List<BaseId> storageAreaOutCollection = new List<BaseId>();
        foreach (string storageAreaExternalId in a_storageAreaConnectorT.StorageAreaConnectorOut)
        {
            StorageArea storageArea = a_warehouse.StorageAreas.GetByExternalId(storageAreaExternalId);
            if (storageArea == null)
            {
                throw new PTValidationException("3084", new object[] { storageAreaExternalId});
            }
            
            storageAreaOutCollection.Add(storageArea.Id);
        }

        List<BaseId> resourceInCollection = new List<BaseId>();
        foreach (ResourceKeyExternal resourceKeyExternal in a_storageAreaConnectorT.ResourceStorageAreaConnectorIn)
        {
            Resource resource = a_sd.PlantManager.GetResource(resourceKeyExternal.PlantExternalId, resourceKeyExternal.DepartmentExternalId, resourceKeyExternal.ResourceExternalId);
            if (resource == null)
            {
                throw new PTValidationException("2577", new object[] { resourceKeyExternal.PlantExternalId, resourceKeyExternal.DepartmentExternalId, resourceKeyExternal.ResourceExternalId });
            }
            resourceInCollection.Add(resource.Id);
        }

        List<BaseId> resourceOutCollection = new List<BaseId>();
        foreach (ResourceKeyExternal resourceKeyExternal in a_storageAreaConnectorT.ResourceStorageAreaConnectorOut)
        {
            Resource resource = a_sd.PlantManager.GetResource(resourceKeyExternal.PlantExternalId, resourceKeyExternal.DepartmentExternalId, resourceKeyExternal.ResourceExternalId);
            if (resource == null)
            {
                throw new PTValidationException("2577", new object[] { resourceKeyExternal.PlantExternalId, resourceKeyExternal.DepartmentExternalId, resourceKeyExternal.ResourceExternalId });
            }
            resourceOutCollection.Add(resource.Id);
        }
        #endregion

        UpdateConnections(storageAreaInCollection, storageAreaOutCollection, resourceInCollection, resourceOutCollection,
            a_autoDeleteStorageAreaConnectorsIn,a_autoDeleteStorageAreaConnectorsOut, a_autoDeleteResourceStorageAreaConnectorsIn, a_autoDeleteResourceStorageAreaConnectorsOut);
    }

    internal void UpdateConnections(List<BaseId> a_storageAreaInIds, List<BaseId> a_storageAreaOutIds, List<BaseId> a_resourceInIds, List<BaseId> a_resourceOutIds, bool a_autoDeleteStorageAreaConnectorIn, bool a_autoDeleteStorageAreaConnectorOut, bool a_autoDeleteResourceStorageAreaConnectorIn, bool a_autoDeleteResourceStorageAreaConnectorOut)
    {
        if (a_autoDeleteStorageAreaConnectorIn)
        {
            m_storageAreaInIdList.Clear();
        }

        if (a_autoDeleteStorageAreaConnectorOut)
        {
            m_storageAreaOutIdList.Clear();
        }

        if (a_autoDeleteResourceStorageAreaConnectorIn)
        {
            m_resourceInIdList.Clear();
        }

        if (a_autoDeleteResourceStorageAreaConnectorOut)
        {
            m_resourceOutIdList.Clear();  
        }

        m_storageAreaInIdList.AddRangeIfNew(a_storageAreaInIds);
        m_storageAreaOutIdList.AddRangeIfNew(a_storageAreaOutIds);
        m_resourceInIdList.AddRangeIfNew(a_resourceInIds);
        m_resourceOutIdList.AddRangeIfNew(a_resourceOutIds);
    }

    public void PopulateImportDataSet(PtImportDataSet a_ds, PtImportDataSet.WarehousesRow a_wRow, ScenarioDetail a_sd)
    {
        PtImportDataSet.StorageAreaConnectorRow storageAreaConnectorRow = a_ds.StorageAreaConnector.AddStorageAreaConnectorRow(
            ExternalId,
            Name,
            Notes,
            Description,
            a_wRow,
            CounterFlow,
            StorageInFlowLimit,
            StorageOutFlowLimit,
            CounterflowLimit,
            UserFields == null ? "" : UserFields.GetUserFieldImportString()
            );

        foreach (BaseId baseId in m_resourceInIdList)
        {
            InternalResource resource = a_sd.PlantManager.GetResource(baseId);
            a_ds.ResourceStorageAreaConnectorIn.AddResourceStorageAreaConnectorInRow(storageAreaConnectorRow.ExternalId, resource.ExternalId, resource.Department.ExternalId, resource.Department.Plant.ExternalId, storageAreaConnectorRow.WarehouseExternalId);
        }
        
        foreach (BaseId baseId in m_resourceOutIdList)
        {
            InternalResource resource = a_sd.PlantManager.GetResource(baseId);
            a_ds.ResourceStorageAreaConnectorOut.AddResourceStorageAreaConnectorOutRow(storageAreaConnectorRow.ExternalId, resource.ExternalId, resource.Department.ExternalId, resource.Department.Plant.ExternalId, storageAreaConnectorRow.WarehouseExternalId);
        }
        
        foreach (BaseId baseId in m_storageAreaInIdList)
        {
            StorageArea storageArea = a_sd.WarehouseManager.GetByExternalId(a_wRow.ExternalId).StorageAreas.GetValue(baseId);
            a_ds.StorageAreaConnectorIn.AddStorageAreaConnectorInRow(storageAreaConnectorRow.ExternalId, storageArea.ExternalId, storageAreaConnectorRow.WarehouseExternalId);
        }
        foreach (BaseId baseId in m_storageAreaOutIdList)
        {
            StorageArea storageArea = a_sd.WarehouseManager.GetByExternalId(a_wRow.ExternalId).StorageAreas.GetValue(baseId);
            a_ds.StorageAreaConnectorOut.AddStorageAreaConnectorOutRow(storageAreaConnectorRow.ExternalId, storageArea.ExternalId, storageAreaConnectorRow.WarehouseExternalId);
        }
    }

#if DEBUG
    public override string ToString()
    {
        string inUse = FlowRangeConstraint.Count > 0 ? "true" : "false";
        string allocatedUsage = "Not Allocated";
        StringBuilder storageAreasIn = new ();
        StringBuilder storageAreasOut = new();

        string storageAreasInString = "None";
        string storageAreasOutString = "None";


        foreach (IInterval interval in FlowRangeConstraint)
        {
            allocatedUsage += interval + " | ";
        }

        foreach (StorageArea sa in m_storageAreasIn)
        {
            storageAreasIn.Append(sa.Name);
            storageAreasIn.Append(", ");
        }
        
        if (storageAreasIn.Length > 2)
        {
            //Remove the last comma and space
            storageAreasIn.Length -= 2;
            storageAreasInString = storageAreasIn.ToString();
        }

        foreach (StorageArea sa in m_storageAreasOut)
        {
            storageAreasOut.Append(sa.Name);
            storageAreasOut.Append(", ");
        }

        if (storageAreasOut.Length > 2)
        {
            //Remove the last comma and space
            storageAreasOut.Length -= 2;
            storageAreasOutString = storageAreasOut.ToString();
        }

        string debugString = $"Storage Area Connector:{Name} | Allocated Usage: {allocatedUsage} | In Use: {inUse} | Storage Areas In: [{storageAreasInString}] | Storage Aras Out: [{storageAreasOutString}] ";
        return debugString;

    }

#endif
}

