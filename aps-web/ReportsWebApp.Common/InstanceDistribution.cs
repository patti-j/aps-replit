using Azure;

namespace ReportsWebApp.DB.Models
{
    public class InstancesDistributionDto
    {
        public List<InstanceDistribution> data { get; set; } = new();

    }
    public class InstanceDistribution
    {
        public string Instance { get; set; }
        public int Area { get; set; }
    }
    public class SaveInstanceResponse
    {
        public bool created { get; set; }
        public bool saved { get; set; }
        public object instance { get; set; }
        public string error { get; set; }
        public bool restartRequired { get; set; }
    }

    // ActionInstanceResponse: A standard response for start/stop/restart operations
    public class ActionInstanceResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }
    public enum EnvironmentType
    {
        Dev,
        QA,
        Production
    }

    /// <summary>
    /// Standard workspace table entity
    /// </summary>
    public class FileEntity 
    {
        public string Filename { get; set; }
        public string DisplayText { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }

        public FileEntity() { }
    }

    public class DistributionRequest
    {
        public EnvironmentType environment { get; set; }
    }
    // InstancesDistributionModel: Holds distribution data for all instances
    public class InstancesDistributionModel
    {
        public int TotalInstances { get; set; }
        public int ActiveInstances { get; set; }
        // Add additional fields as needed
    }

    // InstanceSettingsModel: Holds settings data for a single instance
    public class InstanceSettingsModel
    {
        public string InstanceName { get; set; }
        public string SoftwareVersion { get; set; }
        public string Environment { get; set; }
        // Add additional fields as needed
    }

    // ServiceStatus: Represents the status of an instance
    public class ServiceStatus2
    {
        public string Status { get; set; }
        public string Details { get; set; }
    }

    // InstancesUsersConnectedModel: Represents connected user data for all instances
    public class InstancesUsersConnectedModel
    {
        public List<InstanceUserData> data { get; set; }
    }

    public class InstanceUserData
    {
        public string InstanceName { get; set; }
        public int Users { get; set; }
    }

}
