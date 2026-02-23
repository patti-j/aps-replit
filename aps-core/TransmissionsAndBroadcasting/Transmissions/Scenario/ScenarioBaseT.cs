using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Scenario related transmissions.
/// </summary>
public abstract class ScenarioBaseT : PTTransmission
{
    //public new const int UNIQUE_ID = 119;

    #region IPTSerializable Members
    public ScenarioBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 687)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_checkpointData.Add(new BaseId(reader));
            }
        }
        else if (reader.VersionNumber >= 678)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                m_checkpointData.Add(new BaseId(reader));
            }

            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                //m_checksumData.Add(new BaseId(reader), new ChecksumValues(reader));
                new BaseId(reader);
                new ChecksumValues(reader);
            }
        }
        else if (reader.VersionNumber >= 462)
        {
            m_bools = new BoolVector32(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_bools.Serialize(writer);
        writer.Write(m_checkpointData);
    }
    #endregion

    // A parameter-less constructor is needed for the transmissions unit test to make sure UniqueIds are actually unique.
    // These constructors should only be called using reflection and will show no references
    protected ScenarioBaseT() { }

    public ScenarioBaseT(ScenarioBaseT a_t)
    {
        m_bools = new BoolVector32(a_t.m_bools);
    }

    private BoolVector32 m_bools;

    private const short c_clientWillWaitForResultIdx = 0;

    /// <summary>
    /// For the server, this specifies that a client is waiting for a result from this transmission.
    /// For the Clients, this specifies whether to process this transmission or wait for the result.
    /// </summary>
    public bool ClientWillWaitForScenarioResult
    {
        get => m_bools[c_clientWillWaitForResultIdx];
        set => m_bools[c_clientWillWaitForResultIdx] = value;
    }

    private const short c_replayingForUndoRedoIdx = 1;

    /// <summary>
    /// Specifies that this recording is being played as part of an undo or redo.
    /// </summary>
    public bool ReplayForUndoRedo
    {
        get => m_bools[c_replayingForUndoRedoIdx];
        set => m_bools[c_replayingForUndoRedoIdx] = value;
    }

    private readonly List<BaseId> m_checkpointData = new ();

    public void AddUndoCheckpointData(BaseId a_scenarioId)
    {
        m_checkpointData.Add(a_scenarioId);
    }

    /// <summary>
    /// Wheter an undo checkpoint should be created before processing this transmission
    /// </summary>
    public bool PerformUndoCheckpoint(BaseId a_baseId)
    {
        return m_checkpointData.Contains(a_baseId);
    }

    public override void SetRecording(string a_recordingFilePath, ulong a_originalTransmissionNbr)
    {
        base.SetRecording(a_recordingFilePath, a_originalTransmissionNbr);
    }

    [Obsolete("Remove reference to this. Once remaining references are removed from ScenarioDetail.cs, this declaration can also be deleted. Currently always is false")]
    public bool SuppressEvents => false;
}