using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Sent to the server by clients when an error occurs.
/// </summary>
public class UserErrorT : UserIdBaseT
{
    public UserErrorT() { }

    public UserErrorT(BaseId senderId, Exception e, string extraText)
        : base(senderId)
    {
        if (e != null)
        {
            Type type = e.GetType();
            getTypeName = type.FullName;
            message = e.Message;
            stackTrace = e.StackTrace;
            source = e.Source;

            if (e.InnerException != null)
            {
                innerExceptionMessage = e.InnerException.Message;
                innerExceptionStackTrace = e.InnerException.StackTrace;
            }
        }

        this.extraText = extraText;
    }

    #region IPTSerializable Members
    public UserErrorT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out getTypeName);
            reader.Read(out message);
            reader.Read(out stackTrace);
            reader.Read(out source);
            reader.Read(out innerExceptionMessage);
            reader.Read(out innerExceptionStackTrace);

            reader.Read(out extraText);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(getTypeName);
        writer.Write(message);
        writer.Write(stackTrace);
        writer.Write(source);
        writer.Write(innerExceptionMessage);
        writer.Write(innerExceptionStackTrace);

        writer.Write(extraText);
    }

    public new const int UNIQUE_ID = 459;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public string getTypeName;
    public string message;
    public string stackTrace;
    public string source;

    public string innerExceptionMessage;
    public string innerExceptionStackTrace;

    public string extraText;
}