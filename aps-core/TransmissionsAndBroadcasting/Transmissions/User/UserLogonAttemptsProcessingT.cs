using PT.APSCommon;

namespace PT.Transmissions;

public class UserLogonAttemptsProcessingT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 842;

    #region PT Serialization
    public UserLogonAttemptsProcessingT(IReader a_reader) : base(a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        a_reader.Read(out m_failedLoginAttempts);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write(m_failedLoginAttempts);
    }

    public override int UniqueId => UNIQUE_ID;

    private BoolVector32 m_bools;

    private const short c_failedLoginAttemptsSetIdx = 0;

    private int m_failedLoginAttempts;

    public int FailedLoginAttempts
    {
        get => m_failedLoginAttempts;
        set
        {
            m_failedLoginAttempts = value;
            m_bools[c_failedLoginAttemptsSetIdx] = true;
        }
    }

    public bool FailedLoginAttemptsSet => m_bools[c_failedLoginAttemptsSetIdx];
    #endregion

    public UserLogonAttemptsProcessingT() { }

    public UserLogonAttemptsProcessingT(BaseId a_userId) : base(a_userId)
    {
    }
}