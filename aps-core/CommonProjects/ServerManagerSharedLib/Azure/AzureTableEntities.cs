using Azure;

using Microsoft.WindowsAzure.Storage.Table;

using ITableEntity = Azure.Data.Tables.ITableEntity;

namespace PT.ServerManagerSharedLib.Azure
{
    /// <summary>
    /// Base Software version table entity
    /// </summary>
    public class BaseVersionEntity : ITableEntity
    {
        public DateTime VersionDate { get; set; }
        public string VersionNumber { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }

        public BaseVersionEntity(string a_versionNumber, DateTime a_versionDate, string a_rowKey)
        {
            VersionNumber = a_versionNumber;
            VersionDate = a_versionDate;
            PartitionKey = "Partition1";
            RowKey = a_rowKey;
        }

        public BaseVersionEntity() { }
    }

    /// <summary>
    /// Standard Software version table entity
    /// </summary>
    public class StandardVersionEntity : ITableEntity
    {
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }
        public DateTime VersionDate { get; set; }
        public string VersionNumber { get; set; }

        public StandardVersionEntity() { }
    }

    /// <summary>
    /// Standard workspace table entity
    /// </summary>
    public class FileEntity : ITableEntity
    {
        public string Filename { get; set; }
        public string DisplayText { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }

        public FileEntity() { }
    }

    /// <summary>
    /// Standard Software version table entity
    /// </summary>
    public class IntegrationFilesEntity : ITableEntity
    {
        public string IntegrationName { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }

        public IntegrationFilesEntity() { }
    }

    /// <summary>
    /// Standard scenario file table entity
    /// </summary>
    public class ScenarioFileEntity : ITableEntity
    {
        public string ScenarioName { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }

        public ScenarioFileEntity() { }
    }


    /// <summary>
    /// Early Access Software version table entity
    /// </summary>
    public class EarlyAccessVersionEntity : BaseVersionEntity
    {
        public EarlyAccessVersionEntity() { }
    }

    /// <summary>
    /// Beta Software version table entity
    /// </summary>
    public class BetaVersionEntity : BaseVersionEntity
    {
        public BetaVersionEntity() { }
    }

    /// <summary>
    /// Company Specific Software version entity
    /// </summary>
    public class CompanySpecificVersionEntity : BaseVersionEntity
    {
        public int Id { get; set; }
        public string VersionType { get; set; }

        public CompanySpecificVersionEntity(int a_id, DateTime a_versionDate, string a_versionNumber, string a_versionType, string a_rowKey) : base(a_versionNumber, a_versionDate, a_rowKey)
        {
            Id = a_id;
            VersionType = a_versionType;
        }

        public CompanySpecificVersionEntity() { }
    }

    /// <summary>
    /// Company table entity
    /// </summary>
    public class CompanyEntity : ITableEntity
    {
        public int Id { get; set; }
        public bool EarlyAccessOptIn { get; set; }
        public bool BetaOptIn { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }

        public CompanyEntity() { }
    }

    /// <summary>
    /// Configuration table entity
    /// </summary>
    public class ConfigurationEntity : ITableEntity
    {
        public string MinSMVersion { get; set; }
        public string MinVersion { get; set; }
        public string ServerManagerVersion { get; set; }
        public string MinClientManagerVersion { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }

        public ConfigurationEntity() { }
    }
}