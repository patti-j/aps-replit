using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions.CTP;

/// <summary>
/// Transmission to get a Capable to Promise response.
/// </summary>
public class CtpT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 618;

    #region IPTSerializable Members
    public CtpT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12031)
        {
            m_bools = new BoolVector32(a_reader);
            sourceScenarioId = new BaseId(a_reader);
            ctp = new Ctp(a_reader);

            if (!UseScenarioOptimizeSettings)
            {
                OptimizeSettings = new OptimizeSettings(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            sourceScenarioId = new BaseId(a_reader);
            ctp = new Ctp(a_reader);
        }
        else if (a_reader.VersionNumber >= 748)
        {
            m_bools = new BoolVector32(a_reader);
            sourceScenarioId = new BaseId(a_reader);
            ctp = new Ctp(a_reader);

            if (!UseScenarioOptimizeSettings)
            {
                OptimizeSettings = new OptimizeSettings(a_reader);
            }
        }
        else
        {
            sourceScenarioId = new BaseId(a_reader);
            ctp = new Ctp(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        sourceScenarioId.Serialize(a_writer);
        ctp.Serialize(a_writer);
        OptimizeSettings?.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CtpT() { }

    public CtpT(BaseId a_sourceScenarioId, OptimizeSettings a_optimizeSettings)
    {
        sourceScenarioId = a_sourceScenarioId;
        OptimizeSettings = a_optimizeSettings;
        UseScenarioOptimizeSettings = a_optimizeSettings == null;
    }

    private BoolVector32 m_bools;
    private const short c_useScenarioOptimizeSettings = 0;

    public bool UseScenarioOptimizeSettings
    {
        get => m_bools[c_useScenarioOptimizeSettings];
        set => m_bools[c_useScenarioOptimizeSettings] = value;
    }

    public OptimizeSettings OptimizeSettings { get; private set; }

    public BaseId sourceScenarioId; // The id of the scenario this CTP originated in
    public Ctp ctp = new ();
}