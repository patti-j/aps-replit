using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Summary description for PTTransmission.
/// </summary>
public abstract class PTTransmission : PTTransmissionBase
{
    //public new const int UNIQUE_ID = 96;

    protected PTTransmission()
    {
        m_bools = new BoolVector32();
    }

    protected PTTransmission(PTTransmission a_t)
    {
        m_bools = new BoolVector32(a_t.m_bools);
        SetRecording(a_t.RecordingFilePath, a_t.TransmissionNbr);
    }

    protected PTTransmission(BaseId a_instigator)
    {
        Instigator = a_instigator;
    }

    #region IPTSerializable Members
    protected PTTransmission(IReader a_reader)
        : base(a_reader)
    {
        #region 12026
        if (a_reader.VersionNumber >= 12026)
        {
            m_bools = new BoolVector32(a_reader);
            m_instigator = new BaseId(a_reader);
            a_reader.Read(out m_recordingFilePath);
            a_reader.Read(out m_originalTransmissionNbr);
            a_reader.Read(out m_instigatingEntityName);
        }
        #endregion
        #region 727
        else if (a_reader.VersionNumber >= 727)
        {
            m_bools = new BoolVector32(a_reader);
            m_instigator = new BaseId(a_reader);
            a_reader.Read(out m_recordingFilePath);
            a_reader.Read(out m_originalTransmissionNbr);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_bools.Serialize(writer);
        m_instigator.Serialize(writer);
        writer.Write(m_recordingFilePath);
        writer.Write(m_originalTransmissionNbr);
        writer.Write(m_instigatingEntityName);
    }
    #endregion

    private BoolVector32 m_bools;
    private const int sequencedIdx = 0; //Obsolete
    private const int recordingIdx = 1;

    /// <summary>
    /// Whether the transmission is a replay transmission.
    /// </summary>
    public bool Recording => m_bools[recordingIdx];

    /// <summary>
    /// The file path of the transmission file.
    /// Passing null or empty string will set Recording to false
    /// </summary>
    /// <param name="a_recordingFilePath"></param>
    /// <param name="a_originalTransmissionNbr"></param>
    public virtual void SetRecording(string a_recordingFilePath, ulong a_originalTransmissionNbr)
    {
        m_recordingFilePath = a_recordingFilePath;
        m_originalTransmissionNbr = a_originalTransmissionNbr;
        m_bools[recordingIdx] = !string.IsNullOrEmpty(m_recordingFilePath);
    }

    private BaseId m_instigator = BaseId.NULL_ID; // Id of the user who sent the transmission

    public BaseId Instigator
    {
        get => m_instigator;
        set => m_instigator = value;
    }

    private string m_instigatingEntityName; // Name id of the user or control or entity that triggered this transmission

    public string InstigatingEntityName
    {
        get => m_instigatingEntityName;
        set => m_instigatingEntityName = value;
    }

    private string m_recordingFilePath;

    /// <summary>
    /// The file path of the transmission file
    /// </summary>
    public string RecordingFilePath => m_recordingFilePath;

    private ulong m_originalTransmissionNbr;

    public ulong OriginalTransmissionNbr => m_originalTransmissionNbr;

    /// <summary>
    /// Whether the Instigator was ERP or Automatic Actions.
    /// </summary>
    public bool FromErp => Instigator == BaseId.ERP_ID;
}