using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppImportService.Models
{
	public class InstanceWithUser
	{
		public string SystemServiceUrl { get; set; }
		public string InstanceName { get; set; }
		public string SoftwareVersion { get; set; }
		public string SystemServiceName { get; set; }
		public string InterfaceServiceName { get; set; }
		public string AdminMessage { get; set; }
		public DateTime CreationDate { get; set; }
		public int EnvironmentType { get; set; }
		public bool ActiveDirectoryAllowed { get; set; }
		public bool SsoAllowed { get; set; }
		public bool AllowPasswordSettings { get; set; }
		public bool AutoStart { get; set; }
		public string InstanceId { get; set; }
		public CurrentIntegration CurrentIntegration { get; set; }
	}

	public class CurrentIntegration
	{
		public string ConnectionString { get; set; }
		public string DatabaseName { get; set; }
		public string ErpDatabaseName { get; set; }
		public string PreImportSQL { get; set; }
		public string SqlServerConnectionString { get; set; }
		public bool RunPreImportSQL { get; set; }
		public string UserName { get; set; }
		public string ErpServerName { get; set; }
		public string ErpUserName { get; set; }
		public string ErpPassword { get; set; }
	}
}
