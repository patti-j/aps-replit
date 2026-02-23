using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Summary description for ScenarioDetailAlternatePathMoveT.
/// </summary>
public class ScenarioDetailAlternatePathMoveT : ScenarioDetailMoveT
{
    #region IPTSerializable Members
    public ScenarioDetailAlternatePathMoveT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 742)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                BaseId activityId = new (a_reader);
                a_reader.Read(out string pathId);
                m_activityAltPaths.Add(activityId, pathId);
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out string alternatePathExternalId);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_activityAltPaths.Count);
        foreach (KeyValuePair<BaseId, string> kv in m_activityAltPaths)
        {
            kv.Key.Serialize(a_writer);
            a_writer.Write(kv.Value);
        }
    }

    public new const int UNIQUE_ID = 579;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailAlternatePathMoveT() { }

    public ScenarioDetailAlternatePathMoveT(ScenarioDetailMoveT a_t, Dictionary<BaseId, string> a_activityAltPaths)
        : base(a_t)
    {
        Init(a_activityAltPaths);
    }

    private void Init(Dictionary<BaseId, string> a_activityAltPaths)
    {
        m_activityAltPaths = a_activityAltPaths;
    }

    private Dictionary<BaseId, string> m_activityAltPaths = new ();

    public Dictionary<BaseId, string> ActivityAltPaths
    {
        get => m_activityAltPaths;
        set => m_activityAltPaths = value;
    }
}