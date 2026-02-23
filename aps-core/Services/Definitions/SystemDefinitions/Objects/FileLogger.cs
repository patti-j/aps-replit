using PT.Common.File;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.SystemDefinitions;

/// <summary>
/// Manages the logging of errors on the server.
/// </summary>
public class FileLogger : IErrorLogger
{
    /// <summary>
    /// Where to write exception logs.
    /// </summary>
    private readonly string m_errorFilePath;

    private string m_logType;

    public string GetLogTitle()
    {
        return m_errorFilePath;
    }

    public FileLogger(string a_directory, string a_logName)
    {
        m_errorFilePath = CreateErrorLogPath(a_directory, a_logName);
    }

    public FileLogger(string a_logPath)
    {
        m_errorFilePath = a_logPath;
    }

    #region Exception Logging
    /// <summary>
    /// Log a a_message without a corresponding exception.
    /// </summary>
    /// <param name="msg"></param>
    public void LogMessage(string msg)
    {
        SimpleExceptionLogger.LogMessage(m_errorFilePath, msg);
    }

    /// <summary>
    /// Log an excpetion.
    /// </summary>
    /// <param name="e"></param>
    public void LogException(Exception e)
    {
        LogException(e, null, "", null);
    }

    /// <summary>
    /// Log an error for the overall system.
    /// </summary>
    /// <param name="e"></param>
    public void LogException(Exception e, string suggestion)
    {
        Log(e, null, suggestion);
    }

    /// <summary>
    /// Log an error for the system that was triggered by a transmission.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="t"></param>
    public void LogException(Exception a_e, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
    {
        Log(a_e, a_t, a_suggestion);
    }

    public void LogException(ExceptionDescriptionInfo a_edi, string a_message)
    {
        SimpleExceptionLogger.LogException(m_errorFilePath, a_edi, a_message);
    }

    public void LogExceptions(ApplicationExceptionList a_errs, PTTransmission a_t, string a_suggestion, ScenarioExceptionInfo a_sei)
    {
        string headerText = GetTransmissionExceptionHeader(a_t, a_suggestion);

        // Log the exceptions.
        ApplicationExceptionList.Node node = a_errs.First;
        int count = 0;
        while (node != null)
        {
            if (count == 100)
            {
                ExceptionDescriptionInfo edi = new (new Exception("This log has been truncated to 100 messages of this type."));
                SimpleExceptionLogger.LogException(m_errorFilePath, edi, a_sei, headerText);
                break;
            }

            SimpleExceptionLogger.LogException(m_errorFilePath, node.Data, a_sei, headerText);
            node = node.Next;
            count++;
        }
    }

    /// <summary>
    /// Log an exception. Currently this is done with SimpleExceptionLogger.
    /// </summary>
    private void Log(Exception e, PTTransmission t, string suggestion)
    {
        string errorText = GetTransmissionExceptionHeader(t, suggestion);

        // Log the exception.
        SimpleExceptionLogger.LogException(m_errorFilePath, e, errorText);
    }

    public static string GetTransmissionExceptionHeader(PTTransmission t, string suggestion)
    {
        // Create a description of the problem. 
        const string NONE = "<none>";

        string transmissionType = NONE;
        string transmissinoNbr = NONE;
        string instigator = NONE;
        string recordingPath = NONE;

        if (t != null)
        {
            transmissionType = t.GetType().ToString();
            transmissinoNbr = t.TransmissionNbr.ToString();
            instigator = t.Instigator.ToString();
            if (t.Recording)
            {
                recordingPath = t.RecordingFilePath;
            }
        }

        if (suggestion == null)
        {
            suggestion = "";
        }
        else if (suggestion.Trim() != "") //don't bother with the 'suggestion' text if no suggestion.
        {
            suggestion = "\nSuggestion: " + suggestion;
        }


        string errorText = "";
        if (transmissionType.Length > 0 && transmissionType != NONE)
        {
            errorText += string.Format("\nTransmissionType={0}.", transmissionType);
        }

        if (transmissinoNbr.Length > 0 && transmissinoNbr != NONE)
        {
            errorText += string.Format("\nTransmissionNbr={0}.", transmissinoNbr);
        }

        if (!string.IsNullOrEmpty(recordingPath) && recordingPath != NONE)
        {
            errorText += string.Format("\nRecordingFile={0}.", recordingPath);
        }

        if (instigator.Length > 0 && instigator != NONE)
        {
            errorText += string.Format("\nInstigator={0}.", instigator);
        }

        if (suggestion.Length > 0 && suggestion != NONE)
        {
            errorText += string.Format("\nSuggestion={0}.", suggestion);
        }

        return errorText;
    }
    #endregion Exception Logging

    private string CreateErrorLogPath(string a_directory, string a_logName)
    {
        System.Text.StringBuilder sb = new (a_logName);
        sb.Append(".log");
        string path = Path.Combine(a_directory, sb.ToString());
        return path;
    }

    /// <summary>
    /// Clear the text from the specified file.
    /// </summary>
    public void Clear()
    {
        try
        {
            FileStream s = File.Open(m_errorFilePath, FileMode.Create);
            s.Close();
        }
        catch (Exception)
        {
            // I don't care too much if this fails.
        }
    }

    /// <summary>
    /// Retrieves all the text in the specified file.
    /// </summary>
    /// <param name="a_path">The path to a file.</param>
    /// <param name="a_filtered">If true then only the text for users is shown, filtering stack details, etc.</param>
    public string GetLogContents(bool a_filtered, bool a_startInUserText = false)
    {
        string contents = "";

        if (File.Exists(m_errorFilePath))
        {
            StreamReader sr = File.OpenText(m_errorFilePath);
            try
            {
                contents = sr.ReadToEnd();
            }
            finally
            {
                sr.Close();
            }
        }

        if (a_filtered)
        {
            return SimpleExceptionLogger.FilterLogContents(contents, a_startInUserText);
        }

        return SimpleExceptionLogger.FormatLogContents(contents);
    }

    public void LogMessage(string a_message, string a_eventType, string a_reason, string a_timeOfEvent, string a_durationSinceStart)
    {
        SimpleExceptionLogger.LogMessage(m_errorFilePath, a_message);
    }

    public void SetLogType(string a_logType)
    {
        m_logType = a_logType;
    }
}