using PT.APSCommon;

namespace PT.SchedulerDefinitions;

public class ChecksumValues : IPTSerializable, IEquatable<ChecksumValues>
{
    #region IPTSerializable Members
    public ChecksumValues(IReader a_reader)
    {
        // Pretty sure we don't need to maintain backwards compatibility for this class
        // since we don't serialize it in any sort of persistent format. 
        if (a_reader.VersionNumber >= 12520)
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out m_transmissionId);
            m_scenarioId = new BaseId(a_reader);

            a_reader.Read(out bool hasDiagnostics);
            if (hasDiagnostics)
            {
                m_checksumDiagnostics = new ChecksumDiagnosticsValues(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 12430)
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out m_transmissionId);
            m_scenarioId = new BaseId(a_reader);
            a_reader.Read(out int discardtransmissionInUndoSetsCount);
            a_reader.Read(out int discardguidHash);

            a_reader.Read(out bool hasDiagnostics);
            if (hasDiagnostics)
            {
                m_checksumDiagnostics = new ChecksumDiagnosticsValues(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 12412)
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out m_transmissionId);
            m_scenarioId = new BaseId(a_reader);
            a_reader.Read(out bool hasDiagnostics);
            if (hasDiagnostics)
            {
                m_checksumDiagnostics = new ChecksumDiagnosticsValues(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 12400)
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out m_transmissionId);
            m_scenarioId = new BaseId(a_reader);
        }
        else if (a_reader.VersionNumber >= 12329) // For Saturn/12.2.0 backwards compatibility
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out m_transmissionId);
            m_scenarioId = new BaseId(a_reader);
            a_reader.Read(out bool hasDiagnostics);
            if (hasDiagnostics)
            {
                m_checksumDiagnostics = new ChecksumDiagnosticsValues(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 12212)
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out m_transmissionId);
            m_scenarioId = new BaseId(a_reader);
        }
        else if (a_reader.VersionNumber >= 12020)
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out m_transmissionId);
        }
        else if (a_reader.VersionNumber >= 684)
        {
            a_reader.Read(out m_startAndEndSums);
            a_reader.Read(out m_resourceJobOperationCombos);
            a_reader.Read(out m_blockCount);
            a_reader.Read(out m_scheduleDescription);
            a_reader.Read(out ulong m_lastProcessedTransmissionNbr);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_startAndEndSums);
        a_writer.Write(m_resourceJobOperationCombos);
        a_writer.Write(m_blockCount);
        a_writer.Write(m_scheduleDescription);
        a_writer.Write(m_transmissionId);
        m_scenarioId.Serialize(a_writer);

        // Write whether or not we should read the checksumDiagnostics
        if (m_checksumDiagnostics != null)
        {
            a_writer.Write(true);
            m_checksumDiagnostics.Serialize(a_writer);
        }
        else
        {
            a_writer.Write(false);
        }
    }

    public const int UNIQUE_ID = 736;

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ChecksumValues(BaseId a_scenarioId, decimal a_startAndEndSums, decimal a_resourceJobOperationCombos, int a_blockCount, string a_scheduleDescription, Guid a_lastTransmissionId)
    {
        m_startAndEndSums = a_startAndEndSums;
        m_resourceJobOperationCombos = a_resourceJobOperationCombos;
        m_blockCount = a_blockCount;
        m_scheduleDescription = a_scheduleDescription;
        m_transmissionId = a_lastTransmissionId;
        m_scenarioId = a_scenarioId;
    }

    public ChecksumValues() { }

    private readonly decimal m_startAndEndSums;

    public decimal StartAndEndSums => m_startAndEndSums;

    private readonly decimal m_resourceJobOperationCombos;

    public decimal ResourceJobOperationCombos => m_resourceJobOperationCombos;

    private readonly int m_blockCount;

    public int BlockCount => m_blockCount;

    private readonly string m_scheduleDescription = "";

    public string ScheduleDescription => m_scheduleDescription;

    private readonly Guid m_transmissionId;
    public Guid TransmissionId => m_transmissionId;

    private readonly BaseId m_scenarioId;

    public BaseId ScenarioId => m_scenarioId;

    private ChecksumDiagnosticsValues m_checksumDiagnostics = null;

    public ChecksumDiagnosticsValues ChecksumDiagnostics => m_checksumDiagnostics;
    public void SetDiagnostics(ChecksumDiagnosticsValues a_diagnostics) => m_checksumDiagnostics = a_diagnostics;

    public string GetComparisonDescription(ChecksumValues a_compareValues, long a_scenarioId)
    {
        string description = string.Format(@"The checksums from the server don't match what's on this client.
The checksums are listed below like this [ ServerValue | ClientValue].
StartAndEndTimes [{0} | {1}],  match={2}
ResourceJobOperationCombinations [{3} | {4}], match={5}
BlockCount [{6} | {7}], match={8}
ScenarioId = {9}
TransmissionId = {10} | {11}",
            StartAndEndSums,
            a_compareValues.StartAndEndSums,
            StartAndEndSums == a_compareValues.StartAndEndSums,
            ResourceJobOperationCombos,
            a_compareValues.ResourceJobOperationCombos,
            ResourceJobOperationCombos == a_compareValues.ResourceJobOperationCombos,
            BlockCount,
            a_compareValues.BlockCount,
            BlockCount == a_compareValues.BlockCount,
            a_scenarioId,
            m_transmissionId,
            a_compareValues.m_transmissionId);

        return description;
    }

    public override bool Equals(object obj)
    {
        if (obj is ChecksumValues c)
        {
            return Equals(c);
        }

        return false;
    }

    public bool Equals(ChecksumValues a_compareValues)
    {
        return a_compareValues != null && 
               a_compareValues.m_transmissionId == m_transmissionId && 
               a_compareValues.StartAndEndSums == StartAndEndSums && 
               a_compareValues.ResourceJobOperationCombos == ResourceJobOperationCombos && 
               a_compareValues.BlockCount == BlockCount;
    }

    public override int GetHashCode()
    {
        return m_transmissionId.GetHashCode();
    }

    /// <summary>
    /// For user client-side to track the number of failed attempts to get the corresponding checksum from the server. Not serialized.
    /// </summary>
    public int ValidationCount { get; set; }

    #region ChecksumDiagnosticsValues
    public class ChecksumDiagnosticsValues : IPTSerializable
    {
        #region IPTSerializableMembers 
        public ChecksumDiagnosticsValues(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12428)
            {
                a_reader.Read(out m_userCultureInfo);
                a_reader.Read(out m_recordingIndex);
                a_reader.Read(out m_localMachineTimeZoneId);
                a_reader.Read(out m_userPreferenceTimeZoneId);
                m_recentTransmissionsProcessed = new();
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    a_reader.Read(out ulong transmissionNbr);
                    a_reader.Read(out int transmissionUniqueId);
                    BaseId instigatorId = new BaseId(a_reader);
                    a_reader.Read(out DateTimeOffset timeStamp);
                    m_recentTransmissionsProcessed.Add(new(transmissionNbr, transmissionUniqueId, instigatorId, timeStamp));
                }

                m_recentTransmissionsReceived = new();
                a_reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    a_reader.Read(out ulong transmissionNbr);
                    a_reader.Read(out int transmissionUniqueId);
                    BaseId instigatorId = new BaseId(a_reader);
                    a_reader.Read(out DateTimeOffset timeStamp);
                    m_recentTransmissionsReceived.Add(new(transmissionNbr, transmissionUniqueId, instigatorId, timeStamp));
                }
            }
            else if (a_reader.VersionNumber >= 12426)
            {
                a_reader.Read(out m_userCultureInfo);
                a_reader.Read(out m_recordingIndex);
                a_reader.Read(out m_localMachineTimeZoneId);
                a_reader.Read(out m_userPreferenceTimeZoneId);
                m_recentTransmissionsProcessed = new();
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    a_reader.Read(out ulong transmissionNbr);
                    a_reader.Read(out int transmissionUniqueId);
                    BaseId instigatorId = new BaseId(a_reader);
                    a_reader.Read(out DateTimeOffset timeStamp);
                    m_recentTransmissionsProcessed.Add(new(transmissionNbr, transmissionUniqueId, instigatorId, timeStamp));
                }
            }
            else if (a_reader.VersionNumber >= 12412)
            {
                a_reader.Read(out m_userCultureInfo);
                a_reader.Read(out m_recordingIndex);
                a_reader.Read(out m_localMachineTimeZoneId);
                a_reader.Read(out m_userPreferenceTimeZoneId);
                m_recentTransmissionsProcessed = new();
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    a_reader.Read(out ulong transmissionNbr);
                    a_reader.Read(out int transmissionUniqueId);
                    BaseId instigatorId = new BaseId(a_reader);
                    a_reader.Read(out DateTimeOffset timeStamp);
                    m_recentTransmissionsProcessed.Add(new(transmissionNbr, transmissionUniqueId, instigatorId, timeStamp));
                }
            }
            else if (a_reader.VersionNumber >= 12400)
            {
                // Intentionally do nothing here because this class did not exist
                // when VersionNumber is between 12400 and 12412
            }
            else if (a_reader.VersionNumber >= 12329)
            {
                //Backwards compatibility for Saturn
                a_reader.Read(out m_userCultureInfo);
                a_reader.Read(out m_recordingIndex);
                a_reader.Read(out m_localMachineTimeZoneId);
                a_reader.Read(out m_userPreferenceTimeZoneId);
                m_recentTransmissionsProcessed = new();
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    a_reader.Read(out ulong transmissionNbr);
                    a_reader.Read(out int transmissionUniqueId);
                    BaseId instigatorId = new BaseId(a_reader);
                    a_reader.Read(out DateTimeOffset timeStamp);
                    m_recentTransmissionsProcessed.Add(new(transmissionNbr, transmissionUniqueId, instigatorId, timeStamp));
                }
            }
        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_userCultureInfo);
            a_writer.Write(m_recordingIndex);
            a_writer.Write(m_localMachineTimeZoneId);
            a_writer.Write(m_userPreferenceTimeZoneId);
            a_writer.Write(m_recentTransmissionsProcessed.Count);
            foreach ((ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp) transmissionInfo in m_recentTransmissionsProcessed)
            {
                a_writer.Write(transmissionInfo.TransmissionNbr);
                a_writer.Write(transmissionInfo.TransmissionUniqueId);
                transmissionInfo.InstigatorId.Serialize(a_writer);
                a_writer.Write(transmissionInfo.TimeStamp);
            }

            a_writer.Write(m_recentTransmissionsReceived.Count);
            foreach ((ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp) transmissionInfo in m_recentTransmissionsReceived)
            {
                a_writer.Write(transmissionInfo.TransmissionNbr);
                a_writer.Write(transmissionInfo.TransmissionUniqueId);
                transmissionInfo.InstigatorId.Serialize(a_writer);
                a_writer.Write(transmissionInfo.TimeStamp);
            }
        }
        #endregion

        public ChecksumDiagnosticsValues(long a_recordingsIndex, string a_userPreferenceTimeZoneId, IEnumerable<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> a_transmissionsFromUndoSets)
        {
            m_userCultureInfo = Thread.CurrentThread.CurrentCulture.Name;
            m_recordingIndex = a_recordingsIndex;
            m_localMachineTimeZoneId = TimeZoneInfo.Local.Id;
            m_userPreferenceTimeZoneId = a_userPreferenceTimeZoneId;
            m_recentTransmissionsProcessed = a_transmissionsFromUndoSets.ToList();
            m_recentTransmissionsReceived = new ();
        }

        public string LocalMachineTimeZoneId => m_localMachineTimeZoneId;

        private readonly string m_localMachineTimeZoneId;

        public string UserPreferenceTimeZoneId => m_userPreferenceTimeZoneId;

        private readonly string m_userPreferenceTimeZoneId;

        public string UserCultureInfoName => m_userCultureInfo;

        private readonly string m_userCultureInfo;

        public long RecordingIndex => m_recordingIndex;

        private readonly long m_recordingIndex;

        //TODO: Create a class to replace these Tuples. 
        public List<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> RecentTransmissionsProcessed => m_recentTransmissionsProcessed;

        private readonly List<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> m_recentTransmissionsProcessed;

        public List<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> RecentTransmissionsReceived => m_recentTransmissionsReceived;

        private readonly List<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> m_recentTransmissionsReceived;

        public const int UNIQUE_ID = 1115;

        public int UniqueId => UNIQUE_ID;
    }
    #endregion
}