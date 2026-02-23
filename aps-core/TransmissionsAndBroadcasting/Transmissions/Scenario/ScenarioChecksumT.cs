using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

[Obsolete("This transmission is no longer used. Clients make web requests to the server for checksums")]
public class ScenarioChecksumT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioChecksumT(IReader reader)
        : base(reader)
    {
        bool tmp;
        if (reader.VersionNumber >= 687)
        {
            ChecksumValues = new ChecksumValues(reader);
        }
        else if (reader.VersionNumber >= 652)
        {
            reader.Read(out tmp);
        }
        else if (reader.VersionNumber >= 405)
        {
            ChecksumValues = new ChecksumValues(reader);
            reader.Read(out tmp);
        }

        #region 400
        else if (reader.VersionNumber >= 400)
        {
            ChecksumValues = new ChecksumValues(reader);
        }
        #endregion

        #region 263
        else if (reader.VersionNumber >= 263)
        {
            decimal outDecimal;
            int outInt;
            string outString;
            long outLong;
            reader.Read(out outDecimal);
            reader.Read(out outDecimal);
            reader.Read(out outInt);
            reader.Read(out outString);
            reader.Read(out outLong);
        }
        #endregion

        #region 126
        else if (reader.VersionNumber >= 126)
        {
            decimal outDecimal;
            int outInt;
            string outString;
            reader.Read(out outDecimal);
            reader.Read(out outDecimal);
            reader.Read(out outInt);
            reader.Read(out outString);
        }
        #endregion

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            decimal outDecimal;
            int outInt;
            reader.Read(out outDecimal);
            reader.Read(out outDecimal);
            reader.Read(out outInt);
        }
        #endregion

        if (ChecksumValues == null) // some older versions don't create object which can cause NullReferenceException when serializing
        {
            ChecksumValues = new ChecksumValues();
        }
    }

    public override void Serialize(IWriter writer)
    {
        throw new NotFiniteNumberException();
    }

    public const int UNIQUE_ID = 541;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private ChecksumValues m_checksumValues;

    public ChecksumValues ChecksumValues
    {
        get => m_checksumValues;
        set => m_checksumValues = value;
    }

    public ScenarioChecksumT() { }

    /// <summary>
    /// Signal that an undo checkpoint needs to be created. And send over the server's schedule checksums.
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="aStartAndEndSums">The sum of every block's start and end time.</param>
    /// <param name="aResourceJobOperationCombos">For every block, the sum of Resource*Job*Operation</param>
    /// <param name="aBlockCount">The number of blocks in the schedule.</param>
    public ScenarioChecksumT(BaseId scenarioId, ChecksumValues a_checksumData)
        : base(scenarioId)
    {
        ChecksumValues = a_checksumData;
    }

    public override string Description => "Checksum verified";
}