using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api.Models;

using ReportsWebApp.DB.Models.WebApp;

namespace ReportsWebApp.DB.Models
{
    public class ServerManagerActionRequest : INamedEntity
    {
        [Key]
        public Guid TransactionId { get; set; }
        [ForeignKey("Server")]
        public int ServerId { get; set; }
        public virtual CompanyServer Server { get; set; }
        public string Action { get; set; }
        public string ParameterJson { get; set; }
        public string RequestStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        [NotMapped]
        public int Id => 0;
        [NotMapped]
        public string Name => Action;
        [NotMapped]
        public string TypeDisplayName => "Action";
    }

    public enum EServerActionTypes
    {
        AddPlanningArea,
        CopyPlanningArea,
        DeletePlanningArea,
        StartPlanningArea,
        RestartPlanningArea,
        GetLogs,
        UpdatePlanningAreaSettings,
        StopPlanningArea,
        UpgradePlanningArea,
        UpgradeServerAgent,
        RestartServer,
        UpdateServerSettings, // no call needed from webapp, used for updates originating on SM
        UnregisterServer // no call needed from webapp, used for updates originating on SM
        // Etc TODO: Add as needed
    }

    public enum EActionRequestStatuses
    {
        New,
        Processing,
        Success,
        Error,
        NewFromServer, // Request originated from SM (thus, doesn't need to be picked up by it, handled webside)
        Cancelled
    }

    /// <summary>
    /// Schema for <see cref="ServerManagerActionRequest.ParameterJson"/> when for actions of type <see cref="EServerActionTypes.AddPlanningArea"/>
    /// </summary>
    public class CreatePlanningAreaRequest
    {
        public CreatePlanningAreaRequest() {}
        public CreatePlanningAreaRequest(PADetails model, bool startInstance = false)
        {
            PlanningAreaName = model.Name;
            PlanningAreaVersion = model.Version;
            PlanningAreaKey = model.PlanningAreaKey;
            IntegrationCode = model.PlanningArea.ServicePaths.IntegrationCode;
            EnvironmentType = model.PlanningArea.PublicInfo.EnvironmentType.ToString();
            SerialCode = model.PlanningArea.LicenseInfo.SerialCode;
            ScenarioFile = model.PlanningArea.Settings.ScenarioFile;
            ScenarioFilePath = string.Empty; // TODO ? 
            WorkspaceFilenames = new List<string>(); // TODO
            StartInstanceService = startInstance;
            ApiKey = model.ApiKey;
        }

        public override bool Equals(object? other)
        {
            if (other is CreatePlanningAreaRequest req)
            {
                return req.PlanningAreaKey == PlanningAreaKey;
            }
            return false;
        }

        public string PlanningAreaName { get; set; }
        public string PlanningAreaVersion { get; set; }
        public string PlanningAreaKey { get; set; }
        public string IntegrationCode { get; set; }
        public string EnvironmentType { get; set; }
        public string SerialCode { get; set; }
        public string ScenarioFile { get; set; }
        public bool StartInstanceService { get; set; } = false; // TODO implement in UI
        public string ScenarioFilePath { get; set; } // TODO implement in UI? Might not work on webapp side
        public List<string> WorkspaceFilenames { get; set; } = new(); // TODO implement in UI
        public string ApiKey { get; set; }

    }

    /// <summary>
    /// Schema for <see cref="ServerManagerActionRequest.ParameterJson"/> when for actions of type <see cref="EServerActionTypes.CopyPlanningArea"/>
    /// </summary>
    public class CopyPlanningAreaRequest
    {
        public CopyPlanningAreaRequest() { }
        public CopyPlanningAreaRequest(PADetails a_model, PADetails a_originInstance, bool startInstance = false)
        {
            NewInstance = new CreatePlanningAreaRequest(a_model, startInstance);
            OriginInstance = new InstanceKey(a_originInstance.Name, a_originInstance.Version);
            StartWhenCreated = false; // TODO
        }

        public override bool Equals(object? other)
        {
            if (other is CopyPlanningAreaRequest req)
            {
                return req.NewInstance.PlanningAreaName == NewInstance.PlanningAreaName;
            }
            return false;
        }

        public CreatePlanningAreaRequest NewInstance { get; set; }
        public InstanceKey OriginInstance { get; set; }
        public bool StartWhenCreated { get; set; }
    }

    public class UpgradePlanningAreaRequest : InstanceKey
    {
        public string NewVersion { get; set; }
        public bool StartAfterUpgrade { get; set; }
    }

    public class InstanceKey
    {
        public string InstanceName { get; set; }
        public string SoftwareVersion { get; set; }

        public InstanceKey() { }

        public InstanceKey(string a_instanceName, string a_softwareVersion)
        {
            InstanceName = a_instanceName;
            SoftwareVersion = a_softwareVersion;
        }

        public InstanceKey(PADetails planningArea)
        {
            InstanceName = planningArea.Name;
            SoftwareVersion = planningArea.Version;
        }

        public override bool Equals(object? other)
        {
            if (other is InstanceKey req)
            {
                return req.InstanceName == InstanceName && req.SoftwareVersion == SoftwareVersion;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{InstanceName} {SoftwareVersion}";
        }
    }

    public class InstanceSettingsUpdateRequest
    {
        public InstanceKey InstanceKey { get; set; } = new();
        public int OldPort { get; set; }

        public InstanceSettingsUpdateRequest() { }

        public InstanceSettingsUpdateRequest(string a_instanceName, string a_softwareVersion, int oldPort)
        {
            InstanceKey.InstanceName = a_instanceName;
            InstanceKey.SoftwareVersion = a_softwareVersion;
            OldPort = oldPort;
        }

        public override bool Equals(object? other)
        {
            if (other is InstanceSettingsUpdateRequest req)
            {
                return req.InstanceKey.Equals(InstanceKey);
            }
            return false;
        }
    }
}
