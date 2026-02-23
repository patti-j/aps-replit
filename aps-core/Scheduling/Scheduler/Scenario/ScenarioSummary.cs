using System.ComponentModel;

using PT.APSCommon;
using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Contains summary information for the Scenario that can be accessed separately so it is available even
/// when the Scenario Detail thread is locked.
/// </summary>
public class ScenarioSummary : BaseObject, ICloneable
{
    public new const int UNIQUE_ID = 345;

    #region IPTSerializable Members
    public ScenarioSummary(IReader a_reader)
        : base(a_reader)
    {
        #region 13000
        if (a_reader.VersionNumber >= 13000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_creationDate);
            a_reader.Read(out m_currentUserCount);
            a_reader.Read(out int val);
            m_type = (ScenarioTypes)val;

            m_creator = new BaseId(a_reader);

            //RuleSeek
            a_reader.Read(out m_ruleSeekScore);
            a_reader.Read(out m_lastRefreshDate);
            m_scenarioSettings = new GenericSettingSaver(a_reader);
        }
        #endregion
        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_creationDate);
            a_reader.Read(out m_currentUserCount);
            a_reader.Read(out int val);
            m_type = (ScenarioTypes)val;

            new TransmissionSequencer(a_reader);
            m_creator = new BaseId(a_reader);

            //RuleSeek
            a_reader.Read(out m_ruleSeekScore);
            a_reader.Read(out m_lastRefreshDate);
            m_scenarioSettings = new GenericSettingSaver(a_reader);
        }
        #endregion
        #region 495
        else if (a_reader.VersionNumber >= 495)
        {
            m_scenarioSettings = new GenericSettingSaver();
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_creationDate);
            a_reader.Read(out m_currentUserCount);
            a_reader.Read(out int val);
            m_type = (ScenarioTypes)val;

            new TransmissionSequencer(a_reader);
            m_creator = new BaseId(a_reader);

            //RuleSeek
            a_reader.Read(out m_ruleSeekScore);
        }
        #endregion
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write(m_creationDate);
        a_writer.Write(m_currentUserCount);
        a_writer.Write((int)m_type);

        // Is this something we want to do? This would make it so that
        // users could not set the Production status to false if the Scenario is a Live scenario
        //if (m_type == ScenarioTypes.Live)
        //{
        //    ScenarioPlanningSettings scenarioPlanningSettings = m_scenarioSettings.LoadSetting(new ScenarioPlanningSettings());
        //    scenarioPlanningSettings.Production = true;
        //    m_scenarioSettings.SaveSetting(scenarioPlanningSettings);
        //}

        m_creator.Serialize(a_writer);

        //RuleSeek
        a_writer.Write(m_ruleSeekScore);
        a_writer.Write(m_lastRefreshDate);

        m_scenarioSettings.Serialize(a_writer);
    }

    internal void RestoreReferences(int a_serializationVersionNumber)
    {
        if (m_type == ScenarioTypes.Live &&
            a_serializationVersionNumber < 12000)
        {
            SettingData newScenarioPlanningSettings = new (new ScenarioPlanningSettings());
            m_scenarioSettings.SaveSetting(newScenarioPlanningSettings, false);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    //Property names
    //public const string FROZEN_SPAN = "FrozenSpan";
    public const string TYPE = "Type";
    public const string CREATOR_ID = "Creator";

    public const string CURRENT_USER_COUNT = "CurrentUserCount";
    //public const string CREATION_DATE = "CreationDate";
    //public const string KEEP_FEASIBLE_SPAN = "KeepFeasibleSpan";
    //public const string PLANNING_HORIZON = "PlanningHorizon";
    //public const string SHORT_TERM_HORIZON = "ShortTermSpan";
    //public const string STABLE_SPAN = "StableSpan";
    #endregion

    #region Construction
    public ScenarioSummary(BaseId a_id, List<ISettingData> a_initialSettings, ScenarioTypes a_type, bool a_isProductionScenario, BaseId a_adminId, IUserManager a_userManager)
        : base(a_id)
    {
        m_bools = new BoolVector32();
        m_scenarioSettings = new GenericSettingSaver();
        m_type = a_type;

        if (a_initialSettings.Count == 0)
        {
            ScenarioPermissionSettings permSettings = GenerateDefaultScenarioPermissionSettings(a_adminId, a_userManager);
            a_initialSettings.Add(permSettings);
        }

        foreach (ISettingData data in a_initialSettings)
        {
            m_scenarioSettings.SaveSetting(data);
        }

        ScenarioPlanningSettings planningSettings = m_scenarioSettings.LoadSetting(new ScenarioPlanningSettings());
        planningSettings.Production = a_isProductionScenario;
        m_scenarioSettings.SaveSetting(planningSettings);
    }

    public static ScenarioPermissionSettings GenerateDefaultScenarioPermissionSettings(BaseId a_ownerId, IUserManager a_userManager)
    {
        ScenarioPermissionSettings defaultSettings = new ();
        defaultSettings.OwnerId = a_ownerId;
        defaultSettings.GroupIdToAccessLevel.Add(a_userManager.GetDefaultAdministrationGroupId(), EUserAccess.Edit);
        defaultSettings.UserIdToAccessLevel.Add(a_ownerId, EUserAccess.Edit);
        defaultSettings.GroupIdToAccessLevel.TryAdd(a_userManager.GetDefaultViewOnlyGroupId(), EUserAccess.ViewOnly);
        // TODO: Investigate and check if group permission is getting set correctly
        // I guess if the default ViewOnlyGroup isn't initialized, it defaults to 0 or something
        // because apparently both GetDefaultAdmin...() and GetDefaultView...() are returning 0 as the BaseId
        // when loading V11 scenarios so I had to change this 3rd Add to a TryAdd. It doesn't seem 
        // cause any issues, and it would just mean the DefaultViewOnly group, which isn't created 
        // when loading V11 scenarios, would not correctly get group permissions. 
        return defaultSettings;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Scenario";
    #endregion

    #region Properties
    private BoolVector32 m_bools;

    private long m_creationDate = PTDateTime.UtcNow.RemoveSeconds().Ticks;

    /// <summary>
    /// The date the scenario was first created.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime CreationDateTime
    {
        get => new (m_creationDate, DateTimeKind.Utc);
        set => m_creationDate = value.Ticks;
    }

    private BaseId m_creator = BaseId.NULL_ID;

    /// <summary>
    /// The User who created the Scenario.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public BaseId Creator
    {
        get => m_creator;
        set => m_creator = value;
    }

    private readonly int m_currentUserCount;

    /// <summary>
    /// Number of Users who currently have this Scenario loaded.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public int CurrentUserCount => m_currentUserCount;

    private ScenarioTypes m_type = ScenarioTypes.Whatif;

    /// <summary>
    /// There is always one Live Scenario that is under-construction and one Published Scenario that is followed on the shop floor.  What-Ifs can be used for experimenting.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    [ParenthesizePropertyName(true)]
    public ScenarioTypes Type => m_type;

    /// <summary>
    /// Only ScenarioManager should call this function.
    /// </summary>
    /// <param name="a_type"></param>
    internal void SetType(ScenarioTypes a_type)
    {
        m_type = a_type;
    }

    //The RuleSeek score calculated for the RuleSeekKpi
    private decimal m_ruleSeekScore;

    public decimal RuleSeekScore
    {
        get => m_ruleSeekScore;
        internal set => m_ruleSeekScore = value;
    }

    private DateTime m_lastRefreshDate = PTDateTime.MinDateTime;

    public DateTime LastRefreshDate
    {
        get => m_lastRefreshDate;
        internal set => m_lastRefreshDate = value;
    }

    private const int c_isBlackBoxScenarioIdx = 1;

    public bool IsBlackBoxScenario
    {
        get => m_bools[c_isBlackBoxScenarioIdx];
        internal set => m_bools[c_isBlackBoxScenarioIdx] = value;
    }

    private GenericSettingSaver m_scenarioSettings;

    public ISettingsManager ScenarioSettings => m_scenarioSettings;
    #endregion

    #region Cloning
    public ScenarioSummary Clone()
    {
        ScenarioSummary copy = (ScenarioSummary)MemberwiseClone();
        copy.m_scenarioSettings = new GenericSettingSaver(m_scenarioSettings);
        return copy;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion
}