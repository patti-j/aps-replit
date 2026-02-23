using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Create a new Scenario by copying an old one. Also contains ScenarioDetailExpediteJobTs to schedule jobs after the scenario is created.
/// </summary>
public class ScenarioCopyForInsertJobsT : ScenarioCopyT, IPTSerializable
{
    public new const int UNIQUE_ID = 776;

    #region IPTSerializable Members
    public ScenarioCopyForInsertJobsT(IReader reader)
        : base(reader)
    {
        #region 1
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ScenarioDetailExpediteJobsT newUpdateT = new (reader);
                m_expediteList.Add(newUpdateT);
            }
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_expediteList.Count);
        for (int i = 0; i < m_expediteList.Count; i++)
        {
            m_expediteList[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly List<ScenarioDetailExpediteJobsT> m_expediteList = new ();

    public List<ScenarioDetailExpediteJobsT> ExpediteList => m_expediteList;

    public ScenarioCopyForInsertJobsT() { }

    public ScenarioCopyForInsertJobsT(BaseId originalId, ScenarioTypes a_scenarioType, List<ScenarioDetailExpediteJobsT> a_ruleUpdatesList, int a_scheduled, int a_total)
        : base(originalId, a_scenarioType, GetNameOveride(a_scheduled, a_total))
    {
        m_expediteList = a_ruleUpdatesList;
    }

    private static string GetNameOveride(int a_scheduledOnTime, int a_total)
    {
        if (a_total == 0)
        {
            return "No new Jobs";
        }

        return a_scheduledOnTime + " / " + a_total;
    }
}