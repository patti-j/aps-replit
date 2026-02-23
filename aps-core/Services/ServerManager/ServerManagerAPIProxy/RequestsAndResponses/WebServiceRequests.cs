using Newtonsoft.Json;

namespace PT.ServerManagerAPIProxy.RequestsAndResponses;

[JsonObject]
public class ApsWebServiceRequestBase
{
    [JsonRequired]
    public string UserName { get; set; }

    public string Password { get; set; } = string.Empty;

    public TimeSpan TimeoutDuration { get; set; } = TimeSpan.FromSeconds(30);
}

[JsonObject]
public class ApsWebServiceScenarioRequest : ApsWebServiceRequestBase
{
    public long ScenarioId { get; set; }
}

public class OptimizeRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public bool MRP { get; set; }
}

[JsonObject]
public class AdvanceClockRequest : ApsWebServiceScenarioRequest
{
    public DateTime DateTime { get; set; }
}

[JsonObject]
public class AdvanceClockStringDateRequest : ApsWebServiceScenarioRequest
{
    public string DateTime { get; set; }
}

public class AdvanceClockTicksRequest : ApsWebServiceScenarioRequest
{
    public long DateTimeTicks { get; set; }
}

public class ImportRequest : ApsWebServiceScenarioRequest
{
    public string ScenarioName { get; set; }

    public bool CreateScenarioIfNew { get; set; }
}

public class PublishRequest : ApsWebServiceScenarioRequest
{
    public int PublishType { get; set; }
}

public class ApsWebServiceActionRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public PtObjectId[] PtObjects { get; set; }
}

public class HoldRequest : ApsWebServiceActionRequest
{
    [JsonRequired]
    public DateTime HoldDate { get; set; }

    public string Reason { get; set; }

    public bool Hold { get; set; } = true;
}

public class AnchorRequest : ApsWebServiceActionRequest
{
    public DateTime AnchorDate { get; set; } = PTDateTime.MinDateTime;

    public bool Lock { get; set; } = true;

    public bool Anchor { get; set; } = true;
}

public class LockRequest : ApsWebServiceActionRequest
{
    public bool Lock { get; set; } = true;
}

public class UnscheduleRequest : ApsWebServiceActionRequest { }

public class MoveRequest : ApsWebServiceActionRequest
{
    [JsonRequired]
    public string ResourceExternalId { get; set; }

    [JsonRequired]
    public DateTime MoveDateTime { get; set; }

    public bool ExpediteSuccessors { get; set; }

    public bool LockMove { get; set; } = true;

    public bool AnchorMove { get; set; } = true;

    public bool ExactMove { get; set; }
}

public class ChatRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public string RecipientName { get; set; }

    [JsonRequired]
    public string ChatMessage { get; set; }
}

public class UndoByTransmissionNbrRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public ulong TransmissionNbr { get; set; }
}

public class UndoActionsRequest : ApsWebServiceScenarioRequest
{
    public int NbrOfActionsToUndo { get; set; } = 1;
}

public class UndoLastUserActionRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public string InstigatorName { get; set; }
}

public class ActivityStatusUpdateRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public ActivityStatusUpdateObject[] ActivityStatusUpdates { get; set; }
}

public class ActivityQuantitiesUpdateRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public ActivityQuantitiesUpdateObject[] ActivityQuantitiesUpdates { get; set; }
}

public class ActivityDatesUpdateRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public ActivityDatesUpdateObject[] ActivityDatesUpdates { get; set; }
}

public class KpiSnapshotOfLiveScenarioRequest : ApsWebServiceRequestBase { }

public class BaseActivityUpdateObject
{
    [JsonRequired]
    public PtObjectId PtObjectId { get; set; }
}

public class ActivityQuantitiesUpdateObject : BaseActivityUpdateObject
{
    public decimal ReportedGoodQty { get; set; } = decimal.MinValue;

    public decimal ReportedScrapQty { get; set; } = decimal.MinValue;
}

public class ActivityDatesUpdateObject : BaseActivityUpdateObject
{
    public double ReportedSetupHrs { get; set; } = -1;

    public double ReportedRunHrs { get; set; } = -1;

    public double ReportedPostProcessingHrs { get; set; } = -1;

    public DateTime ReportedStartDate { get; set; } = DateTime.MinValue;

    public DateTime ReportedFinishDate { get; set; } = DateTime.MinValue;
}

public class ActivityStatusUpdateObject : BaseActivityUpdateObject
{
    public string ProductionStatus { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;

    public bool Paused { get; set; }

    public bool OnHold { get; set; }

    public string HoldReason { get; set; } = string.Empty;

    public DateTime HoldUntil { get; set; } = DateTime.MinValue;
}

public class PtObjectId
{
    [JsonRequired]
    public string JobExternalId { get; set; }

    public string MoExternalId { get; set; }

    public string OperationExternalId { get; set; }

    public string ActivityExternalId { get; set; }
}

public class InsertGroupUsersRequest : ApsWebServiceRequestBase
{
    [JsonRequired]
    public string SharedKey { get; set; }

    [JsonRequired]
    public List<ActiveDirectoryUser> users { get; set; }

    [JsonRequired]
    public string AccessLevel { get; set; }

    [JsonRequired]
    public string AdGroup { get; set; }
}

public class DeleteGroupUsersRequest : ApsWebServiceRequestBase
{
    [JsonRequired]
    public string SharedKey { get; set; }

    [JsonRequired]
    public List<ActiveDirectoryUser> users { get; set; }
}

public class ActiveDirectoryUser
{
    [JsonRequired]
    public string ExternalId { get; set; }

    [JsonRequired]
    public string FirstName { get; set; }

    [JsonRequired]
    public string LastName { get; set; }

    [JsonRequired]
    public string Guid { get; set; }
}

public class LoggedInUsersRequest : ApsWebServiceRequestBase
{
    [JsonRequired] private string InstanceName;

    public LoggedInUsersRequest(string a_instanceName)
    {
        InstanceName = a_instanceName;
    }
}

public class ValidateCredentialsRequest : ApsWebServiceRequestBase
{
    [JsonRequired] public string Password;

    public ValidateCredentialsRequest(string a_username, string a_password)
    {
        UserName = a_username;
        Password = a_password;
    }
}

public class ResetCredentialsRequest : ApsWebServiceRequestBase
{
    [JsonRequired] public long UserIdVal;
    [JsonRequired] public string CurrentPassword;
    [JsonRequired] public string NewPassword;
    [JsonRequired] public bool ResetPwdOnNextLogin;

    public bool ResetAdmin;

    public ResetCredentialsRequest() { }

    public ResetCredentialsRequest(long a_userIdVal, string a_currentPassword, string a_newPassword, bool a_resetPwdOnNextLogin, string a_username)
    {
        UserIdVal = a_userIdVal;
        CurrentPassword = a_currentPassword;
        NewPassword = a_newPassword;
        ResetPwdOnNextLogin = a_resetPwdOnNextLogin;
        UserName = a_username;
    }

    public ResetCredentialsRequest(string a_newPassword)
    {
        CurrentPassword = "";
        UserName = "";
        ResetPwdOnNextLogin = false;
        UserIdVal = 0;

        NewPassword = a_newPassword;
        ResetAdmin = true;
    }
}

public class ValidatePasswordRequest : ApsWebServiceRequestBase
{
    public ValidatePasswordRequest(string a_username, string a_password)
    {
        Password = a_password;
        UserName = a_username;
    }
}