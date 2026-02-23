using System.Text.Json.Serialization;

using PT.Common.Extensions;

namespace WebAPI.Models.V2;

public enum EApsWebServicesResponseCodes
{
    Success = 0,
    SuccessWithoutValidation = 1,
    InvalidDateFormat = 2,
    InvalidDatePast = 3,
    InvalidDateFuture = 4,
    InvalidUserCredentials = 5,
    InvalidUserPermissions = 6,
    NoServiceListening = 7,
    NoInstanceWithThatName = 8,
    ValidationTimeout = 9,
    NoServerManager = 10,
    InvalidScenarioName = 11,
    FailedToBroadcast = 12,
    ProcessingTimeout = 13,
    Failure = 14,
    InvalidScenarioId = 15,
    InvalidScenarioIdAndName = 16,
    InvalidLiveScenario = 17,
    NoIdObjects = 18,
    FailedInvalidResource = 19,
    FailedInvalidJob = 20,
    FailedInvalidMO = 21,
    FailedInvalidOp = 22,
    FailedInvalidActivity = 23,
    FailedUndoNoUserFound = 24,
    FailedUndoInvalidPermissions = 25,
    FailedUndoNoActions = 26,
    FailedUndoInvalidTransmissionNbr = 27,
    InvalidLicense = 28,
    FailedRequiredAdminAccess = 29,
    CreatedScenario = 30,
    AppUserExists = 31
}

public class ApsWebServiceResponseBase
{
    public ApsWebServiceResponseBase()
    {
        //Default constructor for serialization
        Exception = false;
        FullExceptionText = "";
    }

    public ApsWebServiceResponseBase(EApsWebServicesResponseCodes a_responseCode, Exception a_exception)
    {
        ResponseCode = a_responseCode;
        Exception = true;
        FullExceptionText = a_exception.GetExceptionFullMessage();
    }

    public ApsWebServiceResponseBase(EApsWebServicesResponseCodes a_responseCode, string a_exceptionText)
    {
        ResponseCode = a_responseCode;
        if (!string.IsNullOrEmpty(a_exceptionText))
        {
            Exception = true;
            FullExceptionText = a_exceptionText;
        }
    }

    public ApsWebServiceResponseBase(EApsWebServicesResponseCodes a_responseCode)
    {
        ResponseCode = a_responseCode;
        Exception = false;
        FullExceptionText = "";
    }

    [JsonRequired]
    public EApsWebServicesResponseCodes ResponseCode { get; set; }

    [JsonRequired]
    public bool Exception { get; set; }

    public string FullExceptionText { get; set; }
}