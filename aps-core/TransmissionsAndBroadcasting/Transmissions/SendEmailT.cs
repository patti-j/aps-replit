namespace PT.Transmissions;

public abstract class SendEmailBaseT : PTTransmission, IPTSerializable
{
    #region IPTSerializable
    public const int UNIQUE_ID = 821;

    public override int UniqueId => UNIQUE_ID;

    public SendEmailBaseT(IReader a_reader) : base(a_reader)
    {
        a_reader.Read(out m_messageSubject);
        a_reader.Read(out m_messageBody);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_messageSubject);
        a_writer.Write(m_messageBody);
    }
    #endregion

    /// <summary>
    /// do not use this directly since it does not specify a to and from email address
    /// </summary>
    /// <param name="a_messageSubject"></param>
    /// <param name="a_messageBody"></param>
    public SendEmailBaseT(string a_messageSubject, string a_messageBody)
    {
        MessageSubject = a_messageSubject;
        MessageBody = a_messageBody;
    }

    protected SendEmailBaseT() { }

    private string m_messageSubject;

    public string MessageSubject
    {
        get => m_messageSubject;
        set => m_messageSubject = value;
    }

    private string m_messageBody;

    public string MessageBody
    {
        get => m_messageBody;
        set => m_messageBody = value;
    }
}

public class SendSupportEmailT : SendEmailBaseT
{
    public new const int UNIQUE_ID = 822;

    public override int UniqueId => UNIQUE_ID;

    public SendSupportEmailT() { }
    public SendSupportEmailT(IReader a_reader) : base(a_reader) { }

    public SendSupportEmailT(string a_messageBody) : base("", a_messageBody) { }
}