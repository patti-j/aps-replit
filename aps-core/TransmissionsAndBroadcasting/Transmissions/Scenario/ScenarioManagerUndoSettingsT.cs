namespace PT.Transmissions;

/// <summary>
/// Summary description for ScenarioManagerUndoSettingsT.
/// </summary>
public class ScenarioManagerUndoSettingsT : ScenarioBaseT, IPTSerializable
{
    public ScenarioManagerUndoSettingsT() { }

    /// <summary>
    /// If the settings you specify are rejected by the server a warning will be written to the system log.
    /// The changes you make with this transmission won't take effect until the next time you restart the PT Service.
    /// </summary>
    /// <param name="a_undoThreshold">
    /// The cumulative amount of processing time of actions that triggers scenario	data to accumulate for the purpose of potential undos.	Smaller values cause more frequent
    /// disk access but result in less time for	an undo action. Larger values require less disk accesses but require	more time for undos. The value is specified in seconds.
    /// </param>
    /// <param name="numberOfUndoSets">
    /// The number of undo sets to store to disk. Example:	If the undoThreshold is set to 10 and numberOfUndoSets is to to 10	then you will be able to undo at least 100 seconds
    /// worth of actions.
    /// </param>
    /// <param name="maxTransmissions">The maximum number of transmissions that an undo set can contain.</param>
    public ScenarioManagerUndoSettingsT(int a_undoThreshold, int a_undoMemoryLimitMB)
    {
        m_undoThreshold = a_undoThreshold;
        m_undoMemoryLimitMB = a_undoMemoryLimitMB;
    }

    #region IPTSerializable Members
    public ScenarioManagerUndoSettingsT(IReader reader) : base(reader)
    {
        if (reader.VersionNumber >= 12310)
        {
            reader.Read(out m_undoThreshold);
            reader.Read(out m_undoMemoryLimitMB);
        }
        else
        {
            reader.Read(out m_undoThreshold);
            reader.Read(out m_undoMemoryLimitMB);
            reader.Read(out int obsoleteChecksumFrequencyType);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_undoThreshold);
        writer.Write(m_undoMemoryLimitMB);
    }

    public const int UNIQUE_ID = 454;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly int m_undoThreshold;

    public int UndoThreshold => m_undoThreshold;

    private readonly decimal m_undoMemoryLimitMB;

    public decimal UndoMemoryLimitMB => m_undoMemoryLimitMB;
}