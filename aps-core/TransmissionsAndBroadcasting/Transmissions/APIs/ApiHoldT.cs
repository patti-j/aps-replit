using PT.APSCommon;
using PT.SchedulerDefinitions.Templates.Lists;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Holding or Unholding a list of Operations.
/// </summary>
public class ApiHoldT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 836;

    #region IPTSerializable Members
    public ApiHoldT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_objects = new List<ExternalIdObject>();
            a_reader.Read(out m_hold);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ExternalIdObject externalIdObject = new (a_reader);
                m_objects.Add(externalIdObject);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_hold);
        writer.Write(m_holdReason);
        writer.Write(m_holdUntilDate);
        writer.Write(m_objects);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ApiHoldT() { }

    public ApiHoldT(BaseId a_scenarioId, IEnumerable<ExternalIdObject> a_objects, bool a_hold, DateTime a_holdUntilDate, string a_holdReason)
        : base(a_scenarioId)
    {
        m_objects = new List<ExternalIdObject>();
        m_objects.AddRange(a_objects);
        m_hold = a_hold;
        m_holdUntilDate = a_holdUntilDate;
        m_holdReason = a_holdReason;
    }

    private readonly List<ExternalIdObject> m_objects = new ();
    public List<ExternalIdObject> Objects => m_objects;

    private readonly bool m_hold;
    public bool Hold => m_hold;

    private readonly DateTime m_holdUntilDate;
    public DateTime HoldUntilDate => m_holdUntilDate;

    private readonly string m_holdReason;

    public string HoldReason => m_holdReason;

    public override string Description
    {
        get
        {
            if (m_hold)
            {
                return "Jobs put on hold";
            }

            return "Jobs taken off hold";
        }
    }
}