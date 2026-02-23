using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler.Schedule.InventoryManagement
{
    public class StorageAreaCollection : BaseIdCustomSortedList<StorageArea>
    {
        private readonly BaseIdGenerator m_idGen;

        internal StorageAreaCollection(BaseIdGenerator a_idGen) : base(new StorageAreaComparer())
        {
            m_idGen = a_idGen;
        }

        internal StorageAreaCollection(IReader a_reader, BaseIdGenerator a_idGenerator) : base(a_reader, new StorageAreaComparer(), a_idGenerator)
        {
            m_idGen = a_idGenerator;
        }

        public StorageArea GetByExternalId(string a_externalId)
        {
            foreach (StorageArea storageArea in this)
            {
                if (storageArea.ExternalId == a_externalId)
                {
                    return storageArea;
                }
            }

            return null;
        }

        protected override StorageArea CreateInstance(IReader a_reader)
        {
            throw new NotImplementedException();
        }

        protected override StorageArea CreateInstance(IReader a_reader, BaseIdGenerator a_idGen)
        {
            return new StorageArea(a_reader, a_idGen);
        }

        protected override StorageArea CreateKeyValue(object a_key)
        {
            throw new NotImplementedException();
        }

        internal void ValidateDelete(JobManager a_jobs, PurchaseToStockManager a_poManager, TransferOrderManager a_transferOrderManager, StorageAreasDeleteProfile a_deleteProfile)
        {
            foreach (StorageArea storageArea in this)
            {
                a_deleteProfile.Add(storageArea);
            }

            foreach (StorageArea storageArea in this)
            {
                storageArea.ValidateDelete(a_jobs, a_poManager, a_transferOrderManager, a_deleteProfile, new ItemStorageDeleteProfile());
                if (a_deleteProfile.HasError(storageArea.Id))
                {
                    throw a_deleteProfile.GetException(storageArea.Id);
                }
            }
        }

        internal void ValidateItemDelete(ItemDeleteProfile a_items)
        {
            foreach (StorageArea storageArea in this)
            {
                storageArea.ValidateItemDelete(a_items);
            }
        }

        internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
        {
            foreach (StorageArea storageArea in this)
            {
                storageArea.ValidateInventoryDelete(a_deleteProfile);
            }
        }

        internal class StorageAreaComparer : IBaseIdKeyObjectComparer<StorageArea>
        {
            public int Compare(StorageArea x, StorageArea y)
            {
                return CompareInventories(x, y);
            }

            internal static int CompareInventories(StorageArea a_inv, StorageArea a_anotherInv)
            {
                return a_inv.Id.CompareTo(a_anotherInv.Id);
            }

            public BaseId GetKey(StorageArea a_itemInv)
            {
                return a_itemInv.Id;
            }
        }

        private List<StorageArea> m_simCollectionOfNonConnectedAreasIn;
        private List<StorageArea> m_simCollectionOfNonConnectedAreasOut;

        internal void SimulationInitialization(StorageAreaConnectorCollection a_storageAreaConnectors)
        {
            m_simCollectionOfNonConnectedAreasIn = new ();
            m_simCollectionOfNonConnectedAreasOut = new ();

            foreach (StorageArea storageArea in this)
            {
                if (!a_storageAreaConnectors.AnyConnectorInForStorageArea(storageArea))
                {
                    m_simCollectionOfNonConnectedAreasIn.Add(storageArea);
                }

                if (!a_storageAreaConnectors.AnyConnectorOutForStorageArea(storageArea))
                {
                    m_simCollectionOfNonConnectedAreasOut.Add(storageArea);
                }

                storageArea.SimulationStageInitialization();
            }
        }

        internal void SimulationComplete()
        {
            foreach (StorageArea storageArea in this)
            {
                storageArea.SimulationComplete();
            }
        }

        internal StorageArea AddStorageArea(WarehouseT.StorageArea a_storageAreaT, Warehouse a_warehouse, ItemManager a_itemManager, UserFieldDefinitionManager a_udfManager, ApplicationExceptionList a_errList, IScenarioDataChanges a_dataChanges)
        {
            StorageArea area = new StorageArea(m_idGen, a_storageAreaT, a_warehouse, a_udfManager);

            if (a_storageAreaT.ItemStorageCount == 0)
            {
                throw new PTValidationException("3093", new object[] { a_storageAreaT.ExternalId});
            }

            for (int i = 0; i < a_storageAreaT.ItemStorageCount; i++)
            {
                try
                {
                    WarehouseT.ItemStorage itemStorageT = a_storageAreaT.GetItemStorageByIndex(i);

                    Item item = a_itemManager.GetByExternalId(itemStorageT.ItemExternalId);

                    if (item == null)
                    {
                        throw new PTValidationException("2194", new object[] { itemStorageT.ItemExternalId, a_storageAreaT.WarehouseExternalId });
                    }

                    Inventory inventory = a_warehouse.Inventories[item.Id];
                    if (inventory == null)
                    {
                        throw new PTValidationException("4500", new object[] { itemStorageT.ItemExternalId, a_storageAreaT.ExternalId });
                    }

                    ItemStorage itemStorage = new ItemStorage(m_idGen.NextID(), item, area);
                    itemStorage.Update(itemStorageT, inventory.Lots, a_udfManager);
                    area.Storage.Add(itemStorage);
                }
                catch (PTValidationException e)
                {
                    a_errList.Add(e);
                }
            }

            if (area.Storage.Count > 0)
            {
                Add(area);
                a_dataChanges.StorageAreaChanges.AddedObject(area.Id);
                a_dataChanges.AuditEntry(new AuditEntry(area.Id, a_warehouse.Id, area), true);

                foreach (ItemStorage itemStorage in area.Storage)
                {
                    a_dataChanges.AuditEntry(new AuditEntry(itemStorage.Id, area.Id, itemStorage), true);
                }
            }
            else
            {
                a_errList.Add(new PTValidationException("3093", new object[] { a_storageAreaT.ExternalId }));
            }
                
            return area;
        }

        internal void UpdateStorageAreas(WarehouseT.Warehouse a_warehouseT,
                                         ScenarioDetail a_sd,
                                         Warehouse a_warehouse,
                                         PTTransmission a_ptTransmission,
                                         bool a_autoDeleteStorageArea, 
                                         bool a_autoDeleteItemStorage, 
                                         ApplicationExceptionList a_errList, 
                                         IScenarioDataChanges a_dataChanges, 
                                         UserFieldDefinitionManager a_udfManager,
                                         List<PostProcessingAction> a_storageAreaAddStorageResourceReferences,
                                         List<PostProcessingAction> a_storageAreaAutoDeleteActions,
                                         List<PostProcessingAction> a_itemStorageAutoDeleteActions,
                                         ScenarioExceptionInfo a_sei)
        {
            HashSet<BaseId> affectedStorageAreas = new();

            for (int i = 0; i < a_warehouseT.StorageAreaCount; i++)
            {
                try
                {
                    WarehouseT.StorageArea storageAreaT = a_warehouseT.GetStorageAreaByIndex(i);
                    
                    StorageArea storageArea = GetByExternalId(storageAreaT.ExternalId);
                    
                    if (storageArea != null)
                    {
                        storageArea.Update(a_sd, storageAreaT, a_sd.ItemManager, a_udfManager, a_ptTransmission, a_autoDeleteItemStorage, a_dataChanges, a_errList, a_itemStorageAutoDeleteActions);
                    }
                    else
                    {
                        storageArea = AddStorageArea(storageAreaT, a_warehouse, a_sd.ItemManager, a_udfManager, a_errList, a_dataChanges);
                    }

                    affectedStorageAreas.Add(storageArea.Id);
                }
                catch (PTValidationException err)
                {
                    a_errList.Add(err);
                }
            }

            //Add Storage Resource References in Post Processing to set all the Resource references
            a_storageAreaAddStorageResourceReferences.Add(new PostProcessingAction(a_ptTransmission, true, () => AddStorageResourceReference(a_sd, a_warehouseT, a_dataChanges, a_errList)));

            if (a_autoDeleteStorageArea)
            {
                StorageAreasDeleteProfile storageAreasDeleteProfile = new StorageAreasDeleteProfile(false, false, false);
                //Remove any Inventories not updated
                foreach (StorageArea storageArea in this)
                {
                    if (!affectedStorageAreas.Contains(storageArea.Id))
                    {
                        storageAreasDeleteProfile.Add(storageArea);
                    }
                }

                //Now perform the deletes
                if (!storageAreasDeleteProfile.Empty)
                {
                    a_storageAreaAutoDeleteActions.Add(new PostProcessingAction(a_ptTransmission, false, () =>
                    {
                        ItemStorageDeleteProfile deleteProfile = new();
                        foreach (StorageArea storageArea in storageAreasDeleteProfile)
                        {
                            Deleting(storageArea, a_sd, storageAreasDeleteProfile, deleteProfile, a_dataChanges);
                        }

                        ApplicationExceptionList exceptionList = new ApplicationExceptionList();
                        foreach (PTValidationException ptValidationException in storageAreasDeleteProfile.ValidationExceptions)
                        {
                            exceptionList.Add(ptValidationException);
                        }

                        foreach (PTValidationException validationException in deleteProfile.ValidationExceptions)
                        {
                            exceptionList.Add(validationException);
                        }

                        if (exceptionList.Count > 0)
                        {
                            m_errorReporter.LogException(exceptionList, a_ptTransmission, a_sei, ELogClassification.PtInterface, false);
                        }
                    }));
                }
            }
        }

        internal void AddStorageResourceReference(ScenarioDetail a_sd, WarehouseT.Warehouse a_warehouseT, IScenarioDataChanges a_dataChanges, ApplicationExceptionList a_errList)
        {
            for (int i = 0; i < a_warehouseT.StorageAreaCount; i++)
            {
                try
                {
                    WarehouseT.StorageArea storageAreaT = a_warehouseT.GetStorageAreaByIndex(i);
                    StorageArea storageArea = GetByExternalId(storageAreaT.ExternalId);

                    if (storageArea == null)
                    {
                        //this storage area may have failed validation and not been added
                        continue;
                    }

                    if (String.IsNullOrEmpty(storageAreaT.PlantExternalId))
                    {
                        //these are handled elsewhere to clear the reference
                        continue;
                    }

                    //the transmission validate that if plant external id is set, the other external ids are also set
                    Scheduler.Resource storageResource = a_sd.PlantManager.GetResource(storageAreaT.PlantExternalId, storageAreaT.DepartmentExternalId, storageAreaT.ResourceExternalId);
                    if (storageResource == null)
                    {
                        throw new PTValidationException("3131", [storageArea.ExternalId, storageAreaT.PlantExternalId, storageAreaT.DepartmentExternalId, storageAreaT.ResourceExternalId]);
                    }

                    //Validate single tasking and no capabilities on new res reference
                    if (storageResource.CapacityType != InternalResourceDefs.capacityTypes.SingleTasking)
                    {
                        throw new PTValidationException("3132", [storageArea.ExternalId, storageAreaT.PlantExternalId, storageAreaT.DepartmentExternalId, storageAreaT.ResourceExternalId]);
                    }

                    if (storageResource.CapabilityCount > 0)
                    {
                        throw new PTValidationException("3133", [storageArea.ExternalId, storageAreaT.PlantExternalId, storageAreaT.DepartmentExternalId, storageAreaT.ResourceExternalId]);
                    }

                    if (storageResource.StorageAreaResource && storageArea.Resource != storageResource)
                    {
                        //Already assigned as storage resource elsewhere, BAD
                        throw new PTValidationException("3136", [storageArea.ExternalId, storageAreaT.PlantExternalId, storageAreaT.DepartmentExternalId, storageAreaT.ResourceExternalId]);
                    }
                    
                    storageArea.SetStorageResourceReference(storageResource);
                    a_dataChanges.FlagConstraintChanges(storageResource.Id);
                }
                catch (PTValidationException err)
                {
                    a_errList.Add(err);
                }
            }
        }

        internal IEnumerable<StorageArea> GetNonConnectedStorageAreasIn(Item a_productItem)
        {
            foreach (StorageArea storageArea in m_simCollectionOfNonConnectedAreasIn)
            {
                if (storageArea.Storage.ContainsKey(storageArea.Id))
                {
                    yield return storageArea;
                }
            }
        }

        internal IEnumerable<StorageArea> GetNonConnectedStorageAreasOut(Item a_productItem)
        {
            foreach (StorageArea storageArea in m_simCollectionOfNonConnectedAreasOut)
            {
                if (storageArea.Storage.ContainsKey(storageArea.Id))
                {
                    yield return storageArea;
                }
            }
        }

        /// <summary>
        /// Deleting storage area so remove references to all objects.
        /// </summary>
        internal void Deleting(StorageArea a_storageArea, ScenarioDetail a_sd, StorageAreasDeleteProfile a_storageAreaDeleteProfile, ItemStorageDeleteProfile a_itemStorageDeleteProfile, IScenarioDataChanges a_dataChanges)
        {
            a_storageArea.ValidateDelete(a_sd.JobManager, a_sd.PurchaseToStockManager, a_sd.TransferOrderManager, a_storageAreaDeleteProfile, a_itemStorageDeleteProfile);
            if (a_storageAreaDeleteProfile.HasError(a_storageArea.Id))
            {
                return;
            }

            if (a_dataChanges != null)
            {
                a_dataChanges.AuditEntry(new AuditEntry(a_storageArea.Id, a_storageArea.Warehouse.Id, a_storageArea), false, true);
                a_dataChanges.StorageAreaChanges.DeletedObject(a_storageArea.Id);
            }

            if (a_storageArea.Resource != null)
            {
                a_storageArea.Resource.DeletingStorageArea();
            }

            //Remove all reference of inventory PurchaseOrderSupplyStorageArea pointing to this SA
            foreach (Inventory inventory in a_storageArea.Warehouse.Inventories)
            {
                if (inventory.PurchaseOrderSupplyStorageArea?.Id == a_storageArea.Id)
                {
                    inventory.PurchaseOrderSupplyStorageArea = null;
                }
            }

            a_storageArea.Warehouse.StorageAreaConnectors.Delete(a_storageArea);
            RemoveByKey(a_storageArea.Id);
        }

        public void Receive(ScenarioDetailClearT a_clearT, ScenarioDetail a_sd, Warehouse a_warehouse, IScenarioDataChanges a_dataChanges)
        {
            foreach (StorageArea storageArea in this)
            {
                storageArea.Receive(a_clearT, a_sd.JobManager, a_sd.PurchaseToStockManager);
            }

            if (a_clearT.ClearStorageAreas)
            {
                StorageAreasDeleteProfile deleteProfile = new StorageAreasDeleteProfile(a_clearT.ClearJobs, a_clearT.ClearPurchaseToStocks, a_clearT.ClearItemStorages);
                foreach (StorageArea storageArea in this)
                {
                    deleteProfile.Add(storageArea);
                }

                foreach (StorageArea storageArea in deleteProfile)
                {
                    ItemStorageDeleteProfile itemStorageDeleteProfile = new ItemStorageDeleteProfile(a_clearT.ClearJobs, a_clearT.ClearPurchaseToStocks);
                    Deleting(storageArea, a_sd, deleteProfile, itemStorageDeleteProfile, a_dataChanges);
                }
            }
        }

        private ISystemLogger m_errorReporter;
        internal void RestoreReferences(PlantManager a_plantManager, CustomerManager a_cm, ItemManager a_itemManager, Warehouse a_warehouse, ISystemLogger a_errorReporter)
        {
            m_errorReporter = a_errorReporter;
            
            foreach (StorageArea storageArea in this)
            {
                storageArea.RestoreReferences(a_plantManager, a_cm, a_itemManager, a_warehouse, a_errorReporter);
            }
        }

        internal void ResetAllocation()
        {
            foreach (StorageArea storageArea in this)
            {
                storageArea.ResetAllocation();
            }
        }
    }
}
