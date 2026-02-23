using System.Runtime.Serialization;

using Newtonsoft.Json;

using PT.APIDefinitions.RequestsAndResponses.DataDtos;
using PT.Common.Extensions;

namespace PT.APIDefinitions.RequestsAndResponses;

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

[Obsolete("Update to use better APIs")]
public class LoggedInUsersData : ApsWebServiceResponseBase
{
    public Dictionary<string, long> Users;

    public LoggedInUsersData(EApsWebServicesResponseCodes a_code, Dictionary<string, long> a_users)
    {
        Exception = false;
        FullExceptionText = "";
        Users = a_users;
        ResponseCode = a_code;
    }
}

public class ConnectedUserData
{
    public long Id { get; set; }
    public string ReadableName { get; set; }
    public int ActiveConnections { get; set; }
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

public class PaginatedDataResponse : ApsWebServiceResponseBase
{
    public PaginatedDataResponse(){}

    public PaginatedDataResponse(int a_totalCount, string a_previousPageUrl, string a_nextPageUrl, EApsWebServicesResponseCodes a_responseCode, Exception a_exception) 
        : base(a_responseCode, a_exception)
    {
        TotalCount = a_totalCount;
        PreviousPageUrl = a_previousPageUrl;
        NextPageUrl = a_nextPageUrl;
    }


    /// <summary>
    /// The total number of records that exist serverside for this entity, regardless of pagination.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The route to the next page of query data, based on the 
    /// </summary>
    public string PreviousPageUrl { get; set; }

    /// <summary>
    /// The route to the next page of query data, based on the 
    /// </summary>
    public string NextPageUrl { get; set; }
}

public class InventoryResponse : PaginatedDataResponse
{
    public InventoryResponse(){}

    public InventoryResponse(int a_totalCount, string a_previousPageUrl, string a_nextPageUrl, EApsWebServicesResponseCodes a_responseCode, Exception a_exception, List<Inventory> a_inventories) 
        : base(a_totalCount, a_previousPageUrl, a_nextPageUrl, a_responseCode, a_exception)
    {
        Inventories = a_inventories;
    }

    public List<Inventory> Inventories { get; set; }
}

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