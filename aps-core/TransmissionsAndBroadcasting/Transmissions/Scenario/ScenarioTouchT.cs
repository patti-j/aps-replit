using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Cause each scenario to lay down all the blocks back in their same position. At least in the usual case the
/// blocks should end up where they were before. Example of cases that may cause some slight shift in the schedule
/// include making modifications to resources without some form of simulation updating the schedule. At the time
/// this transmission was created this is the only thing I know of that could cause the schedule to be changed by
/// a touch.
/// The purpose of this transmission is to cause all the scenarios to perform a very minimalistic simulation, this
/// will cause any simulation data that isn't serialized to be generated. For instance, the inventory
/// adjustements aren't serialized because they potentially consume too much space and will slow down transfer of
/// the scenario over the network. So to see the inventory plots (which are made from the inventory adjustments array),
/// you might need to call this send this transmission after the client has started.
/// It might not be necessary to send this class though if the scenario of the client automatically touched itself
/// after being loaded up, and the touch didn't result in a change to the schedule. At the time of the creation of this
/// class the schedule attempts to optimize performance of the system by touching itself automatically and only sending
/// out this transmission if it detects that the touch results in a schedule change. Search for the use of this class
/// to see how it is used on start up.
/// </summary>
public class ScenarioTouchT : ScenarioBaseT
{
    public static readonly int UNIQUE_ID = 596;

    #region IPTSerializable Members
    public ScenarioTouchT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 502)
        {
            a_reader.Read(out m_sdNbrOfSimulations);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            SdNbrOfSimulations = 0;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_sdNbrOfSimulations);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioTouchT()
    {
        SdNbrOfSimulations = 0;
    }

    private long m_sdNbrOfSimulations;

    /// <summary>
    /// The number of simulations performed by the system when this transmission was sent.
    /// Should be set if needed. 0 is the default for not set/used.
    /// </summary>
    public long SdNbrOfSimulations
    {
        get => m_sdNbrOfSimulations;
        set => m_sdNbrOfSimulations = value;
    }

    public override string Description => "Scenario Adjusted".Localize();
}