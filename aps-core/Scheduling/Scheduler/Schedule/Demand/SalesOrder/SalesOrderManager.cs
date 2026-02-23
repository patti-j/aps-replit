using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler.Demand;

public partial class SalesOrderManager : ScenarioBaseObjectManager<SalesOrder>, IPTSerializable
{
    #region IPTSerializable Members
    internal SalesOrderManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        a_reader.Read(out int soCount);

        for (int i = 0; i < soCount; i++)
        {
            Add(new SalesOrder(a_reader, a_idGen));
        }
    }

    internal void RestoreReferences(ScenarioDetail a_sd, WarehouseManager aWarehouses, ItemManager aItems, JobManager aJobManager, CustomerManager a_customerManager)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            try
            {
                this[i].RestoreReferences(a_sd, aWarehouses, aItems, aJobManager, a_customerManager);
            }
            catch (Exception)
            {
                RemoveAt(i); //fix for a problem we had in the line distribution with deleting Warehouses without checking to be sure no dist points to it.
            }
        }
    }    
    
    internal void AfterRestoreAdjustmentReferences()
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            this[i].AfterRestoreAdjustmentReferences();
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (SalesOrder salesOrder in this)
        {
            a_udfManager.RestoreReferences(salesOrder, UserField.EUDFObjectType.SalesOrders);
        }
    }

    public new const int UNIQUE_ID = 645;

    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    #region Construction
    public SalesOrderManager(BaseIdGenerator aIdGen)
        : base(aIdGen) { }
    #endregion

    #region Transmissions
    public void Receive(ScenarioDetailSetSalesOrderValuesT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        UpdateSalesOrderValues(a_t, a_sd, a_dataChanges);
    }

    internal void Receive(ScenarioDetail a_sd, SalesOrderEditT a_soEditsT, IScenarioDataChanges a_dataChanges)
    {
        SalesOrder salesOrder = null;

        //Update sales orders
        foreach (SalesOrderEdit edit in a_soEditsT.SalesOrderEdits)
        {
            salesOrder = GetById(edit.SalesOrderId);
            if (salesOrder == null && edit.ExternalIdSet)
            {
                salesOrder = GetByExternalId(edit.ExternalId);
            }

            salesOrder.Edit(a_sd, edit);
            a_dataChanges.SalesOrderChanges.UpdatedObject(salesOrder.Id);
        }

        bool itemChanged = false;
        //Update sales order lines
        foreach (SalesOrderLineEdit edit in a_soEditsT.SalesOrderLineEdits)
        {
            salesOrder = GetById(edit.SalesOrderId);
            if (salesOrder == null && edit.ExternalIdSet)
            {
                salesOrder = GetByExternalId(edit.ExternalId);
            }

            SalesOrderLine line = salesOrder.FindSalesOrderLine(edit.SalesOrderLineId);
            line.Edit(a_sd, edit, out itemChanged);
            a_dataChanges.SalesOrderChanges.UpdatedObject(salesOrder.Id);
        }

        //Update sales order line distributions
        foreach (SalesOrderLineDistributionEdit edit in a_soEditsT.SalesOrderLineDistributionEdits)
        {
            salesOrder = GetById(edit.SalesOrderId);
            if (salesOrder == null && edit.ExternalIdSet)
            {
                salesOrder = GetByExternalId(edit.ExternalId);
            }

            SalesOrderLine line = salesOrder.FindSalesOrderLine(edit.SalesOrderLineId);

            SalesOrderLineDistribution lineDistribution = line.FindDistribution(edit.SalesOrderLineDistributionId);
            lineDistribution.Edit(a_sd, edit, itemChanged);
            a_dataChanges.SalesOrderChanges.UpdatedObject(salesOrder.Id);
        }
    }

    public void Update(UserFieldDefinitionManager a_udfManager, SalesOrderT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        try
        {
            //TODO: Return to Parallel.Invoke
            //Temp solution until V12 datachages is in place to reduce number of threads
            Thread t1 = new (() => { InitFastLookupByExternalId(); });
            t1.Start();

            Thread t2 = new (() => { a_sd.ItemManager.InitFastLookupByExternalId(); });
            t2.Start();

            Thread t3 = new (() => { a_sd.WarehouseManager.InitFastLookupByExternalId(); });
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            HashSet<string> transmissionSoExternalIds = new ();

            for (int i = 0; i < a_t.SalesOrders.Count; i++)
            {
                try
                {
                    SalesOrderT.SalesOrder tSo = a_t.SalesOrders[i];
                    tSo.Validate();

                    if (!transmissionSoExternalIds.Contains(tSo.ExternalId))
                    {
                        transmissionSoExternalIds.Add(tSo.ExternalId);
                    }

                    SalesOrder so = GetByExternalId(tSo.ExternalId);

                    if (so != null)
                    {
                        so.Update(a_udfManager, tSo, a_sd, a_t);
                        a_dataChanges.SalesOrderChanges.UpdatedObject(so.Id);
                    }
                    else
                    {
                        SalesOrder newSo = new (a_udfManager, NextID(), tSo, IdGen, a_t, a_sd);
                        Add(newSo);
                        a_dataChanges.SalesOrderChanges.AddedObject(newSo.Id);
                    }
                }
                catch (Exception err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                actions.Add(new PostProcessingAction(a_t, false, () =>
                    {
                        ApplicationExceptionList delErrList = new ();

                        for (int i = Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                SalesOrder so = this[i];
                                if (so.CtpJob == null) //don't delete CTPs.  They probably don't exist in the ERP.  Need to delete the Job.
                                {
                                    if (!transmissionSoExternalIds.Contains(so.ExternalId))
                                    {
                                        Delete(so, a_sd, a_t);
                                        a_dataChanges.SalesOrderChanges.DeletedObject(so.Id);
                                    }
                                }
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
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            actions.Add(new PostProcessingAction(a_t, true, () =>
                {
                    DeInitFastLookupByExternalId();
                    a_sd.ItemManager.DeInitFastLookupByExternalId();
                    a_sd.WarehouseManager.DeInitFastLookupByExternalId();
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

    internal void ValidateDelete(Warehouse a_warehouse)
    {
        //Make sure no Sales Order Line Distributions supply this Warehouse
        for (int soI = 0; soI < Count; soI++)
        {
            SalesOrder salesOrder = this[soI];
            for (int solI = 0; solI < salesOrder.SalesOrderLines.Count; solI++)
            {
                SalesOrderLine salesOrderLine = salesOrder.SalesOrderLines[solI];
                for (int distI = 0; distI < salesOrderLine.LineDistributions.Count; distI++)
                {
                    SalesOrderLineDistribution distribution = salesOrderLine.LineDistributions[distI];
                    if (Equals(distribution.SupplyingWarehouse, a_warehouse) || Equals(distribution.MustSupplyFromWarehouse, a_warehouse))
                    {
                        throw new PTValidationException("2165", new object[] { a_warehouse.Name, salesOrder.Name, salesOrderLine.LineNumber });
                    }
                }
            }
        }
    }

    internal void ValidateDelete(ItemDeleteProfile a_items)
    {
        //Make sure no Sales Order Line Distributions supply this Warehouse
        for (int soI = 0; soI < Count; soI++)
        {
            SalesOrder salesOrder = this[soI];
            for (int solI = 0; solI < salesOrder.SalesOrderLines.Count; solI++)
            {
                SalesOrderLine salesOrderLine = salesOrder.SalesOrderLines[solI];
                for (int distI = 0; distI < salesOrderLine.LineDistributions.Count; distI++)
                {
                    if (a_items.ContainsItem(salesOrderLine.Item.Id))
                    {
                        PTValidationException validationException = new ("2166", new object[] { salesOrderLine.Item.Name, salesOrder.Name, salesOrderLine.LineNumber });
                        a_items.AddValidationException(salesOrderLine.Item, validationException);
                    }
                }
            }
        }
    }
    #endregion Transmissions

    #region CTP
    internal void Clear(ScenarioDetail a_sd, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            SalesOrder salesOrder = GetByIndex(i);
            a_dataChanges.SalesOrderChanges.DeletedObject(salesOrder.Id);
            Delete(salesOrder, a_sd, a_t);
        }
    }

    internal void Delete(SalesOrder a_so, ScenarioDetail a_sd, PTTransmission a_t)
    {
        if (a_so.CtpJob != null)
        {
            a_so.CtpJob.CtpSalesOrder = null;
        }

        a_so.SetItemsReferencedAsNeedingNetChangeMRP(a_sd.WarehouseManager);
        a_so.DeletingSalesOrderOrAllDistributions(a_sd, a_t);
        Remove(a_so.Id);
    }

    /// <summary>
    /// Removes all sales orders, sales order lines, and sales order line distributions and their associated demands if they occur after the specified time.
    /// </summary>
    internal void DeleteSalesOrdersAfterDateTime(ScenarioDetail a_sd, DateTime a_shortTermEnd, PTTransmission a_t)
    {
        //Note: Each delete requires iterating through every job and purchase order.
        for (int soI = Count - 1; soI >= 0; soI--)
        {
            SalesOrder salesOrder = this[soI];
            List<SalesOrderLine> lineList = new ();

            //for every line
            for (int solI = salesOrder.SalesOrderLines.Count - 1; solI >= 0; solI--)
            {
                SalesOrderLine salesOrderLine = salesOrder.SalesOrderLines[solI];
                BaseIdList distIdList = new ();

                //for every distribution
                for (int soldI = salesOrderLine.LineDistributions.Count - 1; soldI >= 0; soldI--)
                {
                    if (salesOrderLine.LineDistributions[soldI].RequiredAvailableDate > a_shortTermEnd)
                    {
                        //this distribution should be deleted
                        distIdList.Add(salesOrderLine.Id);
                    }
                }

                if (salesOrderLine.LineDistributions.Count == distIdList.Count)
                {
                    //Delete the entire line
                    lineList.Add(salesOrderLine);
                }
                else
                {
                    //Delete each distribution individually
                    salesOrderLine.DeleteDistributions(a_sd, distIdList, a_t);
                }
            }

            if (salesOrder.SalesOrderLines.Count == lineList.Count)
            {
                //Delete the entire SalesOrder
                Delete(salesOrder, a_sd, a_t);
            }
            else
            {
                //Delete specific lines individually
                foreach (SalesOrderLine sol in lineList)
                {
                    salesOrder.DeleteSalesOrderLine(a_sd, sol, a_t);
                }
            }
        }
    }

    internal void AdvanceClock(DateTime a_newClock)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].AdvanceClock(a_newClock);
        }
    }
    #endregion CTP

    private void UpdateSalesOrderValues(ScenarioDetailSetSalesOrderValuesT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t.SalesOrders.Count == 0)
        {
            return;
        }

        foreach (BaseId orderId in a_t.SalesOrders)
        {
            SalesOrder order = GetById(orderId);
            if (order == null)
            {
                throw new PTValidationException("Sales Order no longer exists".Localize());
            }

            foreach (SalesOrderLine orderLine in order.SalesOrderLines)
            {
                foreach (SalesOrderLineDistribution distribution in orderLine.LineDistributions)
                {
                    if (a_t.Distributions.Contains(distribution.Id))
                    {
                        //Update qty shipped and decrease on hand inventory
                        if (a_t.ShippedSet)
                        {
                            //Find warehouse. If it doesn't exist we cannot complete the shipment
                            Warehouse warehouse = null;
                            if (a_t.WarehouseOverride != BaseId.NULL_ID)
                            {
                                warehouse = a_sd.WarehouseManager.GetById(a_t.WarehouseOverride);
                            }
                            else
                            {
                                warehouse = distribution.MustSupplyFromWarehouse;
                            }

                            //if (warehouse == null)
                            //{
                            //    throw new PTValidationException("2939", new object[] { order.Name, orderLine.LineNumber });
                            //}

                            decimal qtyAdjusted = a_t.QtySet ? a_t.Qty : distribution.QtyOpenToShip;
                            try
                            {
                                Inventory inventory = null;

                                if (warehouse == null)
                                {
                                    foreach (Warehouse w in a_sd.WarehouseManager)
                                    {
                                        if (w.Inventories.Contains(orderLine.Item.Id))
                                        {
                                            inventory = w.Inventories[orderLine.Item.Id];
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    inventory = warehouse.Inventories[orderLine.Item.Id];
                                }

                                if (inventory == null)
                                {
                                    //TODO: maybe. this is copied from commented out Validation Exception above. would need different error code
                                    throw new PTValidationException("2939", new object[] { order.Name, orderLine.LineNumber });
                                }

                                if (inventory.OnHandQty >= qtyAdjusted)
                                {
                                    distribution.QtyShipped += Math.Min(qtyAdjusted, distribution.QtyOpenToShip);
                                    inventory.SetOnHandQty(inventory.OnHandQty - qtyAdjusted);
                                }
                                else
                                {
                                    throw new PTValidationException("2939", new object[] { order.Name, orderLine.LineNumber });
                                }
                            }
                            catch (Exception e)
                            {
                                throw new PTValidationException("2939", new object[] { order.Name, orderLine.LineNumber });
                            }
                        }

                        if (a_t.ClosedSet)
                        {
                            distribution.Closed = a_t.Closed;
                        }
                    }
                }
            }

            if (a_t.DeleteSet && a_t.Delete)
            {
                Delete(order, a_sd, a_t);
                a_dataChanges.SalesOrderChanges.DeletedObject(orderId);
            }
            else
            {
                a_dataChanges.SalesOrderChanges.UpdatedObject(orderId);
            }
        }
    }

    #region Miscellaneous
    public override Type ElementType => typeof(SalesOrderManager);
    #endregion

    #region Edit
    internal new void Add(SalesOrder a_so)
    {
        base.Add(a_so);
    }
    #endregion

    public IEnumerable<SalesOrder> OpenOrdersEnumerator
    {
        get
        {
            foreach (SalesOrder so in this)
            {
                if (!so.Cancelled)
                {
                    yield return so;
                }
            }
        }
    }

    public SalesOrderLineDistribution FindDistribution(BaseId a_salesOrderId, BaseId a_lineId, BaseId a_distId)
    {
        if (GetById(a_salesOrderId) is SalesOrder so)
        {
            foreach (SalesOrderLine line in so.SalesOrderLines)
            {
                if (line.Id == a_lineId)
                {
                    foreach (SalesOrderLineDistribution dist in line.LineDistributions)
                    {
                        if (dist.Id == a_distId)
                        {
                            return dist;
                        }
                    }
                }
            }
        }

        return null;
    }
}