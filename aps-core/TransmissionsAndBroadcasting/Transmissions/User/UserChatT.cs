using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Sends a chat message to the recipient User.
/// </summary>
public class UserChatT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 153;

    #region IPTSerializable Members
    public UserChatT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out chat);

            senderId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(chat);

        senderId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public string chat;
    public BaseId senderId;

    public UserChatT() { }

    public UserChatT(BaseId recipientId, BaseId senderId, string message)
        : base(recipientId)
    {
        this.senderId = senderId;
        chat = message;
    }
}