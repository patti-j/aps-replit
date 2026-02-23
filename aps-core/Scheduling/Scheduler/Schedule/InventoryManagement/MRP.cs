using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Localization;
using PT.Scheduler.Demand;
using PT.Scheduler.MRP;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.InventoryManagement.MRP;
using PT.Scheduler.Simulation.Customizations.MRP;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// Take inventory into consideration when generating POs and Jobs during MRP.
/// Performance of PurchaseToStock. These are stored in a SortedList. You can sort the list in order and remove things from the end to the start. After the first run through all the non-firm or closed POs will be at the front, so the performance should be good.
/// If your optimize is being done with MRP ignore material requirements by AvailableDate and use by Inventory or Part's Lead Time.
/// Merge multiple adjustments for the same Inventory/Activity into a single requirement so the notes are smaller.
/// Merge code lines. 
/// 
/// DBR: subtract longest resource drum
/// *1. MRP. When setting the need date.
/// *2. When resetting Need Dates of Sub-Components, subtract
/// *3. When calculating operation need date time. Change the need date in this case.
/// 4. DBR is being used when it shouldn't be. If the optimize settings don't include it don't use it in the 3 points above. You might need to 
/// create a new set of need dates; DBRNeedDate and DBRJITDate.
public partial class ScenarioDetail
{
    private void LicenseCheck()
    {
        if (!PTSystem.LicenseKey.IncludeExpressMRP)
        {
            throw new AuthorizationException("ExpressMRP".Localize(), AuthorizationType.LicenseKey, "IncludeExpressMRP", false.ToString());
        }
    }

    private MrpSettings m_activeMrpSettings;

    private void MRP(OptimizeSettings a_optimizeSettings, ScenarioDetailOptimizeT a_optimizeT, IScenarioDataChanges a_dataChanges)
    {
        MaterialRequirementOriginalValueList originalJobMReqValues = null;

        try
        {
            LicenseCheck();

            m_activeMrpSettings = new MrpSettings(a_optimizeSettings, this);

            // if MRP option to set sub job need dates is on, turn on the same system option feature (unless it's already turned on)
            ScenarioOptions.ESubJobNeedDateResetPoint originalSubJobNeedDateResetPoint = ScenarioOptions.SetSubJobNeedDatePoint;
            bool modifiedSetSubJobNeedDates = false;
            bool modifiedCalculateJitStart = false;
            if (a_optimizeSettings.SetSubJobNeedDatePointMrp != ScenarioOptions.ESubJobNeedDateResetPoint.None)
            {
                if (ScenarioOptions.SetSubJobNeedDatePoint != a_optimizeSettings.SetSubJobNeedDatePointMrp)
                {
                    ScenarioOptions.SetSubJobNeedDates_Set(a_optimizeSettings.SetSubJobNeedDatePointMrp);
                    //ScenarioOptionsTrackSubComponentSourceMOsChanged();
                    modifiedSetSubJobNeedDates = true;
                }

                if (!ScenarioOptions.RecalculateJITOnOptimizeEnabled)
                {
                    ScenarioOptions.RecalculateJITOnOptimizeEnabled = true;
                    modifiedCalculateJitStart = true;
                }
            }

            FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_Start);
            WarehouseManager.TemplatesDictionary productTemplates;

            try
            {
                productTemplates = m_warehouseManager.GetTemplatesByProduct();
            }
            catch (JobManager.DuplicateTemplateException e)
            {
                throw new SimulationValidationException(e.Message);
            }

            Scenario.KpiController.SetSuppressKPICalculation(true);

            string lowLevelCodeWarnings = "";

            //Need to set low level codes even if just generating POs to support Net Change MRP which only applies to level 0 items.
            int maxLowLevelCode;

            //Step 1
            SetInventoryLowLevelCodesAndNetChangeFlags(productTemplates, out maxLowLevelCode, ref lowLevelCodeWarnings, m_warehouseManager);

            //step 2
            ClearMRPNotes(a_optimizeSettings);

            //Set all scheduled Job MRs to be non-constraining. This way if any Jobs are anchored or left in the schedule for any other reason they won't be moved when their supplies are removed.
            originalJobMReqValues = new MaterialRequirementOriginalValueList();
            foreach (Job job in JobManager.JobEnumerator)
            {
                originalJobMReqValues.AddJobAndSetMRsToNonConstraining(job);
            }

            if (a_optimizeSettings.RegenerateJobs)
            {
                m_ignorePostSimulationNotification = true;
                DeleteObsoleteJobs(a_optimizeSettings);
                m_ignorePostSimulationNotification = true;
            }

            if (a_optimizeSettings.RegeneratePurchaseOrders)
            {
                DeleteNonFirmPurchaseToStocks(a_optimizeSettings);
            }

            if (a_optimizeSettings.RegenerateJobs || a_optimizeSettings.RegeneratePurchaseOrders)
            {
                ClearAllowedLotCodes();
            }

            //step 3 - 5
            MrpWarnings mrpWarnings = new ();
            Dictionary<BaseId, Tuple<PurchaseToStock, List<InternalActivity>>> poDemands = new ();

            if (a_optimizeSettings.RegenerateJobs)
            {
                //Step 4
                FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_StartInitialOptimize);
                RunSimulationForMrpLevel(a_optimizeT, a_optimizeSettings.MrpJitCompress, a_dataChanges);
                WarehouseManager.ConsumeForecasts(this, a_optimizeSettings.MrpConsumeForecasts); // need ot run this after the first simulation so adjustments from SOs are updated.

                //Step 5
                for (int lowLevelCode = 0; lowLevelCode <= maxLowLevelCode; lowLevelCode++)
                {
                    FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_GeneratingJobs, new object[] { lowLevelCode, maxLowLevelCode });
                    int jobsGenerated = GenerateOrders(productTemplates, lowLevelCode, originalJobMReqValues, a_optimizeSettings, a_optimizeT, mrpWarnings, ref poDemands);
                    if (jobsGenerated > 0)
                    {
                        //LinkSuccessorMOs();
                        FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_OptimizeForLevel, new object[] { lowLevelCode, maxLowLevelCode, jobsGenerated });
                        if (lowLevelCode != maxLowLevelCode) //if on last level don't optimize here. In some cases, can save an optimize by resetting constraints first.
                        {
                            RunSimulationForMrpLevel(a_optimizeT, a_optimizeSettings.MrpJitCompress, a_dataChanges);
                        }
                    }
                }

                //If there are any material requirements that have constraintType=AvailableDateOnly which may get a PO then we need to generate the POs first before resetting the material constraints
                //  or else the jobs can get pushed out past the planning horizon and then misplan the POs from those dates.
                if (!a_optimizeSettings.RegeneratePurchaseOrders || !originalJobMReqValues.HaveMrThatRequiresPoOnlyAndPoMayBeGenerated(this)) //Can reset constraints now
                {
                    originalJobMReqValues.ResetAllMaterialReqtsToOriginalValues();
                }
            }

            //Step 7
            //Done with generation so reset the MR ConstraintTypes and Optimize to enforce material constraints and adjust for new POs if necessary.
            if (!originalJobMReqValues.AlreadyReset)
            {
                originalJobMReqValues.ResetAllMaterialReqtsToOriginalValues();
            }

            m_ignorePostSimulationNotification = false;
            Scenario.KpiController.SetSuppressKPICalculation(false);

            //Last Optimize
            if (a_optimizeSettings.SetSubJobNeedDatePointMrp != ScenarioOptions.ESubJobNeedDateResetPoint.None)
            {
                // material constraints are turned on now, run one last optimize to set
                // need dates and turn off system option if it was off before the MRP.
                if (modifiedSetSubJobNeedDates)
                {
                    FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_StartOptimizeToSetNeedDates);
                    RunSimulationForMrpLevel(a_optimizeT, a_optimizeSettings.MrpJitCompress, a_dataChanges);
                    ScenarioOptions.SetSubJobNeedDates_Set(originalSubJobNeedDateResetPoint);
                    //ScenarioOptionsTrackSubComponentSourceMOsChanged();
                }

                ResetPODates(poDemands, a_optimizeSettings.MrpUseJitDates); // set PO need dates.
            }

            if (modifiedCalculateJitStart)
            {
                ScenarioOptions.RecalculateJITOnOptimizeEnabled = false;
            }

            FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_StartFinalOptimize);
            RunSimulationForMrpLevel(a_optimizeT, a_optimizeSettings.MrpJitCompress, a_dataChanges);
            //Set PO dates again. This will re-align POs with their demand jobs if they scheduled later on the final optimize, or even if they scheduled earlier due to non-constraint
            ResetPODates(poDemands, a_optimizeSettings.MrpUseJitDates);

            if (a_optimizeSettings.RegenerateJobs)
            {
                mrpWarnings.CheckTemplates(this);
                if (mrpWarnings.HaveWarnings || lowLevelCodeWarnings.Length > 0) //This has to be called AFTER the MRP process or else the MRP "missing template" notes will be overwritten.
                {
                    StringBuilder builder = new ();
                    if (mrpWarnings.HaveWarnings)
                    {
                        builder.Append(mrpWarnings.GetLocalizedWarningsText());
                    }

                    if (lowLevelCodeWarnings.Length > 0)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(Environment.NewLine + Environment.NewLine);
                        }

                        builder.Append(lowLevelCodeWarnings);
                    }

                    ScenarioEvents se;
                    using (_scenario.ScenarioEventsLock.EnterRead(out se))
                    {
                        se.FireTransmissionFailureEvent(new PTValidationException(Localizer.GetString("MRP Warnings")), a_optimizeT, new SystemMessage(a_optimizeT.Instigator, builder.ToString()));
                    }
                }
            }

            m_ignorePostSimulationNotification = false;
            m_warehouseManager.SetMrpNetChangeFlagForAllInventoriesToFalse();

            FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_Complete);
        }
        catch (Exception)
        {
            Scenario.KpiController.SetSuppressKPICalculation(false);
            FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_FinishedWithException);
            throw;
        }
        finally
        {
            if (originalJobMReqValues != null && !originalJobMReqValues.AlreadyReset)
            {
                originalJobMReqValues.ResetAllMaterialReqtsToOriginalValues();
            }
        }
    }

    /// <summary>
    /// sets PO need dates based on the scheduled start or JIT of demands they were created for. This is called after
    /// material constraints are turned on.
    /// </summary>
    /// <param name="a_poDemands"></param>
    /// <param name="a_useJitDate"></param>
    private void ResetPODates(Dictionary<BaseId, Tuple<PurchaseToStock, List<InternalActivity>>> a_poDemands, bool a_useJitDate)
    {
        if (a_poDemands == null)
        {
            return;
        }

        foreach (Tuple<PurchaseToStock, List<InternalActivity>> tuple in a_poDemands.Values)
        {
            if (tuple.Item2.Count == 0)
            {
                continue;
            }

            if (tuple.Item1.Demands.QtyToSafetyStock > 0)
            {
                //This PO is supplying inventory, meaning safety stock. Don't update the date.
                continue;
            }

            DateTime date = PTDateTime.MaxDateTime;
            foreach (InternalActivity act in tuple.Item2)
            {
                if (!act.Scheduled)
                {
                    continue;
                }

                DateTime newDate = a_useJitDate ? act.DbrJitStartDate : act.ScheduledStartDate;
                date = PTDateTime.Min(date, newDate);
            }

            if (date == PTDateTime.MaxDateTime)
            {
                continue;
            }

            SetPODate(tuple.Item1, date.Ticks);
        }
    }

    private void RunSimulationForMrpLevel(ScenarioDetailOptimizeT a_optimizeT, bool a_runJitCompress, IScenarioDataChanges a_dataChanges)
    {
        Optimize(a_optimizeT, new ScenarioDataChanges()); //These events won't be tracked, however an end of sim will fire after the MRP process is complete.
        if (a_runJitCompress)
        {
            JitCompress(new ScenarioDetailJitCompressT(a_optimizeT.ScenarioId), a_dataChanges);
        }
    }

    internal class MrpWarnings
    {
        internal void CheckTemplates(ScenarioDetail a_sd)
        {
            for (int wI = 0; wI < a_sd.WarehouseManager.Count; ++wI)
            {
                Warehouse wh = a_sd.WarehouseManager.GetByIndex(wI);
                IEnumerator<Inventory> enumerator = wh.Inventories.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    Inventory inv = enumerator.Current;
                    if (inv.MrpProcessing == InventoryDefs.MrpProcessing.GenerateJobs)
                    {
                        if (inv.TemplateManufacturingOrder == null)
                        {
                            inv.MrpNotes += Localizer.GetString("MISSING TEMPLATE - skipped by MRP");
                            AddInventoryMissingTemplate(inv);
                            continue;
                        }

                        if (inv.TemplateManufacturingOrder.Job.GetPrimaryProduct() == null)
                        {
                            inv.MrpNotes += Localizer.GetString("TEMPLATE HAS NO PRODUCT - skipped by MRP");
                            AddInventoryWithTemplateLackingProduct(inv);
                        }
                    }
                    else if (inv.MrpProcessing == InventoryDefs.MrpProcessing.GeneratePurchaseOrders)
                    {
                        if (inv.PurchaseOrderSupplyStorageArea == null)
                        {
                            inv.MrpNotes += Localizer.GetString("MISSING TEMPLATE - skipped by MRP");
                            AddInventoryMissingTemplate(inv);
                            continue;
                        }
                    }
                }
            }
        }

        internal void AddPartialSupplyViolation(MrpDemand a_demand)
        {
            string warning = string.Format("MRP/MPS could not satisfy a {0} quantity demand for '{1}' because the demand does not allow for partial supply and a single supply source to meet the demand could not be planned.".Localize(), a_demand.DemandQty, a_demand.ReasonDescription);
            m_partialSupplyViolations.Add(warning);
        }

        private readonly List<Inventory> m_inventoriesWithMissingTemplates = new ();
        private readonly List<Inventory> m_inventoriesWithTemplatesLackingProduct = new ();
        private readonly List<Inventory> m_inventoriesWithMissingPoStorageArea = new ();
        private readonly List<string> m_partialSupplyViolations = new ();

        private void AddInventoryMissingTemplate(Inventory aInv)
        {
            m_inventoriesWithMissingTemplates.Add(aInv);
        }

        private void AddInventoryWithTemplateLackingProduct(Inventory aInv)
        {
            m_inventoriesWithTemplatesLackingProduct.Add(aInv);
        }
        
        private void AddInventoryWithMissingPoStorageArea(Inventory aInv)
        {
            m_inventoriesWithMissingPoStorageArea.Add(aInv);
        }

        internal bool HaveWarnings => m_inventoriesWithMissingTemplates.Count > 0 || 
                                      m_inventoriesWithTemplatesLackingProduct.Count > 0 ||
                                      m_inventoriesWithMissingPoStorageArea.Count > 0 || 
                                      m_partialSupplyViolations.Count > 0;

        internal string GetLocalizedWarningsText()
        {
            StringBuilder builder = new ();
            if (m_inventoriesWithMissingTemplates.Count > 0)
            {
                builder.Append(GetLocalizedMissingTemplatesWarning());
            }
            
            if (m_inventoriesWithMissingPoStorageArea.Count > 0)
            {
                builder.Append(GetLocalizedMissingPoStorageAreaWarning());
            }

            if (m_inventoriesWithTemplatesLackingProduct.Count > 0)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine + Environment.NewLine);
                }

                builder.Append(GetLocalizedTemplatesWithNoProductWarning());
            }

            if (m_partialSupplyViolations.Count > 0)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine + Environment.NewLine);
                }

                foreach (string violation in m_partialSupplyViolations)
                {
                    builder.Append(violation);
                }
            }

            return builder.ToString();
        }

        private string GetLocalizedMissingTemplatesWarning()
        {
            if (m_inventoriesWithMissingTemplates.Count > 0)
            {
                StringBuilder errs = new ();
                for (int i = 0; i < m_inventoriesWithMissingTemplates.Count && i < c_maxItemsToShowInMessage; i++)
                {
                    Inventory inv = m_inventoriesWithMissingTemplates[i];
                    if (i > 0)
                    {
                        errs.Append(", ");
                    }

                    errs.Append(string.Format("{0} / {1}", inv.Item.Name, inv.Warehouse.Name));
                }

                return string.Format(Localizer.GetString("Missing a Template for {0} Inventories.  {3}Up to the first {1} are listed:  {3}An Inventory record's MRP Processing property indicates that MRP should create Jobs for shortages but this is not possible without a Template Manufacturing Order specified for the Inventory so MRP ignored these. {3}{2}"), m_inventoriesWithMissingTemplates.Count, c_maxItemsToShowInMessage, errs, Environment.NewLine);
            }

            return "";
        }
        
        private string GetLocalizedMissingPoStorageAreaWarning()
        {
            if (m_inventoriesWithMissingPoStorageArea.Count > 0)
            {
                StringBuilder errs = new ();
                for (int i = 0; i < m_inventoriesWithMissingPoStorageArea.Count && i < c_maxItemsToShowInMessage; i++)
                {
                    Inventory inv = m_inventoriesWithMissingPoStorageArea[i];
                    if (i > 0)
                    {
                        errs.Append(", ");
                    }

                    errs.Append(string.Format("{0} / {1}", inv.Item.Name, inv.Warehouse.Name));
                }

                return string.Format(Localizer.GetString("Missing a Purchase Order Storage Area for {0} Inventories.  {3}Up to the first {1} are listed:  {3}An Inventory record's MRP Processing property indicates that MRP should create Purchase Orders for shortages but this is not possible without a Purchase Order Storage Area specified for the Inventory so MRP ignored these. {3}{2}"), m_inventoriesWithMissingPoStorageArea.Count, c_maxItemsToShowInMessage, errs, Environment.NewLine);
            }

            return "";
        }

        private string GetLocalizedTemplatesWithNoProductWarning()
        {
            if (m_inventoriesWithTemplatesLackingProduct.Count > 0)
            {
                StringBuilder errs = new ();
                for (int i = 0; i < m_inventoriesWithTemplatesLackingProduct.Count && i < c_maxItemsToShowInMessage; i++)
                {
                    Inventory inv = m_inventoriesWithTemplatesLackingProduct[i];
                    if (i > 0)
                    {
                        errs.Append(", ");
                    }

                    errs.Append(string.Format("{0} / {1}", inv.Item.Name, inv.Warehouse.Name));
                }

                return string.Format(Localizer.GetString("Templates for {0} Inventories have no Product defined so MRP will ignore these. {3}Up to the first {1} are listed:  {3}{2}"), m_inventoriesWithTemplatesLackingProduct.Count, c_maxItemsToShowInMessage, errs, Environment.NewLine);
            }

            return "";
        }

        private const int c_maxItemsToShowInMessage = 50;
    }

    /// <summary>
    /// This specifies whether PostSimulationNotification() should be called after the simulation.
    /// During MRP, it's desirable to hold off calling PostSimulationNotification() until the last optimze.
    /// </summary>
    private bool m_ignorePostSimulationNotification;

    /// <summary>
    /// The Localizable Message is passed to Ui for localization.
    /// The Status Code is passed to the UI to signal an update in MRP processing
    /// </summary>
    /// <param name="a_statusCode">SimulationProgress.Status enum</param>
    /// <param name="a_statusParams">Parameters values that will be formatted in the localized string in the UI</param>
    private void FireMRPStatusUpdateEvent(SimulationProgress.Status a_statusCode, object[] a_statusParams = null)
    {
        ScenarioEvents se;
        using (_scenario.ScenarioEventsLock.EnterRead(out se))
        {
            se.FireMrpStatusUpdateEvent(a_statusCode, a_statusParams);
        }
    }

    private void ClearMRPNotes(OptimizeSettings aOptimizeSettings)
    {
        FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_JobNotes);
        for (int wI = 0; wI < m_warehouseManager.Count; wI++)
        {
            Warehouse warehouse = m_warehouseManager.GetByIndex(wI);
            IEnumerator<Inventory> enumerator = warehouse.Inventories.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Inventory inv = enumerator.Current;

                if (!aOptimizeSettings.NetChangeMRP || inv.IncludeInNetChangeMRP)
                {
                    inv.MrpNotes = "";
                }
            }
        }
    }

    private class MaterialRequirementOriginalValueList
    {
        private readonly Dictionary<string, MaterialRequirementOriginalValue> m_mrOrignals = new ();
        private readonly Dictionary<BaseId, BaseId> _mosFromJobsAtTimeMRPCreatedThem = new ();
        private readonly List<Job> _jobsCreatedByMRP = new ();

        internal static string UniqueMaterialRequirementKey(BaseOperation a_op, MaterialRequirement a_mr)
        {
            return $"{a_op.Id}:{a_mr.Id}";
        }

        internal void AddJobAndSetMRsToNonConstraining(Job aJob)
        {
            _jobsCreatedByMRP.Add(aJob);
            for (int moI = 0; moI < aJob.ManufacturingOrderCount; moI++)
            {
                ManufacturingOrder mo = aJob.ManufacturingOrders[moI];
                _mosFromJobsAtTimeMRPCreatedThem.Add(mo.Id, mo.Id); //store so that later we can see if any were created by auto-split during Optimize.
                for (int opI = 0; opI < mo.OperationCount; opI++)
                {
                    BaseOperation op = mo.OperationManager.GetByIndex(opI);
                    for (int mrI = 0; mrI < op.MaterialRequirements.Count; mrI++)
                    {
                        MaterialRequirement mr = op.MaterialRequirements[mrI];
                        MaterialRequirementOriginalValue mrOrig = new (mr);
                        m_mrOrignals.Add(UniqueMaterialRequirementKey(op, mr), mrOrig);
                    }
                }
            }
        }

        private bool m_alreadyReset;

        internal bool AlreadyReset => m_alreadyReset;

        /// <summary>
        /// Sets the MRs to the original values and returns true if any MRs were affected.
        /// </summary>
        /// <returns></returns>
        internal void ResetAllMaterialReqtsToOriginalValues()
        {
            foreach (MaterialRequirementOriginalValue mrOrignalValue in m_mrOrignals.Values)
            {
                if (mrOrignalValue.MR.ConstraintType != mrOrignalValue.OriginalConstraintType)
                {
                    mrOrignalValue.ResetToOriginalValue();
                }
            }

            //Now see if any MOs were created with Autosplit in any MRP generated Jobs.
            //They can have MRs left as non-constraining since they could be copied while the MRs are temporarily set to non-constaining by auto-split.
            for (int jobI = 0; jobI < _jobsCreatedByMRP.Count; jobI++)
            {
                Job job = _jobsCreatedByMRP[jobI];
                for (int moI = 0; moI < job.ManufacturingOrderCount; moI++)
                {
                    ManufacturingOrder newMO = job.ManufacturingOrders[moI];
                    if (!_mosFromJobsAtTimeMRPCreatedThem.ContainsKey(newMO.Id)) //this MO must have been created by an add-in
                    {
                        if (newMO.Split)
                        {
                            ManufacturingOrder sourceMo = job.ManufacturingOrders.GetById(newMO.SplitFromManufacturingOrderId);
                            if (sourceMo == null) // This can happen if the split MO was auto-joined again.
                            {
                                Product product = newMO.GetFirstProductInCurrentPath();
                                if (product != null && product.Inventory != null)
                                {
                                    sourceMo = product.Inventory.TemplateManufacturingOrder;
                                }
                            }

                            //Set the MRs so that their ConstraintType is the same as the orginal MOs.
                            List<MaterialRequirement> newMoMaterials = newMO.GetMaterialRequirements();
                            if (sourceMo != null)
                            {
                                List<MaterialRequirement> sourceMoMaterials = sourceMo.GetMaterialRequirements();
                                for (int mtlI = 0; mtlI < sourceMoMaterials.Count; mtlI++)
                                {
                                    MaterialRequirement sourceMR = sourceMoMaterials[mtlI];
                                    MaterialRequirement newMR = newMoMaterials[mtlI];
                                    if (newMR.ConstraintType != sourceMR.ConstraintType)
                                    {
                                        newMR.ConstraintType = sourceMR.ConstraintType;
                                    }
                                }
                            }
                            else
                            {
                                for (int mtlI = 0; mtlI < newMoMaterials.Count; mtlI++)
                                {
                                    MaterialRequirement newMR = newMoMaterials[mtlI];
                                    if (newMR != null)
                                    {
                                        if (newMR.ConstraintType != MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate)
                                        {
                                            newMR.ConstraintType = MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            m_alreadyReset = true;
        }

        internal bool HaveMrThatRequiresPoOnlyAndPoMayBeGenerated(ScenarioDetail sd)
        {
            foreach (MaterialRequirementOriginalValue mrOrignalValue in m_mrOrignals.Values)
            {
                if (mrOrignalValue.OriginalConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate && mrOrignalValue.MR.Item != null)
                {
                    for (int w = 0; w < sd.WarehouseManager.Count; w++)
                    {
                        Warehouse warehouse = sd.WarehouseManager.GetByIndex(w);
                        if (warehouse.Inventories.Contains(mrOrignalValue.MR.Item.Id))
                        {
                            Inventory inv = warehouse.Inventories[mrOrignalValue.MR.Item.Id];
                            if (inv.MrpProcessing == InventoryDefs.MrpProcessing.GeneratePurchaseOrders)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        internal MaterialRequirementDefs.constraintTypes GetOriginalMRConstraintType(string a_uniqueMRkey)
        {
            if (m_mrOrignals.ContainsKey(a_uniqueMRkey))
            {
                return m_mrOrignals[a_uniqueMRkey].OriginalConstraintType;
            }

            return MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate;
        }

        private class MaterialRequirementOriginalValue
        {
            internal MaterialRequirementOriginalValue(MaterialRequirementOriginalValue a_mrValues, MaterialRequirement a_newMr)
            {
                MR = a_newMr;
                OriginalConstraintType = a_mrValues.OriginalConstraintType;
            }

            internal MaterialRequirementOriginalValue(MaterialRequirement a_mr)
            {
                MR = a_mr;
                OriginalConstraintType = a_mr.ConstraintType;
                a_mr.ConstraintType = MaterialRequirementDefs.constraintTypes.NonConstraint; //MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate;
            }

            internal readonly MaterialRequirement MR;
            internal readonly MaterialRequirementDefs.constraintTypes OriginalConstraintType;

            internal void ResetToOriginalValue()
            {
                MR.ConstraintType = OriginalConstraintType;
            }
        }

        public void CopyMr(InternalOperation a_op, MaterialRequirement a_mr, MaterialRequirement a_newMr)
        {
            if (m_mrOrignals.TryGetValue(UniqueMaterialRequirementKey(a_op, a_mr), out MaterialRequirementOriginalValue original))
            {
                MaterialRequirementOriginalValue newValues = new (original, a_newMr);
                m_mrOrignals.Add(UniqueMaterialRequirementKey(a_op, a_newMr), newValues);
            }
        }
    }

    /// <summary>
    /// Set the Inventory's Low Level Code and flags for Net Change MRP based on the Product Templates.
    /// </summary>
    private void SetInventoryLowLevelCodesAndNetChangeFlags(WarehouseManager.TemplatesDictionary aProductTemplates, out int aMaxLowLevelCode, ref string aLowLevelCodeWarnings, WarehouseManager aWarehouseManager)
    {
        FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_LowLevelCodes);

        //Initialize low level codes for all Inventories
        const int c_initLowlevelcode = -1;

        for (int wI = 0; wI < aWarehouseManager.Count; wI++)
        {
            Warehouse warehouse = aWarehouseManager.GetByIndex(wI);
            IEnumerator<Inventory> invEnumerator = warehouse.Inventories.GetEnumerator();
            while (invEnumerator.MoveNext())
            {
                Inventory inv = invEnumerator.Current;
                inv.LowLevelCode = c_initLowlevelcode;
            }
        }

        //Go through the finished good boms and set levels recursively
        List<WarehouseManager.BOM> fgBOMs = aProductTemplates.GetFinishedGoodBOMS();
        Dictionary<WarehouseManager.BOM, WarehouseManager.BOM> addedBOMs = new ();
        aMaxLowLevelCode = 0;

        for (int bomI = 0; bomI < fgBOMs.Count; bomI++)
        {
            WarehouseManager.BOM bom = fgBOMs[bomI];
            AddBomAndMaterials(bom, 0, addedBOMs, aProductTemplates, ref aMaxLowLevelCode);
        }

        //In case there are Items that are materials for Items that are not planned, give any Items that are materials of unplanned Items, set their low level code the lowest level in case there are demands for it so it gets planned. 
        //Also if an item uses itself as a material (Meduria had this) then it won't get planned. By setting this it'll plan, just ignoring its own need for itself.
        aMaxLowLevelCode++;

        StringBuilder lowLevelCodeWarningsBuilder = new ();
        for (int wI = 0; wI < aWarehouseManager.Count; wI++)
        {
            Warehouse warehouse = aWarehouseManager.GetByIndex(wI);
            IEnumerator<Inventory> invEnumerator = warehouse.Inventories.GetEnumerator();
            while (invEnumerator.MoveNext())
            {
                Inventory inv = invEnumerator.Current;
                if (inv.LowLevelCode == c_initLowlevelcode)
                {
                    inv.LowLevelCode = aMaxLowLevelCode;

                    if (inv.TemplateManufacturingOrder != null && inv.TemplateManufacturingOrder.Job.GetFirstMO().Product != null) //Has a valid template so would expect it to be set
                    {
                        if (lowLevelCodeWarningsBuilder.Length > 0)
                        {
                            lowLevelCodeWarningsBuilder.Append(" ,");
                        }

                        lowLevelCodeWarningsBuilder.Append(string.Format("{0}/{1}", inv.Item.Name, inv.Warehouse.Name));
                    }
                }
            }
        }

        if (lowLevelCodeWarningsBuilder.Length > 0)
        {
            aLowLevelCodeWarnings = string.Format(Localizer.GetString("These Inventories may have circular references or be unused by any Finished Good since Low Level Code was not calculated; set to max depth instead.: {0}"), lowLevelCodeWarningsBuilder);
        }
    }

    private const int c_maxBomLevels = 100;

    private void AddBomAndMaterials(WarehouseManager.BOM a_bom, int a_level, Dictionary<WarehouseManager.BOM, WarehouseManager.BOM> a_addedBOMs, WarehouseManager.TemplatesDictionary aProductTemplates, ref int aMaxLowLevelCode)
    {
        WarehouseManager.BOM value;
        if (a_addedBOMs.TryGetValue(a_bom, out value))
        {
            if (a_level > value.Inventory.LowLevelCode)
            {
                value.Inventory.LowLevelCode = a_level;
            }
        }
        else
        {
            a_addedBOMs.Add(a_bom, a_bom);
            a_bom.Inventory.LowLevelCode = a_level;
        }

        if (a_level > aMaxLowLevelCode)
        {
            aMaxLowLevelCode = a_level;
        }

        int nextLevel = a_level + 1;
        if (nextLevel == c_maxBomLevels)
        {
            throw new PTHandleableException("2902", new object[] { c_maxBomLevels, a_bom.Inventory.Item.Name, a_bom.Inventory.Warehouse.Name, a_bom.Job, a_bom.ParentItem.Name });
        }

        for (int matlI = 0; matlI < a_bom.MaterialsCount; ++matlI)
        {
            MaterialRequirement mr = a_bom.GetMaterialAtIndex(matlI);
            //Determine which Inventories can satisfy this MR either directly or indirectly if no Warehouse is specifed.
            List<Inventory> inventoriesThatMaySatsifyMR = m_warehouseManager.GetInventoriesThatMaySupplyMR(mr);
            for (int invI = 0; invI < inventoriesThatMaySatsifyMR.Count; invI++)
            {
                Inventory inv = inventoriesThatMaySatsifyMR[invI];
                inv.IncludeInNetChangeMRP = inv.IncludeInNetChangeMRP || a_bom.Inventory.IncludeInNetChangeMRP;
                if (inv.TemplateManufacturingOrder != null) //Some Items may be in an inventory with a MO reference but another inventory may not have a reference.
                {
                    WarehouseManager.ItemInventories itemInventories = aProductTemplates[inv.Item];
                    WarehouseManager.BOM bom = itemInventories.GetBomForInventory(inv);
                    AddBomAndMaterials(bom, nextLevel, a_addedBOMs, aProductTemplates, ref aMaxLowLevelCode);
                }
                else //not a template product so treat as purchased and set the LLC if lower than previously set
                {
                    if (inv.LowLevelCode < nextLevel)
                    {
                        inv.LowLevelCode = nextLevel;
                    }
                }
            }
        }
    }

    private void DeleteObsoleteJobs(OptimizeSettings aOptimizeSettings)
    {
        FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_JobCleanUpStart);
        List<Job> jobList = new ();

        for (int i = 0; i < m_jobManager.Count; ++i)
        {
            Job job = m_jobManager[i];
            bool deleteJob = true;

            if (job.Started)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveReleasedJobs && job.Released)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveFirmJobs && job.Firm)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreservePlannedJobs && job.Commitment == JobDefs.commitmentTypes.Planned)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveEstimateJobs && job.Commitment == JobDefs.commitmentTypes.Estimate)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveMrpGeneratedJobs && job.MaintenanceMethod == JobDefs.EMaintenanceMethod.MrpGenerated)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveJobsInStableSpan && job.InStableSpan())
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveCtpJobs && job.CTP)
            {
                deleteJob = false;
            }
            else if (job.DoNotDelete)
            {
                deleteJob = false;
            }
            else if (job.Template)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveAnchoredJobs && job.Anchored != anchoredTypes.Free)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.PreserveLockedJobs && job.Locked != lockTypes.Unlocked)
            {
                deleteJob = false;
            }
            else if (aOptimizeSettings.NetChangeMRP)
            {
                //Preserve Jobs that produce level 0 Item Inventories that haven't been modified since the last MRP because these Inventories won't be replanned by MRP.
                ManufacturingOrder mo = job.GetFirstMO();
                Product product = mo.GetFirstProductInCurrentPath();
                if (product != null && product.Inventory != null && !product.Inventory.IncludeInNetChangeMRP)
                {
                    deleteJob = false;
                }
            }

            if (deleteJob)
            {
                jobList.Add(job);
            }
        }

        if (jobList.Count > 0)
        {
            FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_JobCleanUpDelete, new object[] { jobList.Count });
            m_jobManager.DeleteJobs(jobList, m_activeOptimizeSettingsT, false, new ScenarioDataChanges()); //No need for datachange events here
        }
    }

    /// <summary>
    /// Goes through remaining Jobs and removes any AllowedLotCodes in Material Requirements that reference an old Job.
    /// </summary>
    /// <param name="deletedExternalIds"></param>
    private void ClearAllowedLotCodes()
    {
        System.Collections.Concurrent.ConcurrentBag<Exception> errs = new ();
        JobManager.InitFastLookupByExternalId();
        PurchaseToStockManager.InitFastLookupByExternalId();
        try
        {
            Task[] tasks = new Task[4];
            tasks[0] = Task.Run(() =>
            {
                try
                {
                    //Clear Products First so the can be repegged to existing MRP pegged material requirements
                    foreach (Job j in JobManager.JobEnumerator)
                    {
                        foreach (ManufacturingOrder mo in j.ManufacturingOrders)
                        {
                            for (int i = 0; i < mo.OperationManager.Count; i++)
                            {
                                BaseOperation op = mo.OperationManager.GetByIndex(i);

                                //Clear out MRP created product pegging
                                foreach (Product product in op.Products)
                                {
                                    if (!string.IsNullOrEmpty(product.LotCode))
                                    {
                                        if (product.LotCode.StartsWith("MRP")) // this lot code is not set by MRP, leave it
                                        {
                                            product.ClearLotData();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (Job j in JobManager.JobEnumerator)
                    {
                        foreach (ManufacturingOrder mo in j.ManufacturingOrders)
                        {
                            for (int i = 0; i < mo.OperationManager.Count; i++)
                            {
                                BaseOperation op = mo.OperationManager.GetByIndex(i);
                                op.MaterialRequirements.ClearMrpRequirements();
                                for (int mIdx = 0; mIdx < op.MaterialRequirements.Count; mIdx++)
                                {
                                    MaterialRequirement mr = op.MaterialRequirements[mIdx];
                                    if (mr.BuyDirect || mr.GetEligibleLotCount() == 0)
                                    {
                                        continue;
                                    }

                                    //Get lot codes before clearing them.
                                    HashSet<string> lotCodes = GetNewAndPegExistingLotCodes(mr.GetEligibleLotsEnumerator());
                                    mr.ClearEligibleLots();
                                    //TODO: Rename SetEligibleLots to AddEligibleLots
                                    mr.SetEligibleLots(lotCodes);
                                }
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    errs.Add(err);
                }
            });
            tasks[1] = Task.Run(() =>
            {
                try
                {
                    foreach (SalesOrder so in SalesOrderManager)
                    {
                        foreach (SalesOrderLine sol in so.SalesOrderLines)
                        {
                            foreach (SalesOrderLineDistribution sod in sol.LineDistributions)
                            {
                                if (sod.EligibleLots.Count == 0)
                                {
                                    continue;
                                }

                                //Get lot codes before clearing them.
                                HashSet<string> lotCodes = GetNewAndPegExistingLotCodes(sod.EligibleLots.GetEligibleLotsEnumerator());
                                sod.EligibleLots.Clear();
                                sod.EligibleLots.SetEligibleLots(lotCodes, sod);
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    errs.Add(err);
                }
            });
            tasks[2] = Task.Run(() =>
            {
                try
                {
                    foreach (Warehouse w in WarehouseManager)
                    {
                        for (int i = 0; i < w.Inventories.Count; i++)
                        {
                            Inventory inv = w.Inventories.GetByIndex(i);
                            foreach (Lot lot in inv.Lots)
                            {
                                if (lot.LotSource == ItemDefs.ELotSource.Inventory && lot.Code.StartsWith("MRP"))
                                {
                                    lot.Code = string.Empty;
                                }
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    errs.Add(err);
                }
            });
            tasks[3] = Task.Run(() =>
            {
                try
                {
                    foreach (PurchaseToStock po in PurchaseToStockManager)
                    {
                        if (po.LotCode.StartsWith("MRP"))
                        {
                            po.LotCode = string.Empty;
                        }
                    }
                }
                catch (Exception err)
                {
                    errs.Add(err);
                }
            });

            Task.WaitAll(tasks);
        }
        finally
        {
            JobManager.DeInitFastLookupByExternalId();
            PurchaseToStockManager.DeInitFastLookupByExternalId();
        }

        if (errs.Count > 0)
        {
            throw new AggregateException(errs);
        }
    }

    private HashSet<string> GetNewAndPegExistingLotCodes(Dictionary<string, EligibleLot>.Enumerator a_eligLotEtr)
    {
        HashSet<string> newLotCodes = new ();
        while (a_eligLotEtr.MoveNext())
        {
            if (!a_eligLotEtr.Current.Key.StartsWith("MRP")) // this lot code is not set by MRP, leave it
            {
                newLotCodes.Add(a_eligLotEtr.Current.Key);
            }
            else
            {
                if (JobManager.GetByExternalId(a_eligLotEtr.Current.Key) is Job existingProducer /* || PurchaseToStockManager.GetByExternalId(a_eligLotEtr.Current.Key) != null*/) // Job or PO exists
                {
                    newLotCodes.Add(a_eligLotEtr.Current.Key);

                    //Re-peg the product lot code that was previous cleared. This allows us to keep previous MRP lot pegging so tank demands aren't recreated
                    Product product = existingProducer.GetPrimaryProduct();
                    product.SetLotCode(a_eligLotEtr.Current.Key);
                }
            }
        }

        return newLotCodes;
    }

    /// <summary>
    /// Represents a Job and all the Products associated with it's
    /// Current Path. This is used to avoid getting the list of products
    /// for a Job multiple times during MRP.
    /// </summary>
    private class JobProducts
    {
        private readonly Job m_job;

        public Job Job => m_job;

        private readonly List<Product> m_products = new ();

        public List<Product> Products => m_products;

        public JobProducts(Job a_job)
        {
            m_job = a_job;

            foreach (ManufacturingOrder mo in m_job.ManufacturingOrders)
            {
                m_products.AddRange(mo.GetProducts(true));
            }
        }
    }

    /// <summary>
    /// Gets a list of JobProducts for existing Jobs.
    /// Only include Jobs whose NeedDate may need updating
    /// based on usage in other Jobs.
    /// </summary>
    /// <returns>List of JobProducts</returns>
    private List<JobProducts> GetJobProducts()
    {
        List<JobProducts> jobProducts = new ();

        foreach (Job job in m_jobManager)
        {
            if (!job.Template &&
                (job.ScheduledStatus == JobDefs.scheduledStatuses.Unscheduled ||
                 job.ScheduledStatus == JobDefs.scheduledStatuses.Excluded ||
                 job.ScheduledStatus == JobDefs.scheduledStatuses.FailedToSchedule ||
                 job.ScheduledStatus == JobDefs.scheduledStatuses.New))
            {
                jobProducts.Add(new JobProducts(job));
            }
        }

        return jobProducts;
    }

    private const string c_batchCodeCustomizationUdf = "BatchCodeCustomization";

    private readonly List<SupplyOrder> m_availableSupplyOrders = new ();

    /// <summary>
    /// New jobs need dates are set to the dates when negative adjustments occur.
    /// When you reschedule the demand jobs may end up scheduling earlier or later based on when the material gets produced.
    /// Returns the number of Jobs Generated.
    /// </summary>
    /// <param name="a_productTemplates"></param>
    /// <param name="a_lowLevelCode"></param>
    /// <param name="aOriginalMReqValues"></param>
    /// <param name="aOptimizeSettings"></param>
    /// <param name="a_t"></param>
    /// <param name="a_mrpWarnings"></param>
    /// <param name="a_poDemands"></param>
    /// <param name="productTemplates"></param>
    private int GenerateOrders(WarehouseManager.TemplatesDictionary a_productTemplates, int a_lowLevelCode, MaterialRequirementOriginalValueList aOriginalMReqValues, OptimizeSettings aOptimizeSettings, PTTransmission a_t, MrpWarnings a_mrpWarnings, ref Dictionary<BaseId, Tuple<PurchaseToStock, List<InternalActivity>>> a_poDemands)
    {
        int jobsGeneratedCount = 0;

        List<JobProducts> jobProductsList = GetJobProducts();
        //List<Warehouse> warehouses = GetAllWarehouses();
        foreach (Warehouse wh in m_warehouseManager)
        {
            bool afl = false;
            if (wh.UserFields?.Find("BatchCodeCustomization")?.DataValue is bool custom)
            {
                afl = custom;
            }

            IEnumerator<Inventory> enumerator = wh.Inventories.GetEnumerator();
            int invI = 0;

            while (enumerator.MoveNext())
            {
                invI++;
                Inventory inv = enumerator.Current;
                if (inv.MrpProcessing == InventoryDefs.MrpProcessing.Ignore
                    || inv.LowLevelCode != a_lowLevelCode || (aOptimizeSettings.NetChangeMRP && !inv.IncludeInNetChangeMRP)
                    || (inv.MrpProcessing == InventoryDefs.MrpProcessing.GenerateJobs && (!m_activeMrpSettings.RegenerateJobs || inv.TemplateManufacturingOrder?.Job.GetPrimaryProduct() == null))
                    || (inv.MrpProcessing == InventoryDefs.MrpProcessing.GeneratePurchaseOrders && !m_activeMrpSettings.RegeneratePurchaseOrders))

                {
                    continue;
                }

                Item item = inv.Item;

                m_availableSupplyOrders.Clear();

                switch (inv.MrpProcessing)
                {
                    case InventoryDefs.MrpProcessing.GenerateJobs:
                        inv.MrpNotes += string.Format(Localizer.GetString("MRP Processed as Manufactured/JobTemplate Item: {0}"), DateTime.Now);
                        break;
                    case InventoryDefs.MrpProcessing.GeneratePurchaseOrders:
                        inv.MrpNotes = string.Format("MRP Processed as Purchased Item: {0}".Localize(), DateTime.Now);
                        break;
                }
                inv.MrpAllocated = 0; //initialize              
                List<LotAllocations> lotAllocations = GetLotAllocations(inv);

                AdjustmentArray negativeAdjustments = inv.GetAdjustmentArray().GetNegativeAdjustments(PTDateTime.MaxDateTime);
                List<MrpDemand> demands = GenerateMrpDemands(negativeAdjustments, inv, false, aOptimizeSettings.MrpUseJitDates);

                AdjustmentArray positiveAdjustments = inv.GetAdjustmentArray().GetPositiveAdjustments(m_activeMrpSettings.CutoffDate);
                List<MrpSupply> supplies = GenerateMrpSupplies(positiveAdjustments, inv);

                List<Job> newJobList = new ();
                List<PurchaseToStock> newPOList = new();

                Dictionary<string, Dictionary<BaseId, decimal>> successorMos = new ();

                demands = OrderDemands(demands, afl);

                //Only show items that are being used by MRP
                if (demands.Count > 0)
                {
                    FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_ItemAdjustmentStart,
                        new object[]
                        {
                            wh.Name, 0, m_warehouseManager.Count, item.Name, invI, wh.Inventories.Count,
                            demands.Count, positiveAdjustments.Count
                        });
                }

                /// MRP SOURCING
                /// 
                /// Newest Lot Allocation
                /// 1. Use the remainder of new MRP closed batches. This aims to use remainders soon after the batch
                /// 2. Try pulling from existing supply jobs
                /// 3. Use new batches that are still open
                /// 4. Pull from on-hand
                /// 5. Generate a new order to supply
                ///
                /// Oldest Lot Allocation
                /// 1. Use On-hand
                /// 2. Use the remainder of new MRP closed batches.
                /// 3. Use new batches that are still open
                /// 4. Try pulling from existing supply jobs
                /// 5. Generate a new order to supply
                ///
                /// Default Lot Allocation (And Safetystock)
                /// 1. Use On-hand
                /// 2. Use the remainder of new MRP closed batches.
                /// 3. Try pulling from existing supply jobs
                /// 4. Use new batches that are still open
                /// 5. Generate a new order to supply

                for (int negativeAdjustmentI = 0; negativeAdjustmentI < demands.Count; ++negativeAdjustmentI)
                {
                    // Apply on-hand material first
                    MrpDemand mrpDemand = demands[negativeAdjustmentI];

                    //TODO: SA
                    //if (negativeAdjustment is AdjustmentExpirable)
                    //{
                    //    continue; // ignore lot expirations
                    //}

                    MaterialRequirement demandMR = mrpDemand is ActivityMrpDemand actDemand ? actDemand.MR : null;
                    bool allowPartialSupply = demandMR?.AllowPartialSupply ?? true;
                    decimal qtyNeeded = Math.Abs(mrpDemand.DemandQty);
                    DateTime adjDate = mrpDemand.DemandDate;

                    //TODO: Should this be done later, to not affect other adjDate usage in this function.
                    //Constraint need date by lead time for manufactured items

                    //TODO: Do we need this?
                    //if ((item.Source == ItemDefs.sources.Manufactured || item.Source == ItemDefs.sources.PurchasedOrManufactured) && negativeAdjustment.Reason is Inventory)
                    //{
                    //    //We are creating jobs here, so Purchased or manufactured will be manufactured at this point
                    //    adjDate = PTDateTime.Max(ClockDate.Add(inv.LeadTime), adjDate);
                    //}

                    //We are at a new demand time, close all previous batches
                    foreach (SupplyOrder supplyOrder in m_availableSupplyOrders)
                    {
                        supplyOrder.CloseBatching(adjDate);
                    }

                    //Determine which sourcing method we are using
                    ItemDefs.MaterialAllocation materialAllocation = GetMaterialAllocation(mrpDemand, inv);

                    //TODO: Change the sequence based on some setting. Can't easily combo with MaterialAllocation because within each case sources are sorted.
                    //  So for example we cant choose oldest supplies first and still choose existing orders over new batches.
                    switch (ItemDefs.MaterialAllocation.NotSet)
                    {
                        #region Default
                        case ItemDefs.MaterialAllocation.NotSet:
                            if (AllocateOnHand(ref qtyNeeded, inv, mrpDemand, adjDate, lotAllocations, allowPartialSupply, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            //Closed Batch Remainders
                            if (AllocateFromBatches(ref qtyNeeded, adjDate, materialAllocation, false, mrpDemand, demandMR, aOptimizeSettings.MrpUseJitDates, a_mrpWarnings))
                            {
                                //All demand met
                                continue;
                            }

                            if (AllocateExistingSupplyOrders(inv, ref qtyNeeded, adjDate, materialAllocation, mrpDemand, supplies, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            //Open Batches
                            if (AllocateFromBatches(ref qtyNeeded, adjDate, materialAllocation, true, mrpDemand, demandMR, aOptimizeSettings.MrpUseJitDates, a_mrpWarnings))
                            {
                                //All demand met
                                continue;
                            }

                            if (AllocateNewMRPJobs(inv, ref qtyNeeded, adjDate, materialAllocation, mrpDemand, jobProductsList, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            break;
                        #endregion

                        #region Oldest
                        case ItemDefs.MaterialAllocation.UseOldestFirst:
                            if (!AllocateOnHand(ref qtyNeeded, inv, mrpDemand, adjDate, lotAllocations, allowPartialSupply, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            //Closed Batch Remainders
                            if (AllocateFromBatches(ref qtyNeeded, adjDate, materialAllocation, false, mrpDemand, demandMR, aOptimizeSettings.MrpUseJitDates, a_mrpWarnings))
                            {
                                //All demand met
                                continue;
                            }

                            //Open Batches
                            if (AllocateFromBatches(ref qtyNeeded, adjDate, materialAllocation, true, mrpDemand, demandMR, aOptimizeSettings.MrpUseJitDates, a_mrpWarnings))
                            {
                                //All demand met
                                continue;
                            }


                            if (AllocateExistingSupplyOrders(inv, ref qtyNeeded, adjDate, materialAllocation, mrpDemand, supplies, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            if (AllocateNewMRPJobs(inv, ref qtyNeeded, adjDate, materialAllocation, mrpDemand, jobProductsList, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            break;
                        #endregion

                        #region Newest
                        case ItemDefs.MaterialAllocation.UseNewestFirst:
                            //Closed Batch Remainders
                            if (AllocateFromBatches(ref qtyNeeded, adjDate, materialAllocation, false, mrpDemand, demandMR, aOptimizeSettings.MrpUseJitDates, a_mrpWarnings))
                            {
                                //All demand met
                                continue;
                            }

                            if (AllocateExistingSupplyOrders(inv, ref qtyNeeded, adjDate, materialAllocation, mrpDemand, supplies, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            //Open Batches
                            if (AllocateFromBatches(ref qtyNeeded, adjDate, materialAllocation, true, mrpDemand, demandMR, aOptimizeSettings.MrpUseJitDates, a_mrpWarnings))
                            {
                                //All demand met
                                continue;
                            }

                            if (AllocateNewMRPJobs(inv, ref qtyNeeded, adjDate, materialAllocation, mrpDemand, jobProductsList, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            if (!AllocateOnHand(ref qtyNeeded, inv, mrpDemand, adjDate, lotAllocations, allowPartialSupply, demandMR))
                            {
                                //All demand met
                                continue;
                            }

                            break;
                        #endregion

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    #region Batching
                    if (adjDate >= m_activeMrpSettings.StartDate) //Only create new orders starting at the MRP start date.
                    {
                        bool supplyCreatedForThisDemand = false;
                        while (ScenarioOptions.IsStrictlyGreaterThanZero(qtyNeeded))
                        {
                            SupplyOrder supplyOrder = new (inv);
                            qtyNeeded = supplyOrder.AddDemand(this, mrpDemand, adjDate, ScenarioOptions.RoundQty(qtyNeeded), allowPartialSupply, aOptimizeSettings.MrpUseJitDates, a_mrpWarnings);
                            if (supplyOrder.IsEmpty)
                            {
                                a_mrpWarnings.AddPartialSupplyViolation(mrpDemand);
                                break; //This demand cannot be satisfied
                            }

                            m_availableSupplyOrders.Add(supplyOrder);
                            if (supplyCreatedForThisDemand)
                            {
                                //This supply order was created because a previous one did not fully satisfy the demand
                                supplyOrder.BatchOverflow = true;
                            }

                            supplyCreatedForThisDemand = true;
                        }
                    }
                    #endregion
                }

                //Finish any orders before moving on to the next inv
                for (int i = m_availableSupplyOrders.Count - 1; i >= 0; i--)
                {
                    SupplyOrder supplyOrder = m_availableSupplyOrders[i];
                    if (!supplyOrder.IsEmpty) // create the last Job before the next Inventory
                    {
                        switch (inv.MrpProcessing)
                        {
                            case InventoryDefs.MrpProcessing.GenerateJobs:
                                CreateSupply(supplyOrder, jobProductsList, newJobList, aOriginalMReqValues, a_t, successorMos, a_lowLevelCode);
                                break;
                            case InventoryDefs.MrpProcessing.GeneratePurchaseOrders:
                                CreateSupply(supplyOrder, newPOList, a_poDemands);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                //Set the MaterialRequirements to non-constraining for all new Jobs. This speeds the optimizes and gives the Optimize a chance to batch the higher levels prior to trying to satisfy the material requirements.
                foreach (Job j in newJobList)
                {
                    aOriginalMReqValues.AddJobAndSetMRsToNonConstraining(j);
                    JobManager.AddNewJob(j);
                    j.LinkSuccessorMOs();
                }

                // Add all the new POs to the PO manager
                foreach (PurchaseToStock po in newPOList)
                {
                    PurchaseToStockManager.Add(po);
                }

                jobsGeneratedCount = jobsGeneratedCount + newJobList.Count;
            }
        }

        return jobsGeneratedCount;
    }

    private List<MrpDemand> OrderDemands(List<MrpDemand> a_demands, bool a_custom)
    {
        List<MrpDemand> demands;     
        if (a_custom)
        {
            //Custom for AFL
            demands = a_demands
                      .OrderBy(adj =>
                      {
                          int stage = 0;
                          if (adj is ActivityMrpDemand actAdjustment)
                          {
                              if (actAdjustment.Activity.Operation.UserFields?.Find("Staging")?.DataValue is string stageCode)
                              {
                                  int.TryParse(stageCode, out stage);
                              }
                          }

                          return stage;
                      })
                      .ThenBy(a => a.DemandDate)
                      .ToList();
        }
        else
        {
            demands = a_demands
                      .OrderBy(a => a.DemandDate)
                      .ToList();
        }

        return demands;
    }

    private bool AllocateFromBatches(ref decimal r_qtyNeeded, DateTime a_adjDate, ItemDefs.MaterialAllocation a_materialAllocation, bool a_openBatches, MrpDemand a_demand, MaterialRequirement a_demandMR, bool a_useJitDates, MrpWarnings a_warnings)
    {
        bool allowPartialSupply = a_demandMR?.AllowPartialSupply ?? true;

        // Closed Batch Remainders
        IEnumerator<SupplyOrder> supplyOrderEnumerator = GetSupplyOrderEnumerator(a_materialAllocation, a_openBatches);
        while (supplyOrderEnumerator.MoveNext())
        {
            SupplyOrder supplyOrder = supplyOrderEnumerator.Current;
            //Now check if we can use any remainders
            if (supplyOrder.QuantityToAccept(this, a_demand, a_adjDate, ScenarioOptions.RoundQty(r_qtyNeeded), allowPartialSupply) > 0)
            {
                r_qtyNeeded = supplyOrder.AddDemand(this, a_demand, a_adjDate, ScenarioOptions.RoundQty(r_qtyNeeded), allowPartialSupply, a_useJitDates, a_warnings);
            }

            if (!ScenarioOptions.IsStrictlyGreaterThanZero(r_qtyNeeded))
            {
                return true;
            }
        }

        return !ScenarioOptions.IsStrictlyGreaterThanZero(r_qtyNeeded);
    }

    private bool AllocateExistingSupplyOrders(Inventory a_inv, ref decimal r_qtyNeeded, DateTime a_adjDate, ItemDefs.MaterialAllocation a_materialAllocation, MrpDemand a_demand, List<MrpSupply> a_supplies, MaterialRequirement a_demandMR)
    {
        // Existing jobs and a list of the requirement dates that have allocated some material from it. 
        // long=requirement date, decimal=quantity of job that is allocated to the requirement.
        Dictionary<BaseId, List<Pair<BaseIdObject, decimal>>> existingSupplies = new ();
        bool allowPartialSupply = a_demandMR?.AllowPartialSupply ?? true;

        //Order positive adjustments based on the demand material allocation setting
        List<MrpSupply> sortedAdjustments;
        switch (a_materialAllocation)
        {
            case ItemDefs.MaterialAllocation.NotSet:
                sortedAdjustments = a_supplies;
                break;
            case ItemDefs.MaterialAllocation.UseOldestFirst:
                sortedAdjustments = a_supplies.OrderBy(s => s.SupplyDate).ToList();
                break;
            case ItemDefs.MaterialAllocation.UseNewestFirst:
                sortedAdjustments = a_supplies.OrderByDescending(s => s.SupplyDate).ToList();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (MrpSupply supply in sortedAdjustments)
        {
            if (ScenarioOptions.IsStrictlyGreaterThanZero(supply.UnallocatedQty))
            {
                if (supply is ActivitySupply actSupply)
                {
                    if (a_demandMR != null)
                    {
                        if (!IsAllocationAllowed(actSupply.Activity, a_demandMR, a_inv, supply.OriginallyLotPegged))
                        {
                            continue;
                        }
                    }
                    else if (a_demand is SalesOrderMrpDemand demandSod)
                    {
                        if (!IsAllocationAllowed(actSupply.Activity, demandSod.Distribution, a_inv, supply.OriginallyLotPegged))
                        {
                            continue;
                        }
                    }

                    Job job = actSupply.Activity.Operation.ManufacturingOrder.Job;

                    //TODO: What is this?, seems like we don't need it
                    if (!existingSupplies.TryGetValue(job.Id, out List<Pair<BaseIdObject, decimal>> allocations))
                    {
                        //Check to see if we should update this Job's needdate to the demand date. If so, this will satisfy the default check for whether the supply can satisfy the demand.
                        //TODO: MRP
                        DateTime? newDateTime = m_extensionController.ExistingOrderNeedDateReset(this, job, actSupply.SupplyQty, actSupply.UnallocatedQty, a_demand, a_adjDate);
                        if (newDateTime.HasValue)
                        {
                            SetJobNeedDate(job, newDateTime.Value.Ticks);
                        }

                        allocations = new List<Pair<BaseIdObject, decimal>>();
                        existingSupplies.Add(job.Id, allocations);
                        job.OrderNumber = string.Empty;
                    }

                    //Determine if this supply can be used to satisfy the demand
                    bool? canSupplySatisfyDemand = m_extensionController.CanExistingSupplySatisfyDemand(this, job, a_adjDate);
                    if (!canSupplySatisfyDemand.HasValue)
                    {
                        if (!m_activeOptimizeSettings.SourceFromFirmOrders)
                        {
                            //Use default
                            canSupplySatisfyDemand = MrpExtensionDefaults.CanExistingSupplySatisfyDemand(this, job, a_adjDate);
                        }
                        else
                        {
                            canSupplySatisfyDemand = true;
                        }
                    }

                    if (canSupplySatisfyDemand == true)
                    {
                        bool allocated = false;
                        //TODO: MRP
                        if (supply.UnallocatedQty >= r_qtyNeeded)
                        {
                            allocated = true;
                            supply.Allocate(r_qtyNeeded, a_demand, allocations);
                            r_qtyNeeded = 0;
                            if (m_activeMrpSettings.EnableLotPegging || a_demand.LotControlled)
                            {
                                if (a_demand is ActivityMrpDemand actAdjustmentTmp) // set allowed lot code
                                {
                                    PegSupplyAndDemand(actSupply.Activity, actAdjustmentTmp.Activity, a_inv);
                                }
                                else if (a_demand is SalesOrderMrpDemand demandSod)
                                {
                                    PegSupplyAndDemand(actSupply.Activity.Operation.ManufacturingOrder.Job, demandSod.Distribution, a_inv);
                                }
                            }
                        }
                        else if (allowPartialSupply)
                        {
                            allocated = true;
                            r_qtyNeeded -= supply.UnallocatedQty;
                            supply.Allocate(supply.UnallocatedQty, a_demand, allocations);
                            if (m_activeMrpSettings.EnableLotPegging || a_demand.LotControlled)
                            {
                                if (a_demand is ActivityMrpDemand actAdjustmentTmp1) // set allowed lot code
                                {
                                    PegSupplyAndDemand(actSupply.Activity, actAdjustmentTmp1.Activity, a_inv);
                                }
                                else if (a_demand is SalesOrderMrpDemand demandSod)
                                {
                                    PegSupplyAndDemand(actSupply.Activity.Operation.ManufacturingOrder.Job, demandSod.Distribution, a_inv);
                                }
                            }
                        }

                        // update OrderNumber 
                        if (allocated)
                        {
                            //This Checks for whether to update the supplying Job's needdate to the demand needdate. 
                            DateTime? newDateTime = m_extensionController.PegAllocationNeedDate(this, job, a_demand);
                            if (newDateTime.HasValue)
                            {
                                SetJobNeedDate(job, newDateTime.Value.Ticks);
                            }

                            string orderNumber = null;
                            string customerExternalId = null;
                            if (a_demand is ActivityMrpDemand actAdj) // set allowed lot code
                            {
                                orderNumber = actAdj.Activity.Operation.ManufacturingOrder.Job.Name;
                            }
                            else if (a_demand is SalesOrderMrpDemand demandSod)
                            {
                                orderNumber = demandSod.Distribution.SalesOrderLine.SalesOrder.Name;
                                customerExternalId = demandSod.Distribution.SalesOrderLine.SalesOrder.Customer?.ExternalId;
                            }
                            else if (a_demand is TransferOrderMrpDemand tod)
                            {
                                orderNumber = tod.TransferOrder.Name;
                            }
                            else if (a_demand is ForecastMrpDemand fs)
                            {
                                orderNumber = fs.Shipment.Forecast.Name;
                                customerExternalId = fs.Shipment.Forecast.Customer?.ExternalId;
                            }

                            if (string.IsNullOrWhiteSpace(job.OrderNumber))
                            {
                                job.OrderNumber = orderNumber;
                            }
                            else
                            {
                                List<string> currentOrderNumbers = job.OrderNumber.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                                if (!currentOrderNumbers.Contains(orderNumber))
                                {
                                    job.OrderNumber = $"{job.OrderNumber}, {orderNumber}";
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(customerExternalId))
                            {
                                Customer customer = CustomerManager.GetByExternalId(customerExternalId);
                                if (!job.Customers.Contains(customer.Id))
                                {
                                    job.Customers.Add(customer);
                                }
                            }
                        }
                    }
                }
                else if (supply is PurchaseOrderSupply poSupply)
                {
                    PurchaseToStock po = poSupply.PurchaseOrder;

                    //Verify that this PO is available before the demand
                    if (po.AvailableDate > a_adjDate)
                    {
                        continue;
                    }

                    if (a_demandMR != null)
                    {
                        if (!IsAllocationAllowed(po, a_demandMR, a_inv, supply.OriginallyLotPegged))
                        {
                            continue;
                        }
                    }
                    else if (a_demand is SalesOrderMrpDemand demandSod)
                    {
                        if (!IsAllocationAllowed(po, demandSod.Distribution, a_inv, supply.OriginallyLotPegged))
                        {
                            continue;
                        }
                    }
                    
                    List<Pair<BaseIdObject, decimal>> allocations;
                    List<Pair<BaseIdObject, decimal>> value;
                    if (!existingSupplies.TryGetValue(po.Id, out value))
                    {
                        if (!po.Firm && supply.UnallocatedQty == poSupply.SupplyQty)
                        {
                            po.ScheduledReceiptDate = a_adjDate;
                        }

                        allocations = new List<Pair<BaseIdObject, decimal>>();
                        existingSupplies.Add(po.Id, allocations);
                    }
                    else
                    {
                        allocations = value;
                    }

                    if (supply.UnallocatedQty >= r_qtyNeeded)
                    {
                        supply.Allocate(r_qtyNeeded, a_demand, allocations);
                        r_qtyNeeded = 0;
                        if (m_activeMrpSettings.EnableLotPegging || a_demand.LotControlled)
                        {
                            if (a_demand is ActivityMrpDemand actAdj2) // set allowed lot code
                            {
                                PegSupplyAndDemand(po, actAdj2.Activity, a_inv);
                            }
                            else if (a_demand is SalesOrderMrpDemand demandSod)
                            {
                                PegSupplyAndDemand(po, demandSod.Distribution, a_inv);
                            }
                        }
                    }
                    else if (allowPartialSupply)
                    {
                        r_qtyNeeded -= supply.UnallocatedQty;
                        supply.Allocate(supply.UnallocatedQty, a_demand, allocations);
                        if (m_activeMrpSettings.EnableLotPegging || a_demand.LotControlled)
                        {
                            if (a_demand is ActivityMrpDemand actAdj2) // set allowed lot code
                            {
                                PegSupplyAndDemand(po, actAdj2.Activity, a_inv);
                            }
                            else if (a_demand is SalesOrderMrpDemand demandSod)
                            {
                                PegSupplyAndDemand(po, demandSod.Distribution, a_inv);
                            }
                        }
                    }
                }

                if (!ScenarioOptions.IsStrictlyGreaterThanZero(r_qtyNeeded))
                {
                    return true;
                }
            }
        }

        return !ScenarioOptions.IsStrictlyGreaterThanZero(r_qtyNeeded);
    }

    private bool AllocateNewMRPJobs(Inventory a_inv, ref decimal r_qtyNeeded, DateTime a_adjDate, ItemDefs.MaterialAllocation a_materialAllocation, MrpDemand a_negativeAdjustment, List<JobProducts> a_jobProductsList, MaterialRequirement a_demandMR)
    {
        bool allowPartialSupply = a_demandMR?.AllowPartialSupply ?? true;

        List<SSimNewJobListNode> preMrpNewJobList = new ();
        foreach (JobProducts jobProducts in a_jobProductsList)
        {
            decimal qtyProducedByAllMosInJob = 0;
            foreach (Product prod in jobProducts.Products)
            {
                if (prod.Inventory != null && prod.Inventory.Id == a_inv.Id)
                {
                    qtyProducedByAllMosInJob += prod.RemainingOutputQty;
                }
            }

            if (ScenarioOptions.IsStrictlyGreaterThanZero(qtyProducedByAllMosInJob))
            {
                SSimNewJobListNode newNode = new (jobProducts.Job);
                newNode.m_qtyAvailable = qtyProducedByAllMosInJob;
                preMrpNewJobList.Add(newNode);
            }
        }

        for (int preJobI = 0; preJobI < preMrpNewJobList.Count; preJobI++)
        {
            SSimNewJobListNode jobListNode = preMrpNewJobList[preJobI];

            foreach (ManufacturingOrder supplyingMo in jobListNode.m_job.ManufacturingOrders)
            {
                if (ScenarioOptions.IsStrictlyGreaterThanZero(r_qtyNeeded))
                {
                    if (ScenarioOptions.IsStrictlyGreaterThanZero(jobListNode.m_qtyAvailable))
                    {
                        Tuple<InternalOperation, Product> prodAndOp = supplyingMo.GetPrimaryProductAndItsOp();
                        InternalOperation op = prodAndOp.Item1;
                        if (op == null || prodAndOp.Item2.Inventory.Id != a_inv.Id)
                        {
                            continue;
                        }

                        InternalActivity ia = op.Activities.GetByIndex(0);
                        if (a_demandMR != null)
                        {
                            if (!IsAllocationAllowed(ia, a_demandMR, a_inv, true))
                            {
                                continue; //Send true to validate lot pegging
                            }
                        }
                        else if (a_negativeAdjustment is SalesOrderMrpDemand demandSod)
                        {
                            if (!IsAllocationAllowed(ia, demandSod.Distribution, a_inv, true))
                            {
                                continue; //Send true to validate lot pegging
                            }
                        }

                        if (jobListNode.m_job.Commitment != JobDefs.commitmentTypes.Released && !jobListNode.m_allocatedSome) //if already allocated then that would be to an earlier adjustment
                        {
                            SetJobNeedDate(jobListNode.m_job, a_adjDate.Ticks);
                        }

                        if (jobListNode.m_qtyAvailable > r_qtyNeeded)
                        {
                            jobListNode.m_qtyAvailable -= r_qtyNeeded;
                            jobListNode.m_allocatedSome = true;
                            r_qtyNeeded = 0;
                        }
                        else if (allowPartialSupply)
                        {
                            r_qtyNeeded -= jobListNode.m_qtyAvailable;
                            jobListNode.m_allocatedSome = true;
                            jobListNode.m_qtyAvailable = 0;
                        }
                    }
                }
            }
        }

        return !ScenarioOptions.IsStrictlyGreaterThanZero(r_qtyNeeded);
    }

    /// <summary>
    /// Return SupplyOrders based on the demand material allocation
    /// Either Ascending or Descending by creation order
    /// </summary>
    private IEnumerator<SupplyOrder> GetSupplyOrderEnumerator(ItemDefs.MaterialAllocation a_materialAllocation, bool a_openBatches)
    {
        bool reverse = a_materialAllocation == ItemDefs.MaterialAllocation.UseOldestFirst;

        if (reverse)
        {
            for (int i = m_availableSupplyOrders.Count - 1; i >= 0; i--)
            {
                SupplyOrder availableSupplyOrder = m_availableSupplyOrders[i];
                if (availableSupplyOrder.IsBatchingClosed != a_openBatches)
                {
                    yield return availableSupplyOrder;
                }
            }
        }
        else
        {
            foreach (SupplyOrder order in m_availableSupplyOrders)
            {
                if (order.IsBatchingClosed != a_openBatches)
                {
                    yield return order;
                }
            }
        }
    }

    /// <summary>
    /// Inventory's Adjustment array can have multiple adjustments for the same material requirement multiple sources are supplying
    /// material to it. This can cause issues with MRP. This function merges any adjustments that are made for the same Reason and
    /// on the same date.
    /// </summary>
    /// <param name="a_adjustments"></param>
    /// <param name="a_inv"></param>
    /// <param name="a_includeLotAdjustments">whether to filter out demands that need to be satisfied from a specific lot (and therefore MRP shouldn't create supplies for)</param>
    /// <param name="a_sdOptimizeSettings"></param>
    /// <returns></returns>
    internal List<MrpDemand> GenerateMrpDemands(AdjustmentArray a_adjustments, Inventory a_inv, bool a_includeLotAdjustments, bool a_useJitAdjustedDate, OptimizeSettings a_sdOptimizeSettings = null)
    {
        OptimizeSettings optimizeSettings = m_activeOptimizeSettings;

        if (m_activeOptimizeSettings == null && a_sdOptimizeSettings != null)
        {
            optimizeSettings = a_sdOptimizeSettings;
        }

        Dictionary<BaseId, List<Adjustment>> adjByReasonOnly = new ();
        List<MrpDemand> demands = new();

        foreach (Adjustment adj in GetUsableAdjustments(a_adjustments, a_inv, a_includeLotAdjustments))
        {
            DateTime adjDate = adj.AdjDate;
            if (a_useJitAdjustedDate && adj is ActivityAdjustment actAdj3)
            {
                adjDate = actAdj3.JitAdjustedUsage;
            }

            if (a_inv.Item.CanBatch) //If batching, allow multiple demands to be created at different times for the same demand. For example by cycle demand
            {
                if (CreateDemand(adj, adjDate, adj.ChangeQty, a_inv) is MrpDemand newDemand)
                {
                    demands.Add(newDemand);
                }
            }
            else
            {
                // We aren't batching, so qty that is needed over several time points would create several individual job demands
                // Only store a single time.
                BaseId reasonId = adj.GetReason().Id;
                if (!adjByReasonOnly.TryGetValue(reasonId, out List<Adjustment> adjusts))
                {
                    adjusts = new List<Adjustment>();
                    adjByReasonOnly[reasonId] = adjusts;
                }
                adjusts.Add(adj);
            }
        }

        if (adjByReasonOnly.Count > 0)
        {
            foreach (KeyValuePair<BaseId, List<Adjustment>> kv in adjByReasonOnly)
            {
                decimal qty = kv.Value.Sum(x => x.ChangeQty);
                DateTime earliestDemandDate = kv.Value.Min(a => a.AdjDate);

                Adjustment reasonAdjustment = kv.Value[0];

                if (CreateDemand(reasonAdjustment, earliestDemandDate, qty, a_inv) is MrpDemand newDemand)
                {
                    demands.Add(newDemand);
                }
            }
        }

        return demands;
    }

    private MrpDemand? CreateDemand(Adjustment a_adjustment, DateTime a_demandDate, decimal a_qty, Inventory a_inv)
    {
        if (a_adjustment is MaterialRequirementAdjustment actAdj)
        {
            //TODO: Material Requirements for the same item but different lot codes may need to be separate demands
            MaterialRequirement mr = actAdj.GetAdjMaterialRequirement(a_inv.Item.Id);
            return new ActivityMrpDemand(actAdj.Activity, mr, a_demandDate, a_qty);
        }

        if (a_adjustment is SalesOrderAdjustment distributionAdj && distributionAdj.AdjustmentType == InventoryDefs.EAdjustmentType.SalesOrderDemand)
        {
            return new SalesOrderMrpDemand(distributionAdj.Distribution, a_demandDate, a_qty);
        }
        
        if (a_adjustment is TransferOrderAdjustment todAdj && todAdj.AdjustmentType == InventoryDefs.EAdjustmentType.TransferOrderDemand)
        {
            return new TransferOrderMrpDemand(todAdj.TransferOrderDist, a_demandDate, a_qty);
        }

        if (a_adjustment is SafetyStockMrpAdjustment safetyStockAdj)
        {
            return new SafetyStockMrpDemand(a_inv, a_demandDate, a_qty);
        }

        if (a_adjustment is ForecastMrpDemandAdjustment forecastAdj)
        {
            return new ForecastMrpDemand(forecastAdj.Shipment, a_demandDate, a_qty);
        }
        
        return null;
    }

    private IEnumerable<Adjustment> GetUsableAdjustments(AdjustmentArray a_adjustments, Inventory a_inv, bool a_includeLotAdjustments)
    {
        for (int i = 0; i < a_adjustments.Count; i++)
        {
            Adjustment adj = a_adjustments[i];

            if (!a_includeLotAdjustments)
            {
                if (adj is SalesOrderAdjustment sodAdjustment && sodAdjustment.Distribution.EligibleLots.Count > 0 && !ValidPreservedJob(sodAdjustment.Distribution))
                {
                    continue;
                }

                if (adj is PurchaseOrderAdjustment poAdj && !string.IsNullOrEmpty(poAdj.PurchaseOrder.LotCode))
                {
                    continue;
                }

                if (adj is MaterialRequirementAdjustment mrAdj && mrAdj.Material.MustUseEligLot)
                {
                    if (!mrAdj.Material.MultipleWarehouseSupplyAllowed)
                    {
                        //We are only using a single warehouse, and this inventory has already been pegged
                        continue;
                    }

                    //Check whether the required lots are MRP lots
                    bool isNonMRPLot = false;
                    Dictionary<string, EligibleLot>.Enumerator lots = mrAdj.Material.EligibleLots.GetEligibleLotsEnumerator();
                    while (lots.MoveNext())
                    {
                        if (!lots.Current.Key.StartsWith("MRP"))
                        {
                            //This is pegged to a non MRP lot.
                            isNonMRPLot = true;
                        }
                    }

                    if (isNonMRPLot)
                    {
                        continue;
                    }
                }

                //TODO: What is all of this?
                if (adj is ActivityAdjustment actAdj && actAdj.GetAdjProduct(a_inv.Item.Id) is Product p && !string.IsNullOrEmpty(p.LotCode) && !ValidPreservedJob(p.LotCode, null, actAdj.Activity))
                {
                    continue;
                }
            }

            yield return adj;
        }
    }

    /// <summary>
    /// For sales orders linked to lots, validates that all lot codes link to an existing valid preserved job
    /// </summary>
    /// <param name="sod"></param>
    /// <returns></returns>
    private bool ValidPreservedJob(SalesOrderLineDistribution sod)
    {
        bool validJob = true;
        bool jobsExist = false;

        Dictionary<string, EligibleLot>.Enumerator eligibleLotsEnumerator = sod.EligibleLots.GetEligibleLotsEnumerator();
        while (eligibleLotsEnumerator.MoveNext())
        {
            Job job = JobManager.GetByName(eligibleLotsEnumerator.Current.Key);
            if (job != null)
            {
                jobsExist = true;

                if (!ValidPreservedJob(eligibleLotsEnumerator.Current.Key, job))
                {
                    validJob = false;
                }
            }
        }

        return jobsExist && validJob;
    }

    /// <summary>
    /// For jobs with matching lot code will determine if job is valid based on its preservation settings
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <param name="a_job"></param>
    /// <param name="a_adjReason"></param>
    /// <returns></returns>
    private bool ValidPreservedJob(string a_lotCode, Job a_job = null, BaseIdObject a_adjReason = null)
    {
        bool validJob = false;

        Job job = null;

        if (a_adjReason != null && a_adjReason is InternalActivity act)
        {
            job = act.Job;
        }
        else if (a_job != null)
        {
            job = a_job;
        }

        //Confirm valid lot match to job name
        if (!string.IsNullOrEmpty(a_lotCode) && a_lotCode != job.ExternalId)
        {
            return validJob;
        }

        //Verify job preservation setting match
        if (m_activeOptimizeSettings.PreserveAnchoredJobs)
        {
            if (!validJob)
            {
                validJob = job.Anchored != anchoredTypes.Free;
            }
        }

        if (m_activeOptimizeSettings.PreserveEstimateJobs)
        {
            if (!validJob)
            {
                validJob = job.Commitment == JobDefs.commitmentTypes.Estimate;
            }
        }

        if (m_activeOptimizeSettings.PreserveFirmJobs)
        {
            if (!validJob)
            {
                validJob = job.Firm;
            }
        }

        if (m_activeOptimizeSettings.PreserveReleasedJobs)
        {
            if (!validJob)
            {
                validJob = job.Released;
            }
        }

        if (m_activeOptimizeSettings.PreservePlannedJobs)
        {
            if (!validJob)
            {
                validJob = job.Commitment == JobDefs.commitmentTypes.Planned;
            }
        }

        if (m_activeOptimizeSettings.PreserveMrpGeneratedJobs)
        {
            if (!validJob)
            {
                validJob = job.MaintenanceMethod == JobDefs.EMaintenanceMethod.MrpGenerated;
            }
        }

        if (m_activeOptimizeSettings.PreserveJobsInStableSpan)
        {
            if (!validJob)
            {
                validJob = job.InStableSpan();
            }
        }

        if (m_activeOptimizeSettings.PreserveCtpJobs)
        {
            if (!validJob)
            {
                validJob = job.CTP;
            }
        }

        if (m_activeOptimizeSettings.PreserveLockedJobs)
        {
            if (!validJob)
            {
                validJob = job.Locked != lockTypes.Unlocked;
            }
        }

        if (m_activeOptimizeSettings.PreservePrintedJobs)
        {
            if (!validJob)
            {
                validJob = job.Printed;
            }
        }

        if (m_activeOptimizeSettings.NetChangeMRP)
        {
            if (!validJob)
            {
                ManufacturingOrder mo = job.GetFirstMO();
                Product product = mo.GetFirstProductInCurrentPath();
                validJob = product != null && product.Inventory != null && !product.Inventory.IncludeInNetChangeMRP;
            }
        }

        return validJob;
    }

    public bool IsAllocationAllowed(PurchaseToStock a_supplyingPO, MaterialRequirement a_demandMr, Inventory a_inv, bool a_originallyLotPegged)
    {
        bool? isEligibleOverride = ExtensionController.IsEligibleInventory(this, Clock, a_demandMr, a_inv, null);
        if (isEligibleOverride.HasValue)
        {
            return isEligibleOverride.Value;
        }

        if (!a_originallyLotPegged)
        {
            //This adjustment did not have a lot code before MRP processing. We don't need to check if the demand can use the material
            //It's possible that a lot code was added when pegged to another supply during MRP.
            return true;
        }

        string lotCode = a_supplyingPO.LotCode;
        if (string.IsNullOrEmpty(lotCode))
        {
            if (a_demandMr.GetEligibleLotCount() == 0)
            {
                return true; // neither of supply or demand has lot codes. Allow allocation
            }

            return false;
        }

        bool isEligible = false;
        Dictionary<string, EligibleLot>.Enumerator eligibleLotsEnumerator = a_demandMr.GetEligibleLotsEnumerator();
        while (eligibleLotsEnumerator.MoveNext())
        {
            if (string.Equals(eligibleLotsEnumerator.Current.Key, lotCode))
            {
                isEligible = true;
                break;
            }
        }

        return isEligible;
    }

    public bool IsAllocationAllowed(PurchaseToStock a_supplyingPO, SalesOrderLineDistribution a_demandSod, Inventory a_inv, bool a_originallyLotPegged)
    {
        bool? isEligibleOverride = ExtensionController.IsEligibleInventory(this, Clock, a_demandSod, a_inv, null);
        if (isEligibleOverride.HasValue)
        {
            return isEligibleOverride.Value;
        }
        
        if (!a_originallyLotPegged)
        {
            //This adjustment did not have a lot code before MRP processing. We don't need to check if the demand can use the material
            //It's possible that a lot code was added when pegged to another supply during MRP.
            return true;
        }

        string lotCode = a_supplyingPO.LotCode;
        if (string.IsNullOrEmpty(lotCode))
        {
            if (a_demandSod.EligibleLots.Count == 0)
            {
                return true; // neither of supply or demand has lot codes. Allow allocation
            }

            return false;
        }

        bool isEligible = false;
        Dictionary<string, EligibleLot>.Enumerator eligibleLotsEnumerator = a_demandSod.EligibleLots.GetEligibleLotsEnumerator();
        while (eligibleLotsEnumerator.MoveNext())
        {
            if (string.Equals(eligibleLotsEnumerator.Current.Key, lotCode))
            {
                isEligible = true;
                break;
            }
        }

        return isEligible;
    }

    public bool IsAllocationAllowed(InternalActivity a_supplyingActivity, MaterialRequirement a_demandMr, Inventory a_inv, bool a_originallyLotPegged)
    {
        bool? isEligibleOverride = ExtensionController.IsEligibleInventory(this, Clock, a_supplyingActivity, a_inv);
        if (isEligibleOverride.HasValue)
        {
            return isEligibleOverride.Value;
        }

        if (!a_originallyLotPegged)
        {
            //This adjustment did not have a lot code before MRP processing. We don't need to check if the demand can use the material
            //It's possible that a lot code was added when pegged to another supply during MRP.
            return true;
        }

        string lotCode = a_supplyingActivity.Operation.Products.GetByInventoryId(a_inv.Id)?.LotCode;
        if (string.IsNullOrEmpty(lotCode))
        {
            if (a_demandMr.GetEligibleLotCount() == 0)
            {
                return true; // neither of supply or demand has lot codes. Allow allocation
            }

            return false;
        }

        bool isEligible = false;
        Dictionary<string, EligibleLot>.Enumerator eligibleLotsEnumerator = a_demandMr.GetEligibleLotsEnumerator();
        while (eligibleLotsEnumerator.MoveNext())
        {
            if (string.Equals(eligibleLotsEnumerator.Current.Key, lotCode))
            {
                isEligible = true;
                break;
            }
        }

        return isEligible;
    }

    public bool IsAllocationAllowed(InternalActivity a_supplyingActivity, SalesOrderLineDistribution a_demandSod, Inventory a_inv, bool a_originallyLotPegged)
    {
        bool? isEligibleOverride = ExtensionController.IsEligibleInventory(this, Clock, a_supplyingActivity, a_inv);
        if (isEligibleOverride.HasValue)
        {
            return isEligibleOverride.Value;
        }
        
        if (!a_originallyLotPegged)
        {
            //This adjustment did not have a lot code before MRP processing. We don't need to check if the demand can use the material
            //It's possible that a lot code was added when pegged to another supply during MRP.
            return true;
        }

        string lotCode = a_supplyingActivity.Operation.Products.GetByInventoryId(a_inv.Id)?.LotCode;
        if (string.IsNullOrEmpty(lotCode))
        {
            if (a_demandSod.EligibleLots.Count == 0)
            {
                return true; // neither of supply or demand has lot codes. Allow allocation
            }

            return false;
        }

        bool isEligible = false;
        Dictionary<string, EligibleLot>.Enumerator eligibleLotsEnumerator = a_demandSod.EligibleLots.GetEligibleLotsEnumerator();
        while (eligibleLotsEnumerator.MoveNext())
        {
            if (string.Equals(eligibleLotsEnumerator.Current.Key, lotCode))
            {
                isEligible = true;
                break;
            }
        }

        return isEligible;
    }

    private bool IsAllocationAllowed(Lot a_lot, MrpDemand a_negativeAdj, Inventory a_inv)
    {
        EligibleLots eligibleLots = null;
        MaterialRequirement demandMR = a_negativeAdj is ActivityMrpDemand actAdj ? actAdj.MR : null;
        if (demandMR != null)
        {
            bool? isEligibleOverride = ExtensionController.IsEligibleInventory(this, Clock, demandMR, a_inv, a_lot);
            if (isEligibleOverride.HasValue)
            {
                return isEligibleOverride.Value;
            }

            eligibleLots = demandMR.EligibleLots;
        }
        else if (a_negativeAdj is SalesOrderMrpDemand sod)
        {
            eligibleLots = sod.Distribution.EligibleLots;
        }
        else if (a_negativeAdj is TransferOrderMrpDemand tod)
        {
            eligibleLots = tod.Distribution.EligibleLots;
        }

        string lotCode = a_lot.Code;
        if (string.IsNullOrEmpty(lotCode)) // supply is not pegged to anything
        {
            if (eligibleLots == null || eligibleLots.Count == 0) // demand isn't expecting a specific lot
            {
                return true;
            }

            Dictionary<string, EligibleLot>.Enumerator elEnumerator = eligibleLots.GetEligibleLotsEnumerator();
            while (elEnumerator.MoveNext())
            {
                if (!elEnumerator.Current.Key.StartsWith("MRP"))
                {
                    return false; // demand requires a specific lot that wasn't set by MRP.
                }
            }

            return true; // supply not pegged and all demand lot codes were set by MRP.
        }

        if (eligibleLots == null)
        {
            return true;
        }

        if (lotCode.StartsWith("MRP") && eligibleLots.Count == 0)
        {
            //This lot was previously pegged
            if (a_lot.LotSource == ItemDefs.ELotSource.Inventory)
            {
                //This lot is on-hand, it can be reused since it isn't part of an MRP batch
                return true;
            }
        }

        Dictionary<string, EligibleLot>.Enumerator eligibleLotsEnumerator = eligibleLots.GetEligibleLotsEnumerator();
        while (eligibleLotsEnumerator.MoveNext())
        {
            if (string.Equals(eligibleLotsEnumerator.Current.Key, lotCode))
            {
                return true;
            }
        }

        return false;
    }

    private Job CreateSupply(SupplyOrder a_supplyOrder, List<JobProducts> a_jobProductsList, List<Job> a_newJobList, MaterialRequirementOriginalValueList a_mrOriginalValueList, PTTransmission a_t, Dictionary<string, Dictionary<BaseId, decimal>> a_successorMos, int a_lowLevelCode)
    {
        Job j = CreateMrpJob(a_supplyOrder, a_successorMos, a_lowLevelCode);
        AllocateExcessProduction(a_supplyOrder);
        PegSupplyAndDemand(j, a_supplyOrder, a_mrOriginalValueList);
        if (m_extensionController.RunJobCreationExtensions)
        {
            ChangableJobValues changableJobValues = null;
            j = m_extensionController.CustomizeMrpJob(this, a_supplyOrder, j, ref changableJobValues, a_t);

            if (j != null && changableJobValues != null)
            {
                List<Tuple<ManufacturingOrder, Plant>>.Enumerator etr = changableJobValues.GetLockedPlantsEnumerator();
                while (etr.MoveNext())
                {
                    etr.Current.Item1.LockedPlant = etr.Current.Item2;
                }

                List<Tuple<InternalOperation, Resource>>.Enumerator drEtr = changableJobValues.GetDefaultResourcesEnumerator();
                while (drEtr.MoveNext())
                {
                    ResourceRequirement primaryRr = drEtr.Current.Item1.ResourceRequirements.PrimaryResourceRequirement;
                    ResourceRequirement.DefaultResourceValues defaultValues = new (primaryRr);
                    primaryRr.DefaultResource_Set(drEtr.Current.Item2, defaultValues.m_useJITLimitTicks, defaultValues.m_jitLimitTicks);
                }

                j.ComputeEligibility(ProductRuleManager);
            }
        }

        decimal qtyProduced = j.GetPrimaryProduct().TotalOutputQty;
        a_supplyOrder.SupplyGenerated(qtyProduced);

        a_jobProductsList.Add(new JobProducts(j));
        a_newJobList.Add(j);

        //Order completed
        m_availableSupplyOrders.Remove(a_supplyOrder);

        return j;
    }

    private void CreateSupply(SupplyOrder a_supplyOrder, List<PurchaseToStock> a_newPOList, Dictionary<BaseId, Tuple<PurchaseToStock, List<InternalActivity>>> a_poDemands)
    {
        PurchaseToStock pts = CreatePO(a_supplyOrder.Inventory);
        SetPODate(pts, a_supplyOrder.NeedDateTicks);
        bool valid;
        pts.QtyOrdered = a_supplyOrder.GetOrderQty(out valid);
        EnforceMinOrderQty(pts, a_supplyOrder.Inventory);

        pts.MaintenanceMethod = PurchaseToStockDefs.EMaintenanceMethod.MrpGenerated;
        foreach (DemandPart demandPart in a_supplyOrder.DemandParts)
        {
            AddPoDemand(pts, demandPart.Qty, demandPart.Demand.DemandDate.Ticks, demandPart.Demand);
            if (demandPart.Demand is ActivityMrpDemand actAdj)
            {
                InternalActivity demandAct = actAdj.Activity;
                if (a_poDemands.ContainsKey(pts.Id))
                {
                    a_poDemands[pts.Id].Item2.Add(demandAct);
                }
                else
                {
                    a_poDemands.Add(pts.Id, new Tuple<PurchaseToStock, List<InternalActivity>>(pts, new List<InternalActivity> { demandAct }));
                }

                if (actAdj.LotControlled)
                {
                    PegSupplyAndDemand(pts, demandAct, a_supplyOrder.Inventory);
                }
            }
            else if (demandPart.Demand is SalesOrderMrpDemand demandSod)
            {
                if (demandSod.LotControlled)
                {
                    PegSupplyAndDemand(pts, demandSod.Distribution, a_supplyOrder.Inventory);
                }
            }
        }

        if (m_extensionController.RunMrpExtension)
        {
            ChangeablePoValues changedValues = null;
            PurchaseToStock modifiedPo = m_extensionController.CustomizeMrpPurchaseOrder(this, a_supplyOrder, pts, ref changedValues);
            if (modifiedPo != null)
            {
                pts = modifiedPo;
            }

            if (changedValues != null)
            {
                if (changedValues.BuyerExternalId != null)
                {
                    pts.BuyerExternalId = changedValues.BuyerExternalId;
                }

                if (changedValues.Closed.HasValue)
                {
                    pts.Closed = changedValues.Closed.Value;
                }

                if (changedValues.Firm.HasValue)
                {
                    pts.Firm = changedValues.Firm.Value;
                }

                if (changedValues.LotCode != null)
                {
                    pts.LotCode = changedValues.LotCode;
                }

                if (changedValues.NeedDate.HasValue)
                {
                    pts.NeedDate = changedValues.NeedDate.Value;
                }

                if (changedValues.QtyOrdered.HasValue)
                {
                    pts.QtyOrdered = changedValues.QtyOrdered.Value;
                }

                if (changedValues.QtyReceived.HasValue)
                {
                    pts.QtyReceived = changedValues.QtyReceived.Value;
                }

                if (changedValues.VendorExternalId != null)
                {
                    pts.VendorExternalId = changedValues.VendorExternalId;
                }

                if (changedValues.TransferSpan.HasValue)
                {
                    pts.TransferSpan = changedValues.TransferSpan.Value;
                }
            }
        }

        a_supplyOrder.SupplyGenerated(pts.QtyOrdered);

        a_newPOList.Add(pts);
    }

    private void PegSupplyAndDemand(Job a_generatedJob, SupplyOrder a_supplyOrder, MaterialRequirementOriginalValueList a_mrOriginalValueList)
    {
        if (!m_activeMrpSettings.EnableLotPegging && (a_supplyOrder.DemandParts.Count <= 0 || a_supplyOrder.DemandParts.All(d => !d.Demand.LotControlled)))
        {
            return;
        }

        string lotCode = a_generatedJob.ExternalId;
        Dictionary<BaseId, ManufacturingOrder> predMos = new ();
        foreach (ManufacturingOrder mo in a_generatedJob.ManufacturingOrders)
        {
            foreach (Product p in mo.GetProducts(false))
            {
                if (p.Inventory.Id != a_supplyOrder.Inventory.Id)
                {
                    continue;
                }

                p.SetLotCode(lotCode);
                predMos[mo.Id] = mo;
            }
        }

        Dictionary<BaseId, ManufacturingOrder> successorMos = new ();
        foreach (DemandPart dp in a_supplyOrder.DemandParts)
        {
            if (dp.Demand is ActivityMrpDemand actAdj)
            {
                InternalActivity ia = actAdj.Activity;
                successorMos[ia.Operation.ManufacturingOrder.Id] = ia.Operation.ManufacturingOrder;
                List<MaterialRequirement> materialRequirements = ia.Operation.MaterialRequirements.StockMaterials.ToList();
                foreach (MaterialRequirement mr in materialRequirements)
                {
                    if (mr.MaintenanceMethod == JobDefs.EMaintenanceMethod.MrpGenerated || mr.Item.Id != a_supplyOrder.Inventory.Item.Id)
                    {
                        continue;
                    }

                    if (mr.AllowPartialSupply)
                    {
                        if (mr.UnIssuedQty > dp.Qty)
                        {
                            //This material requirement is not pulling the full qty from this source. We need to break off a portion
                            MaterialRequirement newMr = ia.Operation.MaterialRequirements.SplitOffNewSupplyPeggingForMRP(mr.Id, dp.Qty);
                            a_mrOriginalValueList.CopyMr(ia.Operation, mr, newMr);
                        }

                        //The original MR is the one pegged to the demand. The split is the remainder
                        mr.AddEligibleLotCode(lotCode);
                    }
                    else
                    {
                        mr.ClearEligibleLots();
                        mr.AddEligibleLotCode(lotCode);
                    }
                }
            }
            else if (dp.Demand is SalesOrderMrpDemand sod)
            {
                sod.Distribution.EligibleLots.Add(lotCode, sod.Distribution);
            }
        }

        //LinkBySuccessors(predMos, successorMos);
    }

    private void LinkBySuccessors(Dictionary<BaseId, ManufacturingOrder> a_predMos, Dictionary<BaseId, ManufacturingOrder> a_successorMos)
    {
        foreach (ManufacturingOrder predMo in a_predMos.Values)
        {
            foreach (ManufacturingOrder succMo in a_successorMos.Values)
            {
                predMo.SuccessorMOs.Add(new SuccessorMO($"{predMo.ExternalId}-{succMo.ExternalId}", predMo, succMo, null, null), this);
            }
        }
    }

    private void PegSupplyAndDemand(InternalActivity a_supplyAct, InternalActivity a_demandAct, Inventory a_inv)
    {
        Job supplyJob = a_supplyAct.Job;
        string lotCode = supplyJob.ExternalId;
        if (!lotCode.StartsWith("MRP"))
        {
            lotCode = string.Concat("MRP ", lotCode);
        }

        bool lotCodeSet = false;
        for (int i = 0; i < a_demandAct.Operation.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_demandAct.Operation.MaterialRequirements[i];
            if (mr.Item?.Id == a_inv.Item.Id)
            {
                if (mr.AllowPartialSupply)
                {
                    mr.AddEligibleLotCode(lotCode);
                    lotCodeSet = true;
                }
                else if (mr.GetEligibleLotCount() == 0) // allow partial is not allowed. Lot code is already set, don't reset it.
                {
                    mr.AddEligibleLotCode(lotCode);
                    lotCodeSet = true;
                }

                break; // found the material we were looking for.
            }
        }

        if (!lotCodeSet)
        {
            return;
        }

        foreach (ManufacturingOrder mo in supplyJob.ManufacturingOrders)
        {
            foreach (Product p in mo.GetProducts(false))
            {
                if (p.Inventory.Id == a_inv.Id)
                {
                    p.SetLotCode(lotCode);
                    break; // found the product we were looking for.
                }
            }
        }

        //Now that we pegged the supply, we need to reset its need date because it could have been reset for a temporary optimize pegging.
        List<(InternalActivity, Product)> suppliers = new ();
        Product linkedProduct = a_supplyAct.Operation.Products.GetByItemId(a_inv.Item.Id);
        suppliers.Add((a_supplyAct, linkedProduct));

        supplyJob.UpdateSubJobSettings(Clock, a_demandAct, ScenarioOptions.SetSubJobHotFlags, ScenarioOptions.SetSubJobNeedDatePoint, ScenarioOptions.SetSubJobPriorities, out bool _, ScenarioOptions, m_activeMrpSettings.ScenarioDataChanges, suppliers);
    }

    private void PegSupplyAndDemand(Job a_supplyJob, SalesOrderLineDistribution a_demandSod, Inventory a_inv)
    {
        if (a_demandSod.EligibleLots.Count > 0)
        {
            return; // don't override this
        }

        string lotCode = a_supplyJob.ExternalId;
        a_demandSod.EligibleLots.SetEligibleLots(new HashSet<string> { lotCode }, a_demandSod);
        foreach (ManufacturingOrder mo in a_supplyJob.ManufacturingOrders)
        {
            foreach (Product p in mo.GetProducts(false))
            {
                if (p.Inventory.Id == a_inv.Id)
                {
                    p.SetLotCode(lotCode);
                    break; // found the product we were looking for.
                }
            }
        }
    }

    private void PegSupplyAndDemand(PurchaseToStock a_po, InternalActivity a_demandAct, Inventory a_inv)
    {
        if (a_po == null)
        {
            return;
        }

        string lotCode = a_po.ExternalId;
        if (!lotCode.StartsWith("MRP"))
        {
            lotCode = string.Concat("MRP ", lotCode);
        }

        bool lotCodeSet = false;
        for (int i = 0; i < a_demandAct.Operation.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = a_demandAct.Operation.MaterialRequirements[i];
            if (mr.Item?.Id == a_inv.Item.Id)
            {
                if (mr.AllowPartialSupply)
                {
                    mr.AddEligibleLotCode(lotCode);
                    lotCodeSet = true;
                }
                else if (mr.GetEligibleLotCount() == 0) // allow partial is not allowed. Lot code is already set, don't reset it.
                {
                    mr.AddEligibleLotCode(lotCode);
                    lotCodeSet = true;
                }

                break; // found the material we were looking for.
            }
        }

        if (!lotCodeSet)
        {
            return;
        }

        a_po.LotCode = lotCode;
    }

    private void PegSupplyAndDemand(PurchaseToStock a_po, SalesOrderLineDistribution a_demandSod, Inventory a_inv)
    {
        if (a_demandSod.EligibleLots.Count > 0 || a_po == null)
        {
            return;
        }

        string lotCode = a_po.ExternalId;
        if (!lotCode.StartsWith("MRP"))
        {
            lotCode = string.Concat("MRP ", lotCode);
        }

        a_demandSod.EligibleLots.SetEligibleLots(new HashSet<string> { lotCode }, a_demandSod);
        a_po.LotCode = lotCode;
    }

    private void PegSupplyAndDemand(Lot a_lot, MrpDemand a_negAdj, Inventory a_inv)
    {
        if (!a_negAdj.LotControlled)
        {
            return;
        }

        string lotCode = string.IsNullOrWhiteSpace(a_lot.Code) ? a_lot.ExternalId : a_lot.Code;
        if (!lotCode.StartsWith("MRP"))
        {
            // prepend "MRP" so we know this was set by MRP and clear it out before the next run.
            lotCode = "MRP" + lotCode;
        }

        bool lotCodeSet = false;

        if (a_negAdj is SalesOrderMrpDemand sod)
        {
            sod.Distribution.EligibleLots.Add(lotCode, sod.Distribution);
            lotCodeSet = true;
        }
        else if (a_negAdj is ActivityMrpDemand actAdj)
        {
            for (int i = 0; i < actAdj.Activity.Operation.MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = actAdj.Activity.Operation.MaterialRequirements[i];
                if (mr.Item?.Id == a_inv.Item.Id)
                {
                    if (mr.AllowPartialSupply)
                    {
                        mr.AddEligibleLotCode(lotCode);
                        lotCodeSet = true;
                    }
                    else if (mr.GetEligibleLotCount() == 0) // allow partial is not allowed. Lot code is already set, don't reset it.
                    {
                        mr.AddEligibleLotCode(lotCode);
                        lotCodeSet = true;
                    }

                    //TODO: What do we do here?
                    break; // found the material we were looking for.
                }
            }
        }


        if (!lotCodeSet)
        {
            return;
        }

        a_lot.Code = lotCode;
    }

    private Job CreateMrpJob(SupplyOrder a_supplyOrder, Dictionary<string, Dictionary<BaseId, decimal>> a_successorMos, int a_lowLevelCode)
    {
        Job jobCopy = JobManager.Copy(a_supplyOrder.Inventory.TemplateManufacturingOrder.Job);
        jobCopy.LowLevelCode = a_lowLevelCode;
        bool valid;
        decimal qty = ScenarioOptions.RoundQty(a_supplyOrder.GetOrderQty(out valid));
        List<decimal> moQties = SplitQty(qty, a_supplyOrder.Inventory.Item.JobAutoSplitQty);

        InitNewMrpJob(jobCopy, m_activeOptimizeSettingsT.TimeStamp, qty, moQties, a_supplyOrder.Inventory.TemplateManufacturingOrder.Job);

        UpdateSuccessorMos(a_supplyOrder, jobCopy, a_successorMos);

        jobCopy.DoNotSchedule = jobCopy.DoNotSchedule || !valid;
        jobCopy.Priority = a_supplyOrder.Priority;
        RollupAttributesIfNecessary(jobCopy);
        SetProductWarehouses(jobCopy, a_supplyOrder.Inventory.Warehouse);

        //Get all products in this job, including from any split MOs.
        List<Product> supplyingProductsList = new ();
        foreach (ManufacturingOrder mo in jobCopy.ManufacturingOrders)
        {
            //Only get current path Products
            IEnumerable<Product> supplyingProducts = mo.GetProducts(true).Where(p => p.Inventory == a_supplyOrder.Inventory);
            supplyingProductsList.AddRange(supplyingProducts);
        }

        HashSet<string> orderNums = new ();
        HashSet<string> customers = new ();
        foreach (DemandPart demandPart in a_supplyOrder.DemandParts)
        {
            CollectOrderNumberAndCustomer(demandPart.Demand, orderNums, customers);
            AddProductDemands(supplyingProductsList, demandPart.Qty, demandPart.Demand, a_supplyOrder.Inventory);

            //TODO: SafetyStock
            if (demandPart.Demand is SafetyStockMrpAdjustment)
            {
                jobCopy.Classification = JobDefs.classifications.SafetyStock;
            }
        }

        jobCopy.OrderNumber = string.Join(",", orderNums.OrderBy(orderNumber => orderNumber));
        foreach (string customerExternalId in customers)
        {
            Customer customer = CustomerManager.GetByExternalId(customerExternalId);
            if (customer != null && !jobCopy.Customers.Contains(customer.Id))
            {
                jobCopy.Customers.Add(customer);
            }
        }

        jobCopy.ComputeEligibility(ProductRuleManager); // Call this before setting qties since JIT is calculated when qties are set and requires eligibility info
        SetJobNeedDate(jobCopy, a_supplyOrder.NeedDateTicks);

        return jobCopy;
    }

    /// <summary>
    /// Update Successor MOs that point to other Mos within the template.
    /// </summary>
    private static void UpdateInternalSuccessorMos(Job a_template, Job a_newJob)
    {
        //Handle Successor MO links within the template
        for (int moIdx = 0; moIdx < a_template.ManufacturingOrders.Count; moIdx++)
        {
            ManufacturingOrder mo = a_template.ManufacturingOrders.GetByIndex(moIdx);
            for (int i = 0; i < mo.SuccessorMOs.Count; i++)
            {
                SuccessorMO successorMO = mo.SuccessorMOs[i];
                if (successorMO.SuccessorJobExternalId == mo.Job.ExternalId)
                {
                    int successorMoIdx = -1;
                    //This is an internal successor MO
                    string moExternalId = successorMO.SuccessorManufacturingOrderExternalId;
                    //Translate the externalId into an index
                    if (!string.IsNullOrEmpty(moExternalId))
                    {
                        for (int sucMoIdx = 0; sucMoIdx < a_template.ManufacturingOrders.Count; sucMoIdx++)
                        {
                            if (a_template.ManufacturingOrders.GetByIndex(sucMoIdx).ExternalId == moExternalId)
                            {
                                successorMoIdx = sucMoIdx;
                                break;
                            }
                        }
                    }

                    //Update the successor MO on the job copy
                    ManufacturingOrder moCopy = a_newJob.ManufacturingOrders.GetByIndex(moIdx);
                    SuccessorMO successorMoCopy = moCopy.SuccessorMOs[i];
                    successorMoCopy.SuccessorJobExternalId = a_newJob.ExternalId;
                    if (successorMoIdx != -1)
                    {
                        successorMoCopy.SuccessorManufacturingOrderExternalId = a_newJob.ManufacturingOrders.GetByIndex(successorMoIdx).ExternalId;
                    }
                    //TODO: Operation mapping as well
                }
            }
        }
    }

    /// <summary>
    /// If the template job is referencing any other templates as SuccessorMOs, this function
    /// will update those successor MOs JobExternalId replacing it with the new JobExternalId of the successor MRP job created from the linked template.
    /// </summary>
    /// <param name="a_supplyOrder"></param>
    /// <param name="jobCopy"></param>
    /// <param name="a_successorMos"></param>
    private static void UpdateSuccessorMos(SupplyOrder a_supplyOrder, Job jobCopy, Dictionary<string, Dictionary<BaseId, decimal>> a_successorMos)
    {
        foreach (DemandPart dp in a_supplyOrder.DemandParts)
        {
            if (dp.Demand is ActivityMrpDemand actAdj)
            {
                InternalActivity ia = actAdj.Activity;
                if (!a_successorMos.TryGetValue(ia.Job.ExternalId, out Dictionary<BaseId, decimal> succMaterials))
                {
                    Dictionary<BaseId, decimal> materials = new ();
                    for (int i = 0; i < ia.Operation.MaterialRequirements.Count; i++)
                    {
                        MaterialRequirement mr = ia.Operation.MaterialRequirements[i];
                        if (mr.Item == null || mr.Item.Id != jobCopy.GetPrimaryProduct().Item.Id)
                        {
                            continue;
                        }

                        materials.Add(mr.Item.Id, mr.TotalRequiredQty);
                    }

                    a_successorMos.Add(ia.Job.ExternalId, materials);
                }
            }
        }

        if (jobCopy.ManufacturingOrders.Any(mo => mo.SuccessorMOs.Count > 0))
        {
            foreach (ManufacturingOrder mo in jobCopy.ManufacturingOrders)
            {
                for (int succI = 0; succI < mo.SuccessorMOs.Count; succI++)
                {
                    SuccessorMO succMo = mo.SuccessorMOs[succI];

                    if (succMo?.Operation?.MaterialRequirements.Count > 0)
                    {
                        foreach (MaterialRequirement succMr in succMo.Operation.MaterialRequirements)
                        {
                            foreach (KeyValuePair<string, Dictionary<BaseId, decimal>> succJob in a_successorMos)
                            {
                                if (succJob.Value.TryGetValue(succMr.Item.Id, out decimal qty))
                                {
                                    if (qty < succMr.TotalRequiredQty)
                                    {
                                        continue;
                                    }

                                    if (qty == succMr.TotalRequiredQty)
                                    {
                                        succJob.Value.Remove(succMr.Item.Id);
                                    }
                                    else
                                    {
                                        succJob.Value[succMr.Item.Id] -= succMr.TotalRequiredQty;
                                    }

                                    succMo.SuccessorJobExternalId = succJob.Key;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// This will collect the top level order number to be stored in job.OrderNumber
    /// Adds the number to the hashset that is passed in.
    /// </summary>
    /// <param name="a_demandAdj"></param>
    /// <param name="a_orderNums"></param>
    private static void CollectOrderNumberAndCustomer(MrpDemand a_demandAdj, HashSet<string> a_orderNums, HashSet<string> a_customers)
    {
        if (a_demandAdj is ActivityMrpDemand actAdj)
        {
            InternalActivity demandAct = actAdj.Activity;
            Job job = demandAct.Operation.ManufacturingOrder.Job;
            if (!string.IsNullOrEmpty(job.OrderNumber))
            {
                List<string> currentOrderNumbers = job.OrderNumber.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                foreach (string num in currentOrderNumbers)
                {
                    a_orderNums.Add(num.Trim());
                }
            }
            else
            {
                a_orderNums.Add(demandAct.Job.Name);
            }

            if (job.Customers.Count > 0)
            {
                foreach (Customer customer in job.Customers)
                {
                    a_customers.Add(customer.ExternalId);
                }
            }
        }
        else if (a_demandAdj is SalesOrderMrpDemand demandSod)
        {
            a_orderNums.Add(demandSod.Distribution.SalesOrderLine.SalesOrder.Name);

            if (!string.IsNullOrWhiteSpace(demandSod.Distribution.SalesOrderLine.SalesOrder.Customer?.ExternalId))
            {
                a_customers.Add(demandSod.Distribution.SalesOrderLine.SalesOrder.Customer.ExternalId);
            }
        }
        else if (a_demandAdj is ForecastMrpDemand demandForecast)
        {
            Forecast forecast = demandForecast.Shipment.Forecast;
            a_orderNums.Add(forecast.Name);
            if (!string.IsNullOrWhiteSpace(forecast.Customer?.ExternalId))
            {
                a_customers.Add(forecast.Customer.ExternalId);
            }
        }
        else if (a_demandAdj is TransferOrderMrpDemand demandTo)
        {
            a_orderNums.Add(demandTo.TransferOrder.Name);
        }
    }

    private void AllocateExcessProduction(SupplyOrder a_supplyOrder)
    {
        if (a_supplyOrder.Inventory.MrpExcessQuantityAllocation == InventoryDefs.MrpExcessQuantityAllocation.None)
        {
            return;
        }

        decimal unallocatedQty = a_supplyOrder.GetUnallocatedQty();
        if (!ScenarioOptions.IsStrictlyGreaterThanZero(unallocatedQty))
        {
            return;
        }

        if (a_supplyOrder.Inventory.MrpExcessQuantityAllocation == InventoryDefs.MrpExcessQuantityAllocation.LastParentJob)
        {
            InternalActivity act = null;
            for (int i = a_supplyOrder.DemandParts.Count - 1; i >= 0; i--)
            {
                if (a_supplyOrder.DemandParts[i].Demand is ActivityMrpDemand actAdj)
                {
                    act = actAdj.Activity;
                    break;
                }
            }

            if (act == null)
            {
                return; // can only scale up demands that are Jobs.
            }

            ScaleUpJob(a_supplyOrder, act, unallocatedQty);
        }
        else if (a_supplyOrder.Inventory.MrpExcessQuantityAllocation == InventoryDefs.MrpExcessQuantityAllocation.AllParentJobsEqually)
        {
            List<InternalActivity> actsToAllocate = new ();
            foreach (DemandPart dp in a_supplyOrder.DemandParts)
            {
                if (dp.Demand is ActivityMrpDemand actAdj)
                {
                    actsToAllocate.Add(actAdj.Activity);
                }
            }

            if (actsToAllocate.Count == 0)
            {
                return;
            }

            decimal qtyToAllocate = ScenarioOptions.RoundQty(unallocatedQty / actsToAllocate.Count);
            int i = 0;
            foreach (InternalActivity act in actsToAllocate)
            {
                if (i == actsToAllocate.Count - 1) // last activity, allocate the remaining.
                {
                    ScaleUpJob(a_supplyOrder, act, unallocatedQty);
                }
                else
                {
                    ScaleUpJob(a_supplyOrder, act, qtyToAllocate);
                    unallocatedQty -= qtyToAllocate;
                }

                i++;
            }
        }
        else if (a_supplyOrder.Inventory.MrpExcessQuantityAllocation == InventoryDefs.MrpExcessQuantityAllocation.AllParentJobsProportionally)
        {
            List<InternalActivity> actsToAllocate = new ();
            decimal totalQty = 0;
            foreach (DemandPart dp in a_supplyOrder.DemandParts)
            {
                if (dp.Demand is ActivityMrpDemand actAdj)
                {
                    InternalActivity activity = actAdj.Activity;
                    actsToAllocate.Add(activity);
                    totalQty += activity.Operation.ManufacturingOrder.RequiredQty;
                }
            }

            if (actsToAllocate.Count == 0 || totalQty <= 0)
            {
                return;
            }

            int i = 0;
            foreach (InternalActivity act in actsToAllocate)
            {
                if (i == actsToAllocate.Count - 1) // last activity, allocate the remaining
                {
                    ScaleUpJob(a_supplyOrder, act, unallocatedQty);
                }
                else
                {
                    decimal qtyToAllocate = ScenarioOptions.RoundQty(unallocatedQty * (act.Operation.ManufacturingOrder.RequiredQty / totalQty));
                    ScaleUpJob(a_supplyOrder, act, qtyToAllocate);
                    unallocatedQty -= qtyToAllocate;
                }

                i++;
            }
        }
    }

    /// <summary>
    /// Scale a Job by a qty so it's Material will consume a_qtyToAllocate
    /// </summary>
    /// <param name="a_mo"></param>
    /// <param name="a_qtyToAllocate"></param>
    private void ScaleUpJob(SupplyOrder a_supplyOrder, InternalActivity a_act, decimal a_qtyToAllocate)
    {
        MaterialRequirement demandMR = null;
        for (int j = 0; j < a_act.Operation.MaterialRequirements.Count; j++)
        {
            MaterialRequirement mr = a_act.Operation.MaterialRequirements[j];
            if (mr.Item != null && mr.Item.Id == a_supplyOrder.Inventory.Item.Id)
            {
                demandMR = mr;
                break;
            }
        }

        if (demandMR == null)
        {
            DebugException.ThrowInDebug("ScaleUpDemands. MaterialRequirement not found.".Localize());
            return;
        }

        decimal qtyToIncreaseBy = ScenarioOptions.RoundQty(a_act.ManufacturingOrder.RequiredQty * a_qtyToAllocate / demandMR.TotalRequiredQty);
        a_act.ManufacturingOrder.SetRequiredQty(Clock, a_act.ManufacturingOrder.RequiredQty + qtyToIncreaseBy, ProductRuleManager);
        a_act.ManufacturingOrder.Job.Notes = $"MRP increased the quantity of this Job by '{qtyToIncreaseBy}' to consume excess production.".Localize();
        a_supplyOrder.SupplyGenerated(-qtyToIncreaseBy);
    }

    private void InitNewMrpJob(Job a_newJob, DateTimeOffset a_entryDate, decimal a_totalQty, List<decimal> a_moQties, Job a_template)
    {
        a_newJob.Name = string.Format(Localizer.GetString("MRP {0}"), a_newJob.Id);

        a_newJob.ExternalId = a_newJob.Name;
        a_newJob.ScheduledStatus_set = JobDefs.scheduledStatuses.New;
        a_newJob.MaintenanceMethod = JobDefs.EMaintenanceMethod.MrpGenerated;
        a_newJob.EntryDate = a_entryDate;
        a_newJob.Template = false;
        a_newJob.Commitment = JobDefs.commitmentTypes.Planned;
        a_newJob.Revenue = a_newJob.Revenue * a_totalQty;

        //Now that ExternalIds are updated, map the successor MOs
        UpdateInternalSuccessorMos(a_template, a_newJob);

        //Split each MO individually. //TODO workaround for templates with multiple MOs.
        List<ManufacturingOrder> originalMos = new ();
        for (int moIdx = 0; moIdx < a_newJob.ManufacturingOrderCount; moIdx++)
        {
            originalMos.Add(a_newJob.ManufacturingOrders.GetByIndex(moIdx));
        }

        Dictionary<ManufacturingOrder, SuccessorMO> moLinksToCopy = new ();
        foreach (ManufacturingOrder mo in originalMos)
        {
            //Set qty
            mo.SetRequiredQty(Clock, a_moQties[0], ProductRuleManager);
            mo.ApplyPlannedScrapQty();

            // create MO Copies if necessary
            for (int moIdx = 1; moIdx < a_moQties.Count; moIdx++)
            {
                ManufacturingOrder newMo = a_newJob.ManufacturingOrders.AddManufacturingOrderCopy(mo);
                newMo.SetRequiredQty(Clock, a_moQties[moIdx], ProductRuleManager);
                newMo.ApplyPlannedScrapQty();

                #region Handle Internal Successor MOs
                ManufacturingOrder moWhereSuccessMosGotUpdated = null;
                //See if any any previous Mos that got copied are pointing to the Mo we just copied
                foreach (KeyValuePair<ManufacturingOrder, SuccessorMO> pair in moLinksToCopy)
                {
                    if (pair.Value.SuccessorJobExternalId == a_newJob.ExternalId && pair.Value.SuccessorManufacturingOrderExternalId == mo.ExternalId)
                    {
                        //We copied an MO that points to the one we just copied. We need to update that successor MO to point to this one
                        pair.Value.SuccessorManufacturingOrderExternalId = newMo.ExternalId;
                        mo.LinkSuccessorMOs();
                        moWhereSuccessMosGotUpdated = mo;
                    }
                }

                if (moWhereSuccessMosGotUpdated != null)
                {
                    moLinksToCopy.Remove(moWhereSuccessMosGotUpdated);
                }

                //TODO: Support multiple successor MOs
                if (newMo.SuccessorMOs.Count > 0)
                {
                    moLinksToCopy.Add(newMo, newMo.SuccessorMOs[0]);
                }
                #endregion
            }
        }
    }

    /// <summary>
    /// Copy Operation Attributs from MR Templates into the parent jobs recursively.
    /// </summary>
    private static void RollupAttributesIfNecessary(Job a_job)
    {
        List<BaseOperation> jobOpList = a_job.GetOperations();
        for (int opI = 0; opI < jobOpList.Count; opI++)
        {
            BaseOperation op = jobOpList[opI];
            //Create a hash for this Op's original Attributes
            Dictionary<string, string> originalAttributeNamesHash = new ();
            for (int aI = 0; aI < op.Attributes.Count; aI++)
            {
                originalAttributeNamesHash.Add(op.Attributes[aI].PTAttribute.ExternalId, null);
            }

            Dictionary<string, OperationAttribute> newAttributesForOp = new ();
            //Add Item Attributes for Material Requirements as necessary
            for (int mI = 0; mI < op.MaterialRequirements.Count; mI++)
            {
                MaterialRequirement mr = op.MaterialRequirements[mI];
                if (mr.Item != null && mr.Warehouse != null && mr.Item.RollupAttributesToParent && mr.Warehouse.Inventories.Contains(mr.Item.Id))
                {
                    Inventory mrInv = mr.Warehouse.Inventories[mr.Item.Id];
                    GetAttributesRecursively(mrInv.TemplateManufacturingOrder.Job, ref newAttributesForOp);
                }
            }

            //Copy Attributes from all of the Operation's Materials into the Operation
            Dictionary<string, OperationAttribute>.Enumerator newAttEnumerator = newAttributesForOp.GetEnumerator();
            while (newAttEnumerator.MoveNext())
            {
                OperationAttribute newAttribute = newAttEnumerator.Current.Value;
                if (!originalAttributeNamesHash.ContainsKey(newAttribute.PTAttribute.ExternalId))
                {
                    op.Attributes.Add(newAttribute); //don't store a reference since the attributes in the list are referenced from other Jobs.
                }
            }
        }
    }

    private static void SetProductWarehouses(Job aNewJob, Warehouse aWarehouse)
    {
        List<BaseOperation> jobOpList = aNewJob.GetOperations();
        for (int opI = 0; opI < jobOpList.Count; opI++)
        {
            BaseOperation op = jobOpList[opI];
            for (int prodI = 0; prodI < op.Products.Count; prodI++)
            {
                Product product = op.Products[prodI];

                if (product.SetWarehouseDuringMRP && aWarehouse.Inventories.Contains(product.Item.Id)) //Make sure the Product's Item is stocked in the specified Warehouse. Otherwise don't change its warehouse or they'll be incompatible.
                {
                    product.SetWarehouseAndInventory(aWarehouse.Inventories[product.Item.Id]);
                }
            }
        }
    }

    /// <summary>
    /// sum of the elements of returned value should equal a_totalQty
    /// </summary>
    /// <param name="a_totalQty"></param>
    /// <param name="a_splitQty"></param>
    /// <returns></returns>
    private List<decimal> SplitQty(decimal a_totalQty, decimal a_splitQty)
    {
        List<decimal> moQties = new ();

        if (ScenarioOptions.IsStrictlyGreaterThanZero(a_splitQty))
        {
            decimal remaining = a_totalQty;
            while (ScenarioOptions.IsStrictlyGreaterThanZero(remaining))
            {
                decimal qty = Math.Min(remaining, a_splitQty);
                moQties.Add(qty);
                remaining = remaining - qty;
            }
        }
        else
        {
            moQties.Add(a_totalQty);
        }

        return moQties;
    }

    /// <summary>
    /// Set the NeedDate and recalculate JIT values.
    /// </summary>
    /// <param name="aJob"></param>
    /// <param name="aNewNeedDateTicks"></param>
    private void SetJobNeedDate(Job aJob, long aNewNeedDateTicks)
    {
        //TODO: Should this also account for final operation's material post processing if not defined on the product?
        if (aJob.GetPrimaryProduct() is Product primaryProduct)
        {
            aNewNeedDateTicks -= primaryProduct.MaterialPostProcessingTicks;
        }

        aJob.NeedDateTicks = aNewNeedDateTicks;
        aJob.CalculateJitTimes(Clock, false);
    }

    /// <summary>
    /// Drills down through the Template BOMS and gets all OperationAttributes where Item.RollupAttributesToParent=true.
    /// </summary>
    /// <param name="aTemplateJob"></param>
    /// <param name="aAttributesListSoFar"></param>
    private static void GetAttributesRecursively(Job aTemplateJob, ref Dictionary<string, OperationAttribute> aAttributesListSoFar)
    {
        List<BaseOperation> jobOpList = aTemplateJob.GetOperations();
        for (int opI = 0; opI < jobOpList.Count; opI++)
        {
            BaseOperation op = jobOpList[opI];
            //Add this Op's Attributes
            for (int attI = 0; attI < op.Attributes.Count; attI++)
            {
                OperationAttribute attribute = op.Attributes[attI];
                aAttributesListSoFar.TryAdd(attribute.PTAttribute.Name, attribute);
            }

            //Recursively add Attributes from Materials.
            for (int mI = 0; mI < op.MaterialRequirements.Count; mI++)
            {
                MaterialRequirement mr = op.MaterialRequirements[mI];
                if (mr.Item != null && mr.Item.RollupAttributesToParent && mr.Warehouse != null && mr.Warehouse.Inventories.Contains(mr.Item.Id))
                {
                    Inventory mrInv = mr.Warehouse.Inventories[mr.Item.Id];
                    if (mrInv.TemplateManufacturingOrder != null)
                    {
                        GetAttributesRecursively(mrInv.TemplateManufacturingOrder.Job, ref aAttributesListSoFar);
                    }
                }
            }
        }
    }

    internal class SSimNewJobListNode
    {
        internal SSimNewJobListNode(Job a_job)
        {
            m_job = a_job;
        }

        public Job m_job;
        public bool m_rounded;
        public decimal m_qtyAvailable;
        public bool m_outsideBatchWindowRequirementNotesAdded;
        public bool m_allocatedSome;
    }
    
    private static void AddProductDemands(List<Product> a_products, decimal a_qtyRequired, MrpDemand a_adjustment, Inventory aInv)
    {
        decimal remainingQtyToAssociate = a_qtyRequired;
        foreach (Product product in a_products)
        {
            if (product.Item.Id == aInv.Item.Id && product.Warehouse.Id == aInv.Warehouse.Id)
            {
                if (product.Demands == null)
                {
                    product.Demands = new DemandCollection();
                }

                decimal totalNotPegged = product.TotalOutputQty - product.Demands.QtyToForecasts - product.Demands.QtyToSafetyStock - product.Demands.QtyToSalesOrders;
                if (totalNotPegged <= 0)
                {
                    //This product has been fully pegged
                    continue;
                }

                //Peg the remaining
                decimal qtyToAssociate = totalNotPegged;
                if (remainingQtyToAssociate - qtyToAssociate < 0)
                {
                    qtyToAssociate = remainingQtyToAssociate;
                    remainingQtyToAssociate = 0;
                }
                else
                {
                    remainingQtyToAssociate -= qtyToAssociate;
                }

                product.Demands.AddDemand(a_adjustment, qtyToAssociate, a_adjustment.DemandDate.Ticks, 0, aInv);

                if (remainingQtyToAssociate < 0)
                {
                    //nothing left to peg
                    break;
                }
            }
        }
    }

    private static void AddPoDemand(PurchaseToStock a_pts, decimal a_qtyRequired, long a_requiredDate, MrpDemand a_demand)
    {
        if (a_pts.Demands == null)
        {
            a_pts.Demands = new DemandCollection();
        }

        a_pts.Demands.AddDemand(a_demand, a_qtyRequired, a_requiredDate, 0, a_pts.Inventory);
    }

    internal List<MrpSupply> GenerateMrpSupplies(AdjustmentArray a_adjustments, Inventory a_adjustmentInv)
    {
        List<MrpSupply> supplies = new ();
        for (int i = 0; i < a_adjustments.Count; ++i)
        {
            Adjustment adjustment = a_adjustments[i];
            if (adjustment is PurchaseOrderAdjustment poAdjustment)
            {
                supplies.Add(new PurchaseOrderSupply(poAdjustment.PurchaseOrder, adjustment.AdjDate, adjustment.Qty));
            }
            else if (adjustment is ActivityAdjustment actAdjustment)
            {
                supplies.Add(new ActivitySupply(actAdjustment.Activity, actAdjustment.Inventory, actAdjustment.Qty, adjustment.AdjDate));
            }
        }

        return supplies;
    }

    /// <summary>
    /// Create the PO and increase it's quanitity to MinOrderQty if possible to meet the minimum size.
    /// </summary>
    private PurchaseToStock CreatePO(Inventory a_inv)
    {
        PurchaseToStock pts = new (IdGen.NextID());
        pts.Name = string.Format(Localizer.GetString("MRP {0}".Localize()), pts.Id);
        pts.MaintenanceMethod = PurchaseToStockDefs.EMaintenanceMethod.MrpGenerated;
        pts.ExternalId = pts.Name;
        pts.Inventory = a_inv;
        pts.Warehouse = a_inv.Warehouse;
        pts.StorageArea = a_inv.PurchaseOrderSupplyStorageArea;
        //TODO: Remove once we validate and can set this field
        if (pts.StorageArea == null)
        {
            //Find one from the warehouse
            foreach (StorageArea sa in a_inv.Warehouse.StorageAreas)
            {
                if (!sa.SingleItemStorage)
                {
                    if (sa.CanStoreItem(a_inv.Item.Id))
                    {
                        pts.StorageArea = sa;
                        break;
                    }
                }
            }
        }
        
        
        pts.UnloadSpanTicks = 0;
        pts.TransferSpanTicks = 0;

        return pts;
    }

    private void EnforceMinOrderQty(PurchaseToStock aPts, Inventory aInv)
    {
        decimal minQty = aInv.Item.MinOrderQty;

        if (aPts.QtyOrdered < minQty)
        {
            decimal qtyShort = minQty - aPts.QtyOrdered;
            if (qtyShort <= aInv.Item.MinOrderQtyRoundupLimit)
            {
                aPts.QtyOrdered = minQty;
                aPts.Notes += string.Format(Localizer.GetString("QtyOrdered was increased to satisfy the MinOrderQty={0}"), aInv.Item.MinOrderQty);
            }
            else
            {
                aPts.QtyOrdered = aPts.QtyOrdered + aInv.Item.MinOrderQtyRoundupLimit;
                aPts.Notes += string.Format(Localizer.GetString("QtyOrdered was increased to attempt to satisfy the MinOrderQty={0}.  Since MinOrderQtyRoundupLimit={1} was too restrictive the PO is still under the MinOrderQty."), aInv.Item.MinOrderQty, aInv.Item.MinOrderQtyRoundupLimit);
            }
        }
    }

    //private void CreateSafetyStockForPO(out PurchaseToStock pts, Inventory a_inv, Item a_item, decimal a_curQty, decimal a_reqdQty, long a_NeedDate, long a_clock)
    //{
    //    pts = CreatePO(a_inv);
    //    pts.NeedDate = new DateTime(a_NeedDate);
    //    pts.ScheduledReceiptDateTicks = Math.Max(a_NeedDate, a_clock + a_inv.LeadTimeTicks);
    //    pts.QtyOrdered = a_reqdQty;

    //    if (a_item.CanBatch)
    //    {
    //        StringBuilder aNotes = new StringBuilder();
    //        decimal aExtraQty;
    //        PORound(pts, a_item, aNotes, out aExtraQty);
    //    }
    //    pts.Notes = string.Format("Created for Safety Stock.  {0}".Localize(), pts.Notes);
    //    EnforceMinOrderQty(pts, a_inv);
    //    PurchaseToStockManager.Add(pts);

    //    AddPoSafetyStockDemand(pts, a_reqdQty, a_NeedDate, a_inv, a_curQty + pts.QtyOrdered);
    //}

    //bool CreateSafetyStockForPurchasedItems(OptimizeSettings aOptimizeSettings)
    //{
    //    FireMRPStatusUpdateEvent(SimulationProgress.Status.MRP_GeneratingSafetyStock);
    //    bool safetyStockJobsCreated = false;

    //    for (int wI = 0; wI < m_warehouseManager.Count; ++wI)
    //    {
    //        Warehouse wh = m_warehouseManager[wI];
    //        IEnumerator<InventoryManager.ItemInventory> enumerator = wh.Inventories.GetEnumerator();

    //        while (enumerator.MoveNext())
    //        {
    //            Inventory inv = enumerator.Current.Inventory;
    //            if (inv.MrpProcessing != InventoryDefs.MrpProcessing.GeneratePurchaseOrders || (aOptimizeSettings.NetChangeMRP && !inv.IncludeInNetChangeMRP))
    //                continue;

    //            if (inv.SafetyStock > 0)
    //            {
    //                Item item = inv.Item;

    //                if (inv.MrpUnallocated < inv.SafetyStock) //Need to order more to satsify the safety stock requirement
    //                {
    //                    PurchaseToStock pts;
    //                    decimal reqdQty = inv.SafetyStock - inv.MrpUnallocated;
    //                    CreateSafetyStockForPO(out pts, inv, item, inv.MrpUnallocated, reqdQty, Clock, Clock);
    //                    safetyStockJobsCreated = true;
    //                }
    //            }
    //        }
    //    }

    //    return safetyStockJobsCreated;
    //}

    /// <summary>
    /// class for storing how much of on hand lots have been allocated.
    /// </summary>
    public class LotAllocations
    {
        internal LotAllocations(Lot a_lot)
        {
            Lot = a_lot;
            Allocated = 0;
        }

        internal Lot Lot { get; set; }
        internal decimal Allocated { get; set; }
        internal decimal Unallocated => Lot.Qty - Allocated;
    }

    /// <summary>
    /// Given an inventory, returns a list of LotAllocations if the item is
    /// LotControlled. It will return null, if item is not lot controlled.
    /// </summary>
    /// <param name="a_inv"></param>
    /// <returns></returns>
    public List<LotAllocations> GetLotAllocations(Inventory a_inv)
    {
        List<LotAllocations> lotAllocations = new ();
        foreach (Lot lot in a_inv.Lots)
        {
            if (lot.LotSource != ItemDefs.ELotSource.Inventory)
            {
                continue;
            }

            lotAllocations.Add(new LotAllocations(lot));
        }

        lotAllocations.OrderBy(lotAllo => lotAllo.Lot.ProductionDate);
        return lotAllocations;
    }

    /// <summary>
    /// allocates any unallocated on hand to adjustment. If Item is lot controlled, it will allocate from lotAllocations. Returns the remaining qtyNeeded
    /// </summary>
    internal bool AllocateOnHand(ref decimal r_qtyNeeded, Inventory a_inv, MrpDemand a_demand, DateTime a_adjDate, List<LotAllocations> a_lotAllocations, bool a_allowPartialSupply, MaterialRequirement a_demandMR)
    {
        if (a_lotAllocations != null)
        {
            IEnumerable<LotAllocations> unallocatedLotsSorted = a_lotAllocations
                .Where(l => ScenarioOptions.IsStrictlyGreaterThanZero(l.Unallocated) && l.Lot.ProductionDate <= a_adjDate);

            ItemDefs.MaterialAllocation allocation = GetMaterialAllocation(a_demand, a_inv);
            switch (allocation)
            {
                case ItemDefs.MaterialAllocation.NotSet:
                    unallocatedLotsSorted = unallocatedLotsSorted.OrderBy(l => l.Unallocated);
                    break;
                case ItemDefs.MaterialAllocation.UseOldestFirst:
                    unallocatedLotsSorted = unallocatedLotsSorted.OrderBy(l => l.Lot.ProductionDate)
                                                                 .ThenBy(l => l.Unallocated);
                    break;
                case ItemDefs.MaterialAllocation.UseNewestFirst:
                    unallocatedLotsSorted = unallocatedLotsSorted.OrderByDescending(l => l.Lot.ProductionDate)
                                                                 .ThenBy(l => l.Unallocated);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (LotAllocations lotAllo in unallocatedLotsSorted)
            {
                if (!a_allowPartialSupply && lotAllo.Unallocated < r_qtyNeeded)
                {
                    continue;
                }

                if (lotAllo.Lot.ShelfLifeData.Expirable)
                {
                    if (lotAllo.Lot.ShelfLifeData.ExpirationDate < a_adjDate)
                    {
                        // lot expires before adjust needdate
                        continue;
                    }

                    if (a_demandMR is MaterialRequirement demandMr)
                    {
                        if (lotAllo.Lot.ProductionDate + demandMr.ShelfLifeRequirement.MinAge > a_adjDate)
                        {
                            //Lot is not old enough
                            continue;
                        }

                        if (lotAllo.Lot.ShelfLifeData.ExpirationTicks - a_adjDate.Ticks < demandMr.ShelfLifeRequirement.MinRemainingShelfLife)
                        {
                            //Lot will expire too soon
                            continue;
                        }
                    }
                }

                if (!IsAllocationAllowed(lotAllo.Lot, a_demand, a_inv))
                {
                    continue;
                }

                decimal qtyToallocToThisNeed = Math.Min(r_qtyNeeded, lotAllo.Unallocated);
                lotAllo.Allocated += qtyToallocToThisNeed;
                r_qtyNeeded -= qtyToallocToThisNeed;
                PegSupplyAndDemand(lotAllo.Lot, a_demand, a_inv);
                if (ScenarioOptions.IsApproximatelyZeroOrLess(r_qtyNeeded))
                {
                    break;
                }
            }
        }
        else
        {
            if (ScenarioOptions.IsStrictlyGreaterThanZero(a_inv.MrpUnallocated) &&
                (a_allowPartialSupply || a_inv.MrpUnallocated > r_qtyNeeded))
            {
                decimal qtyToallocToThisNeed = Math.Min(r_qtyNeeded, a_inv.MrpUnallocated);
                a_inv.MrpAllocated += qtyToallocToThisNeed;
                r_qtyNeeded -= qtyToallocToThisNeed;
            }
        }

        return !ScenarioOptions.IsStrictlyGreaterThanZero(r_qtyNeeded);
    }

    /// <summary>
    /// Returns the Material Allocation value for the provided demand.
    /// </summary>
    /// <param name="a_adjustment">The demand</param>
    /// <param name="a_inv"></param>
    /// <returns>Defaults to use the inventory value if no demand is found</returns>
    private static ItemDefs.MaterialAllocation GetMaterialAllocation(MrpDemand a_adjustment, Inventory a_inv)
    {
        if (a_adjustment is ActivityMrpDemand actAdj)
        {
            return actAdj.MR.MaterialAllocation;
        }

        if (a_adjustment is SalesOrderMrpDemand soDemand)
        {
            return soDemand.Distribution.MaterialAllocation;
        }
        
        if (a_adjustment is TransferOrderMrpDemand to)
        {
            return to.Distribution.MaterialAllocation;
        }
        //Forecast does not have MaterialAllocation

        //Default to inventory value
        return a_inv.MaterialAllocation;
    }

    /// <summary>
    /// set need date and scheduled receipt date for newly created PO.
    /// </summary>
    /// <param name="a_po"></param>
    /// <param name="a_needDateTicks"></param>
    private void SetPODate(PurchaseToStock a_po, long a_needDateTicks)
    {
        a_po.NeedDate = new DateTime(a_needDateTicks);
        a_po.ScheduledReceiptDateTicks = Math.Max(m_clock + a_po.Inventory.LeadTime.Ticks, a_po.NeedDate.Ticks);
    }

    private void DeleteNonFirmPurchaseToStocks(OptimizeSettings aOptimizeSettings)
    {
        for (int i = PurchaseToStockManager.Count - 1; i >= 0; i--)
        {
            PurchaseToStock pts = PurchaseToStockManager.GetByIndex(i);
            if (!pts.Firm && !pts.Closed)
            {
                if (!aOptimizeSettings.NetChangeMRP || pts.Inventory.IncludeInNetChangeMRP)
                {
                    PurchaseToStockManager.Remove(pts);
                }
            }
        }
    }

    private class MrpSettings
    {
        internal MrpSettings(OptimizeSettings a_optimizeSettings, ScenarioDetail a_sd)
        {
            StartDate = GetMrpStartDate(a_optimizeSettings, a_sd);
            CutoffDate = GetMrpCutOffDate(a_optimizeSettings, a_sd);
            RegeneratePurchaseOrders = a_optimizeSettings.RegeneratePurchaseOrders;
            RegenerateJobs = a_optimizeSettings.RegenerateJobs;
            EnableLotPegging = a_optimizeSettings.EnableLotPegging;
            ScenarioDataChanges = new ScenarioDataChanges();
        }

        internal readonly DateTime StartDate;
        internal readonly DateTime CutoffDate;
        public readonly bool RegeneratePurchaseOrders;
        public readonly bool RegenerateJobs;
        public readonly bool EnableLotPegging;
        public readonly IScenarioDataChanges ScenarioDataChanges;

        /// <summary>
        /// Determine start date based on the MRP start type
        /// </summary>
        /// <param name="a_optimizeSettings"></param>
        /// <returns></returns>
        private static DateTime GetMrpStartDate(OptimizeSettings a_optimizeSettings, ScenarioDetail a_sd)
        {
            switch (a_optimizeSettings.MrpStartType)
            {
                case OptimizeSettings.EMrpStartType.FirstDemand:
                    return PTDateTime.MinDateTime;
                case OptimizeSettings.EMrpStartType.Clock:
                    return a_sd.ClockDate;
                case OptimizeSettings.EMrpStartType.ShortTerm:
                    return a_sd.GetEndOfShortTerm();
                case OptimizeSettings.EMrpStartType.SpecificDate:
                    return a_optimizeSettings.MrpSpecificStartDate;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Determine datetime based on the MRP Cutoff duration
        /// </summary>
        /// <param name="a_optimizeSettings"></param>
        /// <returns></returns>
        private static DateTime GetMrpCutOffDate(OptimizeSettings a_optimizeSettings, ScenarioDetail a_sd)
        {
            switch (a_optimizeSettings.MrpCutoff)
            {
                case OptimizeSettings.EMrpCutOff.ShortTerm:
                    return a_sd.GetEndOfShortTerm();
                case OptimizeSettings.EMrpCutOff.PlanningHorizon:
                    return a_sd.GetPlanningHorizonEnd();
                case OptimizeSettings.EMrpCutOff.None:
                    return PTDateTime.MaxDateTime;
                case OptimizeSettings.EMrpCutOff.SpecificDuration:
                    return a_sd.ClockDate + a_optimizeSettings.MrpCutoffDateDuration;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}