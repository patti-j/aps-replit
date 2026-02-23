using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of PurchaseToStock objects.
/// </summary>
public partial class PurchaseToStockManager : ScenarioBaseObjectManager<PurchaseToStock>, IPTSerializable
{
    #region IPTSerializable Members
    public PurchaseToStockManager(IReader reader, BaseIdGenerator idGen)
        : base(idGen)
    {
        if (reader.VersionNumber >= 670)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                PurchaseToStock purchaseToStock = new (reader);
                Add(purchaseToStock);
            }

            reader.Read(out m_nextExternalIdNbr);
        }
        else if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                PurchaseToStock purchaseToStock = new (reader);
                Add(purchaseToStock);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        
        writer.Write(m_nextExternalIdNbr);
    }

    public new const int UNIQUE_ID = 531;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class PurchaseToStockManagerException : PTException
    {
        public PurchaseToStockManagerException(string message) : base(message) { }
    }
    #endregion

    #region Construction
    public PurchaseToStockManager(BaseIdGenerator idGen) : base(idGen) { }
    #endregion

    #region PurchaseToStock Edit Functions
    internal new PurchaseToStock Add(PurchaseToStock a_po)
    {
        return base.Add(a_po);
    }

    internal new void Remove(PurchaseToStock purchaseToStock)
    {
        base.Remove(purchaseToStock);
    }

    internal void Clear(IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            PurchaseToStock purchaseToStock = GetByIndex(i);
            a_dataChanges.PurchaseToStockChanges.DeletedObject(purchaseToStock.Id);
            Remove(purchaseToStock);
        }
    }

    /// <summary>
    /// Adjust values to update Demo Data.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long clockAdvanceTicks)
    {
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).AdjustDemoDataForClockAdvance(clockAdvanceTicks);
        }
    }
    #endregion

    #region Internal Transmissions
    internal void Receive(ScenarioDetail sd, PurchaseToStockBaseT t, IScenarioDataChanges a_dataChanges)
    {
        if (t is PurchaseToStockIdBaseT)
        {
            PurchaseToStockIdBaseT idT = (PurchaseToStockIdBaseT)t;
            PurchaseToStock pts = GetById(idT.purchaseToStockId);
            if (pts == null)
            {
                throw new PTValidationException("2186", new object[] { idT.purchaseToStockId, t.scenarioId });
            }

            if (t is PurchaseToStockMoveT)
            {
                pts.Move(m_scenario, (PurchaseToStockMoveT)t);
                //Flag scenario to update for the change
                sd.PurchaseToStockAvailableDateChanged();
            }
            else if (t is PurchaseToStockRevertT)
            {
                pts.Revert((PurchaseToStockRevertT)t, a_dataChanges);
            }
        }
        else if (t is ScenarioDetailSetPurchaseToStockValuesT)
        {
            UpdatePurchaseToStockValues((ScenarioDetailSetPurchaseToStockValuesT)t, a_dataChanges);
        }
    }

    internal void Receive(ScenarioDetail a_sd, PurchaseToStockEditT a_poEditsT, IScenarioDataChanges a_dataChanges)
    {
        foreach (PurchaseToStockEdit edit in a_poEditsT)
        {
            PurchaseToStock purchaseToStock = null;

            if (edit.BaseIdSet)
            {
                purchaseToStock = GetById(edit.Id);
            }
            else if (edit.ExternalIdSet)
            {
                purchaseToStock = GetByExternalId(edit.ExternalId);
            }

            //TODO: create appropriate exception message
            if (purchaseToStock == null)
            {
                throw new ValidationException("Not Found");
            }

            if (edit.HasEdits)
            {
                bool updated = purchaseToStock.Edit(a_sd, edit, a_dataChanges);
                if (updated)
                {
                    a_dataChanges.PurchaseToStockChanges.UpdatedObject(purchaseToStock.Id);
                }
            }
        }
    }
    #endregion

    #region ERP Transmissions
    public void Receive(PurchaseToStockT a_t, WarehouseManager warehouses, ItemManager items, UserFieldDefinitionManager a_udfManager, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        HashSet<BaseId> affectedPurchaseToStocks = new ();

        try
        {
            //TODO: Return to Parallel.Invoke
            //Temp solution until V12 datachages is in place to reduce number of threads
            Thread t1 = new (() => { InitFastLookupByExternalId(); });
            t1.Start();

            Thread t2 = new (() => { warehouses.InitFastLookupByExternalId(); });
            t2.Start();

            Thread t3 = new (() => { items.InitFastLookupByExternalId(); });
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            for (int i = 0; i < a_t.Count; ++i)
            {
                try
                {
                    PurchaseToStockT.PurchaseToStock purchaseToStockNode = a_t[i];
                    Validate(purchaseToStockNode, warehouses, items);

                    if (purchaseToStockNode.Id == BaseId.NEW_OBJECT_ID && string.IsNullOrEmpty(purchaseToStockNode.ExternalId))
                    {
                        //We are creating a new order and need a new external ID
                        purchaseToStockNode.ExternalId = NextExternalId();
                        if (string.IsNullOrEmpty(purchaseToStockNode.Name))
                        {
                            purchaseToStockNode.Name = purchaseToStockNode.ExternalId;
                        }
                    }

                    PurchaseToStock purchaseToStock = GetByExternalId(purchaseToStockNode.ExternalId);

                    if (purchaseToStock == null)
                    {
                        purchaseToStock = new PurchaseToStock(NextID(), purchaseToStockNode, warehouses, items, a_udfManager, a_t, a_sd);
                        Add(purchaseToStock);
                        a_dataChanges.PurchaseToStockChanges.AddedObject(purchaseToStock.Id);
                    }
                    else
                    {
                        if (purchaseToStockNode.Id == BaseId.NEW_OBJECT_ID)
                        {
                            //We are creating a new order, but external id is not unique
                            throw new PTValidationException("2937", new object[] { purchaseToStockNode.ExternalId });
                        }

                        bool updated = purchaseToStock.Update(purchaseToStockNode, warehouses, items, a_udfManager, a_t, a_sd, a_dataChanges);
                        if (updated)
                        {
                            a_dataChanges.PurchaseToStockChanges.UpdatedObject(purchaseToStock.Id);
                        }
                    }

                    if (!affectedPurchaseToStocks.Contains(purchaseToStock.Id))
                    {
                        affectedPurchaseToStocks.Add(purchaseToStock.Id);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                List<PurchaseToStock> toDelete = new ();
                foreach (PurchaseToStock purchaseToStock in this)
                {
                    if (!affectedPurchaseToStocks.Contains(purchaseToStock.Id) && purchaseToStock.MaintenanceMethod == PurchaseToStockDefs.EMaintenanceMethod.ERP)
                    {
                        toDelete.Add(purchaseToStock);
                    }
                }

                if (toDelete.Count > 0)
                {
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            ApplicationExceptionList delErrList = new ();

                            foreach (PurchaseToStock ptsDelete in toDelete)
                            {
                                try
                                {
                                    Remove(ptsDelete);
                                    a_dataChanges.PurchaseToStockChanges.DeletedObject(ptsDelete.Id);
                                    a_dataChanges.FlagProductionChanges(BaseId.NULL_ID);
                                }
                                catch (PTValidationException err)
                                {
                                    delErrList.Add(err);
                                }
                            }

                            if (delErrList.Count > 0)
                            {
                                ScenarioExceptionInfo sei = new ();
                                sei.Create(a_sd);
                                m_errorReporter.LogException(delErrList, a_t, sei, ELogClassification.PtInterface, false);
                            }
                        }));
                }
            }
        }
        catch (PTHandleableException e)
        {
            errList.Add(e);
        }
        finally
        {
            actions.Add(new PostProcessingAction(a_t, true, () =>
                {
                    DeInitFastLookupByExternalId();
                    warehouses.DeInitFastLookupByExternalId();
                    items.DeInitFastLookupByExternalId();
                }));
            m_scenarioDetail.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private static void Validate(PurchaseToStockT.PurchaseToStock a_po, WarehouseManager warehouses, ItemManager items)
    {
        //Make sure the Warehouses and Inventories are valid
        Warehouse warehouse = warehouses.GetByExternalId(a_po.WarehouseExternalId);
        if (warehouse == null)
        {
            throw new PTValidationException("2187", new object[] { a_po.ExternalId, a_po.WarehouseExternalId });
        }


        Item item = items.GetByExternalId(a_po.ItemExternalId);
        if (item == null)
        {
            throw new PTValidationException("2188", new object[] { a_po.ExternalId, a_po.ItemExternalId });
        }

        if (string.IsNullOrEmpty(a_po.StorageAreaExternalId))
        {
           throw new PTValidationException("3110", new object[] { a_po.ExternalId });
        }

        StorageArea storageArea = warehouse.StorageAreas.GetByExternalId(a_po.StorageAreaExternalId);
        if (storageArea == null)
        {
            throw new PTValidationException("3097", new object[] { a_po.ExternalId, a_po.StorageAreaExternalId, a_po.WarehouseExternalId });
        }
        
        if (!storageArea.CanStoreItem(item.Id))
        {
            throw new PTValidationException("3091", new object[] { item.ExternalId, a_po.StorageAreaExternalId });
        }


        Inventory inv = warehouse.Inventories[item.Id];
        if (inv == null)
        {
            throw new PTValidationException("2189", new object[] { a_po.ExternalId, a_po.ItemExternalId, a_po.WarehouseExternalId });
        }

        if (a_po.QtyOrdered <= 0 && a_po.QtyReceived <= 0)
        {
            throw new PTValidationException("2938", new object[] { a_po.ExternalId });
        }
    }
    #endregion

    #region Clock Advance
    /// <summary>
    /// Adjust Purchases that are overdue so they are rescheduled to come in one Tick in the future.
    /// </summary>
    /// <param name="newClock"></param>
    internal void AdvanceClock(long newClock)
    {
        long adjustedClockTicks = newClock + 1;

        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            if (pts.ScheduledReceiptDateTicks <= newClock)
            {
                pts.ScheduledReceiptDateTicks = adjustedClockTicks;
            }
        }
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(PurchaseToStock);
    #endregion

    #region Deleting
    internal void ValidateWarehouseDelete(Warehouse warehouse)
    {
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).ValidateWarehouseDelete(warehouse);
        }
    }

    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).ValidateStorageAreaDelete(a_storageArea, a_deleteProfile);
        }
    }

    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ClearingPurchaseOrders)
        {
            return;
        }

        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).ValidateInventoryDelete(a_deleteProfile);
        }
    }
    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ClearPurchaseOrders)
        {
            return;
        }

        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).ValidateItemStorageDelete(a_deleteProfile);
        }
    }

    #region Demands
    internal void DeletingSalesOrderOrAllDistributions(SalesOrder a_so, PTTransmissionBase a_t)
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.DeletingSalesOrder(a_so, a_t);
        }
    }

    internal void DeletingDistributionsFromLine(SalesOrderLine a_soLine, PTTransmissionBase a_t)
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.DeletingSalesOrderLineDistributions(a_soLine, a_t);
        }
    }

    internal void DeletingDistributionsFromLine(SalesOrderLine a_soLine, BaseIdList a_lineIdList, PTTransmissionBase a_t)
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.DeletingSalesOrderLineDistributions(a_soLine, a_lineIdList, a_t);
        }
    }

    internal void DeletingForecastShipments(Forecast a_forecast, PTTransmissionBase a_t)
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.DeletingForecastShipments(a_forecast, a_t);
        }
    }

    internal void DeletingForecastShipment(ForecastShipment a_forecastShipment, PTTransmissionBase a_t)
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.DeletingForecastShipment(a_forecastShipment, a_t);
        }
    }

    internal void DeletingTransferOrderDistributions(TransferOrder a_transferOrder, PTTransmissionBase a_t)
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.DeletingTransferOrderDistributions(a_transferOrder, a_t);
        }
    }

    internal void DeletingTransferOrderDistributions(TransferOrder a_transferOrder, BaseIdList a_distributionIdList, PTTransmissionBase a_t)
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.DeletingTransferOrderDistribution(a_transferOrder, a_distributionIdList, a_t);
        }
    }
    #endregion Demands

    private void UpdatePurchaseToStockValues(ScenarioDetailSetPurchaseToStockValuesT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (a_t.Purchases.Count == 0)
        {
            return;
        }

        foreach (BaseId purchaseId in a_t.Purchases)
        {
            PurchaseToStock purchaseToStock = GetById(purchaseId);

            if (purchaseToStock == null)
            {
                throw new PTValidationException("Purchase Order no longer exists".Localize());
            }

            if (a_t.ClosedSet)
            {
                purchaseToStock.Closed = a_t.Closed;
            }

            //Update qty received and increase on hand inventory
            if (a_t.ReceiveSet)
            {
                decimal qtyAdjusted = a_t.QtySet ? a_t.Qty : purchaseToStock.QtyRemaining;
                purchaseToStock.Inventory.SetOnHandQty(purchaseToStock.Inventory.OnHandQty + qtyAdjusted);
                purchaseToStock.QtyReceived += qtyAdjusted;
            }

            if (a_t.DeleteSet && a_t.Delete)
            {
                Remove(purchaseToStock);
                a_dataChanges.PurchaseToStockChanges.DeletedObject(purchaseToStock.Id);
            }
            else
            {
                a_dataChanges.PurchaseToStockChanges.UpdatedObject(purchaseToStock.Id);
            }
        }
    }
    #endregion

    #region Restore References

    internal override void RestoreReferences(Scenario a_scenario, ScenarioDetail a_sd, ISystemLogger a_errorReporter)
    {
        base.RestoreReferences(a_scenario, a_sd, a_errorReporter);
        for (int i = 0; i < Count; ++i)
        {
            GetByIndex(i).RestoreReferences(a_sd);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (PurchaseToStock purchaseToStock in this)
        {
            a_udfManager.RestoreReferences(purchaseToStock, UserField.EUDFObjectType.PurchasesToStock);
        }
    }
    #endregion

    private int m_nextExternalIdNbr = 1;

    /// <summary>
    /// Returns the next unique ExternalId and increments the counter
    /// </summary>
    public string NextExternalId()
    {
        int nextId = m_nextExternalIdNbr;
        while (GetByExternalId(MakeExternalId(nextId)) != null)
        {
            nextId++;
        }

        m_nextExternalIdNbr = nextId + 1;
        return MakeExternalId(nextId);
    }

    private static string MakeExternalId(long idNbr)
    {
        return "PO-".Localize() + idNbr.ToString().PadLeft(5, '0');
    }

    public IEnumerable<PurchaseToStock> GetOpenOrdersForInventory(Inventory a_inventory)
    {
        foreach (PurchaseToStock purchaseToStock in this)
        {
            if (purchaseToStock.Inventory == a_inventory)
            {
                if (!purchaseToStock.Closed && purchaseToStock.QtyRemaining > 0m)
                {
                    yield return purchaseToStock;
                }
            }
        }
    }
}