namespace WebAPI.RequestsAndResponses
{
    public class WebCreateInstanceResponseParams : WebInstanceIdentifierResponseParams
    {
        /// <summary>
        /// Planning area settings as a string json blob. TODO: We should define a non-blob class across our repos
        /// </summary>
        public string PlanningAreaSettings { get; set; }
        // Whether the instance is starting up or not
        public bool? Starting { get; set; }
        public string NewVersion { get; set; }
    }

    public class WebUpdateServerSettingsRequestParams
    {
        public ServerSettingsDto ServerSettings { get; set; }
    }

    /// <summary>
    /// Server Settings object from the Server Manager. Should map to a <see cref="CompanyServer"/>
    /// </summary>
    public class ServerSettingsDto : ServerWideInstanceSettings
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
        [Obsolete("WebApp shouldn't need to access server directly, can be null")]
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
        /// A message that's displayed to users when they log into a particular server on the Sign in APp
        /// </summary>
        public string AdminMessage { get; set; }

        #endregion
    }
}
