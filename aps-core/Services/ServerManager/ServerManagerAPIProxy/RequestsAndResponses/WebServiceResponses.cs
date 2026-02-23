using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace PT.ServerManagerAPIProxy.RequestsAndResponses;

public class WebServiceBaseAddresses
{
    public static readonly string ExtraServices = "https://host:port/APSWebService";
    public static readonly string ShopViews = "https://host:port/ShopViews";
}

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
    CreatedScenario = 30
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
        FullExceptionText = a_exception.ToString();
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

public class ApsAddRemoveUserResponse : ApsWebServiceResponseBase
{
    public int ModifiedUsers;

    public ApsAddRemoveUserResponse() { }

    public ApsAddRemoveUserResponse(EApsWebServicesResponseCodes a_code, int a_modifiedUsers)
    {
        Exception = false;
        FullExceptionText = "";
        ModifiedUsers = a_modifiedUsers;
        ResponseCode = a_code;
    }
}

public class LoggedInUsersResponse : ApsWebServiceResponseBase
{
    public Dictionary<string, long> Users;

    public LoggedInUsersResponse(EApsWebServicesResponseCodes a_code, Dictionary<string, long> a_users)
    {
        Exception = false;
        FullExceptionText = "";
        Users = a_users;
        ResponseCode = a_code;
    }
}

public class ValidateCredentialsResponse : ApsWebServiceResponseBase
{
    public long? UserId;

    public ValidateCredentialsResponse(EApsWebServicesResponseCodes a_code, long? a_userId)
    {
        Exception = false;
        FullExceptionText = "";
        UserId = a_userId;
        ResponseCode = a_code;
    }
}

public class ValidatePasswordResponse : ApsWebServiceResponseBase
{
    public bool Valid;

    public ValidatePasswordResponse(EApsWebServicesResponseCodes a_code, bool a_isValid)
    {
        Valid = a_isValid;
        Exception = false;
        ResponseCode = a_code;
    }
}

#region ShopViews
[DataContract]
public class LoginResultResponse : ApsWebServiceResponseBase
{
    [DataMember(IsRequired = false)] public ELoginResult Result;
    public LoginResultResponse() { }

    public LoginResultResponse(ELoginResult a_result)
    {
        Result = a_result;
    }
}
#endregion

public enum ELoginResult { LoggedIn, InvalidUserOrPassword, NoRights, NoMoreLicenses }

/// <summary>
/// Summary description for UserPreferences.
/// </summary>
[DataContract(Name = "UserPreferences", Namespace = "http://www.planettogether.com")]
public class UserPreferences
{
    private bool m_jumpToNextOpInShopViews;

    /// <summary>
    /// Whether, in Shop Views,  to have the system automatically show the next scheduled Activity for the User when an Activity is finished.
    /// </summary>
    [DataMember]
    public bool JumpToNextOpInShopViews
    {
        get => m_jumpToNextOpInShopViews;
        set => m_jumpToNextOpInShopViews = value;
    }

    private bool m_canEditShopViewPreferences;

    /// <summary>
    /// Whether the User can edit personal preferences in Shop Views.
    /// </summary>
    [DataMember]
    public bool CanEditShopViewPreferences
    {
        get => m_canEditShopViewPreferences;
        set => m_canEditShopViewPreferences = value;
    }

    private bool m_promptForShopViewsSave;

    /// <summary>
    /// Whether the system asks the User before saving when closing a modified Activity in Shop Views.
    /// </summary>
    [DataMember]
    public bool PromptForShopViewsSave
    {
        get => m_promptForShopViewsSave;
        set => m_promptForShopViewsSave = value;
    }
}