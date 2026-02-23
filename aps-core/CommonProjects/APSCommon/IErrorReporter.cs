using PT.Common.File;

namespace PT.APSCommon;

/// <summary>
/// Interface for the system error logger (ErrorReporter)
/// </summary>
public interface IErrorReporter
{
    void LogException<T>(T a_e, ScenarioExceptionInfo a_sei);
}