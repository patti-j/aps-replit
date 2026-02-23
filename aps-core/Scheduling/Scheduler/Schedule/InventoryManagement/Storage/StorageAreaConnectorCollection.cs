using PT.APSCommon.Collections;
using PT.APSCommon;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

public partial class StorageAreaConnectorCollection : BaseIdCustomSortedList<StorageAreaConnector>
{
    private readonly BaseIdGenerator m_idGen;
    internal StorageAreaConnectorCollection(BaseIdGenerator a_idGen) : base(new StorageAreaConnectorComparer())
    {
        m_idGen = a_idGen;
    }

    internal StorageAreaConnectorCollection(IReader a_reader, BaseIdGenerator a_idGenerator) : base(a_reader, new StorageAreaConnectorComparer(), a_idGenerator)
    {
        m_idGen = a_idGenerator;
    }

    public StorageAreaConnector GetByExternalId(string a_externalId)
    {
        foreach (StorageAreaConnector storageAreaConnector in this)
        {
            if (storageAreaConnector.ExternalId == a_externalId)
            {
                return storageAreaConnector;
            }
        }

        return null;
    }
    protected override StorageAreaConnector CreateInstance(IReader a_reader)
    {
        return new StorageAreaConnector(a_reader);
    }

    protected override StorageAreaConnector CreateKeyValue(object a_key)
    {
        throw new NotImplementedException();
    }

    internal void Delete(StorageArea a_storageArea)
    {
        foreach (StorageAreaConnector storageAreaConnector in this)
        {
            storageAreaConnector.Deleting(a_storageArea);
        }
    } 
    internal void Delete(Resource a_resource)
    {
        foreach (StorageAreaConnector storageAreaConnector in this)
        {
            storageAreaConnector.Deleting(a_resource);
        }
    }

    internal class StorageAreaConnectorComparer : IBaseIdKeyObjectComparer<StorageAreaConnector>
    {
        public int Compare(StorageAreaConnector x, StorageAreaConnector y)
        {
            return CompareInventories(x, y);
        }

        internal static int CompareInventories(StorageAreaConnector a_inv, StorageAreaConnector a_anotherInv)
        {
            return a_inv.Id.CompareTo(a_anotherInv.Id);
        }

        public BaseId GetKey(StorageAreaConnector a_itemInv)
        {
            return a_itemInv.Id;
        }
    }

    internal void RestoreReferences(Warehouse a_warehouse)
    {
        foreach (StorageAreaConnector connector in this)
        {
            connector.RestoreReferences(a_warehouse);
        }
    }

    internal void UpdateConnectors(WarehouseT.Warehouse a_warehouseT, ScenarioDetail a_sd, bool a_connectorAutoDelete, bool a_autoDeleteStorageAreaConnectorsIn, bool a_autoDeleteStorageAreaConnectorsOut, bool a_autoDeleteResourceStorageAreaConnectorsIn, bool a_autoDeleteResourceStorageAreaConnectorsOut, Warehouse a_warehouse1, IScenarioDataChanges a_dataChanges, UserFieldDefinitionManager a_udfManager, ApplicationExceptionList a_errList)
    {
        HashSet<BaseId> affectedConnectors = new ();
        List<StorageAreaConnector> connectorsToDelete = new ();
        for (int i = 0; i < a_warehouseT.StorageAreaConnectorCount; i++)
        {
            try
            {
                WarehouseT.StorageAreaConnector storageAreaConnectorT = a_warehouseT.GetStorageAreaConnectorByIndex(i);

                StorageAreaConnector storageAreaConnector = GetByExternalId(storageAreaConnectorT.ExternalId);

                if (storageAreaConnector != null)
                {
                    AuditEntry connectorAuditEntry = new AuditEntry(storageAreaConnector.Id, a_warehouse1.Id, storageAreaConnector);
                    StorageAreaConnector tempStorageAreaConnector = new StorageAreaConnector(storageAreaConnector.Id, storageAreaConnectorT);
                    storageAreaConnector.Update(a_sd, a_udfManager, storageAreaConnectorT, tempStorageAreaConnector, a_warehouse1, a_autoDeleteStorageAreaConnectorsIn, a_autoDeleteStorageAreaConnectorsOut, a_autoDeleteResourceStorageAreaConnectorsIn, a_autoDeleteResourceStorageAreaConnectorsOut);
                    a_dataChanges.AuditEntry(connectorAuditEntry);
                    a_dataChanges.StorageAreaConnectorChanges.UpdatedObject(storageAreaConnector.Id);
                }
                else
                {
                    storageAreaConnector = new StorageAreaConnector(m_idGen.NextID(), storageAreaConnectorT);
                    storageAreaConnector.Update(a_sd, a_udfManager, storageAreaConnectorT, storageAreaConnector, a_warehouse1, a_autoDeleteStorageAreaConnectorsIn, a_autoDeleteStorageAreaConnectorsOut, a_autoDeleteResourceStorageAreaConnectorsIn, a_autoDeleteResourceStorageAreaConnectorsOut);
                    Add(storageAreaConnector);
                    a_dataChanges.StorageAreaConnectorChanges.AddedObject(storageAreaConnector.Id);
                    a_dataChanges.AuditEntry(new AuditEntry(storageAreaConnector.Id, a_warehouse1.Id, storageAreaConnector), true);
                }

                affectedConnectors.Add(storageAreaConnector.Id);

                //Connectors can change eligible resources due to storage constraints. Ideally we would track that better, but for now just flag
                a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
            }
            catch (PTValidationException ex)
            {
                a_errList.Add(ex);
            }
        }
        
        if (a_connectorAutoDelete)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                StorageAreaConnector storageAreaConnector = GetByIndex(i);
                if (!affectedConnectors.Contains(storageAreaConnector.Id))
                {
                    connectorsToDelete.Add(storageAreaConnector);
                }
            }

            foreach (StorageAreaConnector connector in connectorsToDelete)
            {
                RemoveByKey(connector.Id);
                a_dataChanges.StorageAreaConnectorChanges.DeletedObject(connector.Id);
                a_dataChanges.AuditEntry(new AuditEntry(connector.Id, a_warehouse1.Id, connector), false, true);
            }
        }
    }

    public IEnumerable<StorageAreaConnector> GetStorageConnectorsForResourceIn(InternalResource a_primaryResource)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.ResourceInList.Contains(a_primaryResource.Id))
            {
                yield return connector;
            }
        }
    }

    public IEnumerable<StorageAreaConnector> GetStorageConnectorsForResourceOut(InternalResource a_primaryResource)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.ResourceOutList.Contains(a_primaryResource.Id))
            {
                yield return connector;
            }
        }
    } 
    public IEnumerable<StorageAreaConnector> GetStorageConnectorsForStorageAreaIn(BaseId a_storageAreaId)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.StorageAreaInList.Contains(a_storageAreaId))
            {
                yield return connector;
            }
        }
    }
    public IEnumerable<StorageAreaConnector> GetStorageConnectorsForStorageAreaOut(BaseId a_storageAreaId)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.StorageAreaOutList.Contains(a_storageAreaId))
            {
                yield return connector;
            }
        }
    }

    internal bool AnyConnectorInForStorageArea(StorageArea a_storageArea)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.StorageAreaInList.Contains(a_storageArea.Id))
            {
                return true;
            }
        }

        return false;
    }

    internal bool AnyConnectorOutForStorageArea(StorageArea a_storageArea)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.StorageAreaOutList.Contains(a_storageArea.Id))
            {
                return true;
            }
        }

        return false;
    }

    internal bool AnyConnectorInForResource(InternalResource a_resource)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.ResourceInList.Contains(a_resource.Id))
            {
                return true;
            }
        }

        return false;
    }

    internal bool AnyConnectorOutForResource(InternalResource a_resource)
    {
        foreach (StorageAreaConnector connector in this)
        {
            if (connector.ResourceOutList.Contains(a_resource.Id))
            {
                return true;
            }
        }

        return false;
    }
}
