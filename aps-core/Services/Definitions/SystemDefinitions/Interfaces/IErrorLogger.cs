using PT.Common.File;
using PT.Transmissions;

namespace PT.SystemDefinitions.Interfaces;

public interface IErrorLogger
{
    /// <summary>
    /// Returns the Log Title or purpose of this log. For example: Exceptions, Warnings, etc
    /// </summary>
    /// <returns></returns>
    string GetLogTitle();

    /// <summary>
    /// Clears the log contents for this instances errors
    /// </summary>
    void Clear();

    /// <summary>
    /// Get the complete error log of all logged exceptions for this instance.
    /// </summary>
    /// <param name="a_filtered">Whether to omit the stack trace and source details of the errors</param>
    /// <returns></returns>
    string GetLogContents(bool a_filtered, bool a_startInUserText = false);

    void LogException(Exception a_e, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei);
    void LogException(ExceptionDescriptionInfo a_edi, string a_message);
    void LogExceptions(ApplicationExceptionList a_errs, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei);
    void LogMessage(string a_message, string a_eventType, string a_reason, string a_timeOfEvent, string a_durationSinceStart);

    void SetLogType(string a_logType);

}