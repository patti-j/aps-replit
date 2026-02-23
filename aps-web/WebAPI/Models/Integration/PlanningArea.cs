using System.ComponentModel.DataAnnotations.Schema;

using static WebAPI.Models.ServiceStatus;

namespace WebAPI.Models.Integration
{
    public class PADetails : BaseEntity
    {
        public virtual List<PlanningAreaTag> Tags { get; set; } = new();
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public string Version { get; set; } = "";
        public int ServerId { get; set; }
        public virtual CompanyServer Server { get; set; }
        public string Environment { get; set; } = "";
        public string Settings { get; set; } = "";
        public DateTime? UpdateDate { get; set; } = DateTime.UtcNow;
        public string PlanningAreaKey { get; set; }
        public string RegistrationStatus { get; set; } = ERegistrationStatus.Pending.ToString();
        public ELicenseStatus LicenseStatus { get; set; } = ELicenseStatus.Unknown;
        public EServiceState ServiceState { get; set; }
        public DateTime ServiceStateUpdated { get; set; }
        
        public int? UsedByCompanyId { get; set; }
        public virtual Company? UsedByCompany { get; set; }
        
        public virtual DBIntegration? CurrentIntegration {get; set;}

        [ForeignKey("CurrentIntegration")]
        public int? DBIntegrationId { get; set; }
        
        public DateTime? DBIntegrationLastAppliedTime { get; set; }

        public virtual List<IntegrationConfig> IntegrationConfigs { get; set; }
        //[ForeignKey("CFGroup")]
        //public int? CFGroupId { get; set; }
        //public virtual CFGroup? CFGroup { get; set; }
        public string FavoriteSettingScenarioIds { get; set; } = string.Empty;
        public int? BackupOf { get; set; }
        public bool IsBackup => BackupOf is > 0;

        public string? ApiKey { get; set; }
        public string? ApiUrl { get; set; }

        //[NotMapped]
        //public PlanningArea PlanningArea { get; set; } = new();
    }
    
    public enum EStartType
    {
        Normal,
        Delayed,
        Manual,
        Automatic
    }

    public enum ERegistrationStatus
    {
        Pending,
        Created,
        Error,
        Deleting
    }
    
    public enum ELicenseStatus
    {
        Unknown,
        Active,
        ReadOnly,
        Error
    }

    public enum EnvironmentType
    {
        Dev,
        QA,
        Production
    }
}
