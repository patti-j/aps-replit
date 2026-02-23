using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.SystemDefinitions;

/// <summary>
/// A message from the system to one or more users.
/// </summary>
public class SystemMessage : IPTSerializable
{
    public const int UNIQUE_ID = 148;

    #region IPTSerializable Members
    public SystemMessage(IReader reader)
    {
        if (reader.VersionNumber >= 398)
        {
            reader.Read(out messageText);
            int val;
            reader.Read(out val);
            severity = (MessageSeverity)val;
            reader.Read(out val);

            reader.Read(out timestamp);

            id = new BaseId(reader);
            instigatorId = new BaseId(reader);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out messageText);
            int val;
            reader.Read(out val);
            severity = (MessageSeverity)val;
            reader.Read(out val);

            reader.Read(out timestamp);

            id = new BaseId(reader);
            instigatorId = new BaseId(reader);
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(messageText);
        writer.Write((int)severity);

        writer.Write(timestamp);

        id.Serialize(writer);
        instigatorId.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    //Property names
    public const string ID = "Id"; //This must match BaseObject ID.
    public const string INSTIGATOR_ID = "InstigatorId";
    public const string SEVERITY = "Severity";
    public const string ERROR_TYPE = "ErrorType";
    public const string MESSAGE_TEXT = "MessageText";
    public const string TIME_STAMP = "Timestamp"; //Must match property name below.  Used in sorting.

    //public enum MessageSeverity
    //{
    //    Information,
    //    Warning,
    //    Critical
    //};
    public enum ErrorTypes
    {
        FailedTransmission,
        Unspecified,
        JobScheduleFailure,
        MOScheduleFailure,
        CapabilityProblem
    }
    #endregion

    #region Construction
    public SystemMessage(BaseId id, BaseId instigator, string messageText, MessageSeverity severity)
    {
        this.id = id;
        instigatorId = instigator;
        this.messageText = messageText;
        this.severity = severity;
        timestamp = PTDateTime.UtcNow.RemoveSeconds().ToDateTime();
    }

    public SystemMessage(BaseId instigator, string messageText)
    {
        instigatorId = instigator;
        this.messageText = messageText;
        severity = MessageSeverity.Warning;

        timestamp = PTDateTime.UtcNow.RemoveSeconds().ToDateTime();
    }
    #endregion

    #region Public Properties
    private BaseId id;

    [System.ComponentModel.Browsable(false)] //This isn't always unique so let's hide it.  Should remove eventually.
    public BaseId Id
    {
        get => id;
        set => id = value;
    }

    private BaseId instigatorId;

    public BaseId InstigatorId
    {
        get => instigatorId;
        set => instigatorId = value;
    }

    private MessageSeverity severity;

    /// <summary>
    /// Indicates the severity of the action that triggered the message.
    /// </summary>
    public MessageSeverity Severity
    {
        get => severity;
        set => severity = value;
    }

    private string messageText;

    /// <summary>
    /// Explanation of the event that triggered the message and any suggested followup actions recommended to the user.
    /// </summary>
    public string MessageText
    {
        get => messageText;
        set => messageText = value;
    }

    private readonly DateTime timestamp;

    public DateTime Timestamp => timestamp;
    #endregion
}