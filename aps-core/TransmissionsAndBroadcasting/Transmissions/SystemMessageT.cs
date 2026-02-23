using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class SystemMessageT : PTTransmission
{
    public SystemMessageT() { }

    public SystemMessageT(string a_msg, MessageSeverity a_severity)
    {
        m_message = a_msg;
        m_severity = a_severity;
    }

    #region IPTSerializable Members
    public SystemMessageT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 494)
        {
            reader.Read(out m_message);
            int val;
            reader.Read(out val);
            m_severity = (MessageSeverity)val;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_message);
        writer.Write((int)m_severity);
    }

    public const int UNIQUE_ID = 795;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private MessageSeverity m_severity;

    /// <summary>
    /// Indicates the severity of the action that triggered the message.
    /// </summary>
    public MessageSeverity Severity
    {
        get => m_severity;
        set => m_severity = value;
    }

    private string m_message;

    /// <summary>
    /// Explanation of the event that triggered the message and any suggested followup actions recommended to the user.
    /// </summary>
    public string MessageText
    {
        get => m_message;
        set => m_message = value;
    }
}