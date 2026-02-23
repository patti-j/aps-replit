using PT.Common.IO;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Contains settings that are common across all Scenarios.  They are set in ScenarioManager and kept in sync across Scenarios.
/// </summary>
public class ScenarioPublishOptions : ICloneable, IPTSerializable, ISetBoolsInitializer
{
    public const int UNIQUE_ID = 613;

    #region IPTSerializable Members
    public ScenarioPublishOptions(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12045)
        {
            a_reader.Read(out m_historyHorizonSpan);
            a_reader.Read(out m_historyMaxAge);
            a_reader.Read(out m_publishHorizonSpan);
            a_reader.Read(out m_whatIfHistoryMaxAge);
            a_reader.Read(out m_netChangeStoredProcedureName);
            a_reader.Read(out m_postPublishStoredProcedureName);
            a_reader.Read(out m_runProgramPath);
            a_reader.Read(out m_runProgramCommandLine);
            a_reader.Read(out m_reportSecurityUserName);
            a_reader.Read(out m_reportSecurityPassword);
            a_reader.Read(out m_automaticPublishDelay);
            a_reader.Read(out int exportDestinations);
            m_exportDestinations = (EExportDestinations)exportDestinations;

            m_boolVector = new BoolVector32(a_reader);
            m_boolVector2 = new BoolVector32(a_reader);

            m_isSetBools = new BoolVector32(a_reader);
            m_isSetBools2 = new BoolVector32(a_reader);
        }
        else if (a_reader.VersionNumber >= 12014)
        {
            a_reader.Read(out m_historyHorizonSpan);
            a_reader.Read(out m_historyMaxAge);
            a_reader.Read(out m_publishHorizonSpan);
            a_reader.Read(out m_whatIfHistoryMaxAge);
            a_reader.Read(out m_netChangeStoredProcedureName);
            a_reader.Read(out m_postPublishStoredProcedureName);
            a_reader.Read(out m_runProgramPath);
            a_reader.Read(out m_runProgramCommandLine);
            a_reader.Read(out m_reportSecurityUserName);
            a_reader.Read(out m_reportSecurityPassword);
            a_reader.Read(out m_automaticPublishDelay);
            a_reader.Read(out int exportDestinations);
            m_exportDestinations = (EExportDestinations)exportDestinations;

            m_boolVector = new BoolVector32(a_reader);
            m_boolVector2 = new BoolVector32(a_reader);
        }
        else if (a_reader.VersionNumber >= 649)
        {
            a_reader.Read(out m_historyHorizonSpan);
            a_reader.Read(out m_historyMaxAge);
            a_reader.Read(out m_publishHorizonSpan);
            a_reader.Read(out m_whatIfHistoryMaxAge);
            a_reader.Read(out m_netChangeStoredProcedureName);
            a_reader.Read(out m_postPublishStoredProcedureName);
            a_reader.Read(out m_runProgramPath);
            a_reader.Read(out m_runProgramCommandLine);
            a_reader.Read(out m_reportSecurityUserName);
            a_reader.Read(out m_reportSecurityPassword);
            a_reader.Read(out m_automaticPublishDelay);
            a_reader.Read(out int exportDestinations);
            ReadOldExportDestination(exportDestinations);

            m_boolVector = new BoolVector32(a_reader);
            m_boolVector2 = new BoolVector32(a_reader);
        }
    }

    /// <summary>
    /// Deserialize int based enum for the new flags based enum
    /// </summary>
    private void ReadOldExportDestination(int a_oldExportDestination)
    {
        switch (a_oldExportDestination)
        {
            case 0:
                m_exportDestinations = EExportDestinations.ToDatabase;
                break;
            case 1:
                m_exportDestinations = EExportDestinations.ToXML;
                break;
            case 2:
                m_exportDestinations = EExportDestinations.Custom;
                break;
            case 3:
                m_exportDestinations = EExportDestinations.ToDatabase | EExportDestinations.ToXML | EExportDestinations.Analytics | EExportDestinations.Custom;
                break;
            case 4:
                m_exportDestinations = EExportDestinations.Analytics;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(a_oldExportDestination));
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_historyHorizonSpan);
        a_writer.Write(m_historyMaxAge);
        a_writer.Write(m_publishHorizonSpan);
        a_writer.Write(m_whatIfHistoryMaxAge);
        a_writer.Write(m_netChangeStoredProcedureName);
        a_writer.Write(m_postPublishStoredProcedureName);
        a_writer.Write(m_runProgramPath);
        a_writer.Write(m_runProgramCommandLine);
        a_writer.Write(m_reportSecurityUserName);
        a_writer.Write(m_reportSecurityPassword);
        a_writer.Write(m_automaticPublishDelay);
        a_writer.Write((int)m_exportDestinations);

        m_boolVector.Serialize(a_writer);
        m_boolVector2.Serialize(a_writer);

        m_isSetBools.Serialize(a_writer);
        m_isSetBools2.Serialize(a_writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioPublishOptions(bool a_publishJobs)
    {
        EnableHistory = false;
        PublishInventory = false;
        KeepHistoryInventory = false;
        RunStoredProcedureAfterPublish = false;
        NetChangePublishingEnabled = false;
        RunStoredProcedureAfterNetChangePublish = false;
        RunMicrosoftProjectStoredProcedureAfterPublish = false;
        RunProgramAfterPublish = false;
        ReportsUseIntegratedSecurity = false;
        PublishToSQL = false;
        PublishToXML = false;
        PublishToCustomDll = false;
        AskUserForResourcesToPublish = false;
        PublishAllActivitesForMO = false;
        PublishCapacityIntervals = false;
        KeepCapacityIntervals = false;
        PublishProductRules = false;
        KeepHistoryProductRules = false;
        PublishToDashboard = false;

        PublishJobs = a_publishJobs; // the order in which we update below is important due to validation
        KeepHistoryJobs = false;
        PublishTemplates = false;
        KeepHistoryTemplates = false;
        PublishManufacturingOrders = false;
        KeepHistoryManufacturingOrders = false;
        PublishOperations = false;
        KeepHistoryOperations = false;
        PublishActivities = false;
        KeepHistoryActivities = false;
        PublishBlocks = false;
        KeepHistoryBlocks = false;
        PublishBlockIntervals = false;
        KeepHistoryBlockIntervals = false;

        AutomaticPublish = false;
    }
    public ScenarioPublishOptions(bool aEnableHistory,
                              bool aPublishInventory,
                              bool aPublishBlocks,
                              bool aPublishBlockIntervals,
                              bool aKeepHistoryInventory,
                              bool aKeepHistoryBlocks,
                              bool aKeepHistoryBlockIntervals,
                              TimeSpan aPublishHorizonSpan,
                              TimeSpan aHistoryHorizonSpan,
                              TimeSpan aHistoryMaxAge,
                              TimeSpan aWhatIfHistoryMaxAge,
                              bool aNetChangePublishingEnabled,
                              bool aRunStoredProcedureAfterNetChangePublish,
                              string aNetChangeStoredProcedureName,
                              bool aRunStoredProcedureAfterPublish,
                              string aPostPublishStoredProcedureName,
                              bool aRunMicrosoftProjectStoredProcedureAfterPublish,
                              bool aRunProgramAfterPublish,
                              string aRunProgramPath,
                              string aRunProgramCommandLine,
                              bool aReportsUseIntegratedSecurity,
                              string aReportsSecurityUserName,
                              string aReportsSecurityPassword,
                              bool aPublishToSql,
                              bool aPublishToXml,
                              bool aPublishToCustomDll,
                              bool aAskUserForResourcesToPublish,
                              bool aPublishAllActivitiesForMO,
                              bool aPublishCapacityIntervals,
                              bool aKeepCapacityIntervalHistory,
                              bool a_publishProductRules,
                              bool a_keepHistoryProductRules,
                              bool a_publishToDashboard,
                              bool a_publishJobs,
                              bool a_keepHistoryJobs,
                              bool a_publishTemplates,
                              bool a_keepHistoryTemplates,
                              bool a_publishManufacturingOrders,
                              bool a_keepHistoryManufacturingOrderes,
                              bool a_publishOperations,
                              bool a_keepHistoryOperations,
                              bool a_publishActivities,
                              bool a_keepHistoryActivities,
                              bool a_automaticPublish,
                              TimeSpan a_automaticPublishDelay,
                              EExportDestinations a_exportDestination)
    {
        EnableHistory = aEnableHistory;
        PublishInventory = aPublishInventory;
        KeepHistoryInventory = aKeepHistoryInventory;
        PublishHorizonSpan = aPublishHorizonSpan;
        HistoryHorizonSpan = aHistoryHorizonSpan;
        HistoryMaxAge = aHistoryMaxAge;
        WhatIfHistoryMaxAge = aWhatIfHistoryMaxAge;
        RunStoredProcedureAfterPublish = aRunStoredProcedureAfterPublish;
        PostPublishStoredProcedureName = aPostPublishStoredProcedureName;
        NetChangePublishingEnabled = aNetChangePublishingEnabled;
        RunStoredProcedureAfterNetChangePublish = aRunStoredProcedureAfterNetChangePublish;
        NetChangeStoredProcedureName = aNetChangeStoredProcedureName;
        RunMicrosoftProjectStoredProcedureAfterPublish = aRunMicrosoftProjectStoredProcedureAfterPublish;
        RunProgramAfterPublish = aRunProgramAfterPublish;
        RunProgramPath = aRunProgramPath;
        RunProgramCommandLine = aRunProgramCommandLine;
        ReportsUseIntegratedSecurity = aReportsUseIntegratedSecurity;
        ReportSecurityUserName = aReportsSecurityUserName;
        ReportSecurityPassword = aReportsSecurityPassword;
        PublishToSQL = aPublishToSql;
        PublishToXML = aPublishToXml;
        PublishToCustomDll = aPublishToCustomDll;
        AskUserForResourcesToPublish = aAskUserForResourcesToPublish;
        PublishAllActivitesForMO = aPublishAllActivitiesForMO;
        PublishCapacityIntervals = aPublishCapacityIntervals;
        KeepCapacityIntervals = aKeepCapacityIntervalHistory;
        PublishProductRules = a_publishProductRules;
        KeepHistoryProductRules = a_keepHistoryProductRules;
        PublishToDashboard = a_publishToDashboard;

        PublishJobs = a_publishJobs; // the order in which we update below is important due to validation
        KeepHistoryJobs = a_keepHistoryJobs;
        PublishTemplates = a_publishTemplates;
        KeepHistoryTemplates = a_keepHistoryTemplates;
        PublishManufacturingOrders = a_publishManufacturingOrders;
        KeepHistoryManufacturingOrders = a_keepHistoryManufacturingOrderes;
        PublishOperations = a_publishOperations;
        KeepHistoryOperations = a_keepHistoryOperations;
        PublishActivities = a_publishActivities;
        KeepHistoryActivities = a_keepHistoryActivities;
        PublishBlocks = aPublishBlocks;
        KeepHistoryBlocks = aKeepHistoryBlocks;
        PublishBlockIntervals = aPublishBlockIntervals;
        KeepHistoryBlockIntervals = aKeepHistoryBlockIntervals;

        AutomaticPublish = a_automaticPublish;
        AutomaticPublishDelay = a_automaticPublishDelay;
        ExportDestination = a_exportDestination;
    }

    /// <summary>
    /// Returns if settings are identical
    /// </summary>
    /// <param name="a_options"></param>
    /// <returns></returns>
    public bool Equals(ScenarioPublishOptions a_options)
    {
        return
            m_boolVector.Equals(a_options.m_boolVector) &&
            m_boolVector2.Equals(a_options.m_boolVector2) &&
            PublishHorizonSpan == a_options.PublishHorizonSpan &&
            HistoryHorizonSpan == a_options.HistoryHorizonSpan &&
            HistoryMaxAge == a_options.HistoryMaxAge &&
            WhatIfHistoryMaxAge == a_options.WhatIfHistoryMaxAge &&
            PostPublishStoredProcedureName == a_options.PostPublishStoredProcedureName &&
            NetChangeStoredProcedureName == a_options.NetChangeStoredProcedureName &&
            RunProgramPath == a_options.RunProgramPath &&
            RunProgramCommandLine == a_options.RunProgramCommandLine &&
            ReportSecurityUserName == a_options.ReportSecurityUserName &&
            ReportSecurityPassword == a_options.ReportSecurityPassword &&
            AutomaticPublishDelay == a_options.AutomaticPublishDelay &&
            ExportDestination == a_options.ExportDestination;
    }

    public ScenarioPublishOptions()
    {
        //Set defaults
        PublishInventory = true;
        ReportsUseIntegratedSecurity = true; //need to specify user otherwise
        PublishJobs = true;
        PublishManufacturingOrders = true;
        PublishOperations = true;
        PublishActivities = true;
        PublishBlocks = true;
        PublishBlockIntervals = true;
    }

    public void Update(ScenarioPublishOptions a_newOptions)
    {
        if (a_newOptions.EnableHistoryIsSet)
        {
            EnableHistory = a_newOptions.EnableHistory;
        }

        if (a_newOptions.PublishInventoryIsSet)
        {
            PublishInventory = a_newOptions.PublishInventory;
        }

        if (a_newOptions.KeepHistoryInventoryIsSet)
        {
            KeepHistoryInventory = a_newOptions.KeepHistoryInventory;
        }

        if (a_newOptions.PublishHorizonSpanIsSet)
        {
            PublishHorizonSpan = a_newOptions.PublishHorizonSpan;
        }

        if (a_newOptions.HistoryHorizonSpanIsSet)
        {
            HistoryHorizonSpan = a_newOptions.HistoryHorizonSpan;
        }

        if (a_newOptions.HistoryMaxAgeIsSet)
        {
            HistoryMaxAge = a_newOptions.HistoryMaxAge;
        }

        if (a_newOptions.WhatIfHistoryMaxAgeIsSet)
        {
            WhatIfHistoryMaxAge = a_newOptions.WhatIfHistoryMaxAge;
        }

        if (a_newOptions.RunStoredProcedureAfterPublishIsSet)
        {
            RunStoredProcedureAfterPublish = a_newOptions.RunStoredProcedureAfterPublish;
        }

        if (a_newOptions.PostPublishStoredProcedureNameIsSet)
        {
            PostPublishStoredProcedureName = a_newOptions.PostPublishStoredProcedureName;
        }

        if (a_newOptions.NetChangePublishingEnabledIsSet)
        {
            NetChangePublishingEnabled = a_newOptions.NetChangePublishingEnabled;
        }

        if (a_newOptions.RunStoredProcedureAfterNetChangePublishIsSet)
        {
            RunStoredProcedureAfterNetChangePublish = a_newOptions.RunStoredProcedureAfterNetChangePublish;
        }

        if (a_newOptions.NetChangeStoredProcedureNameIsSet)
        {
            NetChangeStoredProcedureName = a_newOptions.NetChangeStoredProcedureName;
        }

        if (a_newOptions.RunMicrosoftProjectStoredProcedureAfterPublishIsSet)
        {
            RunMicrosoftProjectStoredProcedureAfterPublish = a_newOptions.RunMicrosoftProjectStoredProcedureAfterPublish;
        }

        if (a_newOptions.RunProgramAfterPublishIsSet)
        {
            RunProgramAfterPublish = a_newOptions.RunProgramAfterPublish;
        }

        if (a_newOptions.RunProgramPathIsSet)
        {
            RunProgramPath = a_newOptions.RunProgramPath;
        }

        if (a_newOptions.RunProgramCommandLineIsSet)
        {
            RunProgramCommandLine = a_newOptions.RunProgramCommandLine;
        }

        if (a_newOptions.ReportsUseIntegratedSecurityIsSet)
        {
            ReportsUseIntegratedSecurity = a_newOptions.ReportsUseIntegratedSecurity;
        }

        if (a_newOptions.ReportSecurityUserNameIsSet)
        {
            ReportSecurityUserName = a_newOptions.ReportSecurityUserName;
        }

        if (a_newOptions.ReportSecurityPasswordIsSet)
        {
            ReportSecurityPassword = a_newOptions.ReportSecurityPassword;
        }

        if (a_newOptions.PublishToSQLIsSet)
        {
            PublishToSQL = a_newOptions.PublishToSQL;
        }

        if (a_newOptions.PublishToXMLIsSet)
        {
            PublishToXML = a_newOptions.PublishToXML;
        }

        if (a_newOptions.PublishToCustomDllIsSet)
        {
            PublishToCustomDll = a_newOptions.PublishToCustomDll;
        }

        if (a_newOptions.AskUserForResourcesToPublishIsSet)
        {
            AskUserForResourcesToPublish = a_newOptions.AskUserForResourcesToPublish;
        }

        if (a_newOptions.PublishAllActivitesForMOIsSet)
        {
            PublishAllActivitesForMO = a_newOptions.PublishAllActivitesForMO;
        }

        if (a_newOptions.PublishCapacityIntervalsIsSet)
        {
            PublishCapacityIntervals = a_newOptions.PublishCapacityIntervals;
        }

        if (a_newOptions.KeepCapacityIntervalsIsSet)
        {
            KeepCapacityIntervals = a_newOptions.KeepCapacityIntervals;
        }

        if (a_newOptions.PublishProductRulesIsSet)
        {
            PublishProductRules = a_newOptions.PublishProductRules;
        }

        if (a_newOptions.KeepHistoryProductRulesIsSet)
        {
            KeepHistoryProductRules = a_newOptions.KeepHistoryProductRules;
        }

        if (a_newOptions.PublishToDashboardIsSet)
        {
            PublishToDashboard = a_newOptions.PublishToDashboard;
        }

        if (a_newOptions.PublishJobsIsSet)
        {
            PublishJobs = a_newOptions.PublishJobs; //the order in which below is set is important.                      
        }

        if (a_newOptions.KeepHistoryJobsIsSet)
        {
            KeepHistoryJobs = a_newOptions.KeepHistoryJobs;
        }

        if (a_newOptions.PublishTemplatesIsSet)
        {
            PublishTemplates = a_newOptions.PublishTemplates;
        }

        if (a_newOptions.KeepHistoryTemplatesIsSet)
        {
            KeepHistoryTemplates = a_newOptions.KeepHistoryTemplates;
        }

        if (a_newOptions.PublishManufacturingOrdersIsSet)
        {
            PublishManufacturingOrders = a_newOptions.PublishManufacturingOrders;
        }

        if (a_newOptions.KeepHistoryManufacturingOrdersIsSet)
        {
            KeepHistoryManufacturingOrders = a_newOptions.KeepHistoryManufacturingOrders;
        }

        if (a_newOptions.PublishOperationsIsSet)
        {
            PublishOperations = a_newOptions.PublishOperations;
        }

        if (a_newOptions.KeepHistoryOperationsIsSet)
        {
            KeepHistoryOperations = a_newOptions.KeepHistoryOperations;
        }

        if (a_newOptions.PublishActivitiesIsSet)
        {
            PublishActivities = a_newOptions.PublishActivities;
        }

        if (a_newOptions.KeepHistoryActivitiesIsSet)
        {
            KeepHistoryActivities = a_newOptions.KeepHistoryActivities;
        }

        if (a_newOptions.PublishBlocksIsSet)
        {
            PublishBlocks = a_newOptions.PublishBlocks;
        }

        if (a_newOptions.KeepHistoryBlocksIsSet)
        {
            KeepHistoryBlocks = a_newOptions.KeepHistoryBlocks;
        }

        if (a_newOptions.PublishBlockIntervalsIsSet)
        {
            PublishBlockIntervals = a_newOptions.PublishBlockIntervals;
        }

        if (a_newOptions.KeepHistoryBlockIntervalsIsSet)
        {
            KeepHistoryBlockIntervals = a_newOptions.KeepHistoryBlockIntervals;
        }

        if (a_newOptions.AutomaticPublishIsSet)
        {
            AutomaticPublish = a_newOptions.AutomaticPublish;
        }

        if (a_newOptions.AutomaticPublishDelayIsSet)
        {
            AutomaticPublishDelay = a_newOptions.AutomaticPublishDelay;
        }

        if (a_newOptions.ExportDestinationIsSet)
        {
            ExportDestination = a_newOptions.ExportDestination;
        }
    }

    public void InitializeSetBools()
    {
        m_isSetBools.Clear();
        m_isSetBools2.Clear();
    }

    #region BoolVector32
    private BoolVector32 m_boolVector;

    //TODO Make the naming here consistent
    private const int EnableHistoryIdx = 0;

    private const int PublishInventoryIdx = 1;

    //const int PublishInventoryAdjustmentsIdx = 2; //not being used
    private const int PublishBlocksIdx = 3;
    private const int PublishBlockIntervalsIdx = 4;

    private const int KeepHistoryInventoryIdx = 5;

    //const int KeepHistoryInventoryAdjustmentsIdx = 6; // not being used
    private const int KeepHistoryBlocksIdx = 7;
    private const int KeepHistoryBlockIntervalsIdx = 8;
    private const int EnableNetChangePublishingIdx = 9;
    private const int RunStoredProcedureAfterPublishIdx = 10;
    private const int c_runStoredProcedureAfterNetChangePublishIdx = 11;
    private const int RunMicrosoftProjectStoredProcedureAfterPublishIdx = 12;
    private const int RunProgramAfterPublishIdx = 13;
    private const int ReportsUseIntegratedSecurityIdx = 14;
    private const int PublishToSqlIdx = 15;
    private const int PublishToCustomDllIdx = 16;
    private const int PublishToXmlIdx = 17;
    private const int AskUserForResourcesToPublishIdx = 18;
    private const int PublishAllActivitesForMOIdx = 19;
    private const int PublishCapacityIntervalsIdx = 20;
    private const int KeepCapacityIntervalsIdx = 21;
    private const int c_publishProductRulesIdx = 22;
    private const int c_keepHistoryProductRulesIdx = 23;
    private const int c_publishToDashboardIdx = 24;
    private const int c_publishJobsIdx = 25;
    private const int c_keepHistoryJobsIdx = 26;
    private const int c_publishTemplatesIdx = 27;
    private const int c_keepHistoryTemplatesIdx = 28;
    private const int c_publishManufacturingOrdersIdx = 29;
    private const int c_keepHistoryManufacturingOrdersIdx = 30;

    private BoolVector32 m_boolVector2;

    private const short c_publishOperationsIdx = 0;
    private const short c_keepHistoryOperationsIdx = 1;
    private const short c_publishActivitiesIdx = 2;
    private const short c_keepHistoryActivitiesIdx = 3;
    private const short c_automaticPublishIdx = 4;
    //private const short c_publishInLocalTimeIdx = 5;
    #endregion

    #region Properties
    [Obsolete("Deprecated use AutomaticPublish on ScenarioPublishAutomaticSettings instead, field not serialized")]
    public bool AutomaticPublish
    {
        get => m_boolVector2[c_automaticPublishIdx];
        set
        {
            m_boolVector2[c_automaticPublishIdx] = value;
            m_isSetBools2[c_automaticPublishIsSetIdx] = true;
        }
    }

    private TimeSpan m_automaticPublishDelay = TimeSpan.FromMinutes(10);
    [Obsolete("Deprecated use AutomaticPublishDelay on ScenarioPublishAutomaticSettings instead, field not serialized")]
    public TimeSpan AutomaticPublishDelay
    {
        get => m_automaticPublishDelay;
        set
        {
            m_automaticPublishDelay = value;
            m_isSetBools2[c_automaticPublishDelayIsSetIdx] = true;
        }
    }

    private EExportDestinations m_exportDestinations = EExportDestinations.BasedOnSystemOptions;
    [Obsolete("Deprecated use ExportDestination on ScenarioPublishAutomaticSettings instead, field not serialized")]
    public EExportDestinations ExportDestination
    {
        get => m_exportDestinations;
        set
        {
            m_exportDestinations = value;
            m_isSetBools2[c_exportDestinationIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use EnableHistory on ScenarioPublishHistory instead, field not serialized")]
    public bool EnableHistory
    {
        get => m_boolVector[EnableHistoryIdx];
        set
        {
            m_boolVector[EnableHistoryIdx] = value;
            m_isSetBools[c_enableHistoryIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishInventory on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishInventory
    {
        get => m_boolVector[PublishInventoryIdx];
        set
        {
            m_boolVector[PublishInventoryIdx] = value;
            m_isSetBools[c_publishInventoryIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryInventory on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryInventory
    {
        get => m_boolVector[KeepHistoryInventoryIdx];
        set
        {
            m_boolVector[KeepHistoryInventoryIdx] = value;
            m_isSetBools[c_keepHistoryInventoryIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishCapacityIntervals on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishCapacityIntervals
    {
        get => m_boolVector[PublishCapacityIntervalsIdx];
        set
        {
            m_boolVector[PublishCapacityIntervalsIdx] = value;
            m_isSetBools[c_publishCapacityIntervalsIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepCapacityIntervals on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepCapacityIntervals
    {
        get => m_boolVector[KeepCapacityIntervalsIdx];
        set
        {
            m_boolVector[KeepCapacityIntervalsIdx] = value;
            m_isSetBools[c_keepCapacityIntervalHistoryIsSetIdx] = true;
        }
    }

    private TimeSpan m_publishHorizonSpan = TimeSpan.FromDays(365);
    [Obsolete("Deprecated use PublishHorizonSpan on ScenarioPublishHistory instead, field not serialized")]
    public TimeSpan PublishHorizonSpan
    {
        get => m_publishHorizonSpan;
        set
        {
            m_publishHorizonSpan = value;
            m_isSetBools[c_publishHorizonSpanIsSetIdx] = true;
        }
    }

    private TimeSpan m_historyHorizonSpan = TimeSpan.FromDays(1);
    [Obsolete("Deprecated use HistoryHorizonSpan on ScenarioPublishHistory instead, field not serialized")]
    public TimeSpan HistoryHorizonSpan
    {
        get => m_historyHorizonSpan;
        set
        {
            m_historyHorizonSpan = value;
            m_isSetBools[c_historyHorizonSpanIsSetIdx] = true;
        }
    }

    private TimeSpan m_historyMaxAge = TimeSpan.FromDays(7);
    [Obsolete("Deprecated use HistoryMaxAge on ScenarioPublishHistory instead, field not serialized")]
    public TimeSpan HistoryMaxAge
    {
        get => m_historyMaxAge;
        set
        {
            m_historyMaxAge = value;
            m_isSetBools[c_historyMaxAgeIsSetIdx] = true;
        }
    }

    private TimeSpan m_whatIfHistoryMaxAge = TimeSpan.FromDays(2);
    [Obsolete("Deprecated use WhatIfHistoryMaxAge on ScenarioPublishHistory instead, field not serialized")]
    public TimeSpan WhatIfHistoryMaxAge
    {
        get => m_whatIfHistoryMaxAge;
        set
        {
            m_whatIfHistoryMaxAge = value;
            m_isSetBools[c_whatIfHistoryMaxAgeIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use RunStoredProcedureAfterPublish on ScenarioPublishPostPublish instead, field not serialized")]
    public bool RunStoredProcedureAfterPublish
    {
        get => m_boolVector[RunStoredProcedureAfterPublishIdx];
        set
        {
            m_boolVector[RunStoredProcedureAfterPublishIdx] = value;
            m_isSetBools[c_runStoredProcedureAfterPublishIsSetIdx] = true;
        }
    }

    private string m_postPublishStoredProcedureName = "APS_Publish";
    [Obsolete("Deprecated use PostPublishStoredProcedureName on ScenarioPublishPostPublish instead, field not serialized")]
    public string PostPublishStoredProcedureName
    {
        get => m_postPublishStoredProcedureName;
        set
        {
            m_postPublishStoredProcedureName = value;
            m_isSetBools[c_postPublishStoredProcedureNameIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use NetChangePublishingEnabled on ScenarioPublishNetChange instead, field not serialized")]
    public bool NetChangePublishingEnabled
    {
        get => m_boolVector[EnableNetChangePublishingIdx];
        set
        {
            m_boolVector[EnableNetChangePublishingIdx] = value;
            m_isSetBools[c_netChangePublishingEnabledIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use RunStoredProcedureAfterNetChangePublish on ScenarioPublishNetChange instead, field not serialized")]
    public bool RunStoredProcedureAfterNetChangePublish
    {
        get => m_boolVector[c_runStoredProcedureAfterNetChangePublishIdx];
        set
        {
            m_boolVector[c_runStoredProcedureAfterNetChangePublishIdx] = value;
            m_isSetBools[c_runStoredProcedureAfterNetChangePublishIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use RunProgramAfterPublish on ScenarioPublishPostPublish instead, field not serialized")]
    public bool RunProgramAfterPublish
    {
        get => m_boolVector[RunProgramAfterPublishIdx];
        set
        {
            m_boolVector[RunProgramAfterPublishIdx] = value;
            m_isSetBools[c_runProgramAfterPublishIsSetIdx] = true;
        }
    }

    private string m_runProgramPath = "";
    [Obsolete("Deprecated use RunProgramPath on ScenarioPublishPostPublish instead, field not serialized")]
    public string RunProgramPath
    {
        get => m_runProgramPath;
        set
        {
            m_runProgramPath = value;
            m_isSetBools[c_runProgramPathIsSetIdx] = true;
        }
    }

    private string m_runProgramCommandLine = "";
    [Obsolete("Deprecated use RunProgramCommandLine on ScenarioPublishPostPublish instead, field not serialized")]
    public string RunProgramCommandLine
    {
        get => m_runProgramCommandLine;
        set
        {
            m_runProgramCommandLine = value;
            m_isSetBools[c_runProgramCommandLineIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use RunMicrosoftProjectStoredProcedureAfterPublish on ScenarioPublishPostPublish instead, field not serialized")]
    public bool RunMicrosoftProjectStoredProcedureAfterPublish
    {
        get => m_boolVector[RunMicrosoftProjectStoredProcedureAfterPublishIdx];
        set
        {
            m_boolVector[RunMicrosoftProjectStoredProcedureAfterPublishIdx] = value;
            m_isSetBools[c_runMicrosoftProjectStoredProcedureAfterPublishIsSetIdx] = true;
        }
    }

    private string m_netChangeStoredProcedureName = "APS_NetChangePublish";
    [Obsolete("Deprecated use NetChangeStoredProcedureName on ScenarioPublishNetChange instead, field not serialized")]
    public string NetChangeStoredProcedureName
    {
        get => m_netChangeStoredProcedureName;
        set
        {
            m_netChangeStoredProcedureName = value;
            m_isSetBools[c_netChangeStoredProcedureNameIsSetIdx] = true;
        }
    }

    [Obsolete("Deprecated use ReportsUseIntegratedSecurity on ScenarioPublishReportSecurity instead, field not serialized")]
    public bool ReportsUseIntegratedSecurity
    {
        get => m_boolVector[ReportsUseIntegratedSecurityIdx];
        set
        {
            m_boolVector[ReportsUseIntegratedSecurityIdx] = value;
            m_isSetBools[c_reportsUseIntegratedSecurityIsSetIdx] = true;
        }
    }

    private string m_reportSecurityUserName = "";

    [Obsolete("Deprecated use ReportSecurityUserName on ScenarioPublishReportSecurity instead, field not serialized")]
    public string ReportSecurityUserName
    {
        get => m_reportSecurityUserName;
        set
        {
            m_reportSecurityUserName = value;
            m_isSetBools[c_reportsSecurityUserNameIsSetIdx] = true;
        }
    }

    private string m_reportSecurityPassword = "";

    [Obsolete("Deprecated use ReportSecurityPassword on ScenarioPublishReportSecurity instead, field not serialized")]
    public string ReportSecurityPassword
    {
        get => m_reportSecurityPassword;
        set
        {
            m_reportSecurityPassword = value;
            m_isSetBools[c_reportsSecurityPasswordIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishToSQL on ScenarioPublishDestinations instead, field not serialized")]
    public bool PublishToSQL
    {
        get => m_boolVector[PublishToSqlIdx];
        set
        {
            m_boolVector[PublishToSqlIdx] = value;
            m_isSetBools[c_publishToSqlIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishToXML on ScenarioPublishDestinations instead, field not serialized")]
    public bool PublishToXML
    {
        get => m_boolVector[PublishToXmlIdx];
        set
        {
            m_boolVector[PublishToXmlIdx] = value;
            m_isSetBools[c_publishToXmlIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishToCustomDll on ScenarioPublishDestinations instead, field not serialized")]
    public bool PublishToCustomDll
    {
        get => m_boolVector[PublishToCustomDllIdx];
        set
        {
            m_boolVector[PublishToCustomDllIdx] = value;
            m_isSetBools[c_publishToCustomDllIsSetIdx] = true;
        }
    }

    [Obsolete("Deprecated use AskUserForResourcesToPublish on PTPublishPreferences instead, field not serialized")]
    private bool AskUserForResourcesToPublish
    {
        get => m_boolVector[AskUserForResourcesToPublishIdx];
        set
        {
            m_boolVector[AskUserForResourcesToPublishIdx] = value;
            m_isSetBools[c_askUserForResourcesToPublishIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishAllActivitesForMO on ScenarioPublishDestinations instead, field not serialized")]
    public bool PublishAllActivitesForMO
    {
        get => m_boolVector[PublishAllActivitesForMOIdx];
        set
        {
            m_boolVector[PublishAllActivitesForMOIdx] = value;
            m_isSetBools[c_publishAllActivitiesForMOIsSetIdx] = true;
        }
    }

    [Obsolete("Deprecated use PublishToDashboard on ScenarioPublishDestinations instead, field not serialized")]
    public bool PublishToDashboard
    {
        get => m_boolVector[c_publishToDashboardIdx];
        set
        {
            m_boolVector[c_publishToDashboardIdx] = value;
            m_isSetBools2[c_publishToDashboardIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishProductRules on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishProductRules
    {
        get => m_boolVector[c_publishProductRulesIdx];
        set
        {
            m_boolVector[c_publishProductRulesIdx] = value;
            m_isSetBools[c_publishProductRulesIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryProductRules on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryProductRules
    {
        get => m_boolVector[c_keepHistoryProductRulesIdx];
        set
        {
            m_boolVector[c_keepHistoryProductRulesIdx] = value;
            m_isSetBools[c_keepHistoryProductRulesIsSetIdx] = true;
        }
    }

    //HERE
    [Obsolete("Deprecated use PublishJobs on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishJobs
    {
        get => m_boolVector[c_publishJobsIdx];
        set
        {
            m_boolVector[c_publishJobsIdx] = value;
            m_isSetBools2[c_publishJobsIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryJobs on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryJobs
    {
        get => m_boolVector[c_keepHistoryJobsIdx];
        set
        {
            if (value && !PublishJobs)
            {
                throw new APSCommon.PTValidationException("Cannot keep history for Jobs if they're not being published.");
            }

            m_boolVector[c_keepHistoryJobsIdx] = value;
            m_isSetBools2[c_keepHistoryJobsIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishTemplates on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishTemplates
    {
        get => m_boolVector[c_publishTemplatesIdx];
        set
        {
            if (value && !PublishJobs)
            {
                throw new APSCommon.PTValidationException("Publish Jobs must be turned on to publish Templates.");
            }

            m_boolVector[c_publishTemplatesIdx] = value;
            m_isSetBools2[c_publishTemplatesIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryTemplates on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryTemplates
    {
        get => m_boolVector[c_keepHistoryTemplatesIdx];
        set
        {
            if (value && !PublishTemplates)
            {
                throw new APSCommon.PTValidationException("Cannot keep history for Templates if they're not being published.");
            }

            m_boolVector[c_keepHistoryTemplatesIdx] = value;
            m_isSetBools2[c_keepHistoryTemplatesIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishManufacturingOrders on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishManufacturingOrders
    {
        get => m_boolVector[c_publishManufacturingOrdersIdx];
        set
        {
            if (value && !PublishJobs)
            {
                throw new APSCommon.PTValidationException("Publish Jobs must be turned on to publish Manufacturing Orders.");
            }

            m_boolVector[c_publishManufacturingOrdersIdx] = value;
            m_isSetBools2[c_publishManufacturingOrdersIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryManufacturingOrders on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryManufacturingOrders
    {
        get => m_boolVector[c_keepHistoryManufacturingOrdersIdx];
        set
        {
            m_boolVector[c_keepHistoryManufacturingOrdersIdx] = value;
            m_isSetBools2[c_keepHistoryManufacturingOrdersIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishOperations on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishOperations
    {
        get => m_boolVector2[c_publishOperationsIdx];
        set
        {
            if (value && !PublishManufacturingOrders)
            {
                throw new APSCommon.PTValidationException("Publish ManufacturingOrders must be turned on to publish Operations.");
            }

            m_boolVector2[c_publishOperationsIdx] = value;
            m_isSetBools2[c_publishOperationsIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryOperations on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryOperations
    {
        get => m_boolVector2[c_keepHistoryOperationsIdx];
        set
        {
            if (value && !PublishOperations)
            {
                throw new APSCommon.PTValidationException("Cannot keep history for Operations if they're not being published.");
            }

            m_boolVector2[c_keepHistoryOperationsIdx] = value;
            m_isSetBools2[c_keepHistoryOperationsIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishActivities on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishActivities
    {
        get => m_boolVector2[c_publishActivitiesIdx];
        set
        {
            if (value && !PublishOperations)
            {
                throw new APSCommon.PTValidationException("Publish Operations must be turned on to publish Activities");
            }

            m_boolVector2[c_publishActivitiesIdx] = value;
            m_isSetBools2[c_publishActivitiesIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryActivities on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryActivities
    {
        get => m_boolVector2[c_keepHistoryActivitiesIdx];
        set
        {
            if (value && !PublishActivities)
            {
                throw new APSCommon.PTValidationException("Cannot keep history for Activities if they're not being published.");
            }

            m_boolVector2[c_keepHistoryActivitiesIdx] = value;
            m_isSetBools2[c_keepHistoryActivitiesIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishBlocks on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishBlocks
    {
        get => m_boolVector[PublishBlocksIdx];

        set
        {
            if (value && !PublishActivities)
            {
                throw new APSCommon.PTValidationException("Publish Activities must be turned on to publish Blocks");
            }

            m_boolVector[PublishBlocksIdx] = value;
            m_isSetBools[c_publishBlocksIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryBlocks on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryBlocks
    {
        get => m_boolVector[KeepHistoryBlocksIdx];

        set
        {
            if (value && !PublishBlocks)
            {
                throw new APSCommon.PTValidationException("Cannot keep history for Blocks if they're not being published.");
            }

            m_boolVector[KeepHistoryBlocksIdx] = value;
            m_isSetBools[c_keepHistoryBlocksIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use PublishBlockIntervals on ScenarioPublishDataLimits instead, field not serialized")]
    public bool PublishBlockIntervals
    {
        get => m_boolVector[PublishBlockIntervalsIdx];

        set
        {
            if (value && !PublishBlocks)
            {
                throw new APSCommon.PTValidationException("Publish Blocks must be turned on to publish Block Intervals.");
            }

            m_boolVector[PublishBlockIntervalsIdx] = value;
            m_isSetBools[c_publishBlockIntervalsIsSetIdx] = true;
        }
    }
    [Obsolete("Deprecated use KeepHistoryBlockIntervals on ScenarioPublishDataLimits instead, field not serialized")]
    public bool KeepHistoryBlockIntervals
    {
        get => m_boolVector[KeepHistoryBlockIntervalsIdx];

        set
        {
            if (value && !PublishBlockIntervals)
            {
                throw new APSCommon.PTValidationException("Cannot keep history for Block Intervals if they're not being published.");
            }

            m_boolVector[KeepHistoryBlockIntervalsIdx] = value;
            m_isSetBools[c_keepHistoryBlockIntervalsIsSetIdx] = true;
        }
    }

    #region "IsSet Bools"
    private BoolVector32 m_isSetBools;
    private BoolVector32 m_isSetBools2;

    private const int c_enableHistoryIsSetIdx = 0;
    private const int c_publishInventoryIsSetIdx = 1;
    private const int c_publishBlocksIsSetIdx = 2;
    private const int c_publishBlockIntervalsIsSetIdx = 3;
    private const int c_keepHistoryInventoryIsSetIdx = 4;
    private const int c_keepHistoryBlocksIsSetIdx = 5;
    private const int c_keepHistoryBlockIntervalsIsSetIdx = 6;
    private const int c_publishHorizonSpanIsSetIdx = 7;
    private const int c_historyHorizonSpanIsSetIdx = 8;
    private const int c_historyMaxAgeIsSetIdx = 9;
    private const int c_whatIfHistoryMaxAgeIsSetIdx = 10;
    private const int c_netChangePublishingEnabledIsSetIdx = 11;
    private const int c_runStoredProcedureAfterNetChangePublishIsSetIdx = 12;
    private const int c_netChangeStoredProcedureNameIsSetIdx = 13;
    private const int c_runStoredProcedureAfterPublishIsSetIdx = 14;
    private const int c_postPublishStoredProcedureNameIsSetIdx = 15;
    private const int c_runMicrosoftProjectStoredProcedureAfterPublishIsSetIdx = 16;
    private const int c_runProgramAfterPublishIsSetIdx = 17;
    private const int c_runProgramPathIsSetIdx = 18;
    private const int c_runProgramCommandLineIsSetIdx = 19;
    private const int c_reportsUseIntegratedSecurityIsSetIdx = 20;
    private const int c_reportsSecurityUserNameIsSetIdx = 21;
    private const int c_reportsSecurityPasswordIsSetIdx = 22;
    private const int c_publishToSqlIsSetIdx = 23;
    private const int c_publishToXmlIsSetIdx = 24;
    private const int c_publishToCustomDllIsSetIdx = 25;
    private const int c_askUserForResourcesToPublishIsSetIdx = 26;
    private const int c_publishAllActivitiesForMOIsSetIdx = 27;
    private const int c_publishCapacityIntervalsIsSetIdx = 28;
    private const int c_keepCapacityIntervalHistoryIsSetIdx = 29;
    private const int c_publishProductRulesIsSetIdx = 30;
    private const int c_keepHistoryProductRulesIsSetIdx = 31;
    private const int c_publishToDashboardIsSetIdx = 0;
    private const int c_publishJobsIsSetIdx = 1;
    private const int c_keepHistoryJobsIsSetIdx = 2;
    private const int c_publishTemplatesIsSetIdx = 3;
    private const int c_keepHistoryTemplatesIsSetIdx = 4;
    private const int c_publishManufacturingOrdersIsSetIdx = 5;
    private const int c_keepHistoryManufacturingOrdersIsSetIdx = 6;
    private const int c_publishOperationsIsSetIdx = 7;
    private const int c_keepHistoryOperationsIsSetIdx = 8;
    private const int c_publishActivitiesIsSetIdx = 9;
    private const int c_keepHistoryActivitiesIsSetIdx = 10;
    private const int c_automaticPublishIsSetIdx = 11;
    private const int c_automaticPublishDelayIsSetIdx = 12;
    private const int c_exportDestinationIsSetIdx = 13;
    private const int c_exportAutomaticallyOnPublishScenarioIsSetIdx = 14; //Obsolete
    //private const int c_publishUsingLocalTimeIsSetIdx = 15;

    public bool EnableHistoryIsSet => m_isSetBools[c_enableHistoryIsSetIdx];
    public bool PublishInventoryIsSet => m_isSetBools[c_publishInventoryIsSetIdx];
    public bool PublishBlocksIsSet => m_isSetBools[c_publishBlocksIsSetIdx];
    public bool PublishBlockIntervalsIsSet => m_isSetBools[c_publishBlockIntervalsIsSetIdx];
    public bool KeepHistoryInventoryIsSet => m_isSetBools[c_keepHistoryInventoryIsSetIdx];
    public bool KeepHistoryBlocksIsSet => m_isSetBools[c_keepHistoryBlocksIsSetIdx];
    public bool KeepHistoryBlockIntervalsIsSet => m_isSetBools[c_keepHistoryBlockIntervalsIsSetIdx];
    public bool PublishHorizonSpanIsSet => m_isSetBools[c_publishHorizonSpanIsSetIdx];
    public bool HistoryHorizonSpanIsSet => m_isSetBools[c_historyHorizonSpanIsSetIdx];
    public bool HistoryMaxAgeIsSet => m_isSetBools[c_historyMaxAgeIsSetIdx];
    public bool WhatIfHistoryMaxAgeIsSet => m_isSetBools[c_whatIfHistoryMaxAgeIsSetIdx];
    public bool NetChangePublishingEnabledIsSet => m_isSetBools[c_netChangePublishingEnabledIsSetIdx];
    public bool RunStoredProcedureAfterNetChangePublishIsSet => m_isSetBools[c_runStoredProcedureAfterNetChangePublishIsSetIdx];
    public bool NetChangeStoredProcedureNameIsSet => m_isSetBools[c_netChangeStoredProcedureNameIsSetIdx];
    public bool RunStoredProcedureAfterPublishIsSet => m_isSetBools[c_runStoredProcedureAfterPublishIsSetIdx];
    public bool PostPublishStoredProcedureNameIsSet => m_isSetBools[c_postPublishStoredProcedureNameIsSetIdx];
    public bool RunMicrosoftProjectStoredProcedureAfterPublishIsSet => m_isSetBools[c_runMicrosoftProjectStoredProcedureAfterPublishIsSetIdx];
    public bool RunProgramAfterPublishIsSet => m_isSetBools[c_runProgramAfterPublishIsSetIdx];
    public bool RunProgramPathIsSet => m_isSetBools[c_runProgramPathIsSetIdx];
    public bool RunProgramCommandLineIsSet => m_isSetBools[c_runProgramCommandLineIsSetIdx];
    public bool ReportsUseIntegratedSecurityIsSet => m_isSetBools[c_reportsUseIntegratedSecurityIsSetIdx];
    public bool ReportSecurityUserNameIsSet => m_isSetBools[c_reportsSecurityUserNameIsSetIdx];
    public bool ReportSecurityPasswordIsSet => m_isSetBools[c_reportsSecurityPasswordIsSetIdx];
    public bool PublishToSQLIsSet => m_isSetBools[c_publishToSqlIsSetIdx];
    public bool PublishToXMLIsSet => m_isSetBools[c_publishToXmlIsSetIdx];
    public bool PublishToCustomDllIsSet => m_isSetBools[c_publishToCustomDllIsSetIdx];
    public bool AskUserForResourcesToPublishIsSet => m_isSetBools[c_askUserForResourcesToPublishIsSetIdx];
    public bool PublishAllActivitesForMOIsSet => m_isSetBools[c_publishAllActivitiesForMOIsSetIdx];
    public bool PublishCapacityIntervalsIsSet => m_isSetBools[c_publishCapacityIntervalsIsSetIdx];
    public bool KeepCapacityIntervalsIsSet => m_isSetBools[c_keepCapacityIntervalHistoryIsSetIdx];
    public bool PublishProductRulesIsSet => m_isSetBools[c_publishProductRulesIsSetIdx];
    public bool KeepHistoryProductRulesIsSet => m_isSetBools[c_keepHistoryProductRulesIsSetIdx];
    public bool PublishToDashboardIsSet => m_isSetBools2[c_publishToDashboardIsSetIdx];
    public bool PublishJobsIsSet => m_isSetBools2[c_publishJobsIsSetIdx];
    public bool KeepHistoryJobsIsSet => m_isSetBools2[c_keepHistoryJobsIsSetIdx];
    public bool PublishTemplatesIsSet => m_isSetBools2[c_publishTemplatesIsSetIdx];
    public bool KeepHistoryTemplatesIsSet => m_isSetBools2[c_keepHistoryTemplatesIsSetIdx];
    public bool PublishManufacturingOrdersIsSet => m_isSetBools2[c_publishManufacturingOrdersIsSetIdx];
    public bool KeepHistoryManufacturingOrdersIsSet => m_isSetBools2[c_keepHistoryManufacturingOrdersIsSetIdx];
    public bool PublishOperationsIsSet => m_isSetBools2[c_publishOperationsIsSetIdx];
    public bool KeepHistoryOperationsIsSet => m_isSetBools2[c_keepHistoryOperationsIsSetIdx];
    public bool PublishActivitiesIsSet => m_isSetBools2[c_publishActivitiesIsSetIdx];
    public bool KeepHistoryActivitiesIsSet => m_isSetBools2[c_keepHistoryActivitiesIsSetIdx];
    public bool AutomaticPublishIsSet => m_isSetBools2[c_automaticPublishIsSetIdx];
    public bool AutomaticPublishDelayIsSet => m_isSetBools2[c_automaticPublishDelayIsSetIdx];
    public bool ExportDestinationIsSet => m_isSetBools2[c_exportDestinationIsSetIdx];
    #endregion
    #endregion Properties

    #region ICloneable Members
    object ICloneable.Clone()
    {
        return Clone();
    }

    public ScenarioPublishOptions Clone()
    {
        return (ScenarioPublishOptions)MemberwiseClone();
    }
    #endregion
}