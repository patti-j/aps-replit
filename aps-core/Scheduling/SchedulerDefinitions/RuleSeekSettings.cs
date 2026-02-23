using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Stores System wide settings for RuleSeek. This is used in ScenarioManager.
/// </summary>
public class RuleSeekSettings : IPTSerializable, IEquatable<RuleSeekSettings>
{
    public const int UNIQUE_ID = 770;

    #region IPTSerializable Members
    public RuleSeekSettings(IReader reader)
    {
        if (reader.VersionNumber >= 12034)
        {
            m_bools = new BoolVector32(reader);

            //Enums
            reader.Read(out short enumInt);
            m_simResults = (SimulationResultsType)enumInt;
            reader.Read(out enumInt);
            m_simMode = (SliderAdjustmentLogic)enumInt;
            reader.Read(out enumInt);
            m_ruleAdjustmentMode = (SlidersToAdjustMode)enumInt;

            reader.Read(out m_maxScenariosToKeep);
            reader.Read(out m_kpiToRun);
            reader.Read(out m_idleTimeDuration);

            //Lists
            reader.Read(out int ruleSetCount);
            for (int i = 0; i < ruleSetCount; i++)
            {
                BaseId ruleSet = new (reader);
                m_RuleSets.Add(ruleSet);
            }

            m_sourceScenarioId = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 460)
        {
            m_bools = new BoolVector32(reader);

            //Enums
            short enumInt;
            reader.Read(out enumInt);
            m_simResults = (SimulationResultsType)enumInt;
            reader.Read(out enumInt);
            m_simMode = (SliderAdjustmentLogic)enumInt;
            reader.Read(out enumInt);
            m_ruleAdjustmentMode = (SlidersToAdjustMode)enumInt;

            reader.Read(out m_maxScenariosToKeep);
            reader.Read(out m_kpiToRun);
            reader.Read(out m_idleTimeDuration);

            //Lists
            int ruleSetCount;
            reader.Read(out ruleSetCount);
            BaseId ruleSet;
            for (int i = 0; i < ruleSetCount; i++)
            {
                ruleSet = new BaseId(reader);
                m_RuleSets.Add(ruleSet);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        m_bools.Serialize(writer);
        writer.Write((short)m_simResults);
        writer.Write((short)m_simMode);
        writer.Write((short)m_ruleAdjustmentMode);
        writer.Write(m_maxScenariosToKeep);
        writer.Write(m_kpiToRun);
        writer.Write(m_idleTimeDuration);
        writer.Write(m_RuleSets.Count);
        for (int i = 0; i < m_RuleSets.Count; i++)
        {
            m_RuleSets[i].Serialize(writer);
        }

        m_sourceScenarioId.Serialize(writer);
    }

    public RuleSeekSettings()
    {
        m_bools = new BoolVector32(false);
        m_simResults = SimulationResultsType.CreateScenario;
        m_simMode = SliderAdjustmentLogic.Random;
        m_ruleAdjustmentMode = SlidersToAdjustMode.InUse;
        m_maxScenariosToKeep = 1;
        m_kpiToRun = "";
        m_idleTimeDuration = TimeSpan.FromMinutes(15).Ticks;
    }

    /// <summary>
    /// Compares two settings to see if they are equal.
    /// </summary>
    /// <returns>True if the settings are different</returns>

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region MEMBERS
    private BoolVector32 m_bools;

    private const short c_enabledIdx = 0;

    /// <summary>
    /// Whether to use RuleSeek
    /// </summary>
    public bool Enabled
    {
        get => m_bools[c_enabledIdx];
        set => m_bools[c_enabledIdx] = value;
    }

    private SimulationResultsType m_simResults;

    /// <summary>
    /// How to report Results
    /// </summary>
    public SimulationResultsType SimulationResultType
    {
        get => m_simResults;
        set => m_simResults = value;
    }

    private SliderAdjustmentLogic m_simMode;

    /// <summary>
    /// What logic is used to adjust the rule set values.
    /// </summary>
    public SliderAdjustmentLogic SliderAdjustmentModeType
    {
        get => m_simMode;
        set => m_simMode = value;
    }

    private SlidersToAdjustMode m_ruleAdjustmentMode;

    /// <summary>
    /// Which slider values to include in the adjustments
    /// </summary>
    public SlidersToAdjustMode RuleAdjustmentMode
    {
        get => m_ruleAdjustmentMode;
        set => m_ruleAdjustmentMode = value;
    }

    private int m_maxScenariosToKeep;

    /// <summary>
    /// The maximum number of best RuleSeek scenarios to keep
    /// </summary>
    public int MaxScenariosToKeep
    {
        get => m_maxScenariosToKeep;
        set => m_maxScenariosToKeep = value;
    }

    private string m_kpiToRun;

    /// <summary>
    /// Name of the KPI to calculate a score for during simulations
    /// </summary>
    public string KpiToRun
    {
        get => m_kpiToRun;
        set => m_kpiToRun = value;
    }

    private List<BaseId> m_RuleSets = new ();

    /// <summary>
    /// List of rule set ids that will be adjusted during simulations
    /// </summary>
    public List<BaseId> RuleSets
    {
        get => m_RuleSets;
        set => m_RuleSets = value;
    }

    private long m_idleTimeDuration;

    /// <summary>
    /// Duration to wait after last action to start RuleSeek simulations
    /// </summary>
    public TimeSpan IdleTimeDuration
    {
        get => new (m_idleTimeDuration);
        set => m_idleTimeDuration = value.Ticks;
    }

    private BaseId m_sourceScenarioId;

    public BaseId SourceScenarioId
    {
        get => m_sourceScenarioId;
        set => m_sourceScenarioId = value;
    }
    #endregion

    public enum SimulationResultsType { CreateScenario }

    public enum SliderAdjustmentLogic { Random, Sequential, Incremental, SequentialRandom }

    public enum SlidersToAdjustMode { InUse, All, AccordingToRuleSettings }

    public bool Equals(RuleSeekSettings a_newSettings)
    {
        if (a_newSettings == null)
        {
            return false;
        }

        if (Enabled != a_newSettings.Enabled || KpiToRun != a_newSettings.KpiToRun || MaxScenariosToKeep != a_newSettings.MaxScenariosToKeep || RuleSets.Count != a_newSettings.RuleSets.Count || RuleAdjustmentMode != a_newSettings.RuleAdjustmentMode || SimulationResultType != a_newSettings.SimulationResultType || SliderAdjustmentModeType != a_newSettings.SliderAdjustmentModeType || IdleTimeDuration != a_newSettings.IdleTimeDuration)
        {
            return false;
        }

        for (int i = 0; i < RuleSets.Count; i++)
        {
            if (RuleSets[i].Value != a_newSettings.RuleSets[i].Value)
            {
                return false;
            }
        }

        return m_sourceScenarioId == a_newSettings.SourceScenarioId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((RuleSeekSettings)obj);
    }

    public override int GetHashCode()
    {
        return m_sourceScenarioId.GetHashCode();
    }
}