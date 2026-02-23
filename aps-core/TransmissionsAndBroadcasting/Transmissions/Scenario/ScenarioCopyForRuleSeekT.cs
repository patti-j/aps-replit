using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Create a new Scenario by copying an old one. Also contains BalancedCompositeDispatcherDefinitionUpdateTs to adjust rule sets after the scenario is created.
/// </summary>
public class ScenarioCopyForRuleSeekT : ScenarioCopyT, IPTSerializable
{
    public new const int UNIQUE_ID = 772;

    #region IPTSerializable Members
    public ScenarioCopyForRuleSeekT(IReader reader)
        : base(reader)
    {
        #region 1
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_score);
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                BalancedCompositeDispatcherDefinitionUpdateT newUpdateT = new (reader);
                m_ruleUpdateList.Add(newUpdateT);
            }

            m_bools = new BoolVector32(reader);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_score);
        writer.Write(m_ruleUpdateList.Count);
        for (int i = 0; i < m_ruleUpdateList.Count; i++)
        {
            m_ruleUpdateList[i].Serialize(writer);
        }

        m_bools.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly List<BalancedCompositeDispatcherDefinitionUpdateT> m_ruleUpdateList = new ();

    public List<BalancedCompositeDispatcherDefinitionUpdateT> RuleUpdateList => m_ruleUpdateList;

    private readonly decimal m_score;

    public decimal Score => m_score;

    public bool IsLowerBetter
    {
        get => m_bools[c_isLowerBetterIdx];
        set => m_bools[c_isLowerBetterIdx] = value;
    }

    private BoolVector32 m_bools;
    private const int c_isLowerBetterIdx = 0;

    public ScenarioCopyForRuleSeekT() { }

    public ScenarioCopyForRuleSeekT(BaseId originalId, ScenarioTypes a_scenarioType, List<BalancedCompositeDispatcherDefinitionUpdateT> a_ruleUpdatesList, string a_kpiName, decimal a_score, bool a_isLowerBetter, string a_kpiFormatString)
        : base(originalId, a_scenarioType, a_kpiName + ": " + a_score.ToString(a_kpiFormatString))
    {
        m_bools = new BoolVector32();

        m_ruleUpdateList = a_ruleUpdatesList;
        m_score = a_score;
        IsLowerBetter = a_isLowerBetter;
    }
}