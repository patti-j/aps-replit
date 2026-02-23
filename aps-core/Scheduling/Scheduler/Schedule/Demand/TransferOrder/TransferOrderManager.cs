using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Demand;

public partial class TransferOrderManager : ScenarioBaseObjectManager<TransferOrder>, IPTSerializable
{
    #region IPTSerializable Members
    public TransferOrderManager(IReader reader, BaseIdGenerator aIdGen)
        : base(aIdGen)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                Add(new TransferOrder(reader, aIdGen), false);
            }
        }
    }

    internal void RestoreReferences(WarehouseManager warehouses, ItemManager items)
    {
        for (int i = 0; i < Count; ++i)
        {
            TransferOrder to = GetByIndex(i);
            to.RestoreReferences(warehouses, items);
        }
    }    
    
    internal void AfterRestoreAdjustmentReferences()
    {
        for (int i = 0; i < Count; ++i)
        {
            TransferOrder to = GetByIndex(i);
            to.AfterRestoreAdjustmentReferences();
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (TransferOrder transferOrder in this)
        {
            a_udfManager.RestoreReferences(transferOrder, UserField.EUDFObjectType.TransferOrders);
        }
    }

    public new const int UNIQUE_ID = 646;

    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    #region Construction
    public TransferOrderManager(BaseIdGenerator aIdGen)
        : base(aIdGen) { }
    #endregion

    #region Transmissions
    public void Receive(UserFieldDefinitionManager a_udfManager, TransferOrderT aT, ItemManager aItems, WarehouseManager aWarehouses, ScenarioDetail a_sd, Transmissions.PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        Transmissions.ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        HashSet<string> affectedTOs = new ();
        Dictionary<BaseId, Item> affectedItems = new ();

        bool fromERP = a_t.FromErp;

        try
        {
            if (!PTSystem.LicenseKey.IncludeCrossWarehousePlanning)
            {
                throw new AuthorizationException("Add TransferOrder", AuthorizationType.LicenseKey, "IncludeCrossWarehousePlanning", PTSystem.LicenseKey.IncludeCrossWarehousePlanning.ToString());
            }

            //TODO: Return to Parallel.Invoke
            //Temp solution until V12 datachages is in place to reduce number of threads
            Thread t1 = new (() => { InitFastLookupByExternalId(); });
            t1.Start();

            Thread t2 = new (() => { aItems.InitFastLookupByExternalId(); });
            t2.Start();

            Thread t3 = new (() => { aWarehouses.InitFastLookupByExternalId(); });
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            for (int i = 0; i < aT.TransferOrders.Count; i++)
            {
                try
                {
                    TransferOrderT.TransferOrder tTO = aT.TransferOrders[i];

                    if (!affectedTOs.Contains(tTO.ExternalId))
                    {
                        affectedTOs.Add(tTO.ExternalId);
                    }

                    TransferOrder to = GetByExternalId(tTO.ExternalId);

                    if (to != null)
                    {
                        to.Update(a_udfManager, tTO, aItems, aWarehouses, a_sd, a_t);
                        to.MaintenanceMethod = fromERP ? JobDefs.EMaintenanceMethod.ERP : JobDefs.EMaintenanceMethod.Manual;
                        a_dataChanges.TransferOrderChanges.UpdatedObject(to.Id);
                    }
                    else
                    {
                        TransferOrder newTO = new (NextID(), tTO, aItems, aWarehouses, IdGen, a_sd, a_t, a_udfManager);
                        newTO.MaintenanceMethod = fromERP ? JobDefs.EMaintenanceMethod.ERP : JobDefs.EMaintenanceMethod.Manual;
                        Add(newTO);
                        a_dataChanges.TransferOrderChanges.AddedObject(tTO.Id);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (aT.AutoDeleteMode)
            {
                actions.Add(new PostProcessingAction(a_t, false, () =>
                    {
                        Transmissions.ApplicationExceptionList delErrs = new ();

                        for (int i = Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                TransferOrder to = GetByIndex(i);

                                if (!affectedTOs.Contains(to.ExternalId))
                                {
                                    Delete(to, a_sd, a_t, a_dataChanges);
                                }
                            }
                            catch (PTValidationException err)
                            {
                                delErrs.Add(err);
                            }

                            if (delErrs.Count > 0)
                            {
                                ScenarioExceptionInfo sei = new ();
                                sei.Create(a_sd);
                                m_errorReporter.LogException(delErrs, a_t, sei, ELogClassification.PtInterface, false);
                            }
                        }
                    }));
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            actions.Add(new PostProcessingAction(a_t, true, () =>
                {
                    DeInitFastLookupByExternalId();
                    aItems.DeInitFastLookupByExternalId();
                    aWarehouses.DeInitFastLookupByExternalId();
                }));
            a_sd.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void AddAffectedItems(ref Dictionary<BaseId, Item> affectedItems, TransferOrder aTO)
    {
        for (int i = 0; i < aTO.Distributions.Count; i++)
        {
            TransferOrderDistribution dist = aTO.Distributions.GetByIndex(i);
            if (!affectedItems.ContainsKey(dist.Item.Id))
            {
                affectedItems.Add(dist.Item.Id, dist.Item);
            }
        }
    }

    private TransferOrder Add(TransferOrder a_transferOrder, bool a_performLicenseCheck = true)
    {
        if (a_performLicenseCheck && !PTSystem.LicenseKey.IncludeCrossWarehousePlanning)
        {
            throw new AuthorizationException("Add TransferOrder", AuthorizationType.LicenseKey, "IncludeCrossWarehousePlanning", PTSystem.LicenseKey.IncludeCrossWarehousePlanning.ToString());
        }

        return base.Add(a_transferOrder);
    }
    #endregion Transmissions

    internal void Clear(ScenarioDetail a_sd, Transmissions.PTTransmissionBase a_t, IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            TransferOrder transferOrder = GetByIndex(i);
            Delete(transferOrder, a_sd, a_t, a_dataChanges);
        }
    }

    internal void ValidateWarehouseDelete(Warehouse a_warehouse)
    {
        //Make sure no TransferOrders to or from the warehouse
        for (int toI = 0; toI < Count; toI++)
        {
            TransferOrder transferOrder = GetByIndex(toI);
            for (int distI = 0; distI < transferOrder.Distributions.Count; distI++)
            {
                TransferOrderDistribution dist = transferOrder.Distributions.GetByIndex(distI);
                if (dist.FromWarehouse.Id == a_warehouse.Id || dist.ToWarehouse.Id == a_warehouse.Id)
                {
                    throw new PTValidationException("2835", new object[] { a_warehouse.Name, transferOrder.Name });
                }
            }
        }
    }

    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_saDeleteProfile)
    {
        //Make sure no TransferOrders to or from the warehouse
        for (int toI = 0; toI < Count; toI++)
        {
            TransferOrder transferOrder = GetByIndex(toI);
            for (int distI = 0; distI < transferOrder.Distributions.Count; distI++)
            {
                TransferOrderDistribution dist = transferOrder.Distributions.GetByIndex(distI);
                dist.ValidateStorageAreaDelete(a_storageArea, a_saDeleteProfile);
            }
        }
    }

    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ClearingTransferOrders)
        {
            return;
        }

        //Make sure no TransferOrders to or from the warehouse
        for (int toI = 0; toI < Count; toI++)
        {
            TransferOrder transferOrder = GetByIndex(toI);
            for (int distI = 0; distI < transferOrder.Distributions.Count; distI++)
            {
                TransferOrderDistribution dist = transferOrder.Distributions.GetByIndex(distI);
                if (a_deleteProfile.ContainsInventory(dist.FromWarehouse.Id, dist.Item.Id))
                {
                    Inventory inv = dist.FromWarehouse.Inventories[dist.Item.Id];
                    a_deleteProfile.AddValidationException(inv, new PTValidationException("2881", new object[] { dist.Item.Name, dist.FromWarehouse.Name, transferOrder.ExternalId }));
                }

                if (a_deleteProfile.ContainsInventory(dist.ToWarehouse.Id, dist.Item.Id))
                {
                    Inventory inv = dist.ToWarehouse.Inventories[dist.Item.Id];
                    a_deleteProfile.AddValidationException(inv, new PTValidationException("2881", new object[] { dist.Item.Name, dist.FromWarehouse.Name, transferOrder.ExternalId }));
                }
            }
        }
    }

    /// <summary>
    /// Removes all transfer orders, their distributions, and their associated demands if they occur after the specified time.
    /// </summary>
    internal void DeleteTransferOrdersAfterDateTime(ScenarioDetail a_sd, DateTime a_shortTermEnd, Transmissions.PTTransmissionBase a_t, IScenarioDataChanges a_dataChange)
    {
        for (int toI = Count - 1; toI >= 0; toI--)
        {
            TransferOrder transferOrder = this[toI];
            BaseIdList idList = new ();
            for (int dI = transferOrder.Distributions.Count - 1; dI >= 0; dI--)
            {
                if (transferOrder.Distributions[dI].ScheduledReceiveDate > a_shortTermEnd)
                {
                    idList.Add(transferOrder.Distributions[dI].Id);
                }
            }

            if (transferOrder.Distributions.Count == idList.Count)
            {
                //Delete the entire transfer order
                Delete(transferOrder, a_sd, a_t, a_dataChange);
            }
            else if (idList.Count > 0)
            {
                //Delete the specified distributions
                transferOrder.DeleteDistributions(a_sd, idList, a_t);
            }
        }
    }

    internal void Delete(TransferOrder a_transferOrder, ScenarioDetail a_sd, Transmissions.PTTransmissionBase a_t, IScenarioDataChanges a_dataChanges)
    {
        a_transferOrder.Distributions.SetNetChangeMRPFlags();
        a_transferOrder.DeletingOrClearingDistributions(a_sd, a_t);
        Remove(a_transferOrder.Id);
        a_dataChanges.TransferOrderChanges.DeletedObject(a_transferOrder.Id);
    }

    #region Miscellaneous
    public override Type ElementType => typeof(TransferOrder);
    #endregion
}