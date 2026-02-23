using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.File;
using PT.Common.Http;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Extensions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.SystemServiceDefinitions.Headers;
using PT.Transmissions;
using System.Text;

using PT.Common.Exceptions;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

/// <summary>
/// Summary description for ScenarioManager.
/// </summary>
public partial class ScenarioManager : IDeserializationInit, IPTSerializable
{
    private IUserManager m_userManager;
    public const int UNIQUE_ID = 342;
    private readonly bool m_TempSystem;

    #region IPTSerializable Members
    public ScenarioManager(IReader a_reader, PTSystem a_liveSystem, List<ILicenseValidationModule> a_licenseValidationModules)
    {
        m_TempSystem = a_liveSystem.IsTempSystem;
        // TODO: This line below is what should actually be in the code and should be uncommented for production
        //m_grantManager = new LicenseGrantManager(a_liveSystem.SerialCode, c_token);
        // This line below should be removed once the LicenseService is fully ready. 
        // I needed to use this for testing purposes since none of the serial codes are registered 
        // since the LicenseService isn't actually ready. 
        //m_grantManager = new LicenseGrantManager("CompanyASerialCode", "MySecret");
        // Uncomment this line when further developments have been made with the LicenseClient's Nuget package
        // so I can come back and test it.
        #region Version 13000 - TransmissionSequencer removed
        if (a_reader.VersionNumber >= 13000)
        {
            a_reader.Read(out int loadedScenarioCount);
            for (int i = 0; i < loadedScenarioCount; i++)
            {
                Scenario s = new(a_reader);
                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                {
                    AddScenario(ss.Id, s);
                }
            }

            a_reader.Read(out int unloadedScenarioCount);
            for (int i = 0; i < unloadedScenarioCount; i++)
            {
                BaseId scenarioId = new(a_reader);
                m_unloadedScenarioIds.Add(scenarioId);
            }

            previousId = new BaseId(a_reader);

            a_reader.Read(out m_undoThreshold);
            a_reader.Read(out m_undoMemoryLimitMB);

            m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

            m_ruleSeekSettings = new RuleSeekSettings(a_reader);
            m_insertJobsSettings = new InsertJobsSettings(a_reader);

            m_idGenerator = new BaseIdGenerator(a_reader);
            m_userFieldDefinitionManager = new UserFieldDefinitionManager(a_reader, m_idGenerator);

            Init(a_liveSystem);
        }
        #endregion
        #region Version 12507
        else if (a_reader.VersionNumber >= 12507)
        {
            a_reader.Read(out int loadedScenarioCount);
            for (int i = 0; i < loadedScenarioCount; i++)
            {
                Scenario s = new(a_reader);
                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                {
                    AddScenario(ss.Id, s);
                }
            }

            a_reader.Read(out int unloadedScenarioCount);
            for (int i = 0; i < unloadedScenarioCount; i++)
            {
                BaseId scenarioId = new(a_reader);
                m_unloadedScenarioIds.Add(scenarioId);
            }

            previousId = new BaseId(a_reader);

            a_reader.Read(out m_undoThreshold);
            a_reader.Read(out m_undoMemoryLimitMB);

            m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

            new TransmissionSequencer(a_reader);

            m_ruleSeekSettings = new RuleSeekSettings(a_reader);
            m_insertJobsSettings = new InsertJobsSettings(a_reader);

            m_idGenerator = new BaseIdGenerator(a_reader);
            m_userFieldDefinitionManager = new UserFieldDefinitionManager(a_reader, m_idGenerator);

            Init(a_liveSystem);
        }
        #endregion
        #region Version 12503 - 12506
        else if (a_reader.VersionNumber >= 12503)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Scenario s = new(a_reader);
                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                {
                    AddScenario(ss.Id, s);
                }
            }

            previousId = new BaseId(a_reader);

            a_reader.Read(out m_undoThreshold);
            a_reader.Read(out m_undoMemoryLimitMB);

            m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

            new TransmissionSequencer(a_reader);

            m_ruleSeekSettings = new RuleSeekSettings(a_reader);
            m_insertJobsSettings = new InsertJobsSettings(a_reader);

            m_idGenerator = new BaseIdGenerator(a_reader);
            m_userFieldDefinitionManager = new UserFieldDefinitionManager(a_reader, m_idGenerator);

            Init(a_liveSystem);
        }
        #endregion
        else
        {
            //Since an Id Generator hasn't been serialized for previous versions before,
            //Initialize one here and it will be serialized for the current/future versions.
            m_idGenerator = new BaseIdGenerator();
            m_idGenerator.InitNextId(0);

            #region Version 12310 - 12502
            if (a_reader.VersionNumber >= 12310)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Scenario s = new(a_reader);
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        AddScenario(ss.Id, s);
                    }
                }

                previousId = new BaseId(a_reader);

                a_reader.Read(out m_undoThreshold);
                a_reader.Read(out m_undoMemoryLimitMB);

                m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

                new TransmissionSequencer(a_reader);

                m_ruleSeekSettings = new RuleSeekSettings(a_reader);
                m_insertJobsSettings = new InsertJobsSettings(a_reader);
                Init(a_liveSystem);
            }
            #endregion
            #region Version 12100 - 12309
            else if (a_reader.VersionNumber >= 12100)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Scenario s = new(a_reader);
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        AddScenario(ss.Id, s);
                    }
                }

                previousId = new BaseId(a_reader);

                a_reader.Read(out m_undoThreshold);
                a_reader.Read(out m_undoMemoryLimitMB);

                m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

                new TransmissionSequencer(a_reader);

                a_reader.Read(out int obsoleteChecksumFrequencyType);

                m_ruleSeekSettings = new RuleSeekSettings(a_reader);
                m_insertJobsSettings = new InsertJobsSettings(a_reader);
                Init(a_liveSystem);
            }
            #endregion
            #region Version 12070 - 12099 (12.0 Backwards compatibility)
            else if (a_reader.VersionNumber >= 12070)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Scenario s = new(a_reader);
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        AddScenario(ss.Id, s);
                    }
                }

                previousId = new BaseId(a_reader);

                a_reader.Read(out m_undoThreshold);
                a_reader.Read(out m_undoMemoryLimitMB);

                // Both of these BaseId are obsolete and no longer used.
                // These Reads are simply here to maintain compatibility
                BaseId liveScenarioId = new(a_reader);
                a_reader.Read(out bool havePublishedScenario);
                if (havePublishedScenario)
                {
                    BaseId publishedScenarioId = new(a_reader);
                }

                m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

                TransmissionSequencer dummy = new TransmissionSequencer(a_reader);

                a_reader.Read(out int obsoleteChecksumFrequencyType);

                m_ruleSeekSettings = new RuleSeekSettings(a_reader);
                m_insertJobsSettings = new InsertJobsSettings(a_reader);
                Init(a_liveSystem);
            }
            #endregion
            #region Version 12056 - 12069
            else if (a_reader.VersionNumber >= 12056)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Scenario s = new(a_reader);
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        AddScenario(ss.Id, s);
                    }
                }

                previousId = new BaseId(a_reader);

                a_reader.Read(out m_undoThreshold);
                a_reader.Read(out m_undoMemoryLimitMB);

                m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

                new TransmissionSequencer(a_reader);

                a_reader.Read(out int obsoleteChecksumFrequencyType);

                m_ruleSeekSettings = new RuleSeekSettings(a_reader);
                m_insertJobsSettings = new InsertJobsSettings(a_reader);
                Init(a_liveSystem);
            }
            #endregion
            #region Version 12009 - 12055
            else if (a_reader.VersionNumber >= 12009)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Scenario s = new(a_reader);
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        AddScenario(ss.Id, s);
                    }
                }

                previousId = new BaseId(a_reader);

                a_reader.Read(out m_undoThreshold);
                a_reader.Read(out m_undoMemoryLimitMB);

                // Both of these BaseId are obsolete and no longer used.
                // These Reads are simply here to maintain compatibility
                BaseId liveScenarioId = new(a_reader);
                a_reader.Read(out bool havePublishedScenario);
                if (havePublishedScenario)
                {
                    BaseId publishedScenarioId = new(a_reader);
                }

                m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

                new TransmissionSequencer(a_reader);

                a_reader.Read(out int obsoleteChecksumFrequencyType);

                m_ruleSeekSettings = new RuleSeekSettings(a_reader);
                m_insertJobsSettings = new InsertJobsSettings(a_reader);
                Init(a_liveSystem);
            }
            #endregion
            #region Version 12000 - 12008
            else if (a_reader.VersionNumber >= 12000)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Scenario s = new(a_reader);
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        AddScenario(ss.Id, s);
                    }
                }

                previousId = new BaseId(a_reader);

                a_reader.Read(out m_undoThreshold);
                a_reader.Read(out m_undoMemoryLimitMB);

                BaseId liveScenarioId = new(a_reader); //obsolete, liveScenarioId

                a_reader.Read(out bool havePublishedScenario);
                if (havePublishedScenario)
                {
                    BaseId publishedScenarioId = new(a_reader);
                }

                m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

                new TransmissionSequencer(a_reader);

                a_reader.Read(out int obsoleteChecksumFrequencyType);

                new CoPilotSettingsDeprecated(a_reader);
                m_ruleSeekSettings = new RuleSeekSettings(a_reader);
                m_insertJobsSettings = new InsertJobsSettings(a_reader);
                Init(a_liveSystem);
            }
            #endregion
            #region Version 659 - 11999
            else if (a_reader.VersionNumber >= 659)
            {
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    Scenario s = new(a_reader);
                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        AddScenario(ss.Id, s);
                    }
                }

                previousId = new BaseId(a_reader);

                a_reader.Read(out m_undoThreshold);
                a_reader.Read(out m_undoMemoryLimitMB);

                BaseId liveScenarioId = new(a_reader); //obsolete, liveScenarioId

                a_reader.Read(out bool havePublishedScenario);
                if (havePublishedScenario)
                {
                    BaseId publishedScenarioId = new(a_reader);
                    Scenario sPublished = Find(publishedScenarioId);
                }

                m_shopViewSystemOptions = new ShopViewSystemOptions(a_reader);

                new TransmissionSequencer(a_reader);

                a_reader.Read(out byte[] oldScheduCustBinaryData);
                a_reader.Read(out int obsoleteChecksumFrequencyType);

                new CoPilotSettingsDeprecated(a_reader);
                m_ruleSeekSettings = new RuleSeekSettings(a_reader);
                m_insertJobsSettings = new InsertJobsSettings(a_reader);
                Init(a_liveSystem);
            }
            #endregion

            m_userFieldDefinitionManager = new UserFieldDefinitionManager(m_idGenerator);
        }

        InitDataLicensing(a_licenseValidationModules);
    }

    public void Serialize(IWriter a_writer, params string[] a_params)
    {
        if (a_params[0] == Scenario.SerializeForClient)
        {
            m_skipUndo = true;
        }
        Serialize(a_writer);

        m_skipUndo = false;
    }
    public void Serialize(IWriter a_writer)
    {
#if DEBUG
        a_writer.DuplicateErrorCheck(this);
#endif
        a_writer.Write(LoadedScenarioCount);
        for (int i = 0; i < LoadedScenarioCount; i++)
        {
            Scenario s = GetByIndex(i);
            if (m_skipUndo)
            {
                s.Serialize(a_writer, Scenario.SerializeForClient);
            }
            else
            {
                s.Serialize(a_writer);
            }
        }

        a_writer.Write(UnloadedScenarioCount);
        foreach (BaseId unloadedScenarioId in m_unloadedScenarioIds)
        {
            unloadedScenarioId.Serialize(a_writer);
        }

        previousId.Serialize(a_writer);
        a_writer.Write(m_undoThreshold);
        a_writer.Write(m_undoMemoryLimitMB);

        m_shopViewSystemOptions.Serialize(a_writer);

        m_ruleSeekSettings.Serialize(a_writer); //451
        m_insertJobsSettings.Serialize(a_writer); // 458

        m_idGenerator.Serialize(a_writer);
        m_userFieldDefinitionManager.Serialize(a_writer);
    }

    public void SerializeForClient(IWriter a_writer, BaseId a_userId, HashSet<BaseId> a_previousSessionLoadedScenarioIds, out IEnumerable<BaseId> o_sessionLoadedScenarioIds)
    {
#if DEBUG
        a_writer.DuplicateErrorCheck(this);
#endif

        SortedList<BaseId, Scenario> scenariosToWrite = GetScenariosToSendToClient(a_userId, a_previousSessionLoadedScenarioIds);

        o_sessionLoadedScenarioIds = scenariosToWrite.Keys;

        SetupScenarioCollectionsForSerializeThenRestore(a_writer, scenariosToWrite);
    }

    private SortedList<BaseId, Scenario> GetScenariosToSendToClient(BaseId a_userId, HashSet<BaseId> a_previousSessionLoadedScenarioIds)
    {
        SortedList<BaseId, Scenario> scenariosToWrite = new SortedList<BaseId, Scenario>();
        
        Scenario productionScenario = null; 
        ScenarioPermissionSettings productionScenarioPermissions = null;
        // The Production Scenario is meant to be loaded if the user's previously loaded scenarios have been deleted or otherwise lost. 
        // I'm avoiding the usage of GetFirstProductionScenario to get it at the variable declaration to avoid iterating through the Scenarios collection twice. 

        (PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType scenarioStartupPreferenceType, BaseId lastActiveScenarioId) = m_userManager.GetUserScenarioStartupInformation(a_userId);
        bool canUserManageAllScenarios = m_userManager.CanUserManageAllScenarios(a_userId);
        BaseId userPermissionSetId = m_userManager.FindUserPermissionSetIdUsingUserId(a_userId);

        for (int i = 0; i < LoadedScenarioCount; i++)
        {
            Scenario scenario = GetByIndex(i);
            using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
            {
                // Either way of loading the setting is fine. We just have two different styles here
                // since this code was written by two people at separate times. 
                ScenarioPermissionSettings permissions = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());
                ScenarioPlanningSettings planningSettings = scenarioSummary.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);
                if (planningSettings.Production)
                {
                    productionScenarioPermissions = scenarioSummary.ScenarioSettings.LoadSetting<ScenarioPermissionSettings>(ScenarioPermissionSettings.Key);
                    productionScenario = scenario;
                }

                if (!permissions.CanUserView(a_userId, userPermissionSetId) && !canUserManageAllScenarios)
                {
                    continue;
                }

                if (scenarioStartupPreferenceType == PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType.LoadAllScenarios)
                {
                    scenariosToWrite.Add(scenarioSummary.Id, scenario);
                }
                else if (scenarioStartupPreferenceType == PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType.LoadLastSession
                         && a_previousSessionLoadedScenarioIds.Contains(scenarioSummary.Id))
                {
                    scenariosToWrite.Add(scenarioSummary.Id, scenario);
                }
                else if (scenarioStartupPreferenceType == PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType.LoadLastActiveScenario
                         && scenarioSummary.Id == lastActiveScenarioId)
                {
                    scenariosToWrite.Add(scenarioSummary.Id, scenario);
                    break;
                }
                else if (scenarioStartupPreferenceType == PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType.LoadProductionScenario 
                         && planningSettings.Production)
                {
                    scenariosToWrite.Add(scenarioSummary.Id, scenario);
                    break;
                }
            }
        }

        // All Scenarios that the user had previously loaded and had access to are gone for some reason.
        // See if they have access to the Production Scenario and send that instead. 
        if (scenariosToWrite.Count == 0)
        {

            if (productionScenario != null)
            {
                if (canUserManageAllScenarios || productionScenarioPermissions.CanUserView(a_userId, userPermissionSetId))
                {
                    scenariosToWrite.Add(productionScenario.Id, productionScenario);
                }
            }

            if (scenariosToWrite.Count == 0)
            {
                // Could not find any scenarios to send to Client, and the user doesn't have permission to access the production Scenario
                ExceptionDescriptionInfo exceptionDescription = new ExceptionDescriptionInfo(new PTException("3075", new[]{a_userId.ToString()}));
                m_errorReporter.LogException(exceptionDescription, ELogClassification.PtSystem);
            }

        }

        return scenariosToWrite;
    }

    // This function is meant to be called after the server attempts to serialize the user's loaded scenarios to send over to the client,
    // but fails to for some reason (maybe ScenarioPermissions changed or Scenarios got deleted).
    private static SortedList<BaseId, Scenario> AddScenarioToBeSentToClient(Scenario a_productionScenario, bool a_isAdminUser)
    {
        SortedList<BaseId, Scenario> scenariosToWrite = new SortedList<BaseId, Scenario>();
        if (!a_isAdminUser)
        {
            // User lacks permission to access scenarios beyond their previously loaded scenarios,
            // and access to all previously loaded scenarios were lost.
            throw new PTException("3076");
        }

        //TODO lite client: When ScenarioPreference for loading/unloading scenario is added, it should be used here
        // to determine what scenario is sent to the client if none of the loaded scenarios are accessible for some reason.
        // Maybe change the function signature, or just directly access the member variables; design is TBD.
        if (a_productionScenario != null) // This null check shouldn't be necessary given the current program structure
        {
            // User is an admin, but has lost access to all the loaded scenarios for some reason
            scenariosToWrite.Add(a_productionScenario.Id, a_productionScenario);
        }
        return scenariosToWrite;
    }

    /// <summary>
    /// This function will cache the two scenario collections, then set the two member variables referencing the collection to
    /// contain the actual values to be serialized and call the default Serialize function. Afterward, the two member variables
    /// will be reset back to the cached collections.
    /// The goal of this function to have SerializeForClient() call ScenarioManager.Serialize() so that there aren't
    /// if the Serialization of ScenarioManager changes, then SerializeForClient will change accordingly. 
    /// </summary>
    /// <param name="a_writer">The writer that scenario data is serialized into</param>
    /// <param name="a_scenariosToWrite">The scenarios to be serialized</param>
    private void SetupScenarioCollectionsForSerializeThenRestore(IWriter a_writer, SortedList<BaseId, Scenario> a_scenariosToWrite)
    {
        SortedList<BaseId, Scenario> cachedLoadedScenarios = m_loadedScenarios;
        HashSet<BaseId> cachedUnloadedScenarioIds = m_unloadedScenarioIds;
        m_loadedScenarios = a_scenariosToWrite;
        m_unloadedScenarioIds = new HashSet<BaseId>(m_unloadedScenarioIds);

        foreach (BaseId scenarioId in cachedLoadedScenarios.Keys)
        {
            if (!a_scenariosToWrite.ContainsKey(scenarioId))
            {
                // The server has the scenario loaded, but scenarios being sent to the client does not contain the scenario
                // so it's effectively unloaded for the user requesting the system bytes. 
                m_unloadedScenarioIds.Add(scenarioId);
            }
        }
        Serialize(a_writer, Scenario.SerializeForClient);
        m_loadedScenarios = cachedLoadedScenarios;
        m_unloadedScenarioIds = cachedUnloadedScenarioIds;
    }

    /// <summary>
    /// This function is called through an API that requests various data related to all the unloaded scenarios for
    /// a particular user. It should only be executed on the server, and it iterates through the server's loaded
    /// Scenarios to create a list of UnloadedScenarioData according to user's loaded Scenarios.  
    /// </summary>
    /// <param name="a_writer"></param>
    /// <param name="a_userId"></param>
    /// <param name="a_loadedScenarioIds"></param>
    /// TODO lite client: We will need to iterate through all the unloaded ScenarioIds too when we implement
    /// loading/unloading of scenarios on the Server.
    /// As I look over this function though, I realize that unload/loading scenarios on the Server will
    /// require some structural changes to ScenarioManager. Should we be storing UnloadedScenarioData
    /// on the server end to send back to the client requesting UnloadedScenarioData? We can't
    /// generate UnloadedScenarioData using the Scenario if the Scenario is not loaded. It doesn't
    /// seem to make sense to load a Scenario just to generate the UnloadedScenarioData, and I don't think
    /// we want to not display unloaded Scenarios in the ScenarioMenuLayoutControl just because the
    /// scenario isn't loaded on the server end either. 
    public void SerializeAllUnloadedScenarioData(IWriter a_writer, BaseId a_userId, HashSet<BaseId> a_loadedScenarioIds)
    {
        //If all scenarios are already loaded by the client do not continue logic
        (PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType scenarioStartupPreferenceType, BaseId lastActiveScenarioId) = m_userManager.GetUserScenarioStartupInformation(a_userId);
        if (scenarioStartupPreferenceType == PTUserScenarioStartupPreferences.EScenarioStartupPreferenceType.LoadAllScenarios)
        {
            a_writer.Write(0);
            return;
        }

        List<UnloadedScenarioData> allUnloadedScenarioData = new List<UnloadedScenarioData>();
        BaseId userPermissionSetId = m_userManager.FindUserPermissionSetIdUsingUserId(a_userId);
        // The assumption is that this function is not ran on the client so until the TODO 
        // mentioned above is done, LoadedScenarioCount == TotalScenarioCount
        for (int i = 0; i < LoadedScenarioCount; i++)
        {
            Scenario scenario = GetByIndex(i);
            if (a_loadedScenarioIds.Contains(scenario.Id))
            {
                continue;
            }

            ScenarioPlanningSettings planningSettings;
            ScenarioPermissionSettings permissions;
            DateTime clockDate;
            string lastAction;
            DateTimeOffset lastActionDate;
            using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
            {
                planningSettings = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
                permissions = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPermissionSettings());
            }

            //Checks if the specified user id has access to the specified scenario id
            if (!permissions.CanUserView(a_userId, userPermissionSetId))
            {
                continue;
            }

            using (scenario.ScenarioDetailLock.EnterRead(out ScenarioDetail scenarioDetail))
            {
                clockDate = scenarioDetail.ClockDate;
            }

            using (scenario.UndoSetsLock.EnterRead(out Scenario.UndoSets undoSets))
            {
                (string, DateTimeOffset, Guid) lastActionInfo = BuildLastActionInfo(undoSets);
                lastAction = lastActionInfo.Item1;
                lastActionDate = lastActionInfo.Item2;
            }

            UnloadedScenarioData data = new UnloadedScenarioData(
                scenario.Id,
                scenario.Name,
                lastAction,
                lastActionDate,
                clockDate,
                planningSettings,
                permissions);

            allUnloadedScenarioData.Add(data);
        }

        a_writer.Write(allUnloadedScenarioData.Count);
        foreach (UnloadedScenarioData data in allUnloadedScenarioData)
        {
            data.Serialize(a_writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    internal void RestoreReferences(int a_serializationVersionNumber, PTSystem a_system, IUserManager a_userManager, ISystemLogger a_erm, bool a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, IPackageManager a_packageManager, int a_maxNumberOfScenarios, int a_checksumDiagnosticsFrequency, (string serialCode, string token, EnvironmentType environmentType) a_licenseSessionInfo)
    {
        m_system = a_system;
        m_errorReporter = a_erm;
        m_packageManager = a_packageManager;
        m_maxNumberOfScenarios = a_maxNumberOfScenarios;

        m_checksumFrequencyType = a_checksumDiagnosticsFrequency;
        SetScenarioUndoSettings();

        m_userManager = a_userManager;
        // TODO: Uncomment this line out when I'm done testing and delete the other one that hardcodes values
        //m_grantManager = new LicenseGrantManager(a_licenseSessionInfo.serialCode, a_licenseSessionInfo.token, a_licenseSessionInfo.environmentType.ToString());

        Parallel.ForEach(m_loadedScenarios.Values, s =>
        {
            s.RestoreReferences(a_serializationVersionNumber, a_erm, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, a_packageManager, a_userManager);
        });
    }

    #region Disposal
    //~ScenarioManager()
    //{
    //    Dispose(true);
    //}

    public void Dispose()
    {
        Dispose(false);
    }

    private bool m_disposed;

    private void Dispose(bool a_finalizing)
    {
        if (!m_disposed)
        {
            m_disposed = true;

            foreach (Scenario s in m_loadedScenarios.Values)
            {
                s.Dispose();
            }
        }
    }
    #endregion

    /// <summary>
    /// Used to help find customizations. typeObj is compared to "PT.Scheduler.ISchedulerCustomization".
    /// </summary>
    /// <param name="typeObj"></param>
    /// <param name="criteriaObj"></param>
    /// <returns></returns>
    public static bool InterfaceTypeFilter(Type a_typeObj, object a_criteriaObj)
    {
        return a_typeObj.ToString() == a_criteriaObj.ToString();
    }

    #region Variables
    private SortedList<BaseId, Scenario> m_loadedScenarios = new ();
    private HashSet<BaseId> m_unloadedScenarioIds = new ();
    // TODO lite client: Implement the proper updating of this HashSet
    // This collection of unloaded Scenario Ids isn't used in the code right now, 
    // but I think it'll become useful when we try to implement loading/unloading scenarios on the Server.
    // When that happens, we should make sure that this HashSet is correctly updated as scenarios are loaded/unloaded
    // on both the client and the server end.

    private PTSystem m_system;
    private ISystemLogger m_errorReporter;
    private TransmissionClassFactory m_transmissionClassFactory;
    private IPackageManager m_packageManager;

    private BaseIdGenerator m_idGenerator;

    private readonly object m_deleteOldScenariosLock = new ();

    private readonly ShopViewSystemOptions m_shopViewSystemOptions = new ();

    public ShopViewSystemOptions ShopViewSystemOptions => m_shopViewSystemOptions;

    //[PT.Scheduler.AfterRestoreReferences.MasterCopyManagerAttribute]
    private UserFieldDefinitionManager m_userFieldDefinitionManager;
    public UserFieldDefinitionManager UserFieldDefinitionManager => m_userFieldDefinitionManager;

    private readonly Dictionary<BaseId,Dispatcher> m_cachedScenarioDispatchers = new Dictionary<BaseId, Dispatcher>();
    private readonly object m_dispatcherCacheLock = new object ();
    #endregion

    #region Scenario Transmission Queueing

    /// <summary>
    /// Adds the ScenarioId to the HashSet used to indicate that a Scenario cannot receive transmissions so
    /// the transmissions need to be added to a queue to be dispatched later. It locks before adding
    /// the ScenarioId to the collection to prevent threading issues. 
    /// </summary>
    /// <param name="a_scenarioId"></param>
    internal void CacheScenarioDispatcher(BaseId a_scenarioId, Dispatcher a_dispatcher)
    {
        lock (m_dispatcherCacheLock)
        {
            if (!m_cachedScenarioDispatchers.TryAdd(a_scenarioId, a_dispatcher))
            {
                //TODO lite client: Throw debug exception here?
            }
        }
    }

    /// <summary>
    /// Adds the specified transmission to the collection of queued transmission for the targeted scenario for the transmission.
    /// This function returning false beings that the intended destination Scenario of the transmission is not being loaded.
    /// </summary>
    /// <param name="a_scenarioId">The Id of the scenario the transmission was intended for</param>
    /// <param name="a_transmission">Transmission to be cached</param>
    /// <returns>True if a transmission was added to the queue, false if no transmission was added to the queue</returns>
    private bool AddTransmissionToDispatcher(BaseId a_scenarioId, PTTransmission a_transmission)
    {
        lock (m_dispatcherCacheLock)
        {
            if (m_cachedScenarioDispatchers.TryGetValue(a_scenarioId, out Dispatcher dispatcher))
            {
                dispatcher.Receive(a_transmission);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Returns the transmission next in line for the specified scenario.
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <returns></returns>
    /// TODO lite client: Figure out the locking steps for this
    private PTTransmission DequeueTransmission(BaseId a_scenarioId)
    {
        PTTransmission transmission = null;
        if (m_cachedScenarioDispatchers.TryGetValue(a_scenarioId, out Dispatcher scenarioDispatcher))
        {
            //transmission = scenarioDispatcher.DequeueTransmission();

            //// May want to put removing the dispatching into a separate function
            //if (scenarioDispatcher.TransmissionsCount == 0)
            //{
            //    m_cachedScenarioDispatchers.Remove(a_scenarioId);
            //}
        }

        return transmission;
    }


    // May need to use m_queueLock
    private bool CheckForExistenceOfLastReceivedTransmission(BaseId a_scenarioId, Guid a_lastReceivedTransmissionId)
    {
        //if (m_cachedScenarioDispatchers.TryGetValue(a_scenarioId, out Dispatcher scenarioDispatcher))
        //{
        //    return scenarioDispatcher.DoesTransmissionExist(a_lastReceivedTransmissionId);
        //}

        return false;
    }
    /// <summary>
    /// Returns the count of the queued transmissions for the specified scenario.
    /// This function is called while m_queueLock is locked.
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <returns>0 if no transmission queue is found, otherwise, the size of the queue found</returns>
    //private int GetTransmissionQueueSize(BaseId a_scenarioId)
    //{
    //    if (m_cachedScenarioDispatchers.TryGetValue(a_scenarioId, out Queue<ScenarioIdBaseT> transmissionQueue))
    //    {
    //        return transmissionQueue.Count;
    //    }
    //    return 0;
    //}

    // Might not need this
    private void MergeTransmissions(BaseId a_scenarioId)
    {
        lock (m_dispatcherCacheLock)
        {
            // Grab needed transmissions from cached dispatcher and dispatch them to the Scenario's new dispatcher.
            // Think about this process a bit more though. Maybe we actually want to keep the cached dispatcher?
            m_cachedScenarioDispatchers.Remove(a_scenarioId);
        }
    }
    #endregion

    #region Construction
    public ScenarioManager(PTSystem a_system, ISystemLogger a_erm, IPackageManager a_pm, User a_getAdministrator, IUserManager a_userManager, int a_maxNumberOfScenarios, int a_checksumDiagnosticsFrequency)
    {
        // TODO: Uncomment this line out when I'm done testing and delete the other one that hardcodes values
        //m_grantManager = new LicenseGrantManager(a_serverStartupVals.SerialCode, a_serverStartupVals.SecurityToken, a_serverStartupVals.EnvironmentType.ToString());

        m_TempSystem = a_system.IsTempSystem;

        m_errorReporter = a_erm;
        m_userManager = a_userManager;

        m_idGenerator = new BaseIdGenerator();
        m_idGenerator.InitNextId(0);

        m_userFieldDefinitionManager = new UserFieldDefinitionManager(m_idGenerator);

        //Create a default Live Scenario
        BaseId id = GetNextScenarioId();
        string scenarioName = $"Scenario {id}";
        Scenario s = new (id, scenarioName, new List<ISettingData>(), a_erm, ScenarioTypes.Live, a_pm, a_getAdministrator.Id, m_userManager, m_userFieldDefinitionManager);

        //TODO: Initialize scenario settings class
        m_maxNumberOfScenarios = a_maxNumberOfScenarios;
        m_checksumFrequencyType = a_checksumDiagnosticsFrequency;

        AddScenario(id, s);

        m_system = a_system;
        m_ruleSeekSettings = new RuleSeekSettings();
        m_insertJobsSettings = new InsertJobsSettings();
        Init(a_system);
        m_packageManager = a_pm;
    }

    private void AddScenario(BaseId a_id, Scenario a_s)
    {
        //Flags scenario as temporary scenario if a temp system created it before 
        //adding to the collection of scenarios on the manager
        if (m_TempSystem)
        {
            a_s.FlagTempScenario();
        }

        m_loadedScenarios.Add(a_id, a_s);
        using (a_s.ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents undoEvents))
        {
            undoEvents.UndoEndEvent += RemoveNameOnUndoEnd;
        }

        using (a_s.ScenarioEventsLock.EnterRead(out ScenarioEvents scenarioEvents))
        {
            scenarioEvents.DesynchronizedScenarioEvent += CachedDesyncedScenarioDispatcher;
        }
    }

    private void CachedDesyncedScenarioDispatcher(Guid a_transmissionId, string a_description, BaseId a_scenarioId)
    {
        Scenario desyncScenario = Find(a_scenarioId);
        CacheScenarioDispatcher(a_scenarioId, desyncScenario.Dispatcher);
        // Attempting the transmission route
        desyncScenario.Dispatcher.CancelTransmissionsDispatching();


        // I need to think about whether or not a_transmissionId is the correct transmissionId to pass in here
        // The transmissionId passed in here should be for the transmission that caused the desync. 
        // I also need to do something if the server is behind the client because that means the transmissions in the 
        // dispatcher that is transmitted along with the Scenario need to be processed before we start going through the 
        // transmissions in the queue
    }

    /// <summary>
    /// The function pretty much mimics the AddScenario function.
    /// The main difference is that it doesn't do the temp system stuff and throws the ScenarioNewEvent,
    /// and it also blocks any actions taken if it's on the server to prevent
    /// duplicate entries in the m_openScenarios collection.
    /// </summary>
    /// <param name="a_scenarioToLoadId">BaseId of the scenario being loaded</param>
    /// <param name="a_scenarioBaseT">Should be ScenarioLoadT or ScenarioReloadT</param>
    private void GetScenarioBytesThenLoadScenario(BaseId a_scenarioToLoadId, ScenarioBaseT a_scenarioBaseT)
    {
        GetScenarioRequest request = new();
        request.ScenarioId = a_scenarioToLoadId.Value;
        ByteResponse response = SystemController.ClientSession.MakePostRequest<ByteResponse>("GetScenario", request, "api/SystemService");

        if (response == null)
        {
            // TODO lite client: Figure out how to handle this situation
            // This could possibly happen if the scenario is deleted before the ScenarioReloadT/ScenarioLoadT is processed,
            // or maybe the user lost access permission to this scenario. 
            // It's probably to just return in that case, but are there other situations where this could occur?
            // Also, should we indicate this situation to the user in some fashion,
            // and does the MakePostRequest call above throw an error if the scenario is not found on the server end?
            return;
        }

        Scenario scenario = AddScenarioUsingBytes(response.Content);
        AttachEventsToScenario(scenario);

        if (a_scenarioBaseT is ScenarioLoadT)
        {
            ScenarioNewEvent?.Invoke(scenario.Id, a_scenarioBaseT);
        }
        else if (a_scenarioBaseT is ScenarioReloadT reloadT)
        {
            ScenarioReloadEvent?.Invoke(scenario.Id, reloadT);
            lock (m_dispatcherCacheLock)
            {
                if (!m_cachedScenarioDispatchers.Remove(scenario.Id, out Dispatcher dispatcher))
                {
                    #if DEBUG
                    throw new DebugException("Unable to find a cached dispatcher for the scenario being reloaded.");
                    // I feel like this shouldn't be possible unless there was some timing issue 
                    // with the desync event handler that caches the dispatcher. If that's the case, then we need
                    // to handle it because it'd mean the dispatcher will get cached and stop dispatching after the reload. 
                    #endif
                    dispatcher = new Dispatcher(EDispatcherOwner.Scenario);
                }

                scenario.Dispatcher.MergeTransmissions(dispatcher, scenario.LastReceivedTransmissionTimeTicks);
            }
            scenario.Dispatcher.StartDispatching();
        }
        else
        {
            #if DEBUG
            throw new PTException("An unsupported transmission type is trying to call ReloadScenario().");
            #endif
        }

        //Check if there are scenario pending to be deleted
        if (m_pendingDelete.Count > 0)
        {
            foreach ((BaseId scenarioId, BaseId instigatorId) in m_pendingDelete)
            {
                ScenarioDeleteT scenarioDeleteT = new ScenarioDeleteT(scenarioId);
                scenarioDeleteT.OriginalInstigatorId = instigatorId;
                Delete(scenarioDeleteT);
            }
            m_pendingDelete.Clear();
        }
    }

    private async void ReplaceScenario(ScenarioReplaceT a_replaceT, Scenario a_oldScenario)
    {
        Scenario scenario = AddScenarioUsingBytes(a_replaceT.ScenarioBytes);
        AttachEventsToScenario(scenario);

        lock (m_dispatcherCacheLock)
        {
            if (!m_cachedScenarioDispatchers.Remove(scenario.Id, out Dispatcher dispatcher))
            {
                dispatcher = a_oldScenario.Dispatcher;
            }

            if (a_replaceT.InstigatorTransmissionId == ScenarioUndoT.UNIQUE_ID)
            {
                scenario.Dispatcher.MergeTransmissions(dispatcher, scenario.LastReceivedTransmissionTimeTicks);
            }
        }

        //We want to wait for all handlers for this delegate before touching because most handlers are going to be 
        //reconnected by these handlers 
        if (ScenarioReplacedEvent != null)
        {
            await ScenarioReplacedEvent.Invoke(scenario.Id, a_replaceT);
        }

        List<QueuedTransmissionData> queueDescription;
        using (scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            queueDescription = scenario.Dispatcher.GetQueueDescriptions(sd);
        }

        FireUndoComplete(a_replaceT, scenario);

        //Fire this so the UI QueuedTransmission notification element is in sync with the queued tranmissions on the dispatcher
        using (scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            //Fire the transmission received event which is currently listened to on the QueuedTransmissionsNotificationElement.
            se.FireTransmissionReceivedEvent(a_replaceT, queueDescription);
        }

        scenario.Dispatcher.StartDispatching();
        CleanupScenario(a_oldScenario);
    }
    private Scenario AddScenarioUsingBytes(byte[] a_scenarioBytes)
    {
        Scenario scenario;
        using (BinaryMemoryReader reader = new(a_scenarioBytes))
        {
            scenario = new Scenario(reader);
            scenario.RestoreReferences(reader.VersionNumber, m_errorReporter, false, m_packageManager, m_userManager);
        }

        if (m_loadedScenarios.ContainsKey(scenario.Id))
        {
            // Should this exception be moved to the top of the function?
            throw new PTException("7014", new object[] { scenario.Id });
        }

        m_loadedScenarios.Add(scenario.Id, scenario);

        return scenario;
    }

    private void AttachEventsToScenario(Scenario scenario)
    {
        using (scenario.ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents undoEvents))
        {
            undoEvents.UndoEndEvent += RemoveNameOnUndoEnd;
        }

        using (scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents scenarioEvents))
        {
            scenarioEvents.DesynchronizedScenarioEvent += CachedDesyncedScenarioDispatcher;
        }
    }

    private void RemoveScenario(BaseId a_id)
    {
        using (m_loadedScenarios[a_id].ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents undoEvents))
        {
            undoEvents.UndoEndEvent -= RemoveNameOnUndoEnd;
        }

        using (m_loadedScenarios[a_id].ScenarioEventsLock.EnterRead(out ScenarioEvents scenarioEvents))
        {
            scenarioEvents.DesynchronizedScenarioEvent -= CachedDesyncedScenarioDispatcher;
        }

        m_loadedScenarios.Remove(a_id);
    }

    private void RemoveNameOnUndoEnd(ScenarioDetail a_sd, UserManager a_userManager, bool a_success)
    {
        ProcessScenarioName(a_sd.Scenario.Id);
    }

    /// <summary>
    /// Naming a scenario can be undone too so it's possible to get duplicate names
    /// after an undo so some processing after an undo is necessary.
    /// Right now, it just appends $"undo{counter}" where counter is just a number
    /// that starts at 1 and continues to go up if it finds a duplicate name.
    /// </summary>
    /// <param name="a_scenarioWithNameChangedId">The Id of the scenario that had an undo happen to it</param>
    private void ProcessScenarioName(BaseId a_scenarioWithNameChangedId)
    {
        HashSet<string> scenarioNames = new ();
        foreach ((BaseId scenarioId, Scenario scenario) in m_loadedScenarios)
        {
            if (scenarioId == a_scenarioWithNameChangedId)
            {
                continue;
            }

            scenarioNames.Add(scenario.Name);
        }

        string newScenarioNameBase = m_loadedScenarios[a_scenarioWithNameChangedId].Name;
        string newScenarioName = newScenarioNameBase;
        int counter = 1;
        while (scenarioNames.Contains(newScenarioName))
        {
            newScenarioName = newScenarioNameBase + string.Format(" undo{0}".Localize(), counter);
            counter++;
        }

        if (counter > 1)
        {
            using (m_loadedScenarios[a_scenarioWithNameChangedId].ScenarioSummaryLock.EnterWrite(out ScenarioSummary scenarioSummary))
            {
                scenarioSummary.Name = newScenarioName;
            }
        }
    }

    /// <summary>
    /// Call this from the constructors after data members have been initialized. If from a reader constructor, call it after all data has been read (data has been initialized). If from another constructor,
    /// after all or most data has been initialized.
    /// </summary>
    private void Init(PTSystem a_liveSystem)
    {
        m_transmissionClassFactory = PTSystem.TrnClassFactory;
        SetScenarioUndoSettings();

        //TODO: Copilot RuleSeek convert to package
        if (PTSystem.Server)
        {
            m_copilotSettings = a_liveSystem.CopilotSettings;
            if (m_ruleSeekSettings.Enabled && !PTSystem.LicenseKey.IncludeCoPilot)
            {
                m_ruleSeekSettings.Enabled = false;
            }

            if (m_ruleSeekSettings.Enabled)
            {
                UpdateRuleSeekTimer(RuleSeekEndReasons.Startup);
            }
        }
    }
    #endregion

    #region Next id functionality
    private BaseId previousId = new (0);

    private BaseId GetNextScenarioId()
    {
        BaseId nextId = previousId.NextId;
        previousId = nextId;
        return nextId;
    }
    #endregion

    #region Default naming functions
    private static string MakeScenarioName(BaseId a_id)
    {
        return string.Format("Scenario {0}".Localize(), a_id);
    }

    /// <summary>
    /// Override to create append a custom name to the Scenario.
    /// </summary>
    /// <param name="a_customName">Name that will appear in the UI after Scenario type</param>
    /// <returns></returns>
    private static string MakeScenarioName(BaseId a_id, ScenarioTypes a_scenarioType, string a_customName)
    {
        if (!string.IsNullOrEmpty(a_customName)) // a_scenarioType == ScenarioTypes.RuleSeek || a_scenarioType == ScenarioTypes.InsertJobs)
        {
            return string.Format(a_customName);
        }

        return string.Format("Scenario {0}".Localize(), a_id);
    }

    private static string MakeMileStoneName(BaseId a_id)
    {
        return string.Format("MileStone {0}".Localize(), a_id);
    }
    #endregion

    #region Search
    public int LoadedScenarioCount => m_loadedScenarios.Count;
    public int UnloadedScenarioCount => m_unloadedScenarioIds.Count;
    public int TotalScenarioCount => m_loadedScenarios.Count + m_unloadedScenarioIds.Count;

    public Scenario Find(BaseId a_id)
    {
        int i = m_loadedScenarios.IndexOfKey(a_id);

        if (i == -1)
        {
            return null;
        }

        return m_loadedScenarios.Values[i];
    }

    public Scenario GetByIndex(int a_index)
    {
        return m_loadedScenarios.Values[a_index];
    }

    public IEnumerable<Scenario> Scenarios
    {
        get
        {
            foreach (Scenario scenario in m_loadedScenarios.Values)
            {
                yield return scenario;
            }
        }
    }

    public IEnumerable<BaseId> UnloadedScenarioIds
    {
        get
        {
            foreach (BaseId scenarioId in m_unloadedScenarioIds)
            {
                yield return scenarioId;
            }
        }
    }

    /// <summary>
    /// Checks if the input scenario is a Production scenario, and if so, is it
    /// the last Production scenario in the ScenarioManager.
    /// </summary>
    /// <param name="a_scenario"></param>
    /// <returns>
    /// False if the input scenario is not the only Production scenario, or if
    /// the input scenario is not a Production scenario. True if the scenario is a Production
    /// scenario, and if the scenario is the last Production scenario.
    /// </returns>
    public bool IsLastProductionScenario(Scenario a_scenario)
    {
        if (!IsProductionScenario(a_scenario))
        {
            return false;
        }

        foreach (Scenario scenario in Scenarios)
        {
            if (scenario.Id == a_scenario.Id)
            {
                continue;
            }

            using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
            {
                ScenarioPlanningSettings scenarioPlanningSettings = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
                if (scenarioPlanningSettings.Production)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsProductionScenario(Scenario a_scenario)
    {
        using (a_scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
        {
            ScenarioPlanningSettings scenarioPlanningSettings = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
            return scenarioPlanningSettings.Production;
        }

    }
    #endregion

    #region Scenario Copy
    private void ValidateCopy(ScenarioCopyT a_t)
    {
        Scenario originalScenario = Find(a_t.OriginalId);

        if (originalScenario == null)
        {
            throw new TransmissionValidationException(a_t, "2611", new object[] { a_t.OriginalId });
        }

        for (int i = 0; i < m_loadedScenarios.Count; i++)
        {
            Scenario scenario = GetByIndex(i);
            if (scenario.Name.Equals(a_t.CustomNameOverride))
            {
                throw new TransmissionValidationException(a_t, "4504", new object[] { scenario.Name });
            }
        }
    }

    /// <summary>
    /// Create and returns the id of a new copy of an existing scenario.
    /// </summary>
    /// <param name="a_t"></param>
    internal BaseId ScenarioCopy(ScenarioCopyT a_t)
    {
        try
        {
            Scenario originalScenario = Find(a_t.OriginalId);
            if (originalScenario == null)
            {
                PTValidationException e = new ("7015", new object[] { a_t.OriginalId });
                m_errorReporter.LogException(e, a_t, new ScenarioExceptionInfo(), ELogClassification.PtSystem, false);
                throw e;
            }

            //TODO: Ideally we would have a extension controller at the system/transmission level
            using (originalScenario.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
            {
                if (originalScenario.ExtensionController.RunTransmissionProcessingExtension)
                {
                    ScenarioCopyT overrideCopyT = originalScenario.ExtensionController.PreProcessingScenarioCopy(a_t, originalScenario, sd);
                    if (overrideCopyT != null)
                    {
                        a_t = overrideCopyT;
                    }
                }
            }

            ValidateCopy(a_t);
            ValidateScenariosLimitForCopy();

            originalScenario = Find(a_t.OriginalId); //In case it changed from the extension point

            Scenario newScenario = CopyScenario(originalScenario, a_t.ScenarioType, a_t.Instigator, a_t.IsBlackBoxScenario, false, a_t.CustomNameOverride, a_t.InitialSettings.ToArray());
            BaseId newId = newScenario.Id;

            //TODO lite client: Try to come up with something that's more predictable and consistent with the general flow
            // Automatically add new copies of the scenario to the LoadedScenarioId collection of the ServerSession
            // since all scenarios are currently loaded automatically on the server. This will likely have to be changed
            // at some point when we implement loading/unloading of scenarios on the server end.
            // I'm also concerned about this here because it's susceptible to silently failing if related changes are made.
            if (PTSystem.Server)
            {
                SystemController.ClientSession.MakePostRequest<BoolResponse>("UpdateUserLoadedScenarioIds", new UpdateLoadedScenarioIdsRequest(newId.Value, true), "api/SystemService");
            }

            using (newScenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                ScenarioPlanningSettings planningSettings = ss.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);

                //Validates total number of compared scenarios
                ValidateScenarioComparisonLimit(newId, planningSettings);

                //TODO: This is for backwards compatibility with V11 extensions. V12 should probably override ScenarioPlanningSettings directly instead of props on the transmission
                //Transmission overwrote isolate settings.
                if (a_t.IsolateFromImport || a_t.IsolateFromClockAdvance)
                {
                    planningSettings.IsolateFromClockAdvance |= a_t.IsolateFromImport;
                    if (SystemController.ImportingType == EImportingType.IntegrationV1)
                    {
                        planningSettings.IsolateFromImport |= a_t.IsolateFromClockAdvance;
                    }
                }
            }

            // Add the new scenario to the scenario manager if it's the server or client of the instigator
            using (SystemController.Sys.ScenariosLock.EnterWrite())
            {
                AddScenario(newId, newScenario);
            }

            //Modify values that should be reset after scenario copied.
            using (newScenario.ScenarioSummaryLock.EnterWrite(out ScenarioSummary ss))
            {
                ScenarioPublishAutomaticSettings scenarioPublishAutomaticSettings = new ScenarioPublishAutomaticSettings();
                scenarioPublishAutomaticSettings = ss.ScenarioSettings.LoadSetting(scenarioPublishAutomaticSettings);
                scenarioPublishAutomaticSettings.AutomaticPublish = false;

                ss.ScenarioSettings.SaveSetting(scenarioPublishAutomaticSettings);
            }

            return newId;
        }
        catch (TransmissionValidationException validationException)
        {
            m_errorReporter.LogException(validationException, a_t, ELogClassification.PtSystem, false);
            return BaseId.NULL_ID;
        }
        catch (PTException pt)
        {
            m_errorReporter.LogException(pt, a_t, ELogClassification.PtSystem, pt.LogToSentry);
            return BaseId.NULL_ID;
        }
        catch (Exception e)
        {
            m_errorReporter.LogException(e, a_t, ELogClassification.Fatal, true);
            return BaseId.NULL_ID;
        }
    }

    /// <summary>
    /// Validates the total number of compared scenarios before allowing a new scenario to be compared
    /// </summary>
    private void ValidateScenarioComparisonLimit(BaseId a_scenarioId, ScenarioPlanningSettings a_newPlanningSettings)
    {
        const int c_limit = 4;

        int currentCompared = 0;
        foreach ((BaseId key, Scenario scenario) in m_loadedScenarios)
        {
            if (a_scenarioId == key)
            {
                continue;
            }

            using (scenario.AutoEnterScenarioSummary(out ScenarioSummary scenarioSummary))
            {
                ScenarioPlanningSettings planningSettings = new ScenarioPlanningSettings();
                planningSettings = scenarioSummary.ScenarioSettings.LoadSetting(planningSettings);

                if (planningSettings.CompareScenario)
                {
                    currentCompared++;
                }
            }
        }

        if (a_newPlanningSettings.CompareScenario)
        {
            currentCompared++;
        }
        
        if (currentCompared > c_limit + 1)
        {
            throw new PTValidationException("3066", new object[] { a_scenarioId, c_limit });
        }
    }
    /// <summary>
    /// Validate the max scenarios number limit before adding the copied scenario
    /// </summary>
    /// <exception cref="PTValidationException"></exception>
    private void ValidateScenariosLimitForCopy()
    {
        if (m_loadedScenarios.Count + 1 > m_maxNumberOfScenarios)
        {
            throw new PTValidationException("3021", new object[] { m_loadedScenarios.Count, m_maxNumberOfScenarios });
        }
    }

    /// <summary>
    /// Copy Scenario
    /// </summary>
    /// <param name="a_originalScenario"></param>
    /// <param name="a_scenarioType"></param>
    /// <param name="a_tInstigator"></param>
    /// <param name="a_isBlackBoxScenario"></param>
    /// <param name="a_copyForInternalUse"></param>
    /// <param name="a_scenarioName"></param>
    /// <param name="a_initialSettings"></param>
    /// <returns></returns>
    private Scenario CopyScenario(Scenario a_originalScenario, ScenarioTypes a_scenarioType, BaseId a_tInstigator, bool a_isBlackBoxScenario, bool a_copyForInternalUse, string a_scenarioName = null, params SettingData[] a_initialSettings)
    {
        //For System only simulations (like InsertJobs) a scenario needs to be copied for internal simulations.
        //In these cases, the ID of the scenario is not important, and if GetNextScenarioId() is called, it will make the 
        //Scenario Ids out of sync from the clients.
        BaseId newId = BaseId.NULL_ID;
        if (!a_copyForInternalUse)
        {
            newId = GetNextScenarioId();
        }

        a_originalScenario.CopyInMemory(out ScenarioDetail sdNew, out ScenarioSummary ssNew);

        Scenario newScenario = new (newId, a_originalScenario, sdNew, ssNew, m_packageManager, m_userManager);

        // If necessary make this the published scenario.
        // Set the type of the ScenarioSummary.
        // Set the instigator of the ScenarioSummary.
        bool clearForShortTerm = false;

        //I feel this like lock isn't necessary since there shouldn't be any other access to this scenario summary.
        // If it is necessary, then shouldn't this be a write lock?
        using (newScenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
        {
            scenarioSummary.SetType(a_scenarioType);

            switch (a_scenarioType)
            {
                case ScenarioTypes.ShortTerm:
                    clearForShortTerm = true;
                    break;
            }

            scenarioSummary.Creator = a_tInstigator;
            scenarioSummary.CreationDateTime = DateTime.UtcNow;
            scenarioSummary.IsBlackBoxScenario = a_isBlackBoxScenario;

            if (!string.IsNullOrEmpty(a_scenarioName))
            {
                scenarioSummary.Name = a_scenarioName;
            }

            foreach (SettingData data in a_initialSettings)
            {
                //Update copied settings with updates for this new scenario
                scenarioSummary.ScenarioSettings.SaveSetting(data, false);
            }

            // There can only be on Production scenario
            if (scenarioSummary.ScenarioSettings.LoadSetting(ScenarioPlanningSettings.Key) != null && DoesProductionScenarioExist())
            {
                ScenarioPlanningSettings planningSettings = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
                planningSettings.Production = false;
                scenarioSummary.ScenarioSettings.SaveSetting(new SettingData(planningSettings), false);
            }
        }

        newScenario.InitializeUndoSet();
        newScenario.KpiController.CopyKpiVisibility(a_originalScenario.KpiController.KpiList.UnsortedKpiList);

        return newScenario;
    }
    #endregion

    #region Ctp
    private void Ctp(Transmissions.CTP.CtpT a_t, IScenarioDataChanges a_dataChanges)
    {
        Scenario sourceScenario = Find(a_t.sourceScenarioId);

        if (sourceScenario == null)
        {
            string msg = string.Format("CTP request failed. ScenarioId {0} doesn't exist.", a_t.sourceScenarioId);
            m_errorReporter.LogException(new PTException(msg), a_t, ELogClassification.PtSystem,false);
            return;
        }

        Scenario workingScenario = CopyScenario(sourceScenario, ScenarioTypes.Whatif, a_t.Instigator, false, false);
        workingScenario.DispatchCTP(a_t, sourceScenario, a_dataChanges);
    }
    #endregion

    #region Transmission creation, validation, and handling.

    private void ValidateDelete(ScenarioDeleteT a_t)
    {
        Scenario s = Find(a_t.scenarioId);

        if (s == null)
        {
            if (!PTSystem.Server && !m_loadedScenarios.ContainsKey(a_t.scenarioId))
            {
                //Return if scenario is not loaded by the client
                return;
            }

            throw new TransmissionValidationException(a_t, "2126", new object[] { a_t.scenarioId });
        }

        if (!PTSystem.Server)
        {
            return;
        }


        if (IsProductionScenario(s) && IsLastProductionScenario(s))
        {
            throw new TransmissionValidationException(a_t, "3061", new object[] { s.Name });
        }

        // We may go back to allowing people to delete Production scenarios as long as there's still one left
        // so I figured this code could be left here. 
        //if (IsLastProductionScenario(s))
        //{
        //    throw new TransmissionValidationException(a_t, "3040", new object[] { s.Name });
        //}

        if (m_loadedScenarios.Count == 1)
        {
            throw new TransmissionValidationException(a_t, "3017", new object[] { s.Name });
        }

        using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            //Disallow delete of the Scenario is in use by someone
            if (ss.CurrentUserCount > 0)
            {
                throw new TransmissionValidationException(a_t, "2128", new object[] { a_t.scenarioId, ss.CurrentUserCount });
            }

            //If the user is not a master scheduler then only allow delete of whatif's created by that user
            if (a_t.Instigator == BaseId.NULL_ID.Value)
            {
                //This is the system.
            }

            //TODO: V12 Validate delete based on scenario permissions: owner, etc
            //using (var oa = m_system.UsersLock.EnterRead())
            //{
            //    User u = oa.Instance.GetById(a_t.Instigator);
            //    if (u != null)
            //    {
            //        if (u.UserPermissions.GetAccess(UserDefs.EPermissions.MaintainScenario) == UserDefs.EPermissionAccessLevels.WhatIf)
            //        {
            //            if (ss.Type != ScenarioTypes.Whatif && ss.Type != ScenarioTypes.ShortTerm)
            //            {
            //                throw new TransmissionValidationException(a_t, "2129", new object[] { ss.Name });
            //            }
            //            else //it is a WhatIf
            //            {
            //                if (ss.Creator != u.Id)
            //                {
            //                    throw new TransmissionValidationException(a_t, "2130", new object[] { ss.Name });
            //                }
            //            }
            //        }
            //        else if (u.UserPermissions.GetAccess(UserDefs.EPermissions.MaintainScenario) == UserDefs.EPermissionAccessLevels.None)
            //        {
            //            throw new TransmissionValidationException(a_t, "2131");
            //        }
            //    }
            //    else
            //    {
            //        throw new TransmissionValidationException(a_t, "2132", new object[] { ss.Name, a_t.Instigator.ToString() });
            //    }
            //}
        }
    }

    // a_isUnload indicates that this we're just deleting the scenario on the client, aka unload/closing a scenario.
    // It still needs to do most of the things that are done for deleting a scenario, but some notifications are different
    // TODO: Unload and Delete should be split up at some point to make the nomenclature more clear.
    // A good time to do it would be when we split up PTSystem to have a server version and a client version. 
    private void Delete(ScenarioDeleteT a_t, bool a_isUnload = false)
    {
        Scenario s = Find(a_t.scenarioId);
        
        Delete(s, a_t, a_isUnload);
    }
    /// <summary>
    /// A collection used to track scenario which are pending delete on the clients.
    /// <remarks>We temporarily cache the id of scenarios to be deleted at a later time to
    /// allow users to select a new scenario to load before proceeding to delete</remarks>
    /// </summary>
    private readonly Dictionary<BaseId, BaseId> m_pendingDelete = new Dictionary<BaseId, BaseId>();
    private void Delete(Scenario a_scenarioToDelete, ScenarioDeleteT a_t, bool a_isUnload = false)
    {
        if (!PTSystem.Server && !m_loadedScenarios.ContainsKey(a_scenarioToDelete.Id))
        {
            //Return if scenario is not loaded by the client
            return;
        }

        BaseId originalInstigatorId = a_t.OriginalInstigatorId != BaseId.NULL_ID ? a_t.OriginalInstigatorId : a_t.Instigator;

        if (!PTSystem.Server && !a_isUnload && m_loadedScenarios.Count < 2 && m_pendingDelete.TryAdd(a_scenarioToDelete.Id, originalInstigatorId))
        {
            ScenarioPromptDeleteEvent?.Invoke(a_scenarioToDelete.Id);
            return;
        }

        using (SystemController.Sys.ScenariosLock.EnterWrite())
        {
            using (a_scenarioToDelete.ScenarioEventsLock.EnterWrite(out ScenarioEvents se))
            {
                using (a_scenarioToDelete.ScenarioUndoEventsLock.EnterWrite(out ScenarioUndoEvents sue))
                {
                    ScenarioBeforeDeleteEvent?.Invoke(a_scenarioToDelete, se, sue, a_t, a_isUnload); // run this before actually deleting the scenario.
                }
            }

            a_scenarioToDelete.StopListeningToEvents();
            RemoveScenario(a_scenarioToDelete.Id);
        }

        HandleDeletedScenario(a_t);
        ScenarioDeleteEvent?.Invoke(a_scenarioToDelete, a_t);

        a_scenarioToDelete.Dispose();
        //Free up the memory right away. If not it could lead to OOM issues on busy servers.
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
    }

    /// <summary>
    /// Update any parts of the ScenarioManager which may have been using the deleted scenario.
    /// </summary>
    /// <param name="a_t"></param>
    private void HandleDeletedScenario(ScenarioBaseT a_t)
    {
        // If deleted scenario was running RuleSeek, reset manager.
        if (a_t is ScenarioDeleteT deleteT &&
            m_ruleSeekSettings.SourceScenarioId == deleteT.scenarioId)
        {
            UpdateRuleSeekTimer(RuleSeekEndReasons.LiveScenarioChanged);
        }
    }

    private void Publish(ScenarioPublishT a_t)
    {
        //Copy the Live Scenario and set it to Published.
        ScenarioCopyT copyT = new (GetFirstProductionScenario().Id, ScenarioTypes.Published);
        ScenarioCopy(copyT);
    }

    #region Deletegates and events
    //Scenario Summary
    public delegate void ScenarioNewDelegate(BaseId a_scenarioId, ScenarioBaseT a_t);

    //TODO lite client: UI elements that subscribe to this event should listen to IMultiScenarioInfo.ScenarioCreated
    public event ScenarioNewDelegate ScenarioNewEvent;

    //Scenario Convert To Production
    public delegate void ConvertToProductionScenarioDelegate(BaseId a_scenarioId, BaseId a_instigator);
    public event ConvertToProductionScenarioDelegate ScenarioConversionCompleteEvent;

    /// <summary>
    /// The ScenarioReloadingEvent fires right before the scenario is removed during the ScenarioReload process.
    /// The ScenarioReload process is meant reload a scenario if a scenario gets desynced.
    /// The event handlers to this function is meant to unsubscribe to various ScenarioEvents
    /// without removing the related ScenarioContext.
    /// </summary>
    public event Action<Scenario, ScenarioEvents, ScenarioUndoEvents> ScenarioBeforeReloadEvent;

    public delegate void ScenarioReloadDelegate(BaseId a_scenarioId, ScenarioReloadT a_scenarioReloadT);
    public event ScenarioReloadDelegate ScenarioReloadEvent;

    public delegate void ScenarioReplaceFailedNotificationEvent(BaseId a_scenarioId, ScenarioReplaceT a_scenarioReloadT);

    public event ScenarioReplaceFailedNotificationEvent ScenarioReplaceFailedEvent;

    /// <summary> for ScenarioNewNotLoadedEvent; Written Nov, 2024
    /// Parameters are ScenarioId of the new Scenario and the ScenarioBaseT that triggered the event being fired.
    /// The only ScenarioBaseT that triggers this event is ScenarioCopyT for now.
    /// 
    /// This event is meant to be fired when a Scenario is created, but not loaded into memory.
    /// This event is separate from ScenarioNewEvent because a lot of other stuff listen to ScenarioNewEvent,
    /// and a lot of those stuff do something with the new Scenario, which is not loaded into memory.
    /// Right now, the listener just then fires the IScenarioInfo.ScenarioCreated event which updates the UI
    /// and pops up a notification element
    /// </summary>
    public event Action<BaseId, ScenarioBaseT> ScenarioNewNotLoadedEvent;

    public delegate void ScenarioBeforeNewDelegate(ScenarioCopyT a_t);

    public event Action<BaseId> ScenarioPromptDeleteEvent;
    public delegate void ScenarioPromptDeleteDelegate(BaseId a_scenarioDeleting);

    // TODO lite client: Nothing seems to listen to this event
    public event ScenarioBeforeNewDelegate ScenarioBeforeNewEvent;

    public delegate Task ScenarioReplacedDelegate(BaseId a_scenarioId, ScenarioBaseT a_t);

    public event ScenarioReplacedDelegate ScenarioReplacedEvent;

    public delegate void ScenarioDeleteDelegate(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue, ScenarioBaseT a_t, bool a_isUnload);

    public event ScenarioDeleteDelegate ScenarioBeforeDeleteEvent;

    public event Action<Scenario, ScenarioBaseT> ScenarioDeleteEvent;
    //TODO lite client: Swap the UI elements that listen to this event to listen to IMultiScenarioInfo.ScenarioDeleted instead
    public event Action<BaseId, BaseId, string> ScenarioDeleteFailedValidationEvent;
    // parameters are: Instigator BaseId, Scenario BaseId, failure message

    public event Action<BaseId> ScenarioOpenedEvent;
    // Parameter is: ScenarioId

    public delegate void SystemProcessingDelegate(PTTransmissionBase a_t);

    public event SystemProcessingDelegate SystemProcessingEvent;

    private void FireSystemProcessingEvent(PTTransmissionBase a_t)
    {
        SystemProcessingEvent?.Invoke(a_t);
    }

    public delegate void ScenarioChangedDelegate(ScenarioChangeT a_t);

    public event ScenarioChangedDelegate ScenarioChangedEvent;

    private void FireScenarioChangedEvent(ScenarioChangeT a_t)
    {
        ScenarioChangedEvent?.Invoke(a_t);
    }

    public event Action<UserField.EUDFObjectType> UDFDataChangedEvent;
    public void FireUDFDataChangedEvent(UserField.EUDFObjectType a_objectType)
    {
        UDFDataChangedEvent?.Invoke(a_objectType);
    }

    #endregion

    /// <summary>
    /// The transmission will go to 1 or 2 scenarios based on its ScenarioIdBaseT.Destination setting.
    /// Returns whether the transmission was sent to the live scenario
    /// </summary>
    /// <param name="t"></param>
    private int PassToScenario(ScenarioIdBaseT a_t)
    {
        switch (a_t.Destination)
        {
            case ScenarioIdBaseT.EDestinations.BasedOnScenarioId:
                DispatchScenario(a_t.scenarioId, a_t);
                return 1;

            case ScenarioIdBaseT.EDestinations.ToLiveScenario:
                Scenario s = GetFirstProductionScenario();
                a_t.ChangeScenarioTarget(s.Id);
                DispatchScenario(a_t.scenarioId, a_t);
                return 1;
        }

        return 0;
    }

    /// <summary>
    /// Pass the transmission to the specified Scenario if the scenario is loaded already.
    /// If the Scenario is being reloaded or in the midst of an undo,
    /// it adds it to the queue to be dispatched after the process is complete
    /// TODO lite client: Update these comments 
    /// </summary>
    /// <param name="a_scenarioId">The scenario to send the transmission to.</param>
    /// <param name="a_t">The transmission to send.</param>
    private void DispatchScenario(BaseId a_scenarioId, PTTransmission a_t)
    {
        if (!PTSystem.Server && AddTransmissionToDispatcher(a_scenarioId, a_t))
        {
            return;
        }

        Scenario s = Find(a_scenarioId);

        if (s == null)
        {
            if (!PTSystem.Server)
            {
                // Scenario being null here means it is not loaded by the client
                return;
            }
            // All scenarios should be loaded on the server so this indicates that a transmission was
            // intended for a scenario that doesn't exist. 
            throw new TransmissionValidationException(a_t, "2135", new object[] { a_scenarioId });
        }

        s.Dispatch(a_t);
    }

    /// <summary>
    /// Send a transmission to every scenario. Transmissions such as clock advances and ERP Transmissions need to go to all scenarios.
    /// Other transmissions like optimizations and moves are performed within a specific scenario.
    /// </summary>
    /// <param name="a_t"></param>
    private int PassToAllScenarios(ScenarioBaseT a_t)
    {
        using (SystemController.Sys.ScenariosLock.EnterRead())
        {
            foreach (Scenario s in m_loadedScenarios.Values)
            {
                ScenarioBaseT t = (ScenarioBaseT)Serialization.CopyInMemory(a_t, m_transmissionClassFactory);
                SetRecordingsForTransmission(a_t, t);
                s.Dispatch(t);
            }
        }

        return m_loadedScenarios.Count;
    }

    private void AdvanceClock(ScenarioClockAdvanceT a_t)
    {
        a_t.time = DateTimeHelper.RoundSeconds(a_t.time);

        using (SystemController.Sys.ScenariosLock.EnterRead())
        {
            // The transmission is send down to each Scenario since they might be out of sync with this clock thanks to a UndoT
            foreach (Scenario s in m_loadedScenarios.Values)
            {
                ScenarioClockAdvanceT t = (ScenarioClockAdvanceT)Serialization.CopyInMemory(a_t, m_transmissionClassFactory);
                SetRecordingsForTransmission(a_t, t);
                s.Dispatch(t);
            }
        }
    }

    private void Touch(ScenarioTouchT a_t)
    {
        using (SystemController.Sys.ScenariosLock.EnterRead())
        {
            foreach (Scenario s in m_loadedScenarios.Values)
            {
                ScenarioTouchT t = (ScenarioTouchT)Serialization.CopyInMemory(a_t, m_transmissionClassFactory);
                SetRecordingsForTransmission(a_t, t);
                s.Dispatch(t);
            }
        }
    }

    private void SetSystemOptions(SystemOptionsT a_t)
    {
        using (SystemController.Sys.ScenariosLock.EnterRead())
        {
            foreach (Scenario s in m_loadedScenarios.Values)
            {
                SystemOptionsT t = (SystemOptionsT)Serialization.CopyInMemory(a_t, m_transmissionClassFactory);
                SetRecordingsForTransmission(a_t, t);
                s.Dispatch(t);
            }
        }
    }

    private void SetSystemPublishOptions(SystemPublishOptionsT a_t)
    {
        using (SystemController.Sys.ScenariosLock.EnterRead())
        {
            foreach (Scenario s in m_loadedScenarios.Values)
            {
                SystemPublishOptionsT t = (SystemPublishOptionsT)Serialization.CopyInMemory(a_t, m_transmissionClassFactory);
                SetRecordingsForTransmission(a_t, t);
                s.Dispatch(t);
            }
        }
    }

    private static void SetRecordingsForTransmission(ScenarioBaseT a_t, ScenarioBaseT a_copyT)
    {
        a_copyT.SetRecording(a_t.RecordingFilePath, a_t.OriginalTransmissionNbr);
    }

    /// <summary>
    /// Delete all RuleSeekScenarios.
    /// This is used when data has changed and old scenarios are not useful.
    /// </summary>
    private void DeleteAllScenariosOfType(ScenarioTypes a_scenarioTypeToDelete)
    {
        List<ScenarioDeleteT> scenariosToDelete = new ();
        for (int i = LoadedScenarioCount - 1; i >= 0; i--)
        {
            Scenario s = GetByIndex(i);
            if (s.Type == a_scenarioTypeToDelete)
            {
                ScenarioDeleteT deleteT = new (s.Id);
                scenariosToDelete.Add(deleteT);
            }
        }

        if (scenariosToDelete.Count > 0)
        {
            SystemController.ClientSession.SendClientActionsPacket(scenariosToDelete);
        }
    }

    /// <summary>
    /// Delete all scenarios whose type is not a_scenarioTypeToKeep.
    /// </summary>
    /// <param name="a_scenarioTypeToKeep"></param>
    private void DeleteAllScenariosExceptOfType(ScenarioTypes a_scenarioTypeToKeep)
    {
        for (int i = LoadedScenarioCount - 1; i >= 0; i--)
        {
            Scenario s = GetByIndex(i);
            if (s.Type != a_scenarioTypeToKeep)
            {
                ScenarioDeleteT deleteT = new (s.Id);
                Receive(deleteT);
            }
        }
    }
    #endregion

    private const int INITIAL_UNDO_THRESHOLD = 5;
    private int m_undoThreshold = INITIAL_UNDO_THRESHOLD;

    public int UndoThreshold => m_undoThreshold;

    private const int c_initialUndoMemoryLimitMB = 40;
    private decimal m_undoMemoryLimitMB = c_initialUndoMemoryLimitMB;

    public decimal UndoMemoryLimitMB => m_undoMemoryLimitMB;

    private int m_checksumFrequencyType;

    /// <summary>
    /// How frequently Checksum transmission is sent. See SchedulerDefinitions.ChecksumFrequencyType.
    /// </summary>
    public ChecksumFrequencyType ChecksumFrequency
    {
        get => (ChecksumFrequencyType)m_checksumFrequencyType;
        set => m_checksumFrequencyType = (int)value;
    }

    private int m_maxNumberOfScenarios;
    private bool m_skipUndo;
    public int MaxNumberOfScenarios => m_maxNumberOfScenarios;

    private int ValidateMinMaxSettings(ScenarioManagerUndoSettingsT a_t, string a_setting, int a_value, int a_minValue, int a_maxValue)
    {
        if (a_value < a_minValue || a_value > a_maxValue)
        {
            PTValidationException e = new ("2138", new object[] { a_setting, a_value, a_minValue, a_maxValue });
            m_errorReporter.LogException(e, a_t, ELogClassification.PtSystem, false);

            return 1;
        }

        return 0;
    }

    private void ScenarioManagerUndoSettingsHandler(ScenarioManagerUndoSettingsT a_t)
    {
        int invalidSetting = 0;

#if DEBUG
        invalidSetting += ValidateMinMaxSettings(a_t, "undoThreshold", a_t.UndoThreshold, 1, 86400);
#else
            invalidSetting += ValidateMinMaxSettings(a_t, "undoThreshold", a_t.UndoThreshold, 1, 300);
#endif

#if DEBUG
        invalidSetting += ValidateMinMaxSettings(a_t, "undoMemoryLimitMB", Convert.ToInt32(a_t.UndoMemoryLimitMB), 1, 10000);
#else
            invalidSetting += ValidateMinMaxSettings(a_t, "undoMemoryLimitMB", Convert.ToInt32(a_t.UndoMemoryLimitMB), 1, 2500);
#endif

        if (invalidSetting == 0)
        {
            using (SystemController.Sys.ScenariosLock.EnterWrite())
            {
                m_undoThreshold = a_t.UndoThreshold;
                m_undoMemoryLimitMB = a_t.UndoMemoryLimitMB;

                SetScenarioUndoSettings();
            }
        }
    }

    /// <summary>
    /// The master settings for Undo are stored in this object.
    /// When the master settings are changed, the static scenario values need to be updated.
    /// </summary>
    private void SetScenarioUndoSettings()
    {
        Scenario.UndoThreshold = m_undoThreshold;
        Scenario.MaxUndoMemoryUsage = UndoMemoryLimitMB;
        Scenario.ChecksumFrequency = ChecksumFrequency;
    }

    void IDeserializationInit.DeserializationInit()
    {
        Scenario.UndoThreshold = m_undoThreshold;
        Scenario.MaxUndoMemoryUsage = UndoMemoryLimitMB;
        Scenario.ChecksumFrequency = ChecksumFrequency;

        foreach (Scenario s in m_loadedScenarios.Values)
        {
            IDeserializationInit di = s;
            di.DeserializationInit();
        }
    }

    internal void TouchScenarios()
    {
#if TEST
            // Needed to prevent the debugger from breaking on different threads when continuing execution.
            // What I was seeing when I continued after hitting a break point is completely different data when the next breakpoint was hit
            // because it stopped in a different scenario. This processes the scenarios one by one so the I stay within  a single scenario
            // until it has been processed. 
            foreach (Scenario s in m_scenarios.Values)
            {
                ScenarioDetail sd;
                using (s.ScenarioDetailLock.EnterWrite(out sd))
                {
                    sd.Touch(new ScenarioTouchT());
                }

            }
#else
        System.Collections.Concurrent.ConcurrentBag<Exception> errs = new ();

        Parallel.ForEach(m_loadedScenarios.Values,
            s =>
            {
                try
                {
                    ScenarioDetail sd;
                    using (s.ScenarioDetailLock.EnterWrite(out sd))
                    {
                        sd.Touch(new ScenarioTouchT());
                    }
                }
                catch (Exception err)
                {
                    errs.Add(err);
                }
            });

        //TODO: V12 this used to throw an aggregate exception for errs and didn't log
        if (errs.Count > 0 && SystemController.Sys != null)
        {
            m_errorReporter.LogException(new AggregateException(errs), null, ELogClassification.PtSystem, false);
        }
#endif
    }

    public string GetSummary()
    {
        StringBuilder sb = new ();

        sb.AppendFormat("Number of Scenarios: {0}".Localize(), LoadedScenarioCount);
        sb.AppendLine();

        for (int scenarioI = 0; scenarioI < LoadedScenarioCount; ++scenarioI)
        {
            Scenario scenario = GetByIndex(scenarioI);

            sb.AppendLine("----------------------------------------");
            sb.AppendFormat("Scenario Number: {0}".Localize(), scenarioI + 1).AppendLine();
            sb.AppendLine();

            ScenarioSummary ss;
            try
            {
                using (scenario.ScenarioSummaryLock.TryEnterRead(out ss, 1000))
                {
                    sb.AppendFormat("Name: {0}; Id: {1}; Type: {2}".Localize(), ss.Name, ss.Id.ToString(), ss.Type.ToString());
                    sb.AppendLine();
                    sb.AppendFormat("Notes:{0}".Localize(), ss.Notes);
                    sb.AppendLine();
                }
            }
            catch (AutoTryEnterException)
            {
                sb.AppendLine("ScenarioSummary Locked".Localize());
            }

            ScenarioDetail sd;
            try
            {
                using (scenario.ScenarioDetailLock.EnterRead(out sd))
                {
                    sb.AppendFormat("Clock: {0}".Localize(), sd.Clock);
                    sb.AppendFormat("Plants: {0}; Departments: {1}; Resources:{2}; Capabilities: {3}".Localize(), sd.PlantManager.Count, sd.PlantManager.GetDepartments().Count, sd.PlantManager.GetResourceList().Count, sd.CapabilityManager.Count);
                    sb.AppendLine();

                    sb.AppendFormat("Dispatcher Rules: {0}; Capacity Intervals: {1}; Recurring Capacity Intervals: {2}".Localize(), sd.DispatcherDefinitionManager.Count, sd.CapacityIntervalManager.Count, sd.RecurringCapacityIntervalManager.Count);
                    sb.AppendLine();

                    sb.AppendFormat("Jobs: {0}".Localize(), sd.JobManager.Count);
                    sb.AppendLine();

                    List<Resource> resources = sd.PlantManager.GetResourceList();
                    long blockCount = 0;

                    long minStartDate = long.MaxValue;

                    long maxStartDate = long.MinValue;
                    long maxFinishDate = long.MinValue;

                    long minBlockLength = long.MaxValue;
                    long maxBlockLength = long.MinValue;
                    ResourceBlock maxBlock = null;

                    for (int resI = 0; resI < resources.Count; ++resI)
                    {
                        Resource res = resources[resI];
                        ResourceBlockList.Node curNode = res.Blocks.First;
                        while (curNode != null)
                        {
                            ++blockCount;

                            ResourceBlock block = curNode.Data;

                            minStartDate = Math.Min(minStartDate, block.StartTicks);
                            maxStartDate = Math.Max(maxStartDate, block.StartTicks);
                            maxFinishDate = Math.Max(maxFinishDate, block.EndTicks);
                            minBlockLength = Math.Min(minBlockLength, block.EndTicks - block.StartTicks);

                            if (block.EndTicks - block.StartTicks > maxBlockLength)
                            {
                                maxBlockLength = block.EndTicks - block.StartTicks;
                                maxBlock = block;
                            }

                            curNode = curNode.Next;
                        }
                    }

                    sb.AppendFormat("Blocks: {0}".Localize(), blockCount);
                    sb.AppendLine();

                    if (blockCount > 0)
                    {
                        sb.AppendFormat("Scheduled From: {0} to: {1}".Localize(), minStartDate, maxFinishDate);
                        sb.AppendLine();

                        sb.AppendFormat("Latest Block Start: {0}".Localize(), maxStartDate);
                        sb.AppendLine();

                        sb.AppendFormat("Min Block Length: {0}".Localize(), DateTimeHelper.PrintTimeSpan(minBlockLength));
                        sb.AppendLine();

                        sb.AppendFormat("Max Block Length: {0}; Job ExternalIds: ".Localize(), DateTimeHelper.PrintTimeSpan(maxBlockLength));

                        IEnumerator<InternalActivity> maxBlockActivityEtr = maxBlock.Batch.GetEnumerator();
                        while (maxBlockActivityEtr.MoveNext())
                        {
                            sb.AppendFormat("{0}; ", maxBlockActivityEtr.Current.Operation.Job.ExternalId);
                        }

                        sb.AppendLine();
                    }

                    // job status summary.

                    JobManager jobs = sd.JobManager;
                    long totalJobs = jobs.Count;
                    long totalNonTemplateJobs = 0;
                    Dictionary<string, long> statuses = new ();
                    foreach (Job j in jobs)
                    {
                        string status = j.ScheduledStatus.ToString();
                        if (statuses.ContainsKey(status))
                        {
                            statuses[status] = statuses[status] + 1;
                        }
                        else
                        {
                            statuses.Add(status, 1);
                        }

                        if (j.ScheduledStatus != JobDefs.scheduledStatuses.Template)
                        {
                            ++totalNonTemplateJobs;
                        }
                    }

                    Dictionary<string, long>.Enumerator etr = statuses.GetEnumerator();
                    sb.AppendFormat("Total Jobs: {0}".Localize(), totalJobs);
                    sb.AppendFormat("Total Jobs not including templates: {0}".Localize(), totalNonTemplateJobs);
                    sb.Append("Job Statuses".Localize());
                    while (etr.MoveNext())
                    {
                        sb.AppendLine();
                        sb.AppendFormat("{0}: {1}", etr.Current.Key, etr.Current.Value);
                    }
                }
            }
            catch (AutoTryEnterException)
            {
                sb.AppendLine("ScenarioDetail Locked".Localize());
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public delegate void ScenarioReloadFailedDelegate(BaseId a_scenarioId, ChecksumValues a_clientChecksums, ChecksumValues a_serverChecksums);

    public event ScenarioReloadFailedDelegate ScenarioReloadFailed;

    public void FireScenarioReloadFailed(BaseId a_scenarioId, ChecksumValues a_clientChecksums, ChecksumValues a_serverChecksums)
    {
        ScenarioReloadFailed?.Invoke(a_scenarioId, a_clientChecksums, a_serverChecksums);
    }

    //TODO lite client: See if we can delete this function
    // A separate ReloadScenario function was written that uses an API to obtain the ScenarioBytes so
    // this function should be unnecessary. It may serve some purpose as to be an example though, but
    // I think the new ReloadScenario function does pretty much all the same thing. 
    //public void ReloadScenario(BaseId a_scenarioToReloadId, byte[] a_scenarioBytes, decimal a_startAndEndSums, decimal a_resourceJobOperationCombos, int a_blockCount, string a_details, long a_nbrOfSimulations)
    //{
    //    Scenario scenarioToReplace = Find(a_scenarioToReloadId);
    //    if (scenarioToReplace == null)
    //    {
    //        throw new PTException("2126", new object[] { a_scenarioToReloadId });
    //    }

    //    if (a_scenarioBytes == null)
    //    {
    //        throw new PTException("4130");
    //    }

        //newScenario.RestoreReferences(verNum, m_errorReporter, false, m_packageManager, m_userManager);

    //    newScenario.RestoreReferences(verNum, m_errorReporter, false, m_packageManager, m_userManager, m_grantManager);

    //    ChecksumValues clientChecksums;
    //    using (newScenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
    //    {
    //        clientChecksums = sd.CalculateChecksums(0);
    //    }

    //    using (m_system.PackageManagerLock.EnterRead(out IPackageManager packageManager))
    //    {
    //        newScenario.InitDataLicensing(packageManager.GetLicenseValidationModules());
    //        newScenario.InitPermissionModules(packageManager.GetPermissionValidationModules());
    //    }

    //    // I don't know if we need to send the diagnostics stuff in this situation here, and
    //    // I feel like there can be a null reference issue here. Leave this commented for now,
    //    // although I don't think it really matters for now since this function doesn't seem to 
    //    // be referenced right now.
    //    //List<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> transmissionInfos = new();
    //    //using (newScenario.UndoSetsLock.EnterRead(out Scenario.UndoSets undoSets))
    //    //{
    //    //    // Just arbitrary choice of UndoSets to iterate through
    //    //    for (int i = 0; i < undoSets.Count; i++)
    //    //    {
    //    //        Scenario.UndoSet undoSet = undoSets[i];
    //    //        for (int j = 0; j < undoSet.Count; j++)
    //    //        {
    //    //            Scenario.TransmissionJar transmissionJar = undoSet[j];
    //    //            (ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp) transmissionInfo = (transmissionJar.TransmissionInfo.TransmissionNbr, transmissionJar.TransmissionInfo.UniqueId, transmissionJar.TransmissionInfo.Instigator, transmissionJar.TransmissionInfo.TimeStamp);
    //    //            transmissionInfos.Add(transmissionInfo);
    //    //        }
    //    //    }
    //    //}

    //    // This entire reloadScenario function isn't used, but if we decide to change this for some reason,
    //    // I added the undoset information to the checksum, and this constructor call will need to take that into consideration if we
    //    // uncomment this.
    //    //if (clientChecksums.StartAndEndSums != a_startAndEndSums || clientChecksums.ResourceJobOperationCombos != a_resourceJobOperationCombos || clientChecksums.BlockCount != a_blockCount)
    //    //{
    //    //    ChecksumValues checksumValues = new ChecksumValues(a_scenarioToReloadId, a_startAndEndSums, a_resourceJobOperationCombos, a_blockCount, a_details, 0);
    //    //    FireScenarioReloadFailed(a_scenarioToReloadId, clientChecksums, checksumValues);
    //    //    return;
    //    //}

    //    Delete(new ScenarioDeleteT(scenarioToReplace.Id));
    //    BaseId newId = newScenario.Id;
    //    AddScenario(newId, newScenario);
    //    ScenarioNewEvent?.Invoke(newId, new ScenarioAddNewT(null, newId, null));
    //}

    public void InitDataLicensing(IEnumerable<ILicenseValidationModule> a_modules)
    {
        foreach (Scenario s in m_loadedScenarios.Values)
        {
            s.InitDataLicensing(a_modules);
        }
    }

    public void InitPermissionValidationModules(List<IPermissionValidationModule> a_modules)
    {
        foreach (Scenario s in m_loadedScenarios.Values)
        {
            s.InitPermissionModules(a_modules);
        }
    }

    /// <summary>
    /// Retrieves the production scenario from scenario manager
    /// </summary>
    /// <returns>
    /// The first scenario that is marked as Production. 
    /// If no Scenarios are marked as production:
    /// <para>The first scenario in SM is returned on the Server. However, null is returned if the Clients made the call to retrieve the Production Scenario </para>
    /// </returns>
    public Scenario GetFirstProductionScenario()
    {
        using (m_system.ScenariosLock.EnterRead())
        {
            foreach (Scenario scenario in m_loadedScenarios.Values)
            {
                using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
                {
                    ScenarioPlanningSettings planningSettings = scenarioSummary.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);
                    if (planningSettings.Production)
                    {
                        return scenario;
                    }
                }
            }

            if (PTSystem.Server)
            {
                if (LoadedScenarioCount > 0)
                {
                    return m_loadedScenarios.Values[0];
                }
            }
        }

        // Should we throw an error here? It shouldn't be possible to 
        // have no production scenarios
        return null;
    }

    /// <summary>
    /// This should only be called on startup before scenarios have been loaded. It does not follow the 'Delete' process.
    /// </summary>
    /// <param name="a_errorReportingManager"></param>
    public void PurgeBlackBoxScenarios(ISystemLogger a_errorReportingManager)
    {
        List<Scenario> scenarios = m_loadedScenarios.Values.ToList();
        foreach (Scenario scenario in scenarios)
        {
            bool isBlackBox;
            DateTime creationDate;
            using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                isBlackBox = ss.IsBlackBoxScenario;
                creationDate = ss.CreationDateTime;
            }

            if (isBlackBox)
            {
                RemoveScenario(scenario.Id);
                ScenarioExceptionInfo info = new ();
                info.Initialize(scenario.Name, scenario.Type.ToString(), creationDate);
                a_errorReportingManager.LogException(new PTException($"Purged Blackbox scenario '{scenario.Id}' '{scenario.Name}' on startup"), info);
            }
        }
    }
    /// <summary>
    /// Removes all the scenarios specified in the list from the scenario manager
    /// </summary>
    /// <param name="a_scenarioToRemove"></param>
    internal void Remove(List<BaseId> a_scenarioToRemove)
    {
        foreach (BaseId baseId in a_scenarioToRemove)
        {
            RemoveScenario(baseId);
        }
    }
    /// <summary>
    /// Iterates through the collection of scenarios and checks if there are any existing scenarios.
    /// This function is used to enforce there being only 1 Production scenario per scenario file.
    /// </summary>
    /// <returns>True if a production scenario exists, false if not</returns>
    private bool DoesProductionScenarioExist()
    {
        foreach ((BaseId baseId, Scenario scenario) in m_loadedScenarios)
        {
            using (scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
            {
                ScenarioPlanningSettings scenarioPlanningSettings = scenarioSummary.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings());
                if (scenarioPlanningSettings.Production)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsScenarioLoaded(BaseId a_scenarioId)
    {
        return m_loadedScenarios.ContainsKey(a_scenarioId);
    }

    private void UnloadScenario(BaseId a_scenarioId, BaseId a_instigatorId)
    {
        //Find and remove the scenario from memory
        Scenario scenarioToUnload = Find(a_scenarioId);
        if (scenarioToUnload == null)
        {
            throw new PTException($"An attempt to unload Scenario was made, but Scenario with Id: {a_scenarioId} was not found."); //TODO: Either find an appropriate error code or add a new one
        }

        ScenarioDeleteT scenarioDeleteT = new ScenarioDeleteT(a_scenarioId);
        scenarioDeleteT.OriginalInstigatorId = a_instigatorId;
        Delete(scenarioToUnload,scenarioDeleteT , true);
        m_unloadedScenarioIds.Add(a_scenarioId);
    }

    /// <summary>
    /// This function is meant to reload a scenario so it unloads it, unsubscribes the ScenarioEvents,
    /// then immediately calls the API to request the scenario bytes from the server to load it back into the client's memory
    /// without all the ScenarioDelete events that happen in Delete. The goal is to reload the Scenario without removing the
    /// related ScenarioContext and firing any events that might try to access the ActiveContext.
    /// This is because many of the UI elements rely on there being an ActiveContext, and if there's only one scenario,
    /// then following the standard Unload/Delete Scenario path then loading the Scenario back in will cause a null reference
    /// error when the UI tries to update after the Unload/Delete (but before the Load, which will re-create the Context). 
    /// </summary>
    /// <param name="a_scenarioToReloadId"></param>
    /// <exception cref="PTException"></exception>
    private void ReloadScenario(BaseId a_scenarioToReloadId, ScenarioBaseT a_scenarioReloadT)
    {
        //Find and remove the scenario from memory
        Scenario scenario = Find(a_scenarioToReloadId);
        if (scenario == null)
        {
            throw new PTException($"An attempt to unload Scenario was made, but Scenario with Id: {a_scenarioToReloadId} was not found."); //TODO: Either find an appropriate error code or add a new one
        }

        if (PTSystem.Server)
        {
            FireUndoComplete(a_scenarioReloadT, scenario);
            // The scenario on the Server is considered the source of truth so it cannot be desynced
            return;
        }

       

        if (!PTSystem.Server && !m_loadedScenarios.ContainsKey(a_scenarioToReloadId))
        {
            //Return if scenario is not loaded by the client
            return;
        }

        using (SystemController.Sys.ScenariosLock.EnterWrite())
        {
            using (scenario.ScenarioEventsLock.TryEnterRead(out ScenarioEvents se, AutoExiter.THREAD_TRY_WAIT_MS))
            {
                using (scenario.ScenarioUndoEventsLock.TryEnterRead(out ScenarioUndoEvents sue, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    ScenarioBeforeReloadEvent?.Invoke(scenario, se, sue);
                    // The event handler for this event should remove the context and unsubscribes various listeners
                    // without firing various events that access ActiveContext. 
                }
            }

            scenario.StopListeningToEvents();
            RemoveScenario(a_scenarioToReloadId);

            if (m_ruleSeekSettings.SourceScenarioId == a_scenarioToReloadId)
            {
                UpdateRuleSeekTimer(RuleSeekEndReasons.LiveScenarioChanged);
            }

            if (a_scenarioReloadT is ScenarioReplaceT replaceT)
            {
                ReplaceScenario(replaceT, scenario);
            }
            else
            {
                CleanupScenario(scenario);
                GetScenarioBytesThenLoadScenario(a_scenarioToReloadId, a_scenarioReloadT);
            }
        }
    }

    /// <summary>
    /// Notifies the UI that a scenario has been replaced, it first re-attaches all removed events and fires all the appropriate events to ensures the UI reloads
    /// appropriately if the loaded data is based on the data from the scenario being reloaded
    /// </summary>
    /// <param name="a_reloadT"></param>
    /// <param name="a_scenario"></param>
    /// <param name="a_success"></param>
    private static void FireUndoComplete(ScenarioBaseT a_reloadT, Scenario a_scenario, bool a_success = true)
    {
        // The scenario on the Server is considered the source of truth so it cannot be desynced
        using (a_scenario.ScenarioUndoEventsLock.TryEnterRead(out ScenarioUndoEvents ue, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            if (a_reloadT is ScenarioReplaceT replaceT && replaceT.CancellingSimulation)
            {
                using (a_scenario.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
                    {
                        ue.FireUndoEndEvent(sd, um, a_success);
                    }
                }

                using (a_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                {
                    se.FireSimulationCancelledEvent();

                }
            }
            else
            {
                using (a_scenario.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
                    {
                       ue.FireUndoEndEvent(sd, um, a_success);
                    }
                }
            }
        }
    }
    private static void CleanupScenario(Scenario a_scenario)
    {
        a_scenario.Dispose();
        //Free up the memory right away. If not it could lead to OOM issues on busy servers.
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
    }
    // Ported over from ScenarioContext
    public (string, DateTimeOffset, Guid) BuildLastActionInfo(Scenario.UndoSets a_undoSets)
    {
        //It is possible the first undoset is empty (just created after the last action)
        //So we need to find the first undoset that has actions.
        Scenario.UndoSet undoSet = null;
        for (int i = a_undoSets.Count - 1; i >= 0; i--)
        {
            undoSet = a_undoSets[i];
            if (undoSet.Count > 0)
            {
                break;
            }
        }

        string lastAction = "No actions have been performed on this scenario yet.".Localize();
        DateTimeOffset lastActionDateUtc = PTDateTime.MinValue;
        Guid transmissionGuid = Guid.Empty;
        if (undoSet != null)
        {
            for (int i = undoSet.Count - 1; i >= 0; i--)
            {
                Scenario.TransmissionJar jar = undoSet[i];

                //System Transmissions, don't display these
                if (jar.TransmissionInfo.IsInternal)
                {
                    continue;
                }
                string userName = m_userManager.GetUserName(jar.TransmissionInfo.Instigator);
                lastAction = string.Format("{0}. Performed by {1}".Localize(), jar.TransmissionInfo.UndoChangeString, userName);
                lastActionDateUtc = jar.TransmissionInfo.TimeStamp;
                transmissionGuid = jar.TransmissionInfo.TransmissionId;
                break;
            }
        }

        return (lastAction, lastActionDateUtc, transmissionGuid);
    }
}