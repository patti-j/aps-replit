namespace PT.Transmissions;

/// <summary>
/// When the startup type is set to RecordingClientDelayed the transmissions in the recordings folder aren't played
/// until this transmission has been received. This allows multiple instances of the client to be instantitated and positioned.
/// </summary>
public class TriggerRecordingPlaybackT : DebuggingBaseT, IPTSerializable
{
    public TriggerRecordingPlaybackT() { }

    #region IPTSerializable Members
    public TriggerRecordingPlaybackT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 481)
        {
            reader.Read(out m_playbackMode);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_playbackMode);
    }

    public enum EPlayBackModes
    {
        /// <summary>
        /// Run all recordings sequentially
        /// </summary>
        Full,

        /// <summary>
        /// Run recordings in sequence until a ScenarioBaseT or ERPTranssmision is broadcast.
        /// </summary>
        SingleSimulation,

        /// <summary>
        /// Run recordings in sequence until a UserLoginT is next
        /// This can be used to run recordings until the next login, so that we can manually log in at the same point in recordings.
        /// </summary>
        Login,

        /// <summary>
        /// Skip the next login transmissions. This can be used before Login type to keep the transmission sequence in sync. Required for playing Undo transmissions correctly
        /// </summary>
        SkipLogin
    }

    private short m_playbackMode;

    public EPlayBackModes PlayBackMode
    {
        get => (EPlayBackModes)m_playbackMode;
        set => m_playbackMode = (short)value;
    }

    public const int UNIQUE_ID = 453;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}