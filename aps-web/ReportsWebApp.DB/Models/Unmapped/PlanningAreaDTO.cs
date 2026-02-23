using Microsoft.VisualBasic;
using ReportsWebApp.DB.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReportsWebApp.Common;

namespace ReportsWebApp.DB.Models
{
	/// <summary>
	/// Lightweight Planning Area object useful for basic info and status.
	/// </summary>
	public class PlanningAreaLiteModel
	{
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int? ExternalIntegrationId { get; set; } 


        public PlanningAreaLiteModel(){}

        public PlanningAreaLiteModel(PADetails pa)
        {
            PlanningAreaId = pa.Id;
            PlanningAreaKey = pa.PlanningAreaKey;
            DisplayText = $"{pa.Name} - {pa.Version}";

            // TODO: We store this data currently in pa.PlanningArea.PublicInfo - but pulling it from there requires a big json deserialization,
            // TODO: which we don't want to have to do for every instance every time we want a status update.
            // TODO: Best future state might be to reduce/remove the PublicInfo subclass, and use top-level PA props where needed.
            // Add more props if needed
            PublicInfo = new InstancePublicInfo()
            {
                InstanceName = pa.Name,
                SoftwareVersion = pa.Version
            };

            Status = new ServiceStatus()
            {
                State = pa.ServiceState,
                RegistrationStatus =
                    Enum.TryParse<ERegistrationStatus>(pa.RegistrationStatus, out var registrationStatus)
                        ? registrationStatus
                        : ERegistrationStatus.Unknown,
            };

            Server = pa.Server;
        }

        public InstancePublicInfo PublicInfo { get; set; }

		public ServiceStatus Status { get; set; }
        /// <summary>
        /// Shows DisplayText → Includes Instance Name + Version
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// AKA InstanceIdentifier (unique GUID) - the main Webapp-side identifier for an instance (this existed, but was not used as a key, in old SM code)
        /// </summary>
        public string PlanningAreaKey { get; set; }

        /// <summary>
        /// Internal db FK to Planning Area. Not returned by old SM methods to get status, but useful to include going forward in Webapp.
        /// </summary>
        public int PlanningAreaId { get; set; }

        public  CompanyServer Server { get; set; }

    }
    public enum EnvironmentTypeEnum
	{
		Dev,
		QA,
		Production
	}

	public class Integration
	{
		public Integration() { }

		public string ConnectionString { get; set; } = "";

		public string DatabaseName { get; set; } = "";

		public string ERPDatabaseName { get; set; } = "";

		public string PreImportSQL { get; set; } = "";

		public string SQLServerConnectionString { get; set; } = "";

		public bool RunPreImportSQL { get; set; } = false;

		public string UserName { get; set; } = "";

		public string ERPServerName { get; set; } = "";

		public string ERPUserName { get; set; } = "";

		public string ERPPassword { get; set; } = "";
	}

	public class ServiceStatus
	{
		#region Status

		public int UsersOnline { get; set; }
		public DateTime LastLogon { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime LastActionTime { get; set; }

		public EServiceState State { get; set; }

		/// <summary>
		/// The state of the Planning Area in terms of the creation workflow, as it is requested from the WebApp, picked up the Server Manager, and created.
		/// All successfully created PAs should be in the <see cref="ERegistrationStatus.Created"/> state.
		/// </summary>
		public ERegistrationStatus RegistrationStatus { get; set; }
		#endregion
	}

	public class ConnectionDetails
	{
        // Additional info tracked for the login page
        public CompanyServer SourceServer { get; set; }
        public string? LaunchId { get; set; }
        public ELaunchStatus? LaunchStatus { get; set; }
        public bool isLaunching { get => LaunchStatus != null && (int)LaunchStatus < 20; }
        public bool hadLaunchError { get => LaunchStatus != null && (int)LaunchStatus > 40; }
    }
    
    public enum ELicenseStatus
    {
	    Unknown,
	    Active,
	    ReadOnly,
	    Error
    }
}