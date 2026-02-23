using PT.Common.Exceptions;

namespace PT.APSCommon.Exceptions;

/// <summary>
/// 
/// </summary>
public class PTNoScenarioException : PTHandleableException
{
    public PTNoScenarioException(string a_message) : base(a_message) { }

    public PTNoScenarioException(string a_message, Exception a_innerException) : base(a_message, a_innerException) { }
}