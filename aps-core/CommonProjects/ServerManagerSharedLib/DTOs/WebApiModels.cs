using PT.APSInstancesClassLibrary.DTOs.Entities;
using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs
{
    /// <summary>
    /// A request from the WebApp to the Server Manager to perform an action.
    /// </summary>
    public class WebApiAction
    {
        public string TransactionId { get; set; }
        public string Action { get; set; }
        public string Parameters { get; set; }
    }

    /// <summary>
    /// A response from the Server Manager based on an action taken.
    /// </summary>
    public class WebApiActionUpdate
    {
        public string Parameters { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// A <see cref="WebApiActionUpdate"/> that originated from the WebApp (referencing its TransactionId), to be returned from the Server Manager after it completes its work.
    /// </summary>
    public class WebApiActionFollowup : WebApiActionUpdate
    {
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class WebApiStatusUpdate
    {
        public string ServerAuthToken { get; set; }
        public int Port { get; set; }
        public List<string> AvailableVersions { get; set; }
        public Dictionary<string, ServiceStatus> Statuses { get; set; }
    }

    public class WebApiAgentStatusUpdate
    {
        public string ServerAuthToken { get; set; }
        public Version Version { get; set; }
        public List<Certificate> Certificates { get; set; }
        public string SystemId { get; set; }

    }

    /// <summary>
    /// A <see cref="WebApiActionUpdate"/> that originated from the Server Manager, and requires final steps to be performed in the WebApp. No TransactionId needed as there's no existing action.
    /// </summary>
    public class WebApiActionFromServer : WebApiActionUpdate
    {
        public string ActionType { get; set; }
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
        UnregisterServer, // no call needed from webapp, used for updates originating on SM
        GetLogs,

    }
}
