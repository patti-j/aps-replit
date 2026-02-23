using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    /// <summary>
    /// Represents a server managed within the application.
    /// </summary>
    public class CompanyServer : BaseEntity
    {
        /// <summary>
        /// The version of the ServerManager currently in use.
        /// This is used when enqueuing actions in case a recently upgraded Server Manager needs to handle actions from its previous version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The authentication token for accessing the server.
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// The URL for accessing the server.
        /// </summary>
        public string Url { get; set; }

        #region Server Settings 
        // Configurable Settings. Same entity to keep EF simpler

        /// <summary>
        /// The name of the server. Historically, has defaulted to the machine name.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// The name used to identify the server to those attempting to reach it. Defaults to the <see cref="ServerName"/>,
        /// but is often configured to the fully qualified domain name
        /// </summary>
        public string ComputerNameOrIP { get; set; }

        /// <summary>
        /// The port used for internal API communication on the server (ie - from instances to SM). Hardcoded to 7980 in the past.
        /// </summary>
        public string ApiPort { get; set; } = "7980";

        /// <summary>
        /// The first CPU Id of the server machine
        /// </summary>
        public string SystemId { get; set; }

        /// <summary>
        /// The location on the server machine where the PT directory resides
        /// (called ServerManager path for historical continuity, but it's the root, not the location of the Server Manager exe)
        /// </summary>
        public string ServerManagerPath { get; set; }

        /// <summary>
        /// A message that's displayed to users when they log into a particular server on the Sign in APp
        /// </summary>
        public string AdminMessage { get; set; }

        public string? Notes { get; set; }
        public string SsoDomain { get; set; }
        public string SsoClientId { get; set; }
        public string CertificateName { get; set; }
        public string Thumbprint { get; set; }

        #endregion

        /// <summary>
        /// The IP address of the server.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// The physical or logical location of the server.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The current operational status of the server.
        /// </summary>
        [NotMapped]
        public string Status => IsOnline ? "Online" : "Offline";

        /// <summary>
        /// The timestamp of the server's last recorded activity.
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Whether the server is online. Servers are assumed to be offline if no activity has been reported for 2 minutes
        /// </summary>
        public bool IsOnline => LastActivity > DateTime.UtcNow.AddMinutes(-2);

        /// <summary>
        /// The thumbprint of the server's certificate for authentication.
        /// </summary>

        /// <summary>
        /// Collection of companies this server is used for (i.e., the customer(s) the instances are for). 
        /// These companies do not manage the server.
        /// </summary>
        public virtual ICollection<ServerUsingCompany> UsingCompanies { get; set; } = new List<ServerUsingCompany>();

        /// <summary>
        /// The ID of the company that manages this server.
        /// </summary>
        [ForeignKey("ManagingCompany")]
        public int ManagingCompanyId { get; set; }
        
        [ForeignKey(nameof(OwningUser))]
        public int? OwningUserId { get; set; }
        public virtual User OwningUser { get; set; }

        /// <summary>
        /// The company that manages this server.
        /// </summary>
        public virtual Company ManagingCompany { get; set; }

        public List<ServerCertificate> ServerCertificates { get; set; }

        public int? AutomaticUpdateHour { get; set; }
        public string? AutomaticUpdateDay { get; set; }
        public string? AutomaticUpdateFrequency { get; set; }
        public DateTime? LastUpdateCheck { get; set; }

        public string? SsoThumbprint { get; set; }
        public string LocalVersions { get; set; }
    }

    /// <summary>
    /// Represents the relationship between a server and the companies it is used for.
    /// </summary>
    public class ServerUsingCompany
    {
        /// <summary>
        /// The ID of the associated server.
        /// </summary>
        public int CompanyServerId { get; set; }

        /// <summary>
        /// The server associated with this relationship.
        /// </summary>
        public virtual CompanyServer CompanyServer { get; set; }

        /// <summary>
        /// The ID of the company that uses the server.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// The company that uses the server.
        /// </summary>
        public virtual Company Company { get; set; }

        public override bool Equals(object comparator)
        {
            if (comparator is ServerUsingCompany typedComparator)
            {
                return CompanyServerId == typedComparator.CompanyServerId &&
                       CompanyId == typedComparator.CompanyId;
            }

            return false;
        }

        public override string ToString()
        {
            return Company?.Name ?? $"Company Id {CompanyId}";
        }
    }
}
