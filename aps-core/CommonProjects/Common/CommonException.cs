namespace PT.Common;

/// <summary>
/// The base of all PT exceptions.
/// </summary>
public class CommonException : ApplicationException
{
    public bool LogToSentry
    {
        get => m_logToSentry;
        
        //If you are trying to set this outside of the exception class, check if the exception class should be changed.
        protected set
        {
            m_logToSentry = value;
        }
    }

    private bool m_logToSentry;
    
    public CommonException() { }

    public CommonException(bool a_logToSentry)
    {
        m_logToSentry = a_logToSentry;
    }

    public CommonException(string a_message, bool a_logToSentry = false)
        : base(a_message)
    {
        m_logToSentry = a_logToSentry;
    }

    public CommonException(string a_message, Exception a_innerException, bool a_logToSentry = false)
        : base(a_message, a_innerException)
    {
        m_logToSentry = a_logToSentry;
    }
}