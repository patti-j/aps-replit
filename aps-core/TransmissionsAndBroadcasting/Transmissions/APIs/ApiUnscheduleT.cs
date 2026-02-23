using PT.APSCommon;
using PT.SchedulerDefinitions.Templates.Lists;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Lock  or Unlocking a list of Operations.
/// </summary>
public class ApiUnscheduleT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 839;

    #region IPTSerializable Members
    public ApiUnscheduleT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_objects = new List<ExternalIdObject>();
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
        a_writer.Write(m_objects);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ApiUnscheduleT() { }

    public ApiUnscheduleT(BaseId a_scenarioId, IEnumerable<ExternalIdObject> a_objects)
        : base(a_scenarioId)
    {
        m_objects = new List<ExternalIdObject>();
        m_objects.AddRange(a_objects);
    }

    private readonly List<ExternalIdObject> m_objects = new ();
    public List<ExternalIdObject> Objects => m_objects;

    public override string Description => "Jobs Unscheduled";
}