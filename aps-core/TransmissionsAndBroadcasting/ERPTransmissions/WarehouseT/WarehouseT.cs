using System.Data;

namespace PT.ERPTransmissions;

public partial class WarehouseT : ERPMaintenanceTransmission<WarehouseT.Warehouse>, IPTSerializable
{
    public new const int UNIQUE_ID = 525;

    #region PT Serialization
    public WarehouseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Warehouse node = new(a_reader);
                Add(node);
            }

            //Load items
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Item node = new(a_reader);
                ItemsList.Add(node);
                ItemsExternalIdSet.Add(node.ExternalId);
            }
        }
        else if (a_reader.VersionNumber >= 437)
        {
            a_reader.Read(out bool autoDeleteInventories);
            AutoDeleteInventories = autoDeleteInventories;
            a_reader.Read(out bool autoDeleteItems);
            AutoDeleteItems = autoDeleteItems;
            a_reader.Read(out bool autoDeleteLots);
            AutoDeleteLots = autoDeleteLots;

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Warehouse node = new(a_reader);
                Add(node);
            }

            //Load items
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Item node = new(a_reader);
                ItemsList.Add(node);
                ItemsExternalIdSet.Add(node.ExternalId);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);

        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }

        a_writer.Write(ItemsList.Count);
        for (int i = 0; i < ItemsList.Count; i++)
        {
            ItemsList[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;
    private const short c_autoDeleteStorageAreasIdx = 0;
    private const short c_autoDeleteInventoriesIdx = 1;
    private const short c_autoDeleteItemsIdx = 2;
    private const short c_autoDeleteLotsIdx = 3;
    private const short c_autoDeleteStorageAreaConnectorsIdx = 4;
    private const short c_autoDeleteStorageAreaConnectorsInIdx = 5;
    private const short c_autoDeleteStorageAreaConnectorsOutIdx = 6;
    private const short c_autoDeleteItemStorageIdx = 7;
    private const short c_autoDeleteItemStorageLotsIdx = 8;
    private const short c_autoDeleteResourceStorageAreaConnectorInIdx = 9;
    private const short c_autoDeleteResourceStorageAreaConnectorOutIdx = 10;

    public bool AutoDeleteItems
    {
        get => m_bools[c_autoDeleteItemsIdx];
        set => m_bools[c_autoDeleteItemsIdx] = value;
    }    
    
    public bool AutoDeleteInventories
    {
        get => m_bools[c_autoDeleteInventoriesIdx];
        set => m_bools[c_autoDeleteInventoriesIdx] = value;
    }    
    
    public bool AutoDeleteLots
    {
        get => m_bools[c_autoDeleteLotsIdx];
        set => m_bools[c_autoDeleteLotsIdx] = value;
    }    
    
    public bool AutoDeleteStorageAreas
    {
        get => m_bools[c_autoDeleteStorageAreasIdx];
        set => m_bools[c_autoDeleteStorageAreasIdx] = value;
    }
    
    public bool AutoDeleteStorageAreaConnectors
    {
        get => m_bools[c_autoDeleteStorageAreaConnectorsIdx];
        set => m_bools[c_autoDeleteStorageAreaConnectorsIdx] = value;
    }

    public bool AutoDeleteStorageAreaConnectorsIn
    {
        get => m_bools[c_autoDeleteStorageAreaConnectorsInIdx];
        set => m_bools[c_autoDeleteStorageAreaConnectorsInIdx] = value;
    }

    public bool AutoDeleteStorageAreaConnectorsOut
    {
        get => m_bools[c_autoDeleteStorageAreaConnectorsOutIdx];
        set => m_bools[c_autoDeleteStorageAreaConnectorsOutIdx] = value;
    }

    public bool AutoDeleteItemStorage
    {
        get => m_bools[c_autoDeleteItemStorageIdx];
        set => m_bools[c_autoDeleteItemStorageIdx] = value;
    }

    public bool AutoDeleteItemStorageLots
    {
        get => m_bools[c_autoDeleteItemStorageLotsIdx];
        set => m_bools[c_autoDeleteItemStorageLotsIdx] = value;
    }

    public bool AutoDeleteResourceStorageAreaConnectorIn
    {
        get => m_bools[c_autoDeleteResourceStorageAreaConnectorInIdx];
        set => m_bools[c_autoDeleteResourceStorageAreaConnectorInIdx] = value;
    }

    public bool AutoDeleteResourceStorageAreaConnectorOut
    {
        get => m_bools[c_autoDeleteResourceStorageAreaConnectorOutIdx];
        set => m_bools[c_autoDeleteResourceStorageAreaConnectorOutIdx] = value;
    }


    public List<Item> ItemsList = new();

    //This hashset is used to store item external ids for future validation so that the ItemsList does not need to be searched. It is not serialized
    public HashSet<string> ItemsExternalIdSet = new();

    public WarehouseT() { }

    public override string Description => "Warehouse inventory data updated";

    public WarehouseT(PtImportDataSet ds)
    {
        for (int i = 0; i < ds.Warehouses.Count; i++)
        {
            Warehouse warehouse = new(ds.Warehouses[i], ds.Lots);
            Add(warehouse);
        }

        for (int i = 0; i < ds.Items.Count; i++)
        {
            Item newItem = new(ds.Items[i]);
            newItem.Validate(ItemsExternalIdSet);
            ItemsList.Add(newItem);
        }
    }

    #region Database Loading
    /// <summary>
    /// Builds the transmission with data from a dataset that is retrieved using the provided database commands.
    /// Since inventories and items are also stored in the warehouse transmission, all commands are optional.
    /// For example, providing only the itemsCommand will retrieve and send only the items.
    /// </summary>
    public void Fill(IDbCommand warehouseCommand, IDbCommand plantWarehouseCommand, IDbCommand inventoryCommand, IDbCommand lotsCommand, IDbCommand itemsCommand, IDbCommand a_storageAreaCommand, IDbCommand a_itemStorageCommand, IDbCommand a_itemStorageLotCommand, IDbCommand a_storageAreaConnectorCommand, IDbCommand a_storageAreaConnectorsInCommand, IDbCommand a_storageAreaConnectorsOutCommand, IDbCommand a_resourceStorageAreaConnectorsInCommand, IDbCommand a_resourceStorageAreaConnectorsOutCommand)
    {
        PtImportDataSet ds = new();

        if (warehouseCommand != null && plantWarehouseCommand != null)
        {
            FillTable(ds.Warehouses, warehouseCommand);
            FillTable(ds.SuppliedPlants, plantWarehouseCommand);
        }

        if (a_storageAreaCommand != null)
        {
            FillTable(ds.StorageAreas, a_storageAreaCommand);
        }

        if (a_storageAreaConnectorCommand != null)
        {
            FillTable(ds.StorageAreaConnector, a_storageAreaConnectorCommand);

            if (a_storageAreaConnectorsInCommand != null)
            {
                FillTable(ds.StorageAreaConnectorIn, a_storageAreaConnectorsInCommand);
            }
            
            if (a_storageAreaConnectorsOutCommand != null)
            {
                FillTable(ds.StorageAreaConnectorOut, a_storageAreaConnectorsOutCommand);
            }
            
            if (a_resourceStorageAreaConnectorsInCommand != null)
            {
                FillTable(ds.ResourceStorageAreaConnectorIn, a_resourceStorageAreaConnectorsInCommand);
            }
            
            if (a_resourceStorageAreaConnectorsOutCommand != null)
            {
                FillTable(ds.ResourceStorageAreaConnectorOut, a_resourceStorageAreaConnectorsOutCommand);
            }
        }
       
        if (a_itemStorageCommand != null)
        {
            FillTable(ds.ItemStorage, a_itemStorageCommand);
        }
        
        if (a_itemStorageLotCommand != null)
        {
            FillTable(ds.ItemStorageLots, a_itemStorageLotCommand);
        }

        if (inventoryCommand != null)
        {
            FillTable(ds.Inventories, inventoryCommand);
            if (lotsCommand != null)
            {
                FillTable(ds.Lots, lotsCommand);
            }
        }

        if (itemsCommand != null)
        {
            FillTable(ds.Items, itemsCommand);
        }

        Fill(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    public void Fill(PtImportDataSet ds)
    {
        for (int i = 0; i < ds.Warehouses.Count; i++)
        {
            Warehouse warehouse = new Warehouse(ds.Warehouses[i], ds.Lots);
            warehouse.Validate();
            Add(warehouse);
        }

        for (int i = 0; i < ds.Items.Count; i++)
        {
            Item newItem = new(ds.Items[i]);
            newItem.Validate(ItemsExternalIdSet);
            ItemsList.Add(newItem);
        }
    }
    #endregion
}