using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Requests that the server export a Scenario to the SQL Server Database or an XML file.
/// </summary>
public class ScenarioDetailExportT : ScenarioIdBaseT, IPTSerializable
{
    public static readonly int UNIQUE_ID = 432;

    #region IPTSerializable Members
    public ScenarioDetailExportT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 649)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_exportDestination = (EExportDestinations)val;
            a_reader.Read(out int resCount);
            for (int i = 0; i < resCount; i++)
            {
                BaseId id = new (a_reader);
                m_resourcesToExport.Add(id);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_bools.Serialize(writer);
        writer.Write((int)m_exportDestination);
        writer.Write(m_resourcesToExport.Count);
        foreach (BaseId id in m_resourcesToExport)
        {
            id.Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;

    private const short c_limitToSpecificResourcesIdx = 0;
    private const short c_automatic = 1;

    public ScenarioDetailExportT() { }

    public ScenarioDetailExportT(BaseId a_scenarioToExport, EExportDestinations a_exportDestination)
        : base(a_scenarioToExport)
    {
        m_exportDestination = a_exportDestination;
        LogErrorToUsualLogFile = true; //This only is handled on the server so the user will not be made aware of it otherwise.
    }

    private readonly EExportDestinations m_exportDestination;
    public EExportDestinations ExportDestination => m_exportDestination;

    /// <summary>
    /// If true the only export Activities on the Resources specified by ResourcesToExport.
    /// </summary>
    public bool LimitToSpecifiedResources
    {
        get => m_bools[c_limitToSpecificResourcesIdx];
        set => m_bools[c_limitToSpecificResourcesIdx] = value;
    }

    /// <summary>
    /// Whether this transmission was sent automatically based on scenario options
    /// </summary>
    public bool SentAutomatically
    {
        get => m_bools[c_automatic];
        set => m_bools[c_automatic] = value;
    }

    private HashSet<BaseId> m_resourcesToExport = new ();

    public HashSet<BaseId> ResourcesToExport
    {
        get => m_resourcesToExport;
        set => m_resourcesToExport = value;
    }

    public override string Description => LimitToSpecifiedResources ? "Scenario published (Limited Resources)" : "Scenario published";
}