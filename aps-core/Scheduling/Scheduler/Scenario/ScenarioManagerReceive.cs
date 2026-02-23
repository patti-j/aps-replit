using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.Http;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Extensions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.Scheduler.CoPilot;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.SystemDefinitions.Interfaces;
using PT.SystemServiceDefinitions.Headers;
using PT.Transmissions;
using PT.Transmissions.User;
using PT.Transmissions2;

using static PT.Scheduler.UserManager;

namespace PT.Scheduler;

public partial class ScenarioManager
{

    IntegrationConfigMappingSettings m_settings = new();
    /// <summary>
    /// Check whether license allows this type of transmission. There are two types of CoPilot, RuleSeek and InsertJobs.
    /// </summary>
    /// <param name="a_t">either </param>
    private static void CoPilotLicenseCheck(ScenarioBaseT a_t)
    {
        if (!PTSystem.LicenseKey.IncludeCoPilot)
        {
            string action = "CoPilot-";

            if (a_t is ScenarioRuleSeekStartT)
            {
                action += "RuleSeek";
            }
            else if (a_t is InsertJobsStartT)
            {
                action += "InsertJobs";
            }
            else
            {
                return; // in case this was called by mistake.
            }

            throw new AuthorizationException(action, AuthorizationType.LicenseKey, "IncludeCoPilot", false.ToString());
        }
    }

    public void Receive(ScenarioBaseT a_t)
    {
        int scenariosProcessed = 0;
        bool tranmissionAlreadyProcessed = false;
        DateTime processingStart = DateTime.UtcNow;
        try
        {
            if (!PTSystem.Server && a_t.ClientWillWaitForScenarioResult && !(a_t is ScenarioReplaceT))
            {
                if (a_t is ScenarioIdBaseT scenarioIdT)
                {
                    DispatchScenario(scenarioIdT.ScenarioId, a_t);
                    scenariosProcessed = 1;
                }

                //Don't process this transmission but fire an event so that the UI knows the system is processing.
                FireSystemProcessingEvent(a_t);

                //Also need to do something to lock the SD
                return;
            }
            else if (a_t is CoPilotSettingsChangeT)
            {
                //Update any settings that have changed.
                ProcessCoPilotSettingsChangeT(a_t);
            }
            else if (a_t is RuleSeekCompletionT ruleSeekCompletionT)
            {
                FireRuleSeekStoppedEvent(ruleSeekCompletionT.EndReason);
            }
            else if (a_t is CoPilotStatusUpdateT copilotStatusT)
            {
                FireRuleSeekStatusEvent(copilotStatusT);
            }
            else if (a_t is ScenarioRuleSeekStartT)
            {
                if (PTSystem.Server)
                {
                    CoPilotLicenseCheck(a_t);
                    ProcessScenarioRuleSeekStartT(a_t);
                }
            }
            else if (a_t is InsertJobsStartT)
            {
                if (PTSystem.Server)
                {
                    CoPilotLicenseCheck(a_t);
                    ProcessInsertJobsStartT(a_t as InsertJobsStartT);
                }
            }
            else if (a_t is InsertJobsUserEndT)
            {
                if (PTSystem.Server)
                {
                    InsertJobsUserEndT endT = a_t as InsertJobsUserEndT;
                    // We're trying to move away from sending transmissions about these types of updates,
                    // but if the InsertJobsManager is not in a state where the Insert Jobs process can be stopped/cancelled,
                    // then we shouldn't change its state to be STOPPED or CANCELED. 
                    // However, the user still  needs to be notified somehow of the cancel process not completing. 
                    if (m_insertJobsManager == null)
                    {
                        SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.InsertJobsNotStarted);
                        return;
                    }
                    else if (m_insertJobsManager.Status == CoPilotSimulationStatus.INITIALIZING)
                    {
                        SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.InsertJobIsInitializing);
                        return;
                    }
                    else if (m_insertJobsManager.Status != CoPilotSimulationStatus.RUNNING) // implicit AND m_insertJobsManager.Status == CoPilotSimulationStatus.INITIALIZING
                    {
                        SendCoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Error, CoPilotStatusUpdateT.CoPilotErrorValues.InsertJobsNotStarted);
                        return;
                    }

                    if (endT.EndType == InsertJobsUserEndT.EndTypes.Stop)
                    {
                        m_insertJobsManager.StopAndSendScenario(a_t.Instigator);
                    }
                    else if (endT.EndType == InsertJobsUserEndT.EndTypes.Cancel)
                    {
                        m_insertJobsManager.CancelInsertJobsSimulation();
                    }
                }
            }
            else if (a_t is CoPilotDiagnositcsUpdateT)
            {
                #if DEBUG
                #else
                if(!PTSystem.Server)
                #endif
                {
                    CoPilotDiagnositcsUpdateT updateT = a_t as CoPilotDiagnositcsUpdateT;
                    m_insertJobsDiagnostics = updateT.InsertJobsDiagnostics;
                    m_ruleSeekDiagnostics = updateT.RuleSeekDiagnostics;
                }
            }
            else if (a_t is ScenarioAddNewT addT)
            {
                if (addT is ScenarioReplaceT rT && !rT.ContainsScenario && !PTSystem.Server)
                {
                    //If scenario undo failed trigger undo complete with false
                    if (rT.InstigatorTransmissionId == ScenarioUndoT.UNIQUE_ID)
                    {
                        Scenario scenario = Find(rT.ScenarioToReplaceId);
                        FireUndoComplete(rT, scenario, false);
                    }
                    else
                    {
                        ScenarioReplaceFailedEvent.Invoke(addT.OriginalInstigatorId, rT);
                    }
                    return;
                }

                if (addT is ScenarioReplaceT replaceT && replaceT.ClientWillWaitForScenarioResult)
                {
                   ReloadScenario(replaceT.ScenarioToReplaceId, replaceT);
                }
                else if (addT is ScenarioAddNewPrunedT)
                {
                    //Add the new scenario, convert it to live, and delete the other scenarios
                    ProcessScenarioAddNewT(addT);
                    DeleteAllScenariosExceptOfType(ScenarioTypes.Live);
                    SystemController.StopAPSService("ScenarioAddNewPrunedT");
                    File.Copy(ServerSessionManager.SystemDataFileFullPath, Path.Combine(PTSystem.WorkingDirectory.Pruned, "0000000000." + ServerSessionManager.SystemDataFileName), true);
                    Environment.Exit(0);
                }
                else
                {
                    //Process a ScenarioAddNew
                    ProcessScenarioAddNewT(addT);
                }
            }
            else if (a_t is ScenarioIdBaseT idBaseT)
            {
                if (idBaseT is ScenarioDetailExportT sdeT)
                {
                    if (PTSystem.Server)
                    {
                        CoPilotSimulationUtilities simulationUtilities = new (m_packageManager);

                        Scenario s = sdeT.ScenarioId == BaseId.NULL_ID ? GetFirstProductionScenario() : Find(sdeT.ScenarioId);
                        //Copy the scenario so it can be modified without affecting the original.
                        simulationUtilities.CopyAndStoreScenario(s, out byte[] sdByteArray, out byte[] ssByteArray);
                        Scenario publishScenario = simulationUtilities.CopySimScenario(sdByteArray, ssByteArray);
                        using (publishScenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
                        {
                            sd.TimeAdjustment(new ScenarioTouchT());
                            publishScenario.KpiController.CalculateKPIs(sd, sdeT.TransmissionNbr, "Publish", sdeT.TimeStamp, KpiOptions.ESnapshotType.Publish, m_errorReporter);
                        }

                        // Since we copied the scenario, the copied scenario has not gone through KPI calculations
                        SystemController.PublishHelper.QueueCopiedScenarioForExport(publishScenario, sdeT);
                    }
                }

                if (idBaseT is ScenarioChangeT pendingChangeT)
                {
                    //Validate change is allowed

                    ValidateScenarioChange(pendingChangeT);

                    if (pendingChangeT.ScenarioSettings.Any(s => s.Key == ScenarioPlanningSettings.Key))
                    {
                        SettingData planningData = pendingChangeT.ScenarioSettings.First(s => s.Key == ScenarioPlanningSettings.Key);
                        ScenarioPlanningSettings planningSettings = planningData.DeserializeSettings<ScenarioPlanningSettings>();

                        ValidateScenarioComparisonLimit(pendingChangeT.ScenarioId, planningSettings);
                    }
                }

                if (PTSystem.Server)
                {
                    ValidateTransmissionForRuleSeekTimerUpdate(idBaseT);
                }

                scenariosProcessed = PassToScenario(idBaseT);
                if (idBaseT is ScenarioChangeT scenarioChangeT)
                {
                    FireScenarioChangedEvent(scenarioChangeT);
                }
            }
            else if (a_t is IScenarioIdBaseT)
            {
                UpdateRuleSeekTimer(RuleSeekEndReasons.ScenarioAction);
                IScenarioIdBaseT tt = (IScenarioIdBaseT)a_t;
                DispatchScenario(tt.ScenarioId, a_t);
                scenariosProcessed = 1;
            }
            else if (a_t is UserFieldDefinitionT udfDefT)
            {
                m_userFieldDefinitionManager.Receive(this, udfDefT);
            }
            else if (a_t is ERPTransmission erpT)
            {
                if (!(a_t is PerformImportBaseT))
                {
                    UpdateRuleSeekTimer(RuleSeekEndReasons.ERP);
                    if (a_t is ImportT importT)
                    {
                        ProcessUserChangesFromImport(importT);

                        ProcessUserFieldDefinitionTFromImport(importT);

                        using (SystemController.Sys.ScenariosLock.EnterRead())
                        {
                            foreach (Scenario s in m_loadedScenarios.Values)
                            {
                                if (PTSystem.EnableDiagnostics)
                                {
                                    PTSystem.SystemLogger.Log("Import Logging", "Getting ScenarioPlanningSettings");
                                }

                                ScenarioPlanningSettings settings;
                                using (s.AutoEnterScenarioSummary(out ScenarioSummary ss))
                                {
                                    settings = ss.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);
                                }

                                if (PTSystem.EnableDiagnostics)
                                {
                                    PTSystem.SystemLogger.Log("Import Logging", "Checking scenario id");
                                }

                                if ((importT.UseSpecificScenarioId && importT.SpecificScenarioId == s.Id) //Importing to this specific scenario 
                                    ||
                                    //Scenario is not isolated. Importing to all scenarios.
                                    //This is a legacy path - IsolateFromImport no  longer used, and config-based imports don't work this way (a v2 import should never have nully values for scenarioId and configId at once)
                                    (((SystemController.ImportingType == EImportingType.IntegrationV1) ? !settings.IsolateFromImport : true) && !importT.UseSpecificScenarioId && !importT.UseSpecificConfigId)
                                    ||
                                    (IsPartOfConfigImport(a_t, importT, s)))  //For ImportT's with Configs
                                {
                                    if (PTSystem.EnableDiagnostics)
                                    {
                                        PTSystem.SystemLogger.Log("Import Logging", "Scenario Id check passed, dispatching importT");
                                    }
                                    scenariosProcessed++;
                                    importT = (ImportT)Serialization.CopyInMemory(importT, m_transmissionClassFactory);
                                    SetRecordingsForTransmission(a_t, importT);
                                    s.Dispatch(importT);
                                }
                                else
                                {
                                    if (PTSystem.EnableDiagnostics)
                                    {
                                        PTSystem.SystemLogger.Log("Import Logging", "Scenario Id check failed, report scenario info complete");
                                    }

                                    SystemController.Sys.ReportScenarioImportComplete();
                                }
                            }
                        }
                    }
                    else
                    {
                        PTSystem.SystemLogger.Log("Import Logging", $"Passing {a_t.GetType().Name} to all {m_loadedScenarios.Count} scenarios.");
                        scenariosProcessed = m_loadedScenarios.Count;
                        PassToAllScenarios(erpT);
                    }
                }
                else
                {
                    if (a_t is RefreshStagingDataStartedT or RefreshStagingDataCompletedT)
                    {
                        PassToAllScenarios(a_t);
                    }
                }
            }
            else if (a_t is ScenarioClockAdvanceT caT)
            {
                UpdateRuleSeekTimer(RuleSeekEndReasons.ClockAdvance);
                using (SystemController.Sys.ScenariosLock.EnterRead())
                {
                    foreach (Scenario s in m_loadedScenarios.Values)
                    {
                        ScenarioPlanningSettings settings;
                        using (s.AutoEnterScenarioSummary(out ScenarioSummary ss))
                        {
                            settings = ss.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);
                        }

                        if ((caT.UseSpecificScenarioId && caT.SpecificScenarioId == s.Id) //Clock Advancing this specific scenario 
                            ||
                            (!settings.IsolateFromClockAdvance && !caT.UseSpecificScenarioId)) //Scenario is not isolated. Clock Advancing to all scenarios
                        {
                            scenariosProcessed++;
                            DispatchScenario(s.Id, caT);
                        }
                    }
                }
            }
            else if (a_t is ScenarioCopyT copyT)
            {
                if (PTSystem.Server)
                {
                    ProcessScenarioCopy(copyT);
                }
                else
                {
                    //Notify the UI that a new scenario is being copied
                    ScenarioBeforeNewEvent?.Invoke(copyT);
                }
            }
            else if (a_t is Transmissions.CTP.CtpT ctpT)
            {
                Ctp(ctpT, new ScenarioDataChanges());
            }
            else if (a_t is ScenarioDeleteT deleteT)
            {
               ValidateDelete(deleteT);
               Delete(deleteT);
            }
            else if (a_t is ScenarioPublishT publishT)
            {
                Publish((ScenarioPublishT)a_t);
            }
            else if (a_t is ScenarioTouchT touchT)
            {
                Touch(touchT);
                scenariosProcessed = m_loadedScenarios.Count;
            }
            else if (a_t is SystemOptionsT systemOptionsT)
            {
                SetSystemOptions(systemOptionsT);
                scenariosProcessed = m_loadedScenarios.Count;
            }
            else if (a_t is SystemPublishOptionsT publishOptionsT)
            {
                SetSystemPublishOptions(publishOptionsT);
                scenariosProcessed = m_loadedScenarios.Count;
            }
            else if (a_t is ShopViewSystemOptionsUpdateT ShopViewOptionsT)
            {
                using (SystemController.Sys.ScenariosLock.EnterWrite())
                {
                    ShopViewSystemOptions.Receive(ShopViewOptionsT);
                }
            }
            else if (a_t is ScenarioManagerUndoSettingsT undoSettingsT)
            {
                ScenarioManagerUndoSettingsHandler(undoSettingsT);
            }
            else if (a_t is PruneScenarioT pruneT)
            {
                PruneScenario(pruneT);
            }
            else if (a_t is ConvertToProductionScenarioT convertT)
            {
                Scenario newProductionScenario = Find(convertT.NonProductionScenarioId);

                Scenario currentProdScenario = GetFirstProductionScenario();

                if (!PTSystem.Server)
                {
                    if (currentProdScenario != null)
                    {
                        if (convertT.AutoDeleteNonProdScenario)
                        {
                            using (newProductionScenario.ScenarioEventsLock.EnterWrite(out ScenarioEvents whatIfEvents))
                            {
                                using (newProductionScenario.ScenarioUndoEventsLock.EnterWrite(out ScenarioUndoEvents whatIfSue))
                                {
                                    whatIfEvents.FireScenarioPromotionEvent(whatIfSue, currentProdScenario.Id, newProductionScenario.Id, convertT.AutoDeleteNonProdScenario);
                                }

                            }
                        }

                        using (currentProdScenario.ScenarioEventsLock.EnterWrite(out ScenarioEvents events))
                        {
                            using (currentProdScenario.ScenarioUndoEventsLock.EnterWrite(out ScenarioUndoEvents sue))
                            {
                                events.FireScenarioPromotionEvent(sue, currentProdScenario.Id, newProductionScenario.Id, convertT.AutoDeleteNonProdScenario);
                            }
                        }
                    }
                    return;
                }
                //The actual conversion to production scenario is only processed on the server

                //Get Scenario Data for whatIf
                newProductionScenario.CopyInMemory(out ScenarioDetail o_sd, out ScenarioSummary o_ss);

                currentProdScenario.SwapProductionData(o_sd);

                byte[] clientScenarioBytes = currentProdScenario.GetClientScenarioBytes();

                ScenarioReplaceT replaceT = new ScenarioReplaceT(currentProdScenario.Id, clientScenarioBytes);
                replaceT.InstigatorTransmissionId = convertT.UniqueId;
                replaceT.OriginalInstigatorId = convertT.Instigator;
                SystemController.ClientSession.SendClientAction(replaceT);

                //Remove non production scenario
                if (convertT.AutoDeleteNonProdScenario)
                {
                    ScenarioDeleteT nonProdScenarioDeleteT = new ScenarioDeleteT(convertT.NonProductionScenarioId);
                    nonProdScenarioDeleteT.OriginalInstigatorId = convertT.Instigator;
                    SystemController.ClientSession.SendClientAction(nonProdScenarioDeleteT);
                }
            }
            else if (a_t is MergeScenarioDataT mergeDataT)
            {
                Scenario targetScenario = Find(mergeDataT.TargetScenarioId);
                Scenario sourceScenario = Find(mergeDataT.SourceScenarioId);

                using (sourceScenario.UndoSetsLock.EnterRead(out Scenario.UndoSets undoSets))
                {
                    targetScenario.PlayTransmissionsInUndoSets(undoSets);
                }
            }
            else if (a_t is UserFieldDefinitionBaseT udfDefBaseT)
            {
                m_userFieldDefinitionManager.Receive(this, udfDefBaseT);

                if (a_t is UserFieldDefinitionDeleteT or UserFieldDefinitionDeleteAllT)
                {
                    PassToAllScenarios(a_t);
                }
            }
            else if (a_t is ScenarioLoadT scenarioLoadT)
            {
                if (PTSystem.Server)
                {
                    // Leaving this here because we want to load and unload scenarios on the server eventually too
                }
                else if (scenarioLoadT.UserRequestingLoadId == SystemController.CurrentUserId)
                {
                    GetScenarioBytesThenLoadScenario(scenarioLoadT.ScenarioToLoadId, scenarioLoadT);
                }
                // Ignore the transmission if the receiver is not the user who sent
            }
            else if (a_t is ScenarioUnloadT scenarioUnloadT)
            {
                if (PTSystem.Server)
                {
                    // Leaving this here because we want to load and unload scenarios on the server eventually too
                }
                else if (scenarioUnloadT.UserRequestingUnloadId == SystemController.CurrentUserId)
                {
                    UnloadScenario(scenarioUnloadT.ScenarioToUnloadId, scenarioUnloadT.UserRequestingUnloadId);
                }
                // Ignore the transmission if the receiver is not the user who sent
            }
            else if (a_t is ScenarioReloadT scenarioReloadT)
            {
                if (PTSystem.Server)
                {
                    byte[] scenarioBytes = SystemController.ServerSessionManager.GetScenarioBytes(scenarioReloadT.ScenarioToReloadId);
                    // Send ScenarioReplaceT here   
                }
                else if (scenarioReloadT.UserRequestingReloadId == SystemController.CurrentUserId)
                {
                    ReloadScenario(scenarioReloadT.ScenarioToReloadId, scenarioReloadT);
                }
            }

            TransmissionPostProcessing(a_t);
        }
        catch (Exception e)
        {
            if (e is TransmissionValidationException transmissionValidationException && a_t is ScenarioDeleteT scenarioDeleteT)
            {
                ScenarioDeleteFailedValidationEvent?.Invoke(scenarioDeleteT.Instigator, scenarioDeleteT.scenarioId, transmissionValidationException.Message);
            }
            else
            {
                throw;
            }
        }
        finally
        {
            FireTransmissionProcessedEvent(a_t, processingStart, scenariosProcessed, tranmissionAlreadyProcessed);
        }
    }

    private void ProcessScenarioCopy(ScenarioCopyT a_copyT)
    {
        Scenario newScenario = null;

        try
        {
            ////Copy a scenario and update rule sets
            if (a_copyT is ScenarioCopyForRuleSeekT seekT)
            {
                BaseId newScenarioId = ScenarioCopy(seekT);
                newScenario = Find(newScenarioId);
                //Update Scenario Info for later use.
                using (newScenario.ScenarioSummaryLock.EnterWrite(out ScenarioSummary ss))
                {
                    ss.RuleSeekScore = seekT.Score;
                }

                //Update the optimize rule and start the optimize so the result will be displayed when the scenario is activated.
                //We don't need to wait for this to complete, continue processing new transmissions.
                Task.Run(new Action(() =>
                {
                    using (newScenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
                    {
                        //Update modified rules
                        foreach (BalancedCompositeDispatcherDefinitionUpdateT updateT in seekT.RuleUpdateList)
                        {
                            sd.DispatcherDefinitionManager.Receive(updateT, new ScenarioDataChanges());
                        }

                        //Optimize the scenario
                        sd.OptimizeHandler(new ScenarioDetailOptimizeT(newScenarioId, null, false), sd.OptimizeSettings, new ScenarioDataChanges());
                    }
                }));
            }
            else if (a_copyT is ScenarioCopyForInsertJobsT insertT)
            {
                //InsertJobs has finished with a list of expedite transmissions.
                BaseId newScenarioId = ScenarioCopy(insertT);
                newScenario = Find(newScenarioId);

                using (newScenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
                {
                    //Send the series of expedites.
                    foreach (ScenarioDetailExpediteJobsT expediteT in insertT.ExpediteList)
                    {
                        sd.Receive(expediteT, new ScenarioDataChanges());
                    }
                }
            }
            else
            {
                BaseId newScenarioCopyId = ScenarioCopy(a_copyT);
                if (m_ruleSeekManager != null)
                {
                    int scenariosRemaining = m_maxNumberOfScenarios - LoadedScenarioCount;
                    m_ruleSeekManager.UpdateScenarioLimitSetting(scenariosRemaining);
                }

                newScenario = Find(newScenarioCopyId);
            }
        }
        finally
        {
            byte[] scenarioBytes = newScenario?.GetClientScenarioBytes();

            ScenarioReplaceT replaceT = new ScenarioReplaceT(newScenario?.Id ?? BaseId.NULL_ID, scenarioBytes ?? []);
            replaceT.OriginalInstigatorId = a_copyT.Instigator;
            replaceT.InstigatorTransmissionId = a_copyT.UniqueId;
            SystemController.ClientSession.SendClientAction(replaceT);
        }
        
    }

    /// <summary>
    /// An ImportT that has a configId, but no specific scenarioId should be applied to all scenarios of that config
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_importT"></param>
    /// <param name="a_s"></param>
    /// <param name="a_settings"></param>
    /// <returns></returns>
    private bool IsPartOfConfigImport(ScenarioBaseT a_t, ImportT a_importT, Scenario a_s)
    {
        IntegrationConfigMappingSettings integrationConfigPerScenarioMappingSettings = new IntegrationConfigMappingSettings();
        if (!a_importT.UseSpecificScenarioId && a_importT.UseSpecificConfigId)
        {
            using (a_s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                integrationConfigPerScenarioMappingSettings = ss.ScenarioSettings.LoadSetting(integrationConfigPerScenarioMappingSettings);
                return integrationConfigPerScenarioMappingSettings.IntegrationConfigId == a_importT.SpecificConfigId;
            }
        }

        return false;
    }

    private void ProcessUserFieldDefinitionTFromImport(ImportT a_importT)
    {
        for (int cI = 0; cI < a_importT.Count; cI++)
        {
            ScenarioBaseT scenarioT = a_importT[cI];
            if (scenarioT is UserFieldDefinitionT userFieldDefT)
            {
                Receive(userFieldDefT);
            }
            else if (scenarioT is ScenarioDetailClearT clearDataT)
            {
                Receive(clearDataT);
            }
        }
    }

    private void ProcessUserChangesFromImport(ImportT a_importT)
    {
        for (int cI = 0; cI < a_importT.Count; cI++)
        {
            ScenarioBaseT scenarioT = a_importT[cI];
            if (scenarioT is IDataChangesDependentT<UserT> userT)
            {
                Receive(userT);
            }
        }
    }

    /// <summary>
    /// Handles transmissions that need to be processed with <see cref="IScenarioDataChanges" /> by wrapping them in data change handling behavior.
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    /// <returns></returns>
    /// <exception cref="PTException"></exception>
    internal void Receive(IDataChangesDependentT<PTTransmission> a_t)
    {
        DateTime processingStart = DateTime.UtcNow;
        Exception error = null;
        try
        {
            UserManagerEventCaller umEventCaller = new ();
            IScenarioDataChanges dataChanges = new ScenarioDataChanges();

            if (a_t is IDataChangesDependentT<UserT> or IDataChangesDependentT<UserBaseT>)
            {
                ReceiveWithDataChangeTrackingInternal(UserFieldDefinitionManager, a_t, dataChanges, umEventCaller);

                using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
                {
                    if (dataChanges.HasChanges)
                    {
                        ume.FireScenarioDataChangedEvent(dataChanges);
                    }

                    umEventCaller.CallEvents(ume);
                }
            }
            //else if (a_t is IDataChangesDependentT<UserFieldDefinitionT>)
            //{
            //    ReceiveWithDataChangeTrackingInternal(a_t, dataChanges);
            //}
        }
        catch (PTException pt)
        {
            error = pt;
            m_errorReporter.LogException(pt, a_t as PTTransmission, ELogClassification.PtSystem, pt.LogToSentry);
        }
        catch (Exception e)
        {
            error = e;
            m_errorReporter.LogException(e, a_t as PTTransmission, ELogClassification.Fatal, false);
        }
        finally
        {
            if (a_t is IDataChangesDependentT<UserT> or IDataChangesDependentT<UserBaseT>)
            {
                using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
                {
                    ume.FireTransmissionProcessedEvent(a_t as PTTransmission, DateTime.UtcNow - processingStart, error);
                }
            }
            else
            {
                SystemController.Sys.FireTransmissionProcessedEvent(a_t as PTTransmission, DateTime.UtcNow - processingStart, error);
            }
        }
    }

    //private void ReceiveWithDataChangeTrackingInternal(IDataChangesDependentT<PTTransmissionBase> a_t, IScenarioDataChanges a_dataChanges)
    //{
    //    if (a_t is UserFieldDefinitionT udfDefT)
    //    {
    //        m_userFieldDefinitionManager.Receive(this, udfDefT, a_dataChanges);
    //    }
    //    else
    //    {
    //        throw new PTException("An unexpected transmission type was used in ScenarioManager.ReceiveWithDataChangeTrackingInternal.");
    //    }
    //}

    private void ReceiveWithDataChangeTrackingInternal(UserFieldDefinitionManager a_udfManager, IDataChangesDependentT<PTTransmissionBase> a_t, IScenarioDataChanges a_dataChanges, UserManagerEventCaller umEventCaller)
    {
        if (a_t is UserBaseT userBaseT)
        {
            // This stuff needs to happen outside of the UsersLock because 
            // they can fire events that might need to read information from the UserManager
            if (userBaseT is UserDeleteT userDeleteT)
            {
                ProcessUserDeleteT(userDeleteT);
            }
            else if (userBaseT is UserDeleteAllT udaT)
            {
                ProcessUserDeleteAllT(udaT);
            }

            using (SystemController.Sys.UsersLock.EnterWrite(out UserManager userManager))
            {
                User u;

                if (userBaseT is UserDefaultT)
                {
                    userManager.CreateUser(a_dataChanges);
                }
                else if (userBaseT is UserCopyT copyT)
                {
                    userManager.AddCopy(a_dataChanges, copyT);
                }
                else if (userBaseT is UserDeleteAllT udaT)
                {
                    userManager.DeleteAll(userBaseT, a_dataChanges);
                }
                else if (userBaseT is UserIdBaseT idBaseT)
                {
                    //Send other transmission to the User to handle.
                    u = userManager.ValidateUserExistence(idBaseT.userId);

                    if (idBaseT is UserLogOnT logonT)
                    {
                        u.Receive(logonT, a_dataChanges);
                        umEventCaller.AddEvent(new UserManagerEventCaller.UserLogOnArgs(u, userManager, logonT));
                    }
                    else if (idBaseT is UserChatT chatT)
                    {
                        u.Receive(chatT, a_dataChanges);
                        umEventCaller.AddEvent(new UserManagerEventCaller.UserChatArgs(u, chatT));
                    }
                    else if (idBaseT is UserErrorT uet)
                    {
                         System.Text.StringBuilder sb = new ();

                        PTException ex = new (uet.message, new Exception(uet.innerExceptionMessage));
                        u = userManager.GetById(uet.userId);

                        string userName = u != null ? u.Name : "PT USER NAME NOT FOUND.";

                        sb.Append("PT User Name: ");
                        sb.Append(userName);
                        sb.Append(Environment.NewLine);
                        sb.Append(uet.getTypeName);
                        sb.Append(Environment.NewLine);
                        sb.Append(uet.stackTrace);
                        sb.Append(Environment.NewLine);
                        sb.Append(uet.source);
                        sb.Append(Environment.NewLine);
                        sb.Append(uet.extraText);

                        m_errorReporter.LogException(ex, null, ELogClassification.UI, false, sb.ToString());
                    }
                    else if (idBaseT is UserAdminLogOffT adminLogoffT)
                    {
                        umEventCaller.AddEvent(new UserManagerEventCaller.UserAdminLogoffArgs(u, adminLogoffT));
                    }
                    else if (userBaseT is UserDeleteT deleteT)
                    {
                        userManager.Delete(a_dataChanges, deleteT);
                    }
                    else
                    {
                        u.Receive(idBaseT, a_dataChanges);
                    }
                }
                else if (userBaseT is UserPermissionSetT permissionSetT)
                {
                    if (permissionSetT.Delete)
                    {
                        userManager.DeleteUserPermissionSet(permissionSetT.PermissionSet, permissionSetT.DeleteReplacementId, a_dataChanges);
                    }
                    else
                    {
                        userManager.UpdateUserPermissionSet(permissionSetT.PermissionSet, permissionSetT.Default, a_dataChanges);
                    }

                    umEventCaller.AddEvent(new UserManagerEventCaller.UserPermissionsUpdatedArgs(userManager));
                }
                else if (userBaseT is PlantPermissionSetT plantPermissionSetT)
                {
                    if (plantPermissionSetT.Delete)
                    {
                        userManager.DeleteUserPlantPermissionSet(plantPermissionSetT.PlantPermissionSet, plantPermissionSetT.DeleteReplacementId, a_dataChanges);
                    }
                    else
                    {
                        userManager.UpdateOrAddUserPlantPermissionSet(plantPermissionSetT.PlantPermissionSet, plantPermissionSetT.Default, a_dataChanges);
                    }

                    umEventCaller.AddEvent(new UserManagerEventCaller.UserPermissionsUpdatedArgs(userManager));
                }
                else if (userBaseT is WorkspaceSharedUpdateT workspaceUpdate)
                {
                    string name = workspaceUpdate.WorkspaceName;
                    byte[] container = workspaceUpdate.WorkspaceBytes;

                    if (workspaceUpdate.Delete)
                    {
                        a_dataChanges.SharedWorkspacesChanges.DeletedObject(BaseId.NULL_ID);
                        userManager.WorkspaceTemplatesManager.DeleteSharedWorkspace(name, container);
                    }
                    else
                    {
                        if (workspaceUpdate.Overwrite)
                        {
                            a_dataChanges.SharedWorkspacesChanges.UpdatedObject(BaseId.NULL_ID);
                            userManager.WorkspaceTemplatesManager.ReplaceSharedWorkspace(name, container);
                        }
                        else
                        {
                            a_dataChanges.SharedWorkspacesChanges.AddedObject(BaseId.NULL_ID);
                            userManager.WorkspaceTemplatesManager.AddSharedWorkspace(name, container);
                        }
                    }
                }
                else if (userBaseT is UserEditT editT)
                {
                    userManager.UpdateUsers(editT, a_dataChanges);
                }
            }
        }
        else if (a_t is UserT userT)
        {

            using (SystemController.Sys.UsersLock.EnterWrite(out UserManager userManager))
            {
                userManager.BulkUpdateUsers(a_udfManager, userT, a_dataChanges);
            }
        }
        else
        {
            throw new PTException("An unexpected transmission type was used in the UserManager.Receive.");
        }
    }

    private void ProcessUserDeleteAllT(UserDeleteAllT a_t)
    {
        // re-assign ownership of scenarios to the server unless they're owned
        // by the transmission instigator
        foreach (Scenario scenario in Scenarios)
        {
            using (scenario.ScenarioSummaryLock.EnterWrite(out ScenarioSummary summary))
            {
                ScenarioPermissionSettings permissions = summary.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());
                if (permissions.OwnerId != a_t.Instigator)
                {
                    permissions.OwnerId = BaseId.ServerId;
                    summary.ScenarioSettings.SaveSetting(permissions);
                }
            }
        }
    }

    private void ProcessUserDeleteT(UserDeleteT a_userDeleteT)
    {
        if (a_userDeleteT.DeleteScenarios)
        {
            List<Scenario> scenariosOwnedByUser = new ();

            foreach (Scenario scenario in Scenarios)
            {
                using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary summary))
                {
                    ScenarioPermissionSettings permissions = summary.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());
                    if (permissions.OwnerId == a_userDeleteT.UserToBeDelete)
                    {
                        scenariosOwnedByUser.Add(scenario);
                    }
                }
            }

            foreach (Scenario scenario in scenariosOwnedByUser)
            {
                // The Delete function signature requires a scenarioDeleteT to be passed along in.
                // I guess some other scenario transmissions can trigger a scenario delete,
                // but certain events don't need to be fired off for those context. 
                // I don't see any reason why this context wouldn't just be a regular delete so
                // I'm just following the syntax, but when I checked how the transmission
                // passed in is being used, it seems to just be used to know when the program
                // should unsubscribe the ruleSeek events. 
                ScenarioDeleteT scenarioDeleteT = new (scenario.Id);
                ValidateDelete(scenarioDeleteT);
                Delete(scenarioDeleteT);
            }
        }
        else
        {
            //re-assign the ownership instead of deleting scenario
            if (!ValidateNewOwner(a_userDeleteT.NewOwnerId))
            {
                throw new PTValidationException("4460", new object[] { a_userDeleteT.NewOwnerId });
            }

            foreach (Scenario scenario in Scenarios)
            {
                using (scenario.ScenarioSummaryLock.EnterWrite(out ScenarioSummary summary))
                {
                    ScenarioPermissionSettings permissions = summary.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());
                    if (permissions.OwnerId == a_userDeleteT.UserToBeDelete)
                    {
                        permissions.OwnerId = a_userDeleteT.NewOwnerId;
                        summary.ScenarioSettings.SaveSetting(permissions);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Performs validation on a user before assign ownership of a scenario to said user.
    /// </summary>
    /// <param name="a_newOwnerId">The Id of the new owner of the scenario</param>
    /// <returns></returns>
    private static bool ValidateNewOwner(BaseId a_newOwnerId)
    {
        using (SystemController.Sys.UsersLock.EnterRead(out UserManager userManager))
        {
            if (userManager.GetById(a_newOwnerId) is User user)
            {
                return user.Active;
            }
        }

        return false;
    }

    /// <summary>
    /// Prevent two scenarios from having the same name
    /// </summary>
    private void ValidateScenarioChange(ScenarioChangeT a_pendingChangeT)
    {
        if (Find(a_pendingChangeT.ScenarioId) is Scenario renamingScenario)
        {
            if (!string.IsNullOrEmpty(a_pendingChangeT.ScenarioName))
            {
                foreach (Scenario scenario in m_loadedScenarios.Values)
                {
                    if (scenario.Name == a_pendingChangeT.ScenarioName && scenario.Id != a_pendingChangeT.ScenarioId)
                    {
                        throw new PTValidationException(string.Format("3018".Localize(), renamingScenario.Name, a_pendingChangeT.ScenarioName));
                    }
                }
            }
        }
    }

    private void TransmissionPostProcessing(ScenarioBaseT a_t)
    {
        //for erp and other misc transsmissions, send a replaceT if needed.
    }

    public delegate void TransmissionProcessedDelegate(ScenarioBaseT a_t, TimeSpan a_processingTime, int a_scenariosProcessed);

    public event TransmissionProcessedDelegate TransmissionProcessedEvent;

    internal void FireTransmissionProcessedEvent(ScenarioBaseT a_t, DateTime a_processingStart, int a_scenariosProcessed, bool a_alreadyProcessed)
    {
        if (!a_alreadyProcessed)
        {
            DateTime processingEnd = DateTime.UtcNow;
            TimeSpan processingTime = processingEnd - a_processingStart;
            TransmissionProcessedEvent?.Invoke(a_t, processingTime, a_scenariosProcessed);
        }
    }

    private void ProcessScenarioAddNewT(ScenarioBaseT a_t)
    {
        // Go through this function and see what we can reuse or need to remove.
        // The main thing that must happen for the reload and undo is that 
        // the cached dispatcher and dispatcher serialized in the transmission must be synced/merged up,
        // then the resulting dispatcher needs to start dispatching transmissions.  
        Scenario newScenario = null;
        if (a_t is ScenarioReplaceT replaceT)
        {
            if (replaceT.ContainsScenario)
            {
                if (!PTSystem.Server)
                {
                    try
                    {
                        int verNum;
                        using (BinaryMemoryReader reader = new (replaceT.ScenarioBytes))
                        {
                            newScenario = new Scenario(reader);
                            verNum = reader.VersionNumber;
                        }

                        newScenario.RestoreReferences(verNum, m_errorReporter, false, m_packageManager, m_userManager);

                        Scenario scenarioToReplace = Find(replaceT.ScenarioToReplaceId);
                        if (scenarioToReplace != null)
                        {
                            if (replaceT.InstigatorTransmissionId == ConvertToProductionScenarioT.UNIQUE_ID)
                            {
                                using (scenarioToReplace.ScenarioDetailLock.TryEnterWrite(out ScenarioDetail liveSd, AutoExiter.THREAD_TRY_WAIT_MS))
                                {
                                    using (newScenario.ScenarioDetailLock.TryEnterWrite(out ScenarioDetail sd, AutoExiter.THREAD_TRY_WAIT_MS))
                                    {
                                        scenarioToReplace.SetScenarioDetail(sd);
                                    }

                                    liveSd.Touch(new ScenarioTouchT());
                                }

                                ScenarioConversionCompleteEvent?.Invoke(newScenario.Id, replaceT.OriginalInstigatorId);
                            }
                            else
                            {
                                Delete(new ScenarioDeleteT(scenarioToReplace.Id));

                                //Validates total number of compared scenarios
                                using (newScenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                                {
                                    ScenarioPlanningSettings loadSetting = ss.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);
                                    ValidateScenarioComparisonLimit(newScenario.Id, loadSetting);
                                }

                                using (m_system.ScenariosLock.EnterWrite())
                                {
                                    AddScenario(newScenario.Id, newScenario);
                                }

                                //Update non-serialized values with a TouchT. Can't send to Receive since it must wait for TouchT to be processed.
                                using (newScenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
                                {
                                    sd.Touch(new ScenarioTouchT());
                                }
                                //FireEvent
                                ScenarioNewEvent?.Invoke(newScenario.Id, a_t);
                            }
                        }
                        else
                        {
                            if (replaceT.InstigatorTransmissionId == ScenarioCopyT.UNIQUE_ID || replaceT.InstigatorTransmissionId == ScenarioAddNewT.UNIQUE_ID)
                            {
                                bool instigatorIsCurrentUser = replaceT.OriginalInstigatorId == SystemController.CurrentUserId;

                                if (instigatorIsCurrentUser)
                                {
                                    AddScenarioUsingBytes(replaceT.ScenarioBytes);
                                    ScenarioNewEvent?.Invoke(replaceT.NewScenarioId, replaceT);
                                }
                                else
                                {
                                    using (newScenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
                                    {
                                        // Either way of loading the setting is fine. We just have two different styles here
                                        // since this code was written by two people at separate times. 
                                        ScenarioPermissionSettings permissions = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());
                                        bool canUserManageAllScenarios = m_userManager.CanUserManageAllScenarios(SystemController.CurrentUserId);
                                        BaseId userPermissionSetId = m_userManager.FindUserPermissionSetIdUsingUserId(SystemController.CurrentUserId);

                                        if (!permissions.CanUserView(SystemController.CurrentUserId, userPermissionSetId) && !canUserManageAllScenarios)
                                        {
                                            return;
                                        }
                                    }

                                    ScenarioNewNotLoadedEvent?.Invoke(replaceT.NewScenarioId, replaceT);
                                }

                                SystemController.ClientSession.MakePostRequest<BoolResponse>("UpdateUserLoadedScenarioIds", new UpdateLoadedScenarioIdsRequest(replaceT.NewScenarioId.Value, instigatorIsCurrentUser), "api/SystemService");
                            }
                        }
                    }
                    catch
                    {
                        //TODO
                    }
                }
                else
                {
                    if (replaceT.InstigatorTransmissionId == ConvertToProductionScenarioT.UNIQUE_ID)
                    {
                        return;
                    }

                    Scenario scenario = Find(replaceT.ScenarioToReplaceId);
                    if (scenario != null)
                    {
                        ScenarioNewEvent?.Invoke(replaceT.NewScenarioId, replaceT);
                    }
                }
            }
        }
        else
        {
            if (!PTSystem.Server)
            {
                return;
            }
            
            ScenarioAddNewT addNewT = a_t as ScenarioAddNewT;

            try 
            {
            
                ValidateScenariosLimitForCopy();

                //Adds a new Scenario, replacing the Id with a new one.
                
                int verNum = 0;
                using (BinaryMemoryReader reader = new (addNewT.ScenarioBytes))
                {
                    newScenario = new Scenario(reader);
                    verNum = reader.VersionNumber;
                }

                BaseId newId = GetNextScenarioId();
                using (newScenario.ScenarioSummaryLock.EnterWrite(out ScenarioSummary oldSS))
                {
                    bool isProduction = oldSS.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings()).Production;
                    //There can only be one production scenario
                    if (DoesProductionScenarioExist())
                    {
                        isProduction = false;
                    }

                    ScenarioSummary scenarioSummary = new ScenarioSummary(newId, new List<ISettingData>(), oldSS.Type, isProduction, a_t.Instigator, m_userManager);
                    scenarioSummary.Name = addNewT.NewScenarioName ?? $"Scenario {newId}";

                    ScenarioPermissionSettings newPermissionSettings = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());
                    ScenarioPermissionSettings permissions = oldSS.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());

                    foreach ((BaseId baseId, EUserAccess userAccess) in permissions.GroupIdToAccessLevel)
                    {
                        newPermissionSettings.GroupIdToAccessLevel.AddIfNew(baseId, userAccess);
                    }
                    
                    foreach ((BaseId baseId, EUserAccess userAccess) in permissions.UserIdToAccessLevel)
                    {
                        newPermissionSettings.UserIdToAccessLevel.AddIfNew(baseId, userAccess);
                    }
                    
                    scenarioSummary.ScenarioSettings.SaveSetting(new SettingData(newPermissionSettings), false);
                    newScenario.SetScenarioSummary(scenarioSummary);
                }

                newScenario.RestoreReferences(verNum, m_errorReporter, false, m_packageManager, m_userManager);

                //Validates total number of compared scenarios
                using (newScenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                {
                    ScenarioPlanningSettings loadSetting = ss.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);
                    ValidateScenarioComparisonLimit(newScenario.Id, loadSetting);
                }
                
                SystemController.ClientSession.MakePostRequest<BoolResponse>("UpdateUserLoadedScenarioIds", new UpdateLoadedScenarioIdsRequest(newScenario.Id.Value, true), "api/SystemService");
                
                using (SystemController.Sys.ScenariosLock.EnterWrite())
                {
                    AddScenario(newScenario.Id, newScenario);
                }

                //Update non-serialized values with a TouchT. Can't send to Receive since it must wait for TouchT to be processed.
                //Skip the touch if this a pruned scenario. It could get an exception on simulation.
                if (!(a_t is ScenarioAddNewPrunedT))
                {
                    using (newScenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
                    {
                        sd.Touch(new ScenarioTouchT());
                    }
                }
            }
            finally
            {

                byte[] scenarioBytes = newScenario?.GetClientScenarioBytes();

                ScenarioReplaceT scenarioReplaceT = new ScenarioReplaceT(newScenario?.Id ?? BaseId.NULL_ID, scenarioBytes ?? []);
                scenarioReplaceT.OriginalInstigatorId = addNewT!.OriginalInstigatorId;
                scenarioReplaceT.InstigatorTransmissionId = addNewT.UniqueId;
                SystemController.ClientSession.SendClientAction(scenarioReplaceT);

            }
        }
    }
}