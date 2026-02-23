using System.Diagnostics;

using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    /// <summary>
    /// Classes that represent strongly-typed *incoming* json blobs to go in the "Parameters" prop of a <see cref="WebApiAction"/>
    /// Ideally, by defining these here and in the Webapp API repo, we have a clear idea of what params return from the SM when an action is completed, and what to pull out of them.
    /// Classes that represent outgoing actions Params should be added to WebAppActionResponses.cs.
    /// TODO: We should really have a shared library of all these common models between repos
    /// </summary>

    public class CopyPlanningAreaRequest
    {
        public CreatePlanningAreaRequest NewInstance { get; set; }
        public InstanceKey OriginInstance { get; set; }
        public bool StartWhenCreated { get; set; }
    }

    public class CreatePlanningAreaRequest
    {
        public string PlanningAreaName { get; set; }
        public string PlanningAreaVersion { get; set; }
        public string PlanningAreaKey { get; set; }
        public string IntegrationCode { get; set; }
        public string EnvironmentType { get; set;}
        public string SerialCode { get; set; }
        public string ScenarioFile { get; set; }
        public bool StartInstanceService { get; set; } = false;
        public string ScenarioFilePath { get; set; } // TODO implement in webapp
        public List<string> WorkspaceFilenames { get; set; } // TODO implement in webapp
        public string ApiKey { get; set; }
    }

    public class UpgradePlanningAreaRequest : InstanceKey
    {
        public string NewVersion { get; set; }
        public bool StartAfterUpgrade { get; set; }
    }

    public class PlanningAreaSettingsUpdateRequest
    {
        public InstanceKey InstanceKey { get; set; } = new();
        public int OldPort { get; set; }
    }

    public class EventLogEntryAdapter
    {
        public string Category;
        public short CategoryNumber;
        public string Message;
        public string Source;
        public DateTime TimeGenerated;

        public static explicit operator EventLogEntryAdapter(EventLogEntry e) => new EventLogEntryAdapter()
        {
            Category = e.Category,
            CategoryNumber = e.CategoryNumber,
            Message = e.Message,
            Source = e.Source,
            TimeGenerated = e.TimeGenerated,
        };
    }

    public class PlanningAreaGetLogsUpdateRequest
    {
        public string TransactionId { get; set; }
        public List<EventLogEntryAdapter> Events { get; set; }
    }
}