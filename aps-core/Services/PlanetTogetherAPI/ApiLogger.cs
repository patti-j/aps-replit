using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.Extensions;
using PT.ImportDefintions;
using PT.Scheduler;
using PT.SystemDefinitions.Interfaces;

namespace PT.PlanetTogetherAPI;

public class ApiLogger
{
    private static Random s_random;

    public readonly int m_uid = int.MinValue;

    private readonly string m_apiName;
    private readonly bool m_apiDiagnosticsOn;
    private readonly TimeSpan m_timeout = TimeSpan.MinValue;
    private readonly ISystemLogger m_errorReporter;

    static ApiLogger()
    {
        s_random = new Random();
    }

    public ApiLogger(string a_apiName)
    {
        m_apiName = a_apiName;
        m_apiDiagnosticsOn = ControllerProperties.ApiDiagnosticsOn;
        m_errorReporter = SystemController.Sys.SystemLoggerInstance;
    }

    public ApiLogger(string a_apiName, bool a_apiDiagnosticsOn, TimeSpan a_timeout)
        : this(a_apiName)
    {
        m_apiDiagnosticsOn = a_apiDiagnosticsOn;
        m_timeout = a_timeout;
    }

    public ApiLogger(int a_uid, string a_apiName, bool a_apiDiagnosticsOn, TimeSpan a_timeout)
        : this(a_apiName)
    {
        m_uid = a_uid;
        m_apiDiagnosticsOn = a_apiDiagnosticsOn;
        m_timeout = a_timeout;
    }

    internal void LogEnter()
    {
        if (m_apiDiagnosticsOn)
        {
            m_errorReporter.Log(BuildLogMessage("Enter Method"), "", ELogClassification.ApiDiagnostics);
        }
    }

    /// <summary>
    /// Builds a log message in the form of: $"UID: {m_uid} | API: {m_apiName} | Additional Message |  Message: Enter Method | Timeout: {m_timeout} | Exception: {a_exception} | Return Code: {a_returnCode}"
    /// </summary>
    /// <returns></returns>
    private string BuildLogMessage(string a_mainMessage, string a_additionalMessage = null, string a_returnCode = null, string a_exception = null)
    {
        List<string> messageComponents = new ();

        if (m_uid != int.MinValue)
        {
            messageComponents.Add($"UID: {m_uid}");
        }

        messageComponents.Add($"API: {m_apiName}");

        if (a_additionalMessage != null)
        {
            messageComponents.Add(a_additionalMessage);
        }

        messageComponents.Add($"Message: {a_mainMessage}");

        if (m_timeout != TimeSpan.MinValue)
        {
            messageComponents.Add($"Timeout: {m_timeout}");
        }

        if (a_exception != null)
        {
            messageComponents.Add($"Exception: {a_exception}");
        }

        if (a_returnCode != null)
        {
            messageComponents.Add($"Return Code: {a_returnCode}");
        }

        return string.Join(" | ", messageComponents);
    }

    internal void LogDiagnostic(string a_message)
    {
        if (m_apiDiagnosticsOn)
        {
            m_errorReporter.Log(BuildLogMessage(a_message), "", ELogClassification.ApiDiagnostics);
        }
    }

    internal void LogBroadcast(string a_broadcastType, string a_broadcastArgs)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage(a_broadcastArgs, $"TransmissionReceived: {a_broadcastType}))");
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }
    }

    //LogEnter method finish
    internal ApsWebServiceResponseBase LogFinishAndReturn(EApsWebServicesResponseCodes a_code)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage("Method Finish", a_returnCode: a_code.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return new ApsWebServiceResponseBase(a_code);
    }

    internal ApsWebServiceResponseBase LogFinishAndReturn(ApsWebServiceResponseBase a_response)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage($"Method Finish with message base: {a_response}", a_returnCode: a_response.ResponseCode.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return a_response;
    }

    internal CtpResponse LogCtpResponseAndReturn(CtpResponse a_ctpResponse)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage("Ctp Finish", a_returnCode: a_ctpResponse.ReturnCode.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return a_ctpResponse;
    }

    internal CopyScenarioResponse LogCopyScenarioResponseAndReturn(CopyScenarioResponse a_copyScenarioResponseResponse)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage("Copy Scenario Finish", a_returnCode: a_copyScenarioResponseResponse.ResponseCode.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return a_copyScenarioResponseResponse;
    }

    internal GetScenariosResponse LogGetScenarioResponseAndReturn(GetScenariosResponse a_getScenarioResponseResponse)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage("Get Scenario Finish", a_returnCode: a_getScenarioResponseResponse.ResponseCode.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return a_getScenarioResponseResponse;
    }
    internal GetScenarioLastActionInfoResponse LogGetScenarioInfoResponseAndReturn(GetScenarioLastActionInfoResponse a_getScenarioInformationResponse)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage("Get Scenario Information", a_returnCode: a_getScenarioInformationResponse.ResponseCode.ToString());
            m_errorReporter.Log(message, message, ELogClassification.ApiDiagnostics);
        }

        return a_getScenarioInformationResponse;
    }

    internal DeleteScenarioResponse LogDeleteScenarioResponseAndReturn(DeleteScenarioResponse a_deleteScenarioResponseResponse)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage("Delete Scenario Finish", a_returnCode: a_deleteScenarioResponseResponse.ResponseCode.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return a_deleteScenarioResponseResponse;
    }

    internal ApsAddRemoveUserResponse LogApsAddRemoveUserResponse(ApsAddRemoveUserResponse a_addRemoveUserResponse)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage("AddRemoveUser Finish", a_returnCode: a_addRemoveUserResponse.ResponseCode.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return a_addRemoveUserResponse;
    }

    internal ApsWebServiceResponseBase LogFinishWithMessageAndReturn(EApsWebServicesResponseCodes a_code, string a_message)
    {
        if (m_apiDiagnosticsOn)
        {
            string message = BuildLogMessage($"Method Finish with message {a_message}", a_returnCode: a_code.ToString());
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return new ApsWebServiceResponseBase(a_code, a_message);
    }

    //LogEnter WebException
    internal ApsWebServiceResponseBase LogWebExceptionAndReturn(string a_message, WebServicesErrorException a_e)
    {
        string message = BuildLogMessage(a_message, a_exception: a_e.ToString(), a_returnCode: a_e.Code.ToString());
        m_errorReporter.LogException(new ImportApiException(message), null);
        if (m_apiDiagnosticsOn)
        {
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return new ApsWebServiceResponseBase(a_e.Code);
    }

    //LogEnter other Exception types
    internal ApsWebServiceResponseBase LogExceptionAndReturn(string a_message, Exception a_e, EApsWebServicesResponseCodes a_code)
    {
        string message = BuildLogMessage(a_message, a_exception: a_e.ToString(), a_returnCode: a_code.ToString());
        m_errorReporter.LogException(new ImportApiException(message), null);

        if (m_apiDiagnosticsOn)
        {
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }

        return new ApsWebServiceResponseBase(a_code);
    }

    #region Generic Logging
    /// <summary>
    /// General-purpose diagnostic logging for APIs not implementing the WebServices Exceptions/Response Codes.
    /// Note that this method will not capture instigator/ user credentials that may be possible for an endpoint specifically built for ApiLogger structures.
    /// </summary>
    /// <param name="a_e"></param>
    internal void LogGenericDiagnostic(string a_message)
    {
        string message = BuildLogMessage(a_message);
        if (m_apiDiagnosticsOn)
        {
            m_errorReporter.Log(message, "", ELogClassification.ApiDiagnostics);
        }
    }

    /// <summary>
    /// General-purpose error logging for APIs not implementing the WebServices Exceptions/Response Codes.
    /// </summary>
    /// <param name="a_e"></param>
    // TODO: Ideally we should adopt all endpoints to use the same structures as above, but this will serve as a catch-all without needing to use types specific to this class.
    internal void LogGenericException(Exception a_e)
    {
        string message = BuildLogMessage("a_e.Message.") + Environment.NewLine + Environment.NewLine + a_e.GetExceptionFull();
        m_errorReporter.LogException(new ImportApiException(message), null);
    }
    #endregion
}