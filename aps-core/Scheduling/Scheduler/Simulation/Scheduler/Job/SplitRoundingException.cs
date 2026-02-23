using PT.Common.Exceptions;

namespace PT.Scheduler;

//Public because this could be handled in the UI
public class SplitRoundingException : PTException
{
    public SplitRoundingException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true) : base(a_message, a_stringParameters, a_appendHelpUrl) { }
}