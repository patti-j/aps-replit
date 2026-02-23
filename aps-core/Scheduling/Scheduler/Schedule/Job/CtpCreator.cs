using System.Runtime.CompilerServices;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Scheduler.Demand;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions.CTP;

namespace PT.Scheduler;

//CTP future enhancements:
//- Create suggested purchaseToStocks if needed
//- show ctpsalesorder in job dlg somewhere
//- add standard cost to Item and add to ctpdialog CalculateCtpCost()
internal class CtpCreator
{
    internal static void CreateCTP(ScenarioCtpT a_t, Scenario a_scenarioToSendResultsTo, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter)
    {
        CreateCTP(a_t.ctp, a_t, a_scenarioToSendResultsTo, a_sd, a_dataChanges, a_errorReporter);
    }

    /// <summary>
    /// This one is used when running a temporary ctp in a what-if
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_scenarioToSendResultsTo"></param>
    /// <param name="a_sd"></param>
    public static void CreateCTP(CtpT a_t, Scenario a_scenarioToSendResultsTo, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter)
    {
        CreateCTP(a_t.ctp, a_t, a_scenarioToSendResultsTo, a_sd, a_dataChanges, a_errorReporter);
    }

    private static void CreateCTP(Ctp a_ctp, ScenarioBaseT a_t, Scenario a_scenarioToSendResultsTo, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter)
    {
        Job newJob = null;
        Exception ctpException = null;
        try
        {
            newJob = CreateCtpJob(a_ctp, a_t, a_sd, a_dataChanges, a_errorReporter);
            SalesOrder newSo = CreateCtpSalesOrder(a_ctp, newJob, a_sd);

            // Update JIT time before scheudling
            //Compute eligibility here to ensure operations' primary rr eligible resources are calculated
            newJob.ComputeEligibility(a_sd.ProductRuleManager);

            List<ManufacturingOrder> mosToRemoveList = new ();
            for (int ctpLineI = 0; ctpLineI < a_ctp.CtpLineList.Count; ctpLineI++)
            {
                ProcessCtpLine(a_ctp, ctpLineI, a_sd, newJob, newSo, mosToRemoveList);
            }

            foreach (ManufacturingOrder m in mosToRemoveList) //Remove MOs that are not needed.
            {
                newJob.ManufacturingOrders.Delete(m, a_dataChanges);
            }

            // Update JIT time before scheudling
            newJob.ComputeEligibility(a_sd.ProductRuleManager);
            newJob.CalculateJitTimes(a_sd.Clock, false);

            a_sd.SalesOrderManager.Add(newSo);
            if (newJob.ManufacturingOrders.Count > 0)
            {
                newSo.CtpJob = newJob;
                newJob.CtpSalesOrder = newSo;
            }

            if (newJob.ManufacturingOrders.Count > 0)
            {
                a_sd.JobManager.AddNewJob(newJob);

                CtpHelper(a_scenarioToSendResultsTo, newJob, a_t, a_sd, a_ctp, a_dataChanges);
                newJob.Anchor(true, a_sd.ScenarioOptions);
            }
            else
            {
                newJob = null;
                CtpHelper(a_scenarioToSendResultsTo, null, a_t, a_sd, a_ctp, a_dataChanges);
            }
        }
        catch (PTException pt)
        {
            ctpException = pt;
            ScenarioExceptionInfo info = new();
            info.Create(a_sd);
            a_errorReporter.LogException(pt, a_t, info, ELogClassification.PtInterface, pt.LogToSentry);
        }
        catch (Exception e)
        {
            ctpException = e;
            ScenarioExceptionInfo info = new ();
            info.Create(a_sd);
            a_errorReporter.LogException(e, a_t, info, ELogClassification.Fatal, false);
        }
        finally
        {
            //Fire this so listeners will continue
            using (a_scenarioToSendResultsTo.AutoEnterScenarioEvents(out ScenarioEvents se))
            {
                se.FireCTPEvent(a_t, a_ctp, newJob, ctpException);
            }
        }
    }

    /// <summary>
    /// Create a new Job for the ctp
    /// </summary>
    private static Job CreateCtpJob(Ctp a_ctp, ScenarioBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter)
    {
        // if an existing job is specified, make sure it exists and use it's Id for the new job.
        Job existingJob = a_ctp.JobId == BaseId.NULL_ID ? null : a_sd.JobManager.GetById(a_ctp.JobId);
        if (existingJob != null) // delete existing job
        {
            a_sd.JobManager.DeleteJob(existingJob, a_t, true, a_dataChanges);
            if (!a_dataChanges.HasChanges || !a_dataChanges.JobChanges.Deleted.Contains(existingJob.Id))
            {
                throw new PTValidationException("3019", new object[] { existingJob.ExternalId });
            }
        }

        // create and init new job
        Job newJob = new (a_sd.JobManager.NextID(), a_sd, a_t.TimeStamp, a_t.Instigator, a_errorReporter);
        newJob.RestoreReferences(a_sd.CustomerManager, a_sd.PlantManager, a_sd.CapabilityManager, a_sd, a_sd.WarehouseManager, a_sd.ItemManager, a_errorReporter);
        newJob.RestoreReferences2();
        AfterRestoreReferences.Helpers.ResetContainedIds(Serialization.VersionNumber, newJob);

        // create a new MO for each Ctp line + set job need date
        for (int ctpLineI = 0; ctpLineI < a_ctp.CtpLineList.Count; ++ctpLineI)
        {
            CtpLine ctpLine = a_ctp.CtpLineList[ctpLineI];
            Warehouse w = GetWarehouse(a_sd, ctpLine);
            BaseId itemId = BaseId.NULL_ID;
            if (ctpLine.UsingIdKey)
            {
                itemId = new BaseId(ctpLine.InventoryKey.ItemId);
            }
            else
            {
                Item item = GetItem(a_sd, ctpLine);
                if (item != null)
                {
                    itemId = item.Id;
                }
            }

            Inventory inv = w.Inventories[itemId];
            if (inv == null)
            {
                throw new PTValidationException("2904", new object[] { ctpLine.UsingIdKey ? ctpLine.InventoryKey.ItemId.ToString() : ctpLine.InventoryExternalIdKey.ItemExternalId, w.Name });
            }

            AddMoToJob(inv, newJob, a_sd, ctpLine.NeedDate, true);
            newJob.NeedDateTicks = Math.Max(newJob.NeedDateTicks, ctpLine.NeedDate.Ticks); // initially the Job.NeedDate is the clock.
        }

        newJob.ExternalId = a_sd.JobManager.NextExternalId();

        // set name
        if (!string.IsNullOrWhiteSpace(a_ctp.JobName))
        {
            newJob.Name = a_ctp.JobName;
        }
        else
        {
            newJob.Name = newJob.ExternalId;
        }

        // set other properties
        newJob.CTP = true;
        newJob.Description = a_ctp.Description;
        newJob.Notes = a_ctp.Note;
        newJob.Commitment = JobDefs.commitmentTypes.Estimate;
        newJob.Hot = a_ctp.Hot;
        newJob.HotReason = a_ctp.HotReason;
        newJob.Priority = a_ctp.Priority;
        Customer customer = a_sd.CustomerManager.GetByExternalId(a_ctp.Customer);
        if (customer != null)
        {
            newJob.AddCustomer(customer);
        }

        newJob.Revenue = a_ctp.Revenue;
        newJob.OrderNumber = newJob.Name;
        newJob.MaintenanceMethod = JobDefs.EMaintenanceMethod.Manual;
        newJob.CanSpanPlants = a_ctp.CanSpanPlants;

        // initialize job for scheduling.
        //newJob.Unschedule();
        //newJob.ComputeEligibility();
        //newJob.CalculateJitTimes(false);

        return newJob;
    }

    private static Item GetItem(ScenarioDetail a_sd, CtpLine a_ctpLine)
    {
        Item item = a_ctpLine.UsingIdKey ? a_sd.ItemManager.GetById(new BaseId(a_ctpLine.InventoryKey.ItemId)) : a_sd.ItemManager.GetByExternalId(a_ctpLine.InventoryExternalIdKey.ItemExternalId);
        if (item == null)
        {
            throw new PTValidationException("2196", new object[] { a_ctpLine.UsingIdKey ? a_ctpLine.InventoryKey.ItemId.ToString() : a_ctpLine.InventoryExternalIdKey.ItemExternalId });
        }

        return item;
    }

    private static Warehouse GetWarehouse(ScenarioDetail a_sd, CtpLine a_ctpLine)
    {
        Warehouse w = a_ctpLine.UsingIdKey ? a_sd.WarehouseManager.GetById(new BaseId(a_ctpLine.InventoryKey.WarehouseId)) : a_sd.WarehouseManager.GetByExternalId(a_ctpLine.InventoryExternalIdKey.WarehouseExternalId);
        if (w == null)
        {
            throw new PTValidationException("2903", new object[] { a_ctpLine.UsingIdKey ? a_ctpLine.InventoryKey.WarehouseId.ToString() : a_ctpLine.InventoryExternalIdKey.WarehouseExternalId });
        }

        return w;
    }

    /// <summary>
    /// create new sales order for ctp
    /// </summary>
    private static SalesOrder CreateCtpSalesOrder(Ctp a_ctp, Job newJob, ScenarioDetail a_sd)
    {
        SalesOrder so = new (a_sd);
        so.Description = a_ctp.Description;
        so.Estimate = true;
        so.Notes = a_ctp.Note;
        so.CancelAtExpirationDate = true; //always for CTP
        so.ExpirationDate = a_ctp.CancelReservationAfter;
        so.Name = newJob.Name;
        return so;
    }

    /// <summary>
    /// Call this for each Ctp Line. It creates a sales order line and manufacturing order for each ctp line. If requirement for the sales order line
    /// can be met without production from the manufacturing order (on hand, other production, etc), then it adds the manufacturing order to a list
    /// for removal later. For the remaining new manufacturing orders that are needed, it calls a function to determine whether they need materials that
    /// can't be satisfied and to create supplies in those cases.
    /// </summary>
    private static void ProcessCtpLine(Ctp a_ctp, int a_ctpLineIndex, ScenarioDetail a_sd, Job a_newJob, SalesOrder a_newSo, List<ManufacturingOrder> a_mosToRemove)
    {
        CtpLine ctpLine = a_ctp.CtpLineList[a_ctpLineIndex];
        ManufacturingOrder mo = a_newJob.ManufacturingOrders[a_ctpLineIndex];
        Item item = GetItem(a_sd, ctpLine);
        Warehouse warehouse = GetWarehouse(a_sd, ctpLine);
        Product product = mo.CurrentPath.GetProductProducingItem(item);

        // add lot code if necessary
        SetLotCodeOnProduct(mo, product.Inventory);

        // create sales order line and distribution for ctp line
        SalesOrderLine sol = new (a_sd.IdGen.NextID(), a_newSo, a_ctpLineIndex.ToString(), item);
        a_newSo.SalesOrderLines.Add(sol);

        // determine net required qty after existing inventory/production has been accounted for
        decimal netRequiredQty = GetNetInventoryRequired(item, a_ctp, ctpLine, a_sd, sol, ctpLine.RequiredQty, ctpLine.NeedDate, warehouse);
        if (netRequiredQty > 0)
        {
            // create a sales order line distribution
            HashSet<string> allowedLotCodes = new ();
            allowedLotCodes.Add(product.LotCode);

            ItemDefs.MaterialAllocation materialAllocation = ItemDefs.MaterialAllocation.NotSet;

            if (warehouse != null)
            {
                Inventory inv = warehouse.Inventories[item.Id];
                materialAllocation = inv.MaterialAllocation;
            }

            AddSoLineDistToSoLine(sol, ctpLine, materialAllocation, product.Warehouse, a_sd.IdGen.NextID(), netRequiredQty, a_sd.WarehouseManager, allowedLotCodes, a_sd.IdGen);

            //TODO: take into account batch size multiplicity
            // set MO quantity
            if (item.BatchSize > 1 && item.MinOrderQty > 0)
            {
                if (netRequiredQty < item.MinOrderQty)
                {
                    netRequiredQty = item.MinOrderQty;
                }
                else
                {
                    decimal nbrBatches = netRequiredQty / item.BatchSize;
                    int nbrBatchesCeiling = (int)Math.Ceiling(nbrBatches);
                    netRequiredQty = item.BatchSize * nbrBatchesCeiling;
                }
            }

            mo.SetRequiredQty(a_sd.Clock, netRequiredQty, a_sd.ProductRuleManager);
            mo.ApplyPlannedScrapQty();

            // create supplies for new manufacturing order's requirements.
            AddMOsForMaterialsForCurrentPathRecursively(a_ctp, mo, a_sd, a_newJob, warehouse);
        }
        else
        {
            //Have the full qty so no MO needed
            //MOs must be removed after this process to not modify the job's mo index.
            a_mosToRemove.Add(mo);
        }
    }

    private static ManufacturingOrder AddMoToJob(Inventory a_inv, Job a_newJob, ScenarioDetail a_sd, DateTime a_needDate, bool a_topLevel)
    {
        if (a_inv.TemplateManufacturingOrder == null)
        {
            throw new PTValidationException("2905", new object[] { a_inv.Item.Name, a_inv.Warehouse.Name });
        }

        ManufacturingOrder mo = a_inv.TemplateManufacturingOrder;
        ManufacturingOrder newMO = mo.CreateUnitializedCopy();
        newMO.Id = a_newJob.ManufacturingOrders.NextID();
        newMO.ExternalId = (a_newJob.ManufacturingOrders.Count + 1).ToString();
        newMO.Name = newMO.ExternalId;
        a_newJob.ManufacturingOrders.Add(newMO);
        newMO.NeedDateTicks = a_needDate.Ticks;
        //newMO.MoNeedDate = true;

        AfterRestoreReferences.Helpers.ResetContainedIds(Serialization.VersionNumber, newMO);
        newMO.RestoreReferences(a_newJob, a_sd.PlantManager, a_sd.CapabilityManager, a_sd, a_sd.WarehouseManager, a_sd.ItemManager);
        newMO.RestoreReferences2();

        return newMO;
    }

    private static void AddMOsForMaterialsForCurrentPathRecursively(Ctp a_ctp, ManufacturingOrder a_parentMo, ScenarioDetail a_sd, Job a_newJob, Warehouse a_parentWarehouse)
    {
        //Calculate JIT dates first since these will drive the material ATP inquiries.
        a_parentMo.CalculateJitTimes(a_sd.Clock, false);

        AlternatePath path = a_parentMo.CurrentPath;
        for (int nodeI = 0; nodeI < path.AlternateNodeSortedList.Count; nodeI++)
        {
            AlternatePath.Node node = path.AlternateNodeSortedList.Values[nodeI];
            for (int matlI = 0; matlI < node.Operation.MaterialRequirements.Count; matlI++)
            {
                MaterialRequirement mr = node.Operation.MaterialRequirements[matlI];
                if (!mr.BuyDirect && mr.Item.Source != ItemDefs.sources.Purchased) //purchased materials must used the Earlier of LeadTime and Available date for now.  May eventually want to create suggested purchases too.
                {
                    //Check if the item should be processed by CTP
                    if (mr.Warehouse != null)
                    {
                        Inventory inv = mr.Warehouse.Inventories[mr.Item.Id];
                        if (inv.MrpProcessing == InventoryDefs.MrpProcessing.GenerateJobs)
                        {
                            decimal qtyToMake = GetNetInventoryRequired(mr.Item, a_ctp, null, a_sd, null, mr.TotalRequiredQty, node.Operation.DbrJitStartDate, mr.Warehouse);
                            if (qtyToMake > 0)
                            {
                                ManufacturingOrder newMo = AddMoToJob(inv, a_newJob, a_sd, node.Operation.DbrJitStartDate, false);
                                SetMaterialPostProcessing(newMo, a_parentWarehouse, a_sd);
                                newMo.SetRequiredQty(a_sd.Clock, qtyToMake, a_sd.ProductRuleManager);
                                newMo.ApplyPlannedScrapQty();
                                AddSuccessorMORelation(newMo, a_parentMo, a_parentMo.CurrentPath, node.Operation, a_sd);

                                SetLotCodeOnProduct(newMo, inv);
                                SetLotCodeOnMaterial(newMo, mr);

                                AddMOsForMaterialsForCurrentPathRecursively(a_ctp, newMo, a_sd, a_newJob, mr.Warehouse);
                            }
                        }
                    }
                }
            }
        }
    }

    private static void AddSuccessorMORelation(ManufacturingOrder a_childMo, ManufacturingOrder a_parentMo, AlternatePath a_path, BaseOperation a_operation, ScenarioDetail a_sd)
    {
        SuccessorMO successorMO = new ($"{a_childMo.ExternalId}-{a_parentMo.ExternalId}", a_childMo, a_parentMo, a_path, a_operation);
        a_childMo.SuccessorMOs.Add(successorMO, a_sd);
    }

    private static void SetLotCodeOnProduct(ManufacturingOrder a_mo, Inventory a_inv)
    {
        string lotCode = GetLotCode(a_mo);
        foreach (Product p in a_mo.GetProducts(false))
        {
            if (p.Inventory.Id == a_inv.Id)
            {
                p.SetLotCode(lotCode);
                break; // found the product we were looking for.
            }
        }
    }

    private static void SetLotCodeOnMaterial(ManufacturingOrder a_mo, MaterialRequirement a_mr)
    {
        a_mr.AddEligibleLotCode(GetLotCode(a_mo));
    }

    private static string GetLotCode(ManufacturingOrder a_mo)
    {
        return $"{a_mo.Job.ExternalId} - {a_mo.ExternalId}";
    }

    private static void SetMaterialPostProcessing(ManufacturingOrder mo, Warehouse a_parentWarehouse, ScenarioDetail a_sd)
    {
        if (a_parentWarehouse == null)
        {
            return;
        }

        Product p = mo.GetPrimaryProduct();
        if (p == null)
        {
            return;
        }

        TimeSpan transferSpan = a_sd.InventoryTransferRuleManager.GetTransferSpan(p.Warehouse, a_parentWarehouse);
        p.MaterialPostProcessingTicks += transferSpan.Ticks;
    }

    /// <summary>
    /// Calculates how much can be satsified from inventory.
    /// </summary>
    private static decimal GetNetInventoryRequired(Item a_item, Ctp a_ctp, CtpLine a_ctpLine, ScenarioDetail a_sd, SalesOrderLine a_sol, decimal a_requiredQty, DateTime a_needDate, Warehouse a_warehouse)
    {
        if (a_ctp.WarehouseEligibility == CtpDefs.warehouseEligibilities.IndividualMaterialsFromSameWarehouse)
        {
            decimal qtyFromWarehouse = 0;
            if (a_warehouse == null) // no warehouse specified, get the max inventory at any one warehouse
            {
                for (int wI = 0; wI < a_sd.WarehouseManager.Count; wI++)
                {
                    Warehouse warehouse = a_sd.WarehouseManager.GetByIndex(wI);

                    if (a_ctp.Warehouses.Contains(warehouse.Id) && warehouse.Inventories.Contains(a_item.Id))
                    {
                        decimal qtyAvailable = warehouse.Inventories[a_item.Id].GetAtpQtyNotInLots(a_needDate, a_sd);
                        if (qtyAvailable > qtyFromWarehouse)
                        {
                            qtyFromWarehouse = qtyAvailable;
                            a_warehouse = warehouse;
                            if (qtyFromWarehouse >= a_requiredQty)
                            {
                                break; //have enough
                            }
                        }
                    }
                }
            }
            else
            {
                qtyFromWarehouse = a_warehouse.Inventories[a_item.Id].GetAtpQtyNotInLots(a_needDate, a_sd);
            }

            decimal qtyToSupplyFromWarehouse = Math.Min(a_requiredQty, qtyFromWarehouse);
            if (a_sol != null && a_warehouse != null && qtyToSupplyFromWarehouse > 0)
            {
                Inventory inv = a_warehouse.Inventories[a_item.Id];
                AddSoLineDistToSoLine(a_sol, a_ctpLine, inv.MaterialAllocation, a_warehouse, a_sd.IdGen.NextID(), qtyToSupplyFromWarehouse, a_sd.WarehouseManager, new HashSet<string>(), a_sd.IdGen);
            }

            return a_requiredQty - qtyToSupplyFromWarehouse;
        }

        //Add up inventory at multiple warehouses
        decimal totalQtySuppliedSoFar = 0;
        for (int wI = 0; wI < a_sd.WarehouseManager.Count; wI++)
        {
            Warehouse warehouse = a_sd.WarehouseManager.GetByIndex(wI);
            if (a_ctp.Warehouses.Contains(warehouse.Id) && warehouse.Inventories.Contains(a_item.Id))
            {
                Inventory inv = warehouse.Inventories[a_item.Id];
                decimal qtyAvail = inv.GetAtpQtyNotInLots(a_needDate, a_sd);
                if (qtyAvail > 0)
                {
                    decimal qtyToSupplyFromWarehouse = Math.Min(qtyAvail, a_requiredQty - totalQtySuppliedSoFar);
                    if (qtyToSupplyFromWarehouse > 0 && a_sol != null)
                    {
                        AddSoLineDistToSoLine(a_sol, a_ctpLine, inv.MaterialAllocation, warehouse, a_sd.IdGen.NextID(), qtyToSupplyFromWarehouse, a_sd.WarehouseManager, new HashSet<string>(), a_sd.IdGen);
                    }

                    totalQtySuppliedSoFar += qtyToSupplyFromWarehouse;
                    if (totalQtySuppliedSoFar >= a_requiredQty)
                    {
                        break; //have enough 
                    }
                }
            }
        }

        return a_requiredQty - totalQtySuppliedSoFar;
    }

    private static void AddSoLineDistToSoLine(SalesOrderLine a_sol, CtpLine a_ctpLine, ItemDefs.MaterialAllocation a_materialAllocation, Warehouse a_warehouse, BaseId a_distId, decimal a_qty, WarehouseManager aWarehouseManager, HashSet<string> a_allowedLotCodes, BaseIdGenerator a_idGen)
    {
        SalesOrderLineDistribution solDist = new (a_sol, a_distId, a_warehouse, a_qty, 0, a_ctpLine.NeedDate, a_materialAllocation, aWarehouseManager, a_allowedLotCodes, a_idGen);
        a_sol.LineDistributions.Add(solDist);
    }

    private static void CtpHelper(Scenario a_scenarioToSendResultsTo, Job a_newJob, ScenarioBaseT a_t, ScenarioDetail a_sd, Ctp a_ctp, IScenarioDataChanges a_dataChanges)
    {
        if (a_newJob != null)
        {
            if (a_ctp.SchedulingType == CtpDefs.ESchedulingType.ExpediteToClock)
            {
                CtpHelperExpedite(a_newJob, a_t, a_sd, ScenarioDetailExpediteBaseT.ExpediteStartDateType.Clock, a_sd.Clock, a_dataChanges);
            }
            else if (a_ctp.SchedulingType == CtpDefs.ESchedulingType.ExpediteToJIT)
            {
                long earliestJITDate = DateTime.MaxValue.Ticks;
                foreach (ManufacturingOrder mo in a_newJob.ManufacturingOrders)
                {
                    for (int i = 0; i < mo.AlternatePaths.Count; i++)
                    {
                        AlternatePath p = mo.AlternatePaths[i];
                        AlternatePath.NodeCollection altPathNodes = p.GetEffectiveLeaves();
                        for (int opIdx = 0; opIdx < altPathNodes.Count; opIdx++)
                        {
                            earliestJITDate = Math.Min(altPathNodes[opIdx].Operation.DbrJitStartDateTicks, earliestJITDate);
                        }
                    }
                }

                if (a_ctp.UseUserSettings)
                {
                    earliestJITDate -= a_sd.OptimizeSettings.JITSlackTicks;
                }
                else
                {
                    using (SystemController.Sys.AutoEnterRead(a_t.Instigator, out User instigator))
                    {
                        if (instigator != null)
                        {
                            earliestJITDate -= instigator.OptimizeSettings.JITSlackTicks;
                        }
                    }
                }

                if (earliestJITDate <= a_sd.Clock)
                {
                    CtpHelperExpedite(a_newJob, a_t, a_sd, ScenarioDetailExpediteBaseT.ExpediteStartDateType.Clock, a_sd.Clock, a_dataChanges);
                }
                else
                {
                    CtpHelperExpedite(a_newJob, a_t, a_sd, ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime, earliestJITDate, a_dataChanges);
                }
            }
            else
            {
                CtpHelperOptimize(a_t, a_sd, a_ctp, a_dataChanges);
            }
        }
        else if (a_ctp.Reserve)
        {
            a_sd.TimeAdjustment(a_t); // this is required to schedule Sales Orders (add them to adjustments) and refresh panes such as InventoryPlan
        }
    }

    private static void CtpHelperOptimize(ScenarioBaseT a_t, ScenarioDetail a_sd, Ctp a_ctp, IScenarioDataChanges a_dataChanges)
    {
        bool useScenarioOptions = true;
        OptimizeSettings optimizeSettings = a_sd.OptimizeSettings;

        if (a_t is ScenarioCtpT scenarioCtpT && !scenarioCtpT.UseScenarioOptimizeSettings)
        {
            useScenarioOptions = false;
            optimizeSettings = scenarioCtpT.OptimizeSettings;
        }

        if (a_t is CtpT ctpT && !ctpT.UseScenarioOptimizeSettings)
        {
            useScenarioOptions = false;
            optimizeSettings = ctpT.OptimizeSettings;
        }

        ScenarioDetailOptimizeT optimizeT = new (a_sd.Scenario.Id, useScenarioOptions ? null : optimizeSettings, false);
        optimizeT.TransmissionNbr = a_t.TransmissionNbr;

        a_sd.OptimizeHandler(optimizeT, optimizeSettings, a_dataChanges);
    }

    private static void CtpHelperExpedite(Job a_newJob, ScenarioBaseT a_t, ScenarioDetail a_sd, ScenarioDetailExpediteBaseT.ExpediteStartDateType a_expediteStartDateType, long a_startTime, IScenarioDataChanges a_dataChanges)
    {
        MOKeyList mos = new ();
        foreach (ManufacturingOrder mo in a_newJob.ManufacturingOrders)
        {
            mos.Add(new ManufacturingOrderKey(a_newJob.Id, mo.Id));
        }

        ScenarioDetailExpediteMOsT expediteT = new (a_sd.Scenario.Id, mos, a_expediteStartDateType, a_startTime, true, false, true);
        expediteT.TransmissionNbr = a_t.TransmissionNbr;
        a_sd.ExpediteMOs(expediteT, a_dataChanges);
    }
}