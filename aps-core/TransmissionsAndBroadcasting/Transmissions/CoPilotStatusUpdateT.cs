using PT.Common.Localization;

namespace PT.Transmissions;

public class CoPilotStatusUpdateT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 774;

    public CoPilotStatusUpdateT() { }

    public CoPilotStatusUpdateT(CoPilotStatusUpdateValues a_status, CoPilotErrorValues a_errorStatus)
    {
        m_status = a_status;
        m_errorStatus = a_errorStatus;
    }

    #region IPTSerializable Members
    public CoPilotStatusUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            short enumValue;
            reader.Read(out enumValue);
            m_status = (CoPilotStatusUpdateValues)enumValue;
            reader.Read(out enumValue);
            m_errorStatus = (CoPilotErrorValues)enumValue;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write((short)m_status);
        writer.Write((short)m_errorStatus);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public enum CoPilotStatusUpdateValues
    {
        RuleSeekStarted,
        InsertJobsStarted,
        InsertJobsStopped,
        Error,
        Progress,
        Enabled
    } //RuleSeekStopped is handled by a seperate transmission.

    public enum CoPilotErrorValues
    {
        FailedToStart,
        SimulationException,
        Unknown,
        None,
        InsertJobsNotStarted,
        InsertJobIsInitializing
    }

    private readonly CoPilotStatusUpdateValues m_status;
    private readonly CoPilotErrorValues m_errorStatus;

    /// <summary>
    /// The current status of the RuleSeekSimulations
    /// </summary>
    public CoPilotStatusUpdateValues Status => m_status;

    /// <summary>
    /// The Error status of the RuleSeekSimulations.
    /// This error is logged from the server. This is for display purposes
    /// </summary>
    public CoPilotErrorValues ErrorStatus => m_errorStatus;

    public string ErrorMessage
    {
        get
        {
            switch (m_errorStatus)
            {
                case CoPilotErrorValues.FailedToStart:
                    return Localizer.GetString("Failed to Start");
                case CoPilotErrorValues.SimulationException:
                    return Localizer.GetString("An error occurred during simulation");
                case CoPilotErrorValues.Unknown:
                    return Localizer.GetString("Error. CoPilot will attempt to restart");
                case CoPilotErrorValues.InsertJobsNotStarted:
                    return Localizer.GetString("InsertJobs is not running");
                default:
                    return "";
            }
        }
    }
}