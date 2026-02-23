using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Sql.SqlServer;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Extensions;
using PT.Scheduler.Simulation.Customizations.TransmissionPreprocessing;
using PT.Scheduler.Simulation.UndoReceive;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.MassRecordings;
using PT.SchedulerDefinitions.Templates.Lists;
using PT.SystemDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions.CleanoutTrigger;
using PT.Transmissions.ResourceConnectors;
using PT.Transmissions2;
using System.Collections;
using System.Diagnostics;

using PT.Scheduler.AuditObject;

namespace PT.Scheduler;

public partial class ScenarioDetail
{
    /// <summary>
    /// Processes a transmission and performs post process tasks.
    /// </summary>
    /// <param name="t"></param>
    internal void Receive(ScenarioBaseT a_t, IScenarioDataChanges a_scenarioDataChanges, UndoReceive a_undoReapply = null)
    {
        PT.Common.Testing.Timing tranmissionTiming = null;

        List<ScenarioBaseT> processedTs = new(); // this holds delegates to functions that need to be called after processing.
        bool runPostProcessing = true;
        ScenarioExceptionInfo sei = new();
        sei.Create(this);

        try
        {
            tranmissionTiming = new Common.Testing.Timing(true, a_t.TransmissionNbr + ": " + a_t.GetType().Name);

            #if DEBUG
            if (m_processingActions.Count > 0)
            {
                throw new DebugException("Last transmission's processing actions were not all executed.");
            }
            #endif
            m_processingActions.Clear();

            List<ScenarioBaseT> alteredActions = null;
            try
            {
                alteredActions = PreProcessingTransformationCustomizationPoint(a_t);
            }
            catch (Exception e)
            {
                throw new PTException("2457", new object[] { e.Message });
            }

            if (alteredActions != null)
            {
                //This list could include the original action, or a completely new set of actions
                foreach (ScenarioBaseT t in alteredActions)
                {
                    ProcessT(t, ref processedTs, a_scenarioDataChanges, sei, a_undoReapply); // process transmission
                    ExecuteProcessingActions(sei);
                }
            }
            else
            {
                ProcessT(a_t, ref processedTs, a_scenarioDataChanges, sei, a_undoReapply); // process transmission
                ExecuteProcessingActions(sei);
            }
        }
        catch (PTException e)
        {
            if (e is not PTSimCancelledException)
            {
                runPostProcessing = e is PTHandleableException; // only run if it's a handleable exception
                ScenarioEvents se;
                using (Scenario.ScenarioEventsLock.EnterRead(out se))
                {
                    se.FireTransmissionFailureEvent(e, a_t, new SystemMessage(a_t.Instigator, e.Message));
                }
            }

            throw;
        }
        finally
        {
            //Run any remaining cleanup actions.
            ExecuteAlwaysRunPostProcessingActions(sei);

            if (runPostProcessing)
            {
                PostProcessT(processedTs, a_scenarioDataChanges, sei);
            }

            if (tranmissionTiming != null)
            {
                tranmissionTiming.Stop();
                m_transmissionTimingQueue.Enqueue(tranmissionTiming);
            }

            #region Debugging
#if DEBUG
            //for (int pI = 0; pI < PlantManager.Count; ++pI)
            //{
            //    Plant p = PlantManager[pI];

            //    for (int dI = 0; dI < p.Departments.Count; ++dI)
            //    {
            //        Department d = p.Departments[dI];

            //        for (int rI = 0; rI < d.Resources.Count; ++rI)
            //        {
            //            Resource r = d.Resources[rI];

            //            for (int rciI = 0; rciI < r.RecurringCapacityIntervals.Count; ++rciI)
            //            {
            //                bool found = false;
            //                RecurringCapacityInterval rci = r.RecurringCapacityIntervals[rciI];

            //                for (int crI = 0; crI < rci.CalendarResources.Count; ++crI)
            //                {
            //                    Resource x = (Resource)rci.CalendarResources[crI];
            //                    if (x == r)
            //                    {
            //                        found = true;
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //for (int rciI = 0; rciI < RecurringCapacityIntervalManager.Count; ++rciI)
            //{
            //    RecurringCapacityInterval rci = RecurringCapacityIntervalManager[rciI];
            //    for (int crI = 0; crI < rci.CalendarResources.Count; ++crI)
            //    {
            //        bool found = false;
            //        Resource r = (Resource)rci.CalendarResources[crI];
            //        for (int xI = 0; xI < r.RecurringCapacityIntervals.Count; ++xI)
            //        {
            //            RecurringCapacityInterval x=r.RecurringCapacityIntervals[xI];
            //            if (x == rci)
            //            {
            //                found = true;
            //                break;
            //            }
            //        }

            //        for (int xI = 0; xI < r.RecurringCapacityIntervals.Count; ++xI)
            //        {
            //            RecurringCapacityInterval rci2=(RecurringCapacityInterval)r.RecurringCapacityIntervals.GetByIndex(xI);
            //            if(rci2.Id!=r.RecurringCapacityIntervals.GetKeyByIndex(xI))
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}
#endif

#if TEST
                if (_csSDs != null)
                {
                    _csSDs.Receive(t);
                }
#endif
            #endregion
        }
    }

    /// <summary>
    /// Processes a transmission.
    /// </summary>
    private void ProcessT(ScenarioBaseT a_t, ref List<ScenarioBaseT> r_processedTs, IScenarioDataChanges a_dataChanges, ScenarioExceptionInfo a_sei, UndoReceive a_undoReapply = null)
    {
        PTConsole.WriteLine($"Processing transmission {a_t.GetType().ToString()} | ");
        // Whether a forecast or sales order transmission was processed; demands hav been updated.
        bool soOrForecastDemandChanges = false;
        if (!r_processedTs.Contains(a_t))
        {
            r_processedTs.Add(a_t);
        }

        try
        {
            PreProcessingCustomizationPoint(a_t, a_dataChanges);
        }
        catch (Exception e)
        {
            throw new PTException("2457", new object[] { e.Message });
        }

        ExtensionController.ResetDataChanges();

        UserFieldDefinitionManager udfManager = null;
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            udfManager = sm.UserFieldDefinitionManager;
        }

        if (a_t is Transmissions.CTP.ScenarioCtpT)
        {
            CtpCreator.CreateCTP((Transmissions.CTP.ScenarioCtpT)a_t, Scenario, this, a_dataChanges, m_errorReporter);
        }
        else if (a_t is ImportT)
        {
            if (!PTSystem.LicenseKey.IncludeImport)
            {
                throw new LicenseKeyException("2935");
            }

            ProcessImportT((ImportT)a_t, ref r_processedTs, a_dataChanges);
        }
        else if (a_t is PlantBaseT)
        {
            PlantManager.Receive(this, (PlantBaseT)a_t, _scenario, a_dataChanges);
        }
        else if (a_t is CapacityIntervalBaseT)
        {
            if (a_t is CapacityIntervalConvertT)
            {
                CapacityIntervalConvertHandler(udfManager, (CapacityIntervalConvertT)a_t, a_dataChanges);
            }
            else
            {
                CapacityIntervalManager.Receive(udfManager, (CapacityIntervalBaseT)a_t, this, a_dataChanges);
            }
        }
        else if (a_t is RecurringCapacityIntervalBaseT)
        {
            if (a_t is RecurringCapacityIntervalConvertT convertT)
            {
                RecurringCapacityIntervalConvertHandler(udfManager, convertT, a_dataChanges);
            }
            else
            {
                RecurringCapacityIntervalManager.Receive(udfManager, (RecurringCapacityIntervalBaseT)a_t, this, a_dataChanges);
            }
        }
        else if (a_t is CellBaseT)
        {
            CellManager.Receive((CellBaseT)a_t, a_dataChanges);
        }
        else if (a_t is CustomerBaseT)
        {
            CustomerManager.Receive((CustomerBaseT)a_t, a_dataChanges);
        }
        else if (a_t is CapabilityBaseT)
        {
            CapabilityManager.Receive((CapabilityBaseT)a_t, m_productRuleManager, a_dataChanges);
        }
        else if (a_t is BalancedCompositeDispatcherDefinitionBaseT)
        {
            DispatcherDefinitionManager.Receive((BalancedCompositeDispatcherDefinitionBaseT)a_t, a_dataChanges);
        }
        else if (a_t is ResourceConnectorsBaseT)
        {
            ResourceConnectorManager.Receive(this, (ResourceConnectorsBaseT)a_t, a_dataChanges);
        }
        else if (a_t is ScheduleCommitT commitT)
        {
            JobManager.Receive(commitT, a_dataChanges, this);

            if (commitT.AutoJoinMOs)
            {
                bool autoJoinedAtLeastOnce = false;

                if (m_extensionController.RunAutoJoinExtension)
                {
                    m_extensionController.BeforeAutoJoinProcessing(this);
                }

                ResourceKeyList.Node node = commitT.Resources.First;
                while (node != null)
                {
                    BaseResource resource = PlantManager.GetResource(node.Data.Resource);
                    if (resource != null)
                    {
                        if (AutoJoin(resource as Resource, a_dataChanges))
                        {
                            autoJoinedAtLeastOnce = true;
                        }
                    }

                    node = node.Next;
                }

                if (autoJoinedAtLeastOnce)
                {
                    TimeAdjustment(a_t);
                }
            }
        }
        else if (a_t is JobBaseT)
        {
            JobManager.Receive((JobBaseT)a_t, a_dataChanges, this);
        }
        else if (a_t is ScenarioDetailSetCapabilitiesT)
        {
            SetMachineCapabilitiesTransmission((ScenarioDetailSetCapabilitiesT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailSetCapabilityResourcesT)
        {
            SetCapabilityMachinesTransmission((ScenarioDetailSetCapabilityResourcesT)a_t, a_dataChanges);
        }
        else if (a_t is PurchaseToStockBaseT)
        {
            PurchaseToStockManager.Receive(this, (PurchaseToStockBaseT)a_t, a_dataChanges);
        }
        else if (a_t is AddInControlUpdateT)
        {
            UpdateSimulationCustomizationEnabling((AddInControlUpdateT)a_t);
        }
        else if (a_t is ScenarioDetailOnlineT)
        {
            SetOnline((ScenarioDetailOnlineT)a_t);
        }
        else if (a_t is ScenarioDetailOfflineT)
        {
            SetOffline((ScenarioDetailOfflineT)a_t);
        }

        //ERP Transmissions
        else if (a_t is PlantT)
        {
            m_plantManager.Receive(udfManager, (PlantT)a_t, a_dataChanges, a_sei);
        }
        else if (a_t is DepartmentT)
        {
            ProcessDepartmentT(this, udfManager, (DepartmentT)a_t, a_dataChanges);
        }
        else if (a_t is ResourceT)
        {
            ProcessResourceT(this, udfManager, (ResourceT)a_t, a_dataChanges);
        }
        else if (a_t is WarehouseT)
        {
            ItemManager.Receive(this, a_dataChanges, (WarehouseT)a_t, JobManager, WarehouseManager, PurchaseToStockManager, ProductRuleManager, SalesOrderManager, TransferOrderManager, udfManager, a_sei);
            WarehouseManager.Receive((WarehouseT)a_t, udfManager, PlantManager, ItemManager, JobManager, PurchaseToStockManager, SalesOrderManager, TransferOrderManager, this, a_dataChanges, a_sei);
        }
        else if (a_t is PurchaseToStockT)
        {
            PurchaseToStockManager.Receive((PurchaseToStockT)a_t, WarehouseManager, ItemManager, udfManager, this, a_dataChanges);
        }
        else if (a_t is ItemT)
        {
            ProcessItemT(udfManager, (ItemT)a_t, a_dataChanges);
        }
        else if (a_t is CellT)
        {
            m_cellManager.Receive(udfManager, (CellT)a_t, a_dataChanges);
        }
        else if (a_t is CapabilityT)
        {
            m_capabilityManager.Receive((CapabilityT)a_t, this, a_dataChanges);
        }
        else if (a_t is CustomerT)
        {
            CustomerManager.Receive(this, udfManager, (CustomerT)a_t, a_dataChanges);
        }
        else if (a_t is JobT)
        {
            ProcessJobT((JobT)a_t, udfManager, a_dataChanges, a_sei);
        }
        else if (a_t is InternalActivityUpdateT)
        {
            m_jobManager.Receive((InternalActivityUpdateT)a_t, this, a_dataChanges);
        }
        else if (a_t is DispatcherDefinitionManagerBaseT)
        {
            //TODO: ScenarioDataChanges
            DispatcherDefinitionManager.Receive((BalancedCompositeDispatcherDefinitionBaseT)a_t, a_dataChanges);
        }
        else if (a_t is CapacityIntervalT)
        {
            ProcessCapacityIntervalT(udfManager, (CapacityIntervalT)a_t, a_dataChanges);
        }
        else if (a_t is RecurringCapacityIntervalT)
        {
            ProcessRecurringCapacityIntervalT(udfManager, (RecurringCapacityIntervalT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailOptimizeT)
        {
            ScenarioDetailOptimizeT optimizeT = (ScenarioDetailOptimizeT)a_t;
            if (optimizeT.UseScenarioOptimizeSettings)
            {
                OptimizeHandler(optimizeT, OptimizeSettings.Clone(), a_dataChanges); //don't change the saved settings
            }
            else
            {
                OptimizeHandler(optimizeT, optimizeT.OptimizeSettings.Clone(), a_dataChanges); //don't change the saved settings
            }
        }
        else if (a_t is ScenarioDetailCompressT)
        {
            ScenarioDetailCompressT compressT = (ScenarioDetailCompressT)a_t;
            OptimizeSettings settingsToUse = null;
            if (compressT.UseScenarioCompressSettings)
            {
                settingsToUse = CompressSettings.Clone();
            }
            else
            {
                settingsToUse = compressT.CompressSettings.Clone(); //don't change the saved settings
            }

            if (settingsToUse == null)
            {
                throw new SimulationValidationException("The user that triggered the compress no longer exists".Localize());
            }

            if (compressT.SpecificStartTimeSet)
            {
                settingsToUse.StartTime = OptimizeSettings.ETimePoints.SpecificDateTime;
                settingsToUse.SpecificStartTime = compressT.SpecificStartTime;
            }

            if (compressT.PlantIdSet)
            {
                settingsToUse.ResourceScope = OptimizeSettings.resourceScopes.OnePlant;
                settingsToUse.PlantToInclude = compressT.PlantId;
            }
            else
            {
                settingsToUse.ResourceScope = OptimizeSettings.resourceScopes.All;
                settingsToUse.PlantToInclude = BaseId.NULL_ID; //The scheduler looks at plant, not scope in some cases
            }

            if (compressT.LimitToResourcesOverrideIsSet)
            {
                settingsToUse.LimitToResources = compressT.LimitToResourcesOverride;
            }

            Compress(compressT, settingsToUse, a_dataChanges);
        }
        else if (a_t is ScenarioDetailSplitMOT)
        {
            SplitMO((ScenarioDetailSplitMOT)a_t);
        }
        else if (a_t is ScenarioDetailSplitJobOrMOT)
        {
            SplitJobOrMOByCycleAtTimeT(Clock, (ScenarioDetailSplitJobOrMOT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailJoinJobOrMOT)
        {
            JoinJobOrMOT((ScenarioDetailJoinJobOrMOT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailChangeMOQtyT)
        {
            HandleScenarioDetailChangeMOQtyT((ScenarioDetailChangeMOQtyT)a_t);
        }
        else if (a_t is ScenarioDetailExportT)
        {
            if (!PTSystem.LicenseKey.IncludeExport)
            {
                throw new LicenseKeyException("2934");
            }

            Stopwatch publishTimer = Stopwatch.StartNew();
            //if (PTSystem.RunningMassRecordings)
            //{
            //    using (PtDatabaseUpdater testBuilder = new PtDatabaseUpdater(this))
            //    {
            //        //Moved database functionality out of scheduler mass recordings will have to reproduce this behavior 
            //        //possibly using scenarioevents
            //        //testBuilder.GetPtDataSet((ScenarioDetailExportT)a_t, false);
            //    }
            //}
            //else
            {
                ExportScenario((ScenarioDetailExportT)a_t);
            }
            Scenario.KpiController.CalculateKPIsForPublish(_scenario, this, (ScenarioDetailExportT)a_t, m_errorReporter);
            publishTimer.Stop();
            RecordActionDuration("Publish", publishTimer.ElapsedMilliseconds, a_dataChanges);
        }
        else if (a_t is ScenarioDetailExpediteJobsT)
        {
            ExpediteJobs((ScenarioDetailExpediteJobsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailScheduleJobsT)
        {
            ScheduleJobs((ScenarioDetailScheduleJobsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailExpediteMOsT)
        {
            ExpediteMOs((ScenarioDetailExpediteMOsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailMoveT)
        {
            // Any transmission based on type ScenarioDetailMoveT is handled by this call.
            HandleScenarioDetailMoveTAndDerivatives((ScenarioDetailMoveT)a_t, a_undoReapply, a_dataChanges);
        }
        else if (a_t is ScenarioDetailAnchorJobsT anchorJobsT)
        {
            ProcessAnchorJobsT(anchorJobsT, a_dataChanges);
        }
        else if (a_t is ScenarioDetailJobResetJITAndSubJobNeedDateT resetJITAndSubJobNeedDateT)
        {
            ProcessResetJobJITAndSubJobNeedDates(resetJITAndSubJobNeedDateT, a_dataChanges);
        }
        else if (a_t is ScenarioDetailAnchorMOsT anchorMosT)
        {
            ProcessAnchorMOsT(anchorMosT, a_dataChanges);
        }
        else if (a_t is ScenarioDetailAnchorOperationsT)
        {
            ProcessAnchorOperationsT((ScenarioDetailAnchorOperationsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailAnchorActivitiesT)
        {
            ProcessAnchorActivitiesT((ScenarioDetailAnchorActivitiesT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailLockAndAnchorActivitiesT)
        {
            ProcessLockAndAnchorActivitiesT((ScenarioDetailLockAndAnchorActivitiesT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailLockJobsT)
        {
            ProcessLockJobsT((ScenarioDetailLockJobsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailLockMOsT)
        {
            ProcessLockMOsT((ScenarioDetailLockMOsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailLockMOsToPathT)
        {
            ProcessLockToPathMOsT((ScenarioDetailLockMOsToPathT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailLockOperationsT)
        {
            ProcessLockOperationsT((ScenarioDetailLockOperationsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailLockActivitiesT)
        {
            ProcessLockActivitiesT((ScenarioDetailLockActivitiesT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailLockBlocksT)
        {
            ProcessLockBlocksT((ScenarioDetailLockBlocksT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailHoldJobsT)
        {
            ProcessHoldJobsT((ScenarioDetailHoldJobsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailHoldMOsT)
        {
            ProcessHoldMOsT((ScenarioDetailHoldMOsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailHoldOperationsT)
        {
            ProcessHoldOperationsT((ScenarioDetailHoldOperationsT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailSetJobPropertiesT)
        {
            ProcessSetJobCommitmentsAndPrioritiesT((ScenarioDetailSetJobPropertiesT)a_t, a_dataChanges);
        }
        else if (a_t is SystemOptionsT)
        {
            ProcessSystemOptionsT((SystemOptionsT)a_t, a_dataChanges);
        }
        else if (a_t is SystemPublishOptionsT)
        {
            SystemPublishOptionsT systemPublishOptionsT = (SystemPublishOptionsT)a_t;
            m_scenarioPublishOptions.Update(systemPublishOptionsT.PublishOptions);
        }
        else if (a_t is ShopViewResourceOptionBaseT)
        {
            m_shopViewResourceOptionsManager.Receive((ShopViewResourceOptionBaseT)a_t, this, a_dataChanges);
        }
        else if (a_t is ShopViewOptionsAssignmentT)
        {
            ShopViewResourceOptionsManager.Receive((ShopViewOptionsAssignmentT)a_t, PlantManager);
        }
        else if (a_t is ScenarioDetailOptimizeSettingsChangeT optimizeSettingsT)
        {
            // Optimize settings have been modified, must verify they are still validated
            m_optimizeSettings.Update(optimizeSettingsT.Settings);
            m_optimizeSettings.VerifyLicenseConstraintsForOptimizeSettings();
            using (Scenario.AutoEnterScenarioEvents(out ScenarioEvents se))
            {
                se.FireOptimizeSettingsChangedEvent();
            }
        }
        else if (a_t is ScenarioDetailCompressSettingsChangeT compressSettingsT)
        {
            m_compressSettings.Update(compressSettingsT.Settings);
        }
        else if (a_t is AttributeCodeTableBaseT)
        {
            AttributeCodeTableManager.Receive((AttributeCodeTableBaseT)a_t, this, AttributeManager, a_dataChanges);
        }
        else if (a_t is LookupTableDeleteAllT)
        {
            DeleteAllLookupTables(a_dataChanges);
        }
        else if (a_t is SetupRangeBaseT)
        {
            if (a_t is SetupRangeUpdateT)
            {
                SetupRangeUpdateT setupRangeUpdateT = (SetupRangeUpdateT)a_t;
                RangeLookup.FromRangeSets attributeFromRangeSetList = FromRangeSetManager.Receive(setupRangeUpdateT, AttributeManager, a_dataChanges);
                UpdateAttributeFromRangeSetListReferences(attributeFromRangeSetList, setupRangeUpdateT.assignedResources);
            }
            else if (a_t is SetupRangeDeleteT)
            {
                SetupRangeDeleteT setupRangeDeleteT = (SetupRangeDeleteT)a_t;
                RangeLookup.FromRangeSets attributeFromRangeSetList = FromRangeSetManager.Receive(setupRangeDeleteT, a_dataChanges);

                // Clear the resource from all the tables.
                Dictionary<string, Resource> dictionary = PlantManager.GetResourceHash();
                ClearResourceFromToRanges(dictionary, attributeFromRangeSetList.Id);
            }
            else
            {
                FromRangeSetManager.Receive((SetupRangeBaseT)a_t, a_dataChanges);
            }
        }
        else if (a_t is LookupAttributeNumberRangeT)
        {
            ProcessLookupAttributeNumberRangeT((LookupAttributeNumberRangeT)a_t, a_dataChanges);
        }
        else if (a_t is LookupAttributeCodeTableT)
        {
            AttributeCodeTableManager.Receive((LookupAttributeCodeTableT)a_t, Scenario, this, a_dataChanges);
        }
        else if (a_t is CleanoutTriggerTablesT)
        {
            CleanoutTriggerTableManager.Receive((CleanoutTriggerTablesT)a_t, Scenario, this, a_dataChanges);
        }
        else if (a_t is LookupItemCleanoutTableT)
        {
            ItemCleanoutTableManager.Receive((LookupItemCleanoutTableT)a_t, Scenario, this, a_dataChanges);
        }
        else if (a_t is ProductRulesT productRuleT)
        {
            UpdateProductRules((ProductRulesT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailClearT clearDataT)
        {
            if (clearDataT.GeneralizeDataInsteadOfClear)
            {
                GeneralizeScenarioData(clearDataT, a_dataChanges);
            }
            else
            {
                ClearScenarioData(clearDataT, a_dataChanges);
            }
        }
        else if (a_t is ForecastT)
        {
            WarehouseManager.ProcessForecastT(udfManager, (ForecastT)a_t, ItemManager, this);
            soOrForecastDemandChanges = true;
        }
        else if (a_t is Transmissions.Forecast.ForecastBaseT)
        {
            WarehouseManager.Receive((Transmissions.Forecast.ForecastBaseT)a_t, this, a_dataChanges);
            soOrForecastDemandChanges = true;
            TimeAdjustment(a_t);
        }
        else if (a_t is ScenarioDetailSetSalesOrderValuesT)
        {
            SalesOrderManager.Receive((ScenarioDetailSetSalesOrderValuesT)a_t, this, a_dataChanges);
            soOrForecastDemandChanges = true;
        }
        else if (a_t is SalesOrderT)
        {
            SalesOrderT salesOrderT = (SalesOrderT)a_t;
            SalesOrderManager.Update(udfManager, salesOrderT, this, a_dataChanges);
            soOrForecastDemandChanges = true;
        }
        else if (a_t is TransferOrderT)
        {
            TransferOrderT toT = (TransferOrderT)a_t;
            TransferOrderManager.Receive(udfManager, toT, ItemManager, WarehouseManager, this, a_t, a_dataChanges);
        }
        else if (a_t is ResourceConnectorsT)
        {
            ResourceConnectorManager.Receive(udfManager, (ResourceConnectorsT)a_t, Scenario, this, a_dataChanges);
        }
        //else if (a_t is ManufacturingOrderBatchDefinitionSetT)
        //{
        //    m_mobDefManager.Receive(this, (ManufacturingOrderBatchDefinitionSetT)a_t);
        //}
        else if (a_t is ScenarioDetailJitCompressT)
        {
            JitCompress((ScenarioDetailJitCompressT)a_t, a_dataChanges);
        }
        else if (a_t is ScenarioDetailAlternatePathLockT)
        {
            // !ALTERNATE_PATH!; ScenarioDetailAlternatePathLockT
            ProcessLockAlternatePathT((ScenarioDetailAlternatePathLockT)a_t);
        }
        else if (a_t is LotAllocationRuleT)
        {
            //Deprecated
        }
        else if (a_t is ActivityUpdateT)
        {
            ProcessActivityUpdateT((ActivityUpdateT)a_t, a_dataChanges);
        }
        else if (a_t is InventoryTransferRulesT)
        {
            ProcessInventoryTransferRuleT((InventoryTransferRulesT)a_t);
        }
        else if (a_t is MaterialIdBaseT)
        {
            JobManager.Receive((MaterialIdBaseT)a_t, WarehouseManager, a_dataChanges);
            //TODO: handle with update events, this does not affect schedulability
        }
        else if (a_t is ScenarioClockAdvanceT)
        {
            ScenarioClockAdvanceT scenarioClockAdvanceT = (ScenarioClockAdvanceT)a_t;

            // The new time should be rounded to the nearest second.
            long newClock = scenarioClockAdvanceT.time.Ticks;

            // Validate new time is in the future
            // This can happen when the ScenarioDetail clocks are desynchronised by undos.
            // Undoing a clock advance only affects the one Scenario Detail.
            // but Clock advances potentially affect all scenarios.
            if (newClock > Clock)
            {
                AdvanceClock(newClock, scenarioClockAdvanceT, a_dataChanges);
            }
#if TEST
                        if (_csSDs != null)
                        {
                         _csSDs.Receive(scenarioClockAdvanceT);
                        }
#endif
        }
        else if (a_t is ScenarioTouchT)
        {
            // Typically used to regenerate simulation data that aren't serialized (such as inventory adjustments).
            // This should result in no change to the schedule unless resource changes have been made.
            Touch(a_t);
#if TEST
                if (_csSDs != null)
                {
                    _csSDs.Receive(touchT);
                }
#endif
        }
        else if (a_t is ScenarioDetailClearPastShortTermT)
        {
            ClearJobsAndAdjustmentsAfterShortTerm(a_t, a_dataChanges);
        }
        else if (a_t is ApiAnchorT apiAnchorT)
        {
            ProcessApiAnchorT(apiAnchorT, a_dataChanges);
        }
        else if (a_t is ApiLockT apiLockT)
        {
            ProcessApiLockT(apiLockT, a_dataChanges);
        }
        else if (a_t is ApiHoldT apiHoldT)
        {
            ProcessApiHoldT(apiHoldT, a_dataChanges);
        }
        else if (a_t is ApiUnscheduleT apiUncheduleT)
        {
            UnscheduleJobs(apiUncheduleT, a_dataChanges);
        }
        else if (a_t is ApiActivityUpdateT activityUpdateT)
        {
            InternalActivityUpdateT newActivityUpdateT = new();
            newActivityUpdateT.Instigator = activityUpdateT.Instigator;

            foreach (ApiActivityUpdateT.ActivityUpdate activityUpdate in activityUpdateT.ActivityUpdates)
            {
                ExternalIdObject externalIdObject = activityUpdate.PtObjectId;
                InternalActivityUpdateT.ActivityStatusUpdate statusUpdate = new(activityUpdate.ProductionStatus, externalIdObject.JobExternalId, externalIdObject.MoExternalId, externalIdObject.OperationExternalId, externalIdObject.ActivityExternalId);
                if (activityUpdate.ReportedGoodQtyIsSet)
                {
                    statusUpdate.ReportedGoodQty = activityUpdate.ReportedGoodQty;
                }

                if (activityUpdate.ReportedScrapQtyIsSet)
                {
                    statusUpdate.ReportedScrapQty = activityUpdate.ReportedScrapQty;
                }

                if (activityUpdate.ReportedRunHrsIsSet)
                {
                    statusUpdate.ReportedRunSpan = TimeSpan.FromHours(activityUpdate.ReportedRunHrs);
                }

                if (activityUpdate.ReportedSetupHrsIsSet)
                {
                    statusUpdate.ReportedSetupSpan = TimeSpan.FromHours(activityUpdate.ReportedSetupHrs);
                }

                if (activityUpdate.ReportedPostProcessingHrsIsSet)
                {
                    statusUpdate.ReportedPostProcessingSpan = TimeSpan.FromHours(activityUpdate.ReportedPostProcessingHrs);
                }

                if (activityUpdate.ReportedStartDateIsSet)
                {
                    statusUpdate.ReportedStartDate = activityUpdate.ReportedStartDate;
                }

                if (activityUpdate.ReportedProcessingStartDateIsSet)
                {
                    statusUpdate.ReportedProcessingStartDate = activityUpdate.ReportedProcessingStartDate;
                }

                if (activityUpdate.ReportedProcessingEndDateIsSet)
                {
                    statusUpdate.ReportedProcessingEndDate = activityUpdate.ReportedProcessingEndDate;
                }


                if (activityUpdate.ReportedFinishDateIsSet)
                {
                    statusUpdate.ReportedFinishDate = activityUpdate.ReportedFinishDate;
                }

                if (activityUpdate.CommentsIsSet)
                {
                    statusUpdate.Comments = activityUpdate.Comments;
                }

                if (activityUpdate.PausedIsSet)
                {
                    statusUpdate.Paused = activityUpdate.Paused;
                }

                if (activityUpdate.HoldReasonIsSet)
                {
                    statusUpdate.HoldReason = activityUpdate.HoldReason;
                }

                if (activityUpdate.HoldUntilIsSet)
                {
                    statusUpdate.HoldUntil = activityUpdate.HoldUntil;
                }

                if (activityUpdate.OnHoldIsSet)
                {
                    statusUpdate.OnHold = activityUpdate.OnHold;
                }

                if (activityUpdate.ReportedCleanIsSet)
                {
                    statusUpdate.ReportedCleanSpan = TimeSpan.FromHours(activityUpdate.ReportedCleanHrs);
                }

                if (activityUpdate.ReportedCleanGradeIsSet)
                {
                    statusUpdate.ReportedCleanoutGrade = activityUpdate.ReportedCleanoutGrade;
                }

                newActivityUpdateT.Add(statusUpdate);
            }

            JobManager.Receive(newActivityUpdateT, this, a_dataChanges);
        }
        else if (a_t is PurchaseToStockEditT poEditsT)
        {
            PurchaseToStockManager.Receive(this, poEditsT, a_dataChanges);
        }
        else if (a_t is CustomerEditT cusT)
        {
            CustomerManager.Receive(this, cusT, a_dataChanges);
        }
        else if (a_t is SalesOrderEditT soEditsT)
        {
            SalesOrderManager.Receive(this, soEditsT, a_dataChanges);
        }
        else if (a_t is MaterialEditT materialEditT)
        {
            JobManager.Receive(this, materialEditT, a_dataChanges);
        }
        else if (a_t is JobEditT jobEditT)
        {
            JobManager.Receive(this, jobEditT, a_dataChanges);
        }
        else if (a_t is ResourceEditT resEditT)
        {
            PlantManager.Receive(this, udfManager, resEditT, a_dataChanges, m_errorReporter);
        }
        else if (a_t is InventoryEditT invEditT)
        {
            WarehouseManager.Receive(this, invEditT, a_dataChanges);
        }
        else if (a_t is StorageAreaEditT storageAreaEditT)
        {
            WarehouseManager.Receive(this, storageAreaEditT, a_dataChanges);
        }
        else if (a_t is PTAttributeBaseT ptAttributeBaseT)
        {
            AttributeManager.Receive(ptAttributeBaseT, a_dataChanges);
        }
        else if (a_t is PTAttributeEditT ptAttributeEditT)
        {
            AttributeManager.Receive(this, ptAttributeEditT, a_dataChanges);
        }
        else if (a_t is PTAttributeT ptAttributeT)
        {
            AttributeManager.Receive(this, ptAttributeT, a_dataChanges);
        }
        else if (a_t is TimeCleanoutTriggerTableBaseT timeCleanOutTriggerTableBaseT)
        {
            CleanoutTriggerTableManager.Receive(timeCleanOutTriggerTableBaseT, this, a_dataChanges);
        }
        else if (a_t is OperationCountCleanoutTriggerTableBaseT operationCleanOutTriggerTableT)
        {
            CleanoutTriggerTableManager.Receive(operationCleanOutTriggerTableT, this, a_dataChanges);
        }
        else if (a_t is ProductionUnitsCleanoutTriggerTableBaseT productionCleanOutTriggerTableT)
        {
            CleanoutTriggerTableManager.Receive(productionCleanOutTriggerTableT, this, a_dataChanges);
        }
        else if (a_t is ItemCleanoutTableBaseT itemCleanoutTableBaseT)
        {
            ItemCleanoutTableManager.Receive(itemCleanoutTableBaseT, this, a_dataChanges);
        }
        else if (a_t is CompatibilityCodeTableBaseT compatibilityCodeTableBaseT)
        {
            CompatibilityCodeTableManager.Receive(compatibilityCodeTableBaseT,  this, a_dataChanges);
        }
        else if (a_t is CompatibilityCodeTableT compatibilityErpT)
        {
            m_compatibilityCodeTableManager.Receive(compatibilityErpT, this, a_dataChanges);
        }
        else if (a_t is UserFieldDefinitionDeleteT or UserFieldDefinitionDeleteAllT)
        {
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
            {
                sm.UserFieldDefinitionManager.Receive(sm, this, (UserFieldDefinitionBaseT)a_t);
            }
        }
        else if (a_t is RefreshStagingDataStartedT or RefreshStagingDataCompletedT)
        {
            //this doesnt do anything, its just used for signalling that the Importer is busy
        }
        else
        {
            //Backwards Compatibility
            if (a_t.Recording && (a_t is ScenarioChecksumT || a_t is ScenarioUndoCheckpointT))
            {
                //For backwards compatibility with old recordings (serialization 651 and lower) don't throw these errors. There will be too many logged.
            }
            else
            {
                throw new TransmissionProcessingException("2870", new object[] { GetType().ToString(), a_t.GetType().ToString() });
            }
        }

        // [DaysOnHand:Synchronization:1]
        if (soOrForecastDemandChanges)
        {
            WarehouseManager.ProcessDemandChanges(SalesOrderManager);
        }
    }

    #region Processing Actions
    private readonly Stack<List<PostProcessingAction>> m_processingActions = new ();

    /// <summary>
    /// Pushes a List of Actions onto a stack. Before PostProcess stage, each list is popped off
    /// and list actions are executed in order.
    /// </summary>
    /// <param name="a_listOfActions"></param>
    internal void AddProcessingAction(List<PostProcessingAction> a_listOfActions)
    {
        m_processingActions.Push(a_listOfActions);
    }

    private void ExecuteProcessingActions(ScenarioExceptionInfo a_sei)
    {
        while (m_processingActions.Count > 0)
        {
            List<PostProcessingAction> nextActions = m_processingActions.Pop();
            foreach (PostProcessingAction item in nextActions)
            {
                try
                {
                    item.PostAction();
                }
                catch (PTHandleableException err)
                {
                    m_errorReporter.LogException(err, item.PTTransmission, a_sei, ELogClassification.PtSystem, false); // continue with the next actions
                }
            }
        }
    }

    /// <summary>
    /// Run any AlwaysRun actions that have not been executed.
    /// </summary>
    /// <param name="a_sei"></param>
    private void ExecuteAlwaysRunPostProcessingActions(ScenarioExceptionInfo a_sei)
    {
        while (m_processingActions.Count > 0)
        {
            List<PostProcessingAction> nextActions = m_processingActions.Pop();
            foreach (PostProcessingAction item in nextActions.Where(a => a.AlwaysRun))
            {
                try
                {
                    item.PostAction();
                }
                catch (PTException pt)
                {
                    //Always catch so we can run the remaining actions
                    m_errorReporter.LogException(pt, item.PTTransmission, a_sei, ELogClassification.PtSystem, pt.LogToSentry); // continue with the next actions
                }
                catch (Exception err)
                {
                    //Always catch so we can run the remaining actions
                    m_errorReporter.LogException(err, item.PTTransmission, a_sei, ELogClassification.Fatal, true); // continue with the next actions
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// All transmissions that require a SetTableAdjustmentChanged call
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private bool RequiresSetTableAdjustmentChanged(ScenarioBaseT a_t)
    {
        return a_t is AttributeCodeTableBaseT ||
               a_t is LookupTableDeleteAllT ||
               a_t is SetupRangeUpdateT ||
               a_t is SetupRangeBaseT ||
               a_t is LookupAttributeNumberRangeT;
    }

    /// <summary>
    /// All transmissions that require a EligibilitySignal_SetupRangeUpdated call
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private bool RequiresEligibilitySignal_SetupRangeUpdated(ScenarioBaseT a_t)
    {
        return a_t is SetupRangeUpdateT || (a_t is ScenarioDetailClearT && ((ScenarioDetailClearT)a_t).ClearAttributeNumberRangeTables);
    }

    /// <summary>
    /// All transmissions that require a RequiresBlocksChangedFired call
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private bool RequiresBlocksChangedFired(ScenarioBaseT a_t)
    {
        return a_t is ScenarioDetailSetJobPropertiesT or ScenarioDetailAnchorJobsT or ScenarioDetailAnchorMOsT or ScenarioDetailCapabilityT or ScenarioDetailLockJobsT or ScenarioDetailLockMOsT or ScenarioDetailAnchorActivitiesT or ScenarioDetailLockAndAnchorActivitiesT or ScenarioDetailLockOperationsT or ScenarioDetailLockActivitiesT or ScenarioDetailLockBlocksT or ScenarioDetailHoldJobsT or ScenarioDetailHoldMOsT or ScenarioDetailHoldOperationsT or ScenarioDetailAnchorOperationsT;
    }

    private bool RequiresComputeEligibility(ScenarioBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        //TODO: remove catch-alls.All should be encompassed in data changes flag
        return
            a_dataChanges.HasEligibilityChanges(BaseId.NULL_ID) ||
            // Resource change
            a_t is ResourceT ||
            a_t is ResourceDeleteT ||
            a_t is ResourceCopyT ||

            // Capability deletion
            a_t is CapabilityDeleteT ||
            a_t is CapabilityDeleteAllT ||

            // Capability modification
            a_t is ScenarioDetailSetCapabilityResourcesT ||
            a_t is ScenarioDetailSetCapabilitiesT ||
            a_t is ScenarioDetailCapabilityT ||

            // Plant deletion
            a_dataChanges.PlantChanges.TotalAddedObjects > 0 || //TODO: Is the add check needed?
            a_dataChanges.PlantChanges.TotalDeletedObjects > 0 ||

            // Some of the setup tables had some eligibility stuff built into them.
            m_eligibilitySignals.AnyFlagsSet ||

            // ProductRules and new capabilities were created/assigned 
            (a_t is ProductRulesT && ((ProductRulesT)a_t).GenerateCapabilitiesBaseOnProductRules);
    }

    /// <summary>
    /// Call any post processing functions required by each of the transmissions processed.
    /// </summary>
    /// <param name="a_t"></param>
    private void PostProcessT(List<ScenarioBaseT> a_processedTs, IScenarioDataChanges a_dataChanges, ScenarioExceptionInfo a_sei)
    {
        m_batchManager.RemoveDeadBatches();

        bool associatedTemplates = false;
        bool computeEligibility = false;
        bool criticalUpdates = PostProcessJobs(a_dataChanges) ||
                               AdditionalTransmissionProcessingHelper.AnyUpdatedOrDeletedJobs(a_dataChanges);

        // Loop through all transmisssions, setting flags, calling customization and additional processing.
        foreach (ScenarioBaseT procT in a_processedTs)
        {
            PTConsole.WriteLine($"Finishing transmission {procT.GetType()} | ");

            ScenarioHistoryManager.RecordScenarioHistory(procT);

            if (!associatedTemplates && (procT is JobT || procT is WarehouseT))
            {
                WarehouseManager.SetImportedTemplateMoReferences(JobManager, procT, a_sei);
                associatedTemplates = true;
            }

            if (procT is SalesOrderT)
            {
                WarehouseManager.RegenerateForecasts(this, procT);
            }

            //TODO: Replace with a_dataChange updates
            if (RequiresSetTableAdjustmentChanged(procT))
            {
                SetupTableAdjustmentChanged();
            }

            if (RequiresBlocksChangedFired(procT))
            {
                FireBlocksChangedEvent();
            }

            if (RequiresEligibilitySignal_SetupRangeUpdated(procT))
            {
                _eligibilitySignal_SetupRangeUpdated();
            }

            if (a_dataChanges.PlantChanges.TotalDeletedObjects > 0 || a_dataChanges.PlantChanges.TotalAddedObjects > 0)
            {
                m_jobManager.AdjustEligiblePlants(procT);
            }

            if (RequiresComputeEligibility(procT, a_dataChanges))
            {
                computeEligibility = true;
            }

            PostProcessingCustomizationPoint(procT, a_dataChanges);
            //Note: Don't short circuit AdditionalTransmissionProcessingWIP(). The function may perform important tasks such as linking successor MOs.
            criticalUpdates = AdditionalTransmissionProcessingWIP(procT) || criticalUpdates || procT is ScenarioDetailClearPastShortTermT || procT is ForecastT;

            if (procT is PerformImportCompletedT || procT is ScenarioDetailOptimizeT)
            {
                //If this is a short term scenairo, delete all excess data

                if (_scenario.Type == ScenarioTypes.ShortTerm)
                {
                    ClearJobsAndAdjustmentsAfterShortTerm(procT, a_dataChanges);
                    criticalUpdates = true; //Need to perform at least a time adjustment
                }
            }

            if (procT is ImportT)
            {
                SystemController.Sys.ReportScenarioImportComplete();
            }
        }

        if (computeEligibility)
        {
            if (m_jobManager.ComputeEligibility())
            {
                m_batchManager.RemoveDeadBatches();
            }

            FireJobResourceEligibilityRecomputedEventWIP();
        }

        if (a_dataChanges.HasVisualChanges(BaseId.NULL_ID))
        {
            FireBlocksChangedEvent();
        }

        if (a_processedTs.Count > 0)
        {
            // Time adjustment, constraints change adjustment, and/or fire a scenario event.
            //Clear data change signals out before the simulation. This is required since additional simulations may be processed as a result
            if (a_dataChanges.HasConstraintChanges(BaseId.NULL_ID)
                || ExtensionController.DataChanges.HasConstraintChanges(BaseId.NULL_ID))
            {
                ConstraintsChangeAdjustment(a_processedTs[0], a_dataChanges);
                a_dataChanges.SimulationProcessed = true;
            }
            else if (a_dataChanges.HasProductionChanges(BaseId.NULL_ID) || ExtensionController.DataChanges.HasProductionChanges(BaseId.NULL_ID))
            {
                TimeAdjustment(a_processedTs[0]);
                a_dataChanges.SimulationProcessed = true;
            }
            else
            {
                a_dataChanges.SimulationProcessed = false;
            }
        }

        //If this was an import, store a snapshot.
        foreach (ScenarioBaseT procT in a_processedTs)
        {
            if (procT is ImportT)
            {
                Scenario.KpiController.CalculateKPIsForImport(Scenario, this, (ImportT)procT, m_errorReporter);
                break;
            }
        }

        // Make sure the signals flags have been cleared out.
        m_signals.Clear();
        m_successorMOSignals.Clear();

        DBCalculateChecksums(a_processedTs);

        if (a_processedTs.Count > 0)
        {
            //Don't fire in a background thread so the UI can handle this event before simulation complete
            foreach (ScenarioBaseT scenarioBaseT in a_processedTs)
            {
                using (Scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                {
                    se.FireScenarioDetailTransmissionProcessedEvent(scenarioBaseT);
                }
            }
        }
    }

    /// <summary>
    /// Log MassRecordings checksum values to database.
    /// </summary>
    /// <param name="a_processedTs"></param>
    private void DBCalculateChecksums(List<ScenarioBaseT> a_processedTs)
    {
        //TODO: Removed until TFS 9020 is complete. This is logging too much data in MR database.
        //return;
        if (!PTSystem.RunningMassRecordings || PTSystem.MassRecordingsDatabase == null || a_processedTs.Count == 0)
        {
            return;
        }

        ScenarioSummary ss;
        BaseId scenarioId;

        using (_scenario.ScenarioSummaryLock.EnterRead(out ss))
        {
            scenarioId = ss.Id;
        }

        MassRecordingsTableDefinitions.ScheduleIssues tableDef = new();
        DatabaseConnections databaseConnection = new(PTSystem.MassRecordingsDatabase);

        foreach (Plant plant in PlantManager)
        {
            foreach (Department dept in plant.Departments)
            {
                foreach (Resource res in dept.Resources)
                {
                    ResourceBlockList.Node currentNode = res.Blocks.First;

                    while (currentNode != null)
                    {
                        ResourceBlock rb = currentNode.Data;

                        IEnumerator<InternalActivity> enumerator = rb.Batch.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            InternalActivity act = enumerator.Current;

                            if (act.Operation.Late)
                            {
                                string lateOpQuery = $@"INSERT INTO {tableDef.TableName} VALUES ({scenarioId}, {PTSystem.MassRecordingsSessionId}, {PTSystem.MassRecordingsPlayerPath}, '{Filtering.FilterString(a_processedTs[0].GetType().ToString())}', {a_processedTs[0].TransmissionNbr},
                                                        '{Filtering.FilterString("Operation")}', '{act.Operation.Name}', {act.Operation.Id})";
                                try
                                {
                                    databaseConnection.SendSQLTransaction(new[] { lateOpQuery });
                                }
                                catch (Exception ex)
                                {
                                    string error = ex.Message;
                                }
                            }

                            if (act.Operation.ManufacturingOrder.Job.Late)
                            {
                                string lateJobQuery = $@"INSERT INTO {tableDef.TableName} VALUES ({scenarioId}, {PTSystem.MassRecordingsSessionId}, {PTSystem.MassRecordingsPlayerPath}, '{Filtering.FilterString(a_processedTs[0].GetType().ToString())}', {a_processedTs[0].TransmissionNbr},
                                                        '{Filtering.FilterString("Job")}', '{act.Operation.ManufacturingOrder.Job.Name}', {act.Operation.ManufacturingOrder.Job.Id})";

                                try
                                {
                                    databaseConnection.SendSQLTransaction(new[] { lateJobQuery });
                                }
                                catch (Exception ex)
                                {
                                    string error = ex.Message;
                                }
                            }
                        }

                        currentNode = currentNode.Next;
                    }
                }
            }
        }

        //ChecksumValues checksums = CalculateChecksums();
        //ScheduleIssues tableDef = new ScheduleIssues();
        //PT.Common.SqlServer.DatabaseConnections m_databaseConnection = new PT.Common.SqlServer.DatabaseConnections(PTSystem.MassRecordingsDatabase);
        //string cmdAddCheckSums = $@"INSERT INTO {tableDef.TableName} VALUES ({checksums.StartAndEndSums}, {checksums.ResourceJobOperationCombos}, {checksums.BlockCount}, '{ Common.SqlServer.Filtering.FilterString(checksums.ScheduleDescription) }', { checksums.LastTransmissionNbr }, 
        //                              {scenarioId}, {PTSystem.MassRecordingsSessionId}, {PTSystem.MassRecordingsPlayerPath}, '{ Common.SqlServer.Filtering.FilterString(a_processedTs[0].GetType().ToString()) }', {a_processedTs[0].TransmissionNbr})";
        //try
        //{
        //    m_databaseConnection.SendSQLTransaction(new string[] { cmdAddCheckSums });
        //}
        //catch (Exception ex)
        //{
        //    string error = ex.Message;
        //}
    }

    /// <summary>
    /// Exception thrown when ScenarioDetail does not recognize the type of transmission
    /// it received
    /// </summary>
    public class TransmissionProcessingException : PTValidationException
    {
        public TransmissionProcessingException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false) :
            base(a_message, a_stringParameters, a_appendHelpUrl)
        { }
    }

    /// <summary>
    /// A helper class of AdditionalTransmissionProcessing() created to keep the helper functions out of ScenarioDetail.
    /// </summary>
    private class AdditionalTransmissionProcessingHelper
    {
        internal static bool AnyUpdatedOrDeletedJobs(IScenarioDataChanges a_dataChanges)
        {
            return a_dataChanges.JobChanges.TotalUpdatedObjects > 0 || a_dataChanges.JobChanges.TotalDeletedObjects > 0;
        }
    }

    /// <summary>
    /// Point at which a customization can access the received transmission.
    /// The transmission has been received and processed but has not gone through AdditionalTransmissionProcessesing()
    /// Customizations that need to operate in Scenario detail should return a result to be processed in this funtion
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    private void PostProcessingCustomizationPoint(ScenarioBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (m_extensionController.RunTransmissionProcessingExtension)
        {
            TransmissionHandlingResult custResult = m_extensionController.Postprocessing(a_t, this);
            ProcessCustomTransmissionHandlingResults(custResult, a_dataChanges);
        }
    }

    private void PreProcessingCustomizationPoint(ScenarioBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (m_extensionController.RunTransmissionProcessingExtension)
        {
            TransmissionHandlingResult custResult = m_extensionController.Preprocessing(a_t, this);
            ProcessCustomTransmissionHandlingResults(custResult, a_dataChanges);
        }
    }

    private List<ScenarioBaseT> PreProcessingTransformationCustomizationPoint(ScenarioBaseT a_t)
    {
        if (m_extensionController.RunTransmissionProcessingExtension)
        {
            return m_extensionController.PreProcessingTransformation(a_t, this);
        }

        return null;
    }

    private void ProcessCustomTransmissionHandlingResults(TransmissionHandlingResult a_custResult, IScenarioDataChanges a_dataChanges)
    {
        if (a_custResult != null)
        {
            //This region should be altered when new customizations return results.

            //Process Result
            bool constraintChanged = false;
            if (a_custResult.ActivityUpdateList.Count > 0)
            {
                foreach (TransmissionHandlingResult.ActivityChange change in a_custResult.ActivityUpdateList)
                {
                    if (change.ProductionStatus.HasValue)
                    {
                        change.Activity.ProductionStatus = change.ProductionStatus.Value;
                        constraintChanged = true;
                    }

                    if (change.Anchored.HasValue)
                    {
                        change.Activity.SetAnchor(change.Anchored.Value);
                        constraintChanged = true;
                    }

                    if (change.Locked.HasValue)
                    {
                        change.Activity.Lock(change.Locked.Value);
                        constraintChanged = true;
                    }
                }

                //Constraint has changed
                if (constraintChanged)
                {
                    a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
                }
            }
        }
    }

    /// <summary>
    /// PostProcessing for Jobs. Only needs to run once.
    /// </summary>
    /// <param name="a_dataChanges"></param>
    private bool PostProcessJobs(IScenarioDataChanges a_dataChanges)
    {
        bool unscheduled = false;

        foreach (BaseId jobId in a_dataChanges.JobChanges.Updated)
        {
            Job job = JobManager.GetById(jobId);
            if (!job.VerifyEligibilityOfScheduledResourceRequirements())
            {
                job.Unschedule();
                unscheduled = true;
            }
        }

        return unscheduled;
    }

    private bool AdditionalTransmissionProcessingWIP(ScenarioBaseT a_t)
    {
        bool critical = false;

        if (a_t is ResourceDefaultT ||
            a_t is ResourceT ||
            a_t is ResourceCopyT)
        {
            ResetResSimSeqNbrs();
        }

        // Link successor MOs
        if (m_successorMOSignals.AnyFlagsSet)
        {
            LinkSuccessorMOs();
            critical = UnscheduleMOsWithUnscheduledPredecessors();

            if (a_t is JobT)
            {
                // I only expect markers to be set when JobTs are processed.
                critical |= JobManager.HandleMarkedJobTJobs();
                if (critical)
                {
                    Batches.RemoveDeadBatches();
                }
            }
        }

        //if (a_t is InternalActivityFinishT finishT)
        //{
        //    if (finishT.NbrOfPeopleIsSet || finishT.PeopleUsageIsSet)
        //    {
        //        m_dataChangeSignals[Signal_jobChangesIdx] = true;
        //    }
        //}

        return critical ||
               m_signals.AnyFlagsSet ||
               a_t is ScenarioDetailProductRulesT;
    }

    /// <summary>
    /// We need to unschedule MOs whose predecessor MO is unscheduled. For example:
    /// A Job already exists in the system and is scheduled. A new Job is imported that has a
    /// successor MO link to the first Job. The first Job fails to schedule on
    /// ConstraintsChangeAdjustment unless we unschedule it.
    /// </summary>
    private bool UnscheduleMOsWithUnscheduledPredecessors()
    {
        bool critical = false;

        foreach (Job j in JobManager)
        {
            if (j.Template)
            {
                continue;
            }

            foreach (ManufacturingOrder mo in j.ManufacturingOrders)
            {
                if (mo.Finished || mo.Scheduled || mo.SuccessorMOs == null || mo.SuccessorMOs.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < mo.SuccessorMOs.Count; i++)
                {
                    SuccessorMO succMo = mo.SuccessorMOs[i];
                    if (succMo.SuccessorManufacturingOrder == null)
                    {
                        continue;
                    }

                    succMo.SuccessorManufacturingOrder.Unschedule();
                    critical = true;
                }
            }
        }

        return critical;
    }

    /// <summary>
    /// Called after processing any transmission that results in changed blocks, such as ScenarioDetailAnchorJobsT or ScenarioDetailLockActivitiesT.
    /// </summary>
    private void FireBlocksChangedEvent()
    {
        ScenarioEvents se;
        using (_scenario.ScenarioEventsLock.EnterRead(out se))
        {
            se.FireBlocksChangedEvent();
        }
    }

    private void FireJobResourceEligibilityRecomputedEventWIP()
    {
        ScenarioEvents se;
        using (_scenario.ScenarioEventsLock.EnterRead(out se))
        {
            se.FireJobResourceEligibilityRecomputedEvent(this);
        }
    }

    //This is a workaround to handle subsequent optimizes performed from extensions.
    private readonly Queue<(ScenarioDetailOptimizeT, OptimizeSettings)> m_cachedActiveOptimizeSettings = new();

    internal void OptimizeHandler(ScenarioDetailOptimizeT a_optimizeT, OptimizeSettings a_optimizeSettings, IScenarioDataChanges a_dataChanges)
    {
        a_optimizeSettings.VerifyLicenseConstraintsForOptimizeSettings();
        Stopwatch optimizeTimer = Stopwatch.StartNew();
        
        try
        {
            if (m_activeOptimizeSettingsT != null)
            {
                //Another optimize is being processed during another Optimize handler. This is possible from extensions
                //Store the existing settings to restore later
                m_cachedActiveOptimizeSettings.Enqueue((a_optimizeT, a_optimizeSettings));
            }

            m_activeOptimizeSettingsT = a_optimizeT;
            m_activeOptimizeSettings = a_optimizeSettings;

            //Override the original optimize settings with values from the transmission
            if (m_activeOptimizeSettingsT.SpecificStartTimeSet)
            {
                m_activeOptimizeSettings.StartTime = OptimizeSettings.ETimePoints.SpecificDateTime;
                m_activeOptimizeSettings.SpecificStartTime = a_optimizeT.SpecificStartTime;
            }

            if (m_activeOptimizeSettingsT.SpecificPlantIdSet)
            {
                a_optimizeSettings.ResourceScope = OptimizeSettings.resourceScopes.OnePlant;
                m_activeOptimizeSettings.PlantToInclude = a_optimizeT.SpecificPlantId;
            }
            else
            {
                a_optimizeSettings.ResourceScope = OptimizeSettings.resourceScopes.All;
                m_activeOptimizeSettings.PlantToInclude = BaseId.NULL_ID;
            }
            
            if (m_activeOptimizeSettingsT.RunMRP)
            {
                m_activeOptimizeSettings.RunMrpDuringOptimizations = true;
                MRP(m_activeOptimizeSettings, a_optimizeT, a_dataChanges);
            }
            else
            {
                m_activeOptimizeSettings.RunMrpDuringOptimizations = false;
                Optimize(a_optimizeT, a_dataChanges);
            }
        }
        finally
        {
            if (m_cachedActiveOptimizeSettings.Count > 0)
            {
                //An optimize is complete, but not the original, dequeue and overwrite the current active settings.
                (ScenarioDetailOptimizeT scenarioDetailOptimizeT, OptimizeSettings optimizeSettings) = m_cachedActiveOptimizeSettings.Dequeue();
                m_activeOptimizeSettingsT = scenarioDetailOptimizeT;
                m_activeOptimizeSettings = optimizeSettings;
            }
            else
            {
                //Last optimize is complete.
                m_activeOptimizeSettingsT = null;
                m_activeOptimizeSettings = null;
            }

            optimizeTimer.Stop();
            RecordActionDuration("Optimize", optimizeTimer.ElapsedMilliseconds, a_dataChanges);
        }
    }
    
    /// <summary>
    /// Handle ScenarioDetailMoveT and any transmissions that inherit from it, such as Alternate Path moves.
    /// </summary>
    /// <param name="a_moveT"></param>
    // [BATCH_CODE]
    private bool HandleScenarioDetailMoveTAndDerivatives(ScenarioDetailMoveT a_moveT, UndoReceive a_undoReapply, IScenarioDataChanges a_dataChanges)
    {
        SimulationType simType = a_moveT.ExpediteSuccessors || a_moveT.ExpediteSpecificSuccessors ? SimulationType.MoveAndExpedite : SimulationType.Move;
        MoveResult moveResult = new(a_moveT);
        Stopwatch moveTimer = Stopwatch.StartNew();

        try
        {
            MoveData md = CreateMoveInfo(a_moveT, moveResult);

            if (md.Count == 0)
            {
                moveResult.SetFailureReason(MoveFailureEnum.AllBlocksHadProblems);
                HandleMoveFailure(a_moveT, simType, moveResult, a_dataChanges);
                return false;
            }

            // Setup activities that must stay on their current resources.
            // They are kept on their current resource by temporairily locking them to their resources.
            // They're locked before the Move and unlocked after the move.
            List<InternalActivity> unlockKeepOnCurResActs = new();
            IEnumerator<InternalActivity> keepOnEtr = md.GetKeepOnCurResActivitiesEnumerator();
            while (keepOnEtr.MoveNext())
            {
                InternalActivity ia = keepOnEtr.Current;
                if (ia.Locked == lockTypes.Unlocked && !a_moveT.LockMove)
                {
                    ia.Lock(true);
                    unlockKeepOnCurResActs.Add(ia);
                }
            }

            if (HandleMoveFailure(a_moveT, simType, moveResult, a_dataChanges))
            {
                // The move failed.
                return false;
            }

            SimulationProgress.FireSimulationProgressEvent(this, simType, a_moveT, SimulationProgress.Status.Initializing, m_nbrOfSimulationsSinceStartup);

            ScenarioDetailAlternatePathMoveT apMoveT = a_moveT as ScenarioDetailAlternatePathMoveT;
            m_undoReapplyMoveT = (Simulation.UndoReceive.Move.UndoReceiveMove)a_undoReapply;
            m_moveData = md;

            Dictionary<ActivityKey, ActivityMoveAudit> moveAudits = new Dictionary<ActivityKey, ActivityMoveAudit>();

            ActivityPreMoveAudit(md, moveAudits);
            
            CancelSimulationEnum cancelSim;
            if (apMoveT != null)
            {
                cancelSim = MoveAlternatePath(apMoveT, md, moveResult, a_dataChanges);
            }
            else
            {
                cancelSim = Move(md, moveResult, a_dataChanges);
            }

            if (cancelSim == CancelSimulationEnum.Cancel)
            {
                HandleMoveFailure(a_moveT, simType, moveResult, a_dataChanges);
                return false;
            }
            else
            {
                ActivityPostMoveAudit(a_dataChanges, md, moveAudits);
            }

            // Unlock activities that weren't allowed to be moved from their current resources.
            foreach (InternalActivity ia in unlockKeepOnCurResActs)
            {
                ia.Lock(false);
            }

            if (HandleMoveFailure(a_moveT, simType, moveResult, a_dataChanges))
            {
                // The move failed.
                return false;
            }

            ScenarioEvents scenarioEvents;
            using (_scenario.AutoEnterScenarioEvents(out scenarioEvents))
            {
                scenarioEvents.FireMoveFinishedEvent(this, moveResult);
            }

            SimulationActionComplete();
            m_simulationProgress.PostSimulationWorkComplete();

            CheckForRequiredAdditionalSimulation(a_dataChanges);

            return true;
        }
        catch (SimulationValidationException e)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_moveT, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
            FireSimulationValidationFailureEvent(e, a_moveT);
            throw;
        }
        catch (Exception)
        {
            SimulationProgress.FireSimulationProgressEvent(this, simType, a_moveT, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
            throw;
        }
        finally
        {
            moveTimer.Stop();
            RecordActionDuration("Move", moveTimer.ElapsedMilliseconds, a_dataChanges);
        }
    }

   
    private void UpdateProductRules(ProductRulesT a_t, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new();
        List<PostProcessingAction> actions = new();

        try
        {
            ItemManager.InitFastLookupByExternalId();

            HashSet<ProductRuleKey> affectedProductRulesHash = new();

            Hashtable affectedResourcesHash = new();

            Hashtable affectedCapabilitiesHash = new();

            foreach (ProductRulesT.ProductRule rule in a_t.ProductRules)
            {
                try
                {
                    Item item = ItemManager.GetByExternalId(rule.ProductItemExternalId);
                    if (item == null)
                    {
                        throw new PTValidationException("2576", new object[] { rule.ProductItemExternalId });
                    }

                    Resource resource = PlantManager.GetResource(rule.PlantExternalId, rule.DepartmentExternalId, rule.ResourceExternalId);
                    if (resource == null)
                    {
                        throw new PTValidationException("2577", new object[] { rule.PlantExternalId, rule.DepartmentExternalId, rule.ResourceExternalId });
                    }

                    ProductRule pr = m_productRuleManager.AddOrUpdateProductRule(this, rule, resource, item, a_dataChanges);

                    affectedProductRulesHash.Add(pr.GetKey());

                    if (a_t.GenerateCapabilitiesBaseOnProductRules)
                    {
                        //Add a Capability for each Product Rule.
                        string capabilityName;
                        if (!string.IsNullOrEmpty(rule.ProductCode.Trim()))
                        {
                            capabilityName = string.Format("{0} Op {1}".Localize(), item.Name, rule.ProductCode);
                        }
                        else
                        {
                            capabilityName = string.Format("{0}", item.Name);
                        }

                        Capability capability = CapabilityManager.GetByName(capabilityName);
                        if (capability == null) //add a new Capability
                        {
                            capability = CapabilityManager.AutoAdd(capabilityName, capabilityName, a_dataChanges);
                        }

                        if (resource != null) //link to the Resource
                        {
                            ResourceKey resKey = resource.GetKey();
                            if (!capability.Resources.Contains(resKey))
                            {
                                capability.AddResourceAssociation(resource);
                                resource.AddCapability(capability);
                                if (!affectedResourcesHash.Contains(resKey))
                                {
                                    a_dataChanges.MachineChanges.UpdatedObject(resource.Id);
                                    affectedResourcesHash.Add(resKey, null);
                                }

                                if (!affectedCapabilitiesHash.Contains(capability.Id))
                                {
                                    affectedCapabilitiesHash.Add(capability.Id, null);
                                    a_dataChanges.CapabilityChanges.UpdatedObject(capability.Id);
                                }
                            }
                        }
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                actions.Add(new PostProcessingAction(a_t, false, () =>
                {
                    ApplicationExceptionList delErrList = new();
                    try
                    {
                        m_productRuleManager.AutoDelete(affectedProductRulesHash);
                    }
                    catch (PTHandleableException valErr)
                    {
                        delErrList.Add(valErr);
                    }
                    finally
                    {
                        if (delErrList.Count > 0)
                        {
                            ScenarioExceptionInfo sei = new();
                            sei.Create(this);
                            m_errorReporter.LogException(delErrList, a_t, sei, ELogClassification.PtInterface, false);
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
            actions.Add(new PostProcessingAction(a_t, true, () => { ItemManager.DeInitFastLookupByExternalId(); }));
            AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new();
                sei.Create(this);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    /// <summary>
    /// Records a duration audit entry for a scenario-level action without triggering automatic comparison.
    /// </summary>
    private void RecordActionDuration(string a_actionName, long a_durationMs, IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges == null)
        {
            return;
        }

        //AuditEntry durationAudit = new AuditEntry(_scenario.Id, _scenario);
        //durationAudit.AddManualChange($"{a_actionName}DurationMs", 0, a_durationMs);
        //a_dataChanges.AuditEntry(durationAudit);
    }
}
