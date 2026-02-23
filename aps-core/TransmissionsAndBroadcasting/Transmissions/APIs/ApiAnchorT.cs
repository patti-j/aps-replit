using PT.APSCommon;
using PT.SchedulerDefinitions.Templates.Lists;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Lock  or Unlocking a list of Operations.
/// </summary>
public class ApiAnchorT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 838;

    #region IPTSerializable Members
    public ApiAnchorT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_objects = new List<ExternalIdObject>();
            a_reader.Read(out m_lock);
            a_reader.Read(out m_anchor);
            a_reader.Read(out m_anchorDate);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ExternalIdObject externalIdObject = new (a_reader);
                m_objects.Add(externalIdObject);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_lock);
        a_writer.Write(m_anchor);
        a_writer.Write(m_anchorDate);
        a_writer.Write(m_objects);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ApiAnchorT() { }

    public ApiAnchorT(BaseId a_scenarioId, IEnumerable<ExternalIdObject> a_objects, bool a_anchor, bool a_lock, DateTime a_anchorDate)
        : base(a_scenarioId)
    {
        m_objects = new List<ExternalIdObject>();
        m_objects.AddRange(a_objects);
        m_anchorDate = a_anchorDate;
        m_lock = a_lock;
        m_anchor = a_anchor;
    }

    private readonly List<ExternalIdObject> m_objects = new ();
    public List<ExternalIdObject> Objects => m_objects;

    private readonly bool m_lock;
    public bool Lock => m_lock;

    private readonly bool m_anchor;
    public bool Anchor => m_anchor;

    private readonly DateTime m_anchorDate = PTDateTime.MinDateTime;
    public DateTime AnchorDate => m_anchorDate;

    public override string Description
    {
        get
        {
            if (m_anchorDate != PTDateTime.MinDateTime)
            {
                return m_lock ? "Jobs Anchored and locked" : "Jobs Anchored'";
            }

            return "Jobs UnAnchored";
        }
    }
}