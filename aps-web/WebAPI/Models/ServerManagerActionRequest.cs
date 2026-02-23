using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    public class ServerManagerActionRequest
    {
        [Key]
        public Guid TransactionId { get; set; }
        [ForeignKey("CompanyServer")]
        public int ServerId { get; set; }
        public CompanyServer Server { get; set; }
        public string Action { get; set; }
        public string ParameterJson { get; set; }
        public string RequestStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }

    public enum EActionRequestStatuses
    {
        New,
        Processing,
        Success, 
        Error,
        NewFromServer // Request originated from SM (thus, doesn't need to be picked up by it, handled webside)
    }

    public enum EServerActionTypes
    {
        AddPlanningArea,
        CopyPlanningArea,
        DeletePlanningArea,
        StartPlanningArea,
        RestartPlanningArea,
        UpdatePlanningAreaSettings,
        StopPlanningArea,
        UpgradePlanningArea,
        UpgradeServerAgent,
        RestartServer,
        UpdateServerSettings, // no call needed from webapp, used for updates originating on SM
        UnregisterServer // no call needed from webapp, used for updates originating on SM
        // Etc TODO: Add as needed
    }
}
