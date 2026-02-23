using Newtonsoft.Json;

namespace PT.APIDefinitions.RequestsAndResponses;

[JsonObject]
public class ApsWebServiceRequestBase
{
    [JsonRequired]
    public string UserName { get; set; }

    public string Password { get; set; } = string.Empty;

    public TimeSpan TimeoutDuration { get; set; } = TimeSpan.FromSeconds(30);
}
[JsonObject]
public class ApsWebServiceRequestBaseV2
{
    //[JsonRequired]
    //public string ApiKey { get; set; }
    public TimeSpan TimeoutDuration { get; set; } = TimeSpan.FromSeconds(30);
}
[JsonObject]
public class ApsWebServiceScenarioRequestV2 : ApsWebServiceRequestBaseV2
{
    public long ScenarioId { get; set; }
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
public class OptimizeRequestV2 : ApsWebServiceScenarioRequestV2
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
public class AdvanceClockRequestV2 : ApsWebServiceScenarioRequestV2
{
    public DateTime DateTime { get; set; }
}

[JsonObject]
public class AdvanceClockStringDateRequest : ApsWebServiceScenarioRequest
{
    public string DateTime { get; set; }
}

[JsonObject]
public class AdvanceClockStringDateRequestV2 : ApsWebServiceScenarioRequestV2
{
    public string DateTime { get; set; }
}

public class AdvanceClockTicksRequest : ApsWebServiceScenarioRequest
{
    public long DateTimeTicks { get; set; }
}
public class AdvanceClockTicksRequestV2 : ApsWebServiceScenarioRequestV2
{
    public long DateTimeTicks { get; set; }
}

public class ImportRequest : ApsWebServiceScenarioRequest
{
    public string ScenarioName { get; set; }

    public bool CreateScenarioIfNew { get; set; }
}
public class ImportRequestV2 : ApsWebServiceScenarioRequestV2
{
    public string ScenarioName { get; set; }

    public bool CreateScenarioIfNew { get; set; }
}
public class PublishRequest : ApsWebServiceScenarioRequest
{
    public int PublishType { get; set; }
}
public class PublishRequestV2 : ApsWebServiceScenarioRequestV2
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
public class UndoByTransmissionNbrRequestV2 : ApsWebServiceScenarioRequestV2
{
    [JsonRequired]
    public ulong TransmissionNbr { get; set; }
}
public class UndoActionsRequest : ApsWebServiceScenarioRequest
{
    public int NbrOfActionsToUndo { get; set; } = 1;
}
public class UndoActionsRequestV2 : ApsWebServiceScenarioRequestV2
{
    public int NbrOfActionsToUndo { get; set; } = 1;
}
public class UndoLastUserActionRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public string InstigatorName { get; set; }
}
public class UndoLastUserActionRequestV2 : ApsWebServiceScenarioRequestV2
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
public class KpiSnapshotOfLiveScenarioRequestV2 : ApsWebServiceRequestBaseV2 { }

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
    public double ReportedCleanHrs { get; set; } = -1;
    public int ReportedCleanoutGrade { get; set; } = -1;

    public DateTime ReportedStartDate { get; set; } = DateTime.MinValue;
    public DateTime ReportedProcessingStartDate { get; set; } = DateTime.MinValue;
    public DateTime ReportedProcessingEndDate { get; set; } = DateTime.MinValue;
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
public class DownloadScenarioRequest
{
    [JsonRequired]
    public string ScenarioIdList { get; set; }

    [JsonRequired]
    public bool ClearUndoSets { get; set; }
}