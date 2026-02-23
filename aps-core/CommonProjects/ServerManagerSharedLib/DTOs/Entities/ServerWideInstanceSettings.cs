namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    /// <summary>
    /// Holds instance settings that are common to all instances managed by a particular Server Manager, to be applied to all such managed instances.
    /// Acts as a subset of the overall <see cref="ServerSettingsDto"/> that instances care about (and is all they should need to access).
    /// </summary>
    public class ServerWideInstanceSettings
    {
        /// <summary>
        /// The location on the server machine where the PT directory resides
        /// (called ServerManager path for historical continuity, but it's the root, not the location of the Server Manager exe)
        /// </summary>
        public string ServerManagerPath { get; set; }

        public string Thumbprint { get; set; }

        public string SsoDomain { get; set; }

        public string SsoClientId { get; set; }

        [Obsolete("Removed in favor of storing Webapp Id in the instance's config, so that it can connect to the webapp before being sent values.")]
        public string WebAppUrl { get; set; }

        public string WebAppClientId { get; set; }
        
        public long AutoLogoffTimeout { get; set; }

        public ServerWideInstanceSettings() {}
        public ServerWideInstanceSettings(ServerSettingsDto a_allServerSettingsDto)
        {
            ServerManagerPath = a_allServerSettingsDto.ServerManagerPath;
            Thumbprint = a_allServerSettingsDto.Thumbprint;
            SsoClientId = a_allServerSettingsDto.SsoClientId;
            SsoDomain = a_allServerSettingsDto.SsoDomain;
            // TODO: Add WebappClientId if still needed
        }
    }
}
