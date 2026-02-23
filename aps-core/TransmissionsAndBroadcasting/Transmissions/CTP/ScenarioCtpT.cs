using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions.CTP;

/// <summary>
/// Transmission to get a Capable to Promise response.
/// </summary>
public class ScenarioCtpT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 807;

    #region IPTSerializable Members
    public ScenarioCtpT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12031)
        {
            m_bools = new BoolVector32(a_reader);
            ctp = new Ctp(a_reader);

            if (!UseScenarioOptimizeSettings)
            {
                OptimizeSettings = new OptimizeSettings(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            ctp = new Ctp(a_reader);
        }
        else if (a_reader.VersionNumber >= 748)
        {
            m_bools = new BoolVector32(a_reader);
            ctp = new Ctp(a_reader);

            if (!UseScenarioOptimizeSettings)
            {
                OptimizeSettings = new OptimizeSettings(a_reader);
            }
        }
        else
        {
            ctp = new Ctp(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        ctp.Serialize(a_writer);

        OptimizeSettings?.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioCtpT() { }

    public ScenarioCtpT(BaseId a_scenarioId, OptimizeSettings a_optimizeSettings)
        : base(a_scenarioId)
    {
        OptimizeSettings = a_optimizeSettings;
        UseScenarioOptimizeSettings = a_optimizeSettings == null;
    }

    public Ctp ctp = new ();
    private BoolVector32 m_bools;
    private const short c_useScenarioOptimizeSettings = 0;

    public bool UseScenarioOptimizeSettings
    {
        get => m_bools[c_useScenarioOptimizeSettings];
        set => m_bools[c_useScenarioOptimizeSettings] = value;
    }

    public OptimizeSettings OptimizeSettings { get; private set; }

    public override string Description => string.Format("CTP {0} {1} ".Localize(), ctp.JobName, ctp.Customer);
}