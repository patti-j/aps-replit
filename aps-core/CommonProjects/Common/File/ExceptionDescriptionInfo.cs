using PT.Common.Extensions;
using PT.Common.Text;

namespace PT.Common.File;

/// <summary>
/// Summary description for ExceptionDescriptionInfo.
/// </summary>
public class ExceptionDescriptionInfo : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 515;

    public ExceptionDescriptionInfo(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 741)
        {
            a_reader.Read(out m_getTypeName);
            a_reader.Read(out m_message);
            a_reader.Read(out m_source);
            a_reader.Read(out m_stackTrace);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_getTypeName);
        a_writer.Write(m_message);
        a_writer.Write(m_source);
        a_writer.Write(m_stackTrace);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ExceptionDescriptionInfo() { }

    public ExceptionDescriptionInfo(Exception a_e)
    {
        if (a_e != null)
        {
            Type type = a_e.GetType();
            m_getTypeName = type.FullName;
            m_message = a_e.GetExceptionFullMessage();
            Exception baseException = a_e.GetBaseException();
            m_stackTrace = a_e.GetExceptionFullStackTrace();
            m_source = baseException.Source;
        }
    }

    public ExceptionDescriptionInfo(string a_getTypeName, string a_message, string a_stackTrace, string a_source)
    {
        m_getTypeName = a_getTypeName;
        m_message = a_message;
        m_stackTrace = a_stackTrace;
        m_source = a_source;
    }

    private string m_getTypeName;

    public string GetTypeName
    {
        get => TextUtil.NonNullString(m_getTypeName);

        set => m_getTypeName = value;
    }

    private string m_message;

    public string Message
    {
        get => TextUtil.NonNullString(m_message);

        set => m_message = value;
    }

    private string m_stackTrace;

    public string StackTrace
    {
        get => TextUtil.NonNullString(m_stackTrace);

        set => m_stackTrace = value;
    }

    private string m_source;

    public string Source
    {
        get => TextUtil.NonNullString(m_source);

        set => m_source = value;
    }
}