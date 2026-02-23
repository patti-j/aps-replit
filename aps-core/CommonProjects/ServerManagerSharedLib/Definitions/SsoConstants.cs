using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class SsoConstants
    {
        public const string Auth0DomainStandard = "https://planettogether.us.auth0.com/";
        public const string Auth0ClientIdStandard = "SubVk4ohZW2uq9GCY8zKCQOTKk6HIzgP";

        public const string Auth0DomainStarter = "https://planettogether.us.auth0.com/";
        public const string Auth0ClientIdStarter = "5AYQMRsIJm9DDtBoZ6EEbQC9zLVRjKIi";

        public const string Auth0DomainDebug = "https://planettogether-dev.us.auth0.com/";
        public const string Auth0ClientIdDebug = "XdMCWRhywgiO9oLbT3qGI6Ak1qnU8NpC";

        public const string Auth0ClientIdWebAppDebug = "XdMCWRhywgiO9oLbT3qGI6Ak1qnU8NpC";
        public const string Auth0ClientIdWebAppProd = "SubVk4ohZW2uq9GCY8zKCQOTKk6HIzgP";

        /// <summary>
        /// Returns an SSO Domain value based on the value set for the instance, server, or application default (in descending priority)
        /// </summary>
        /// <param name="a_instanceSettings"></param>
        /// <param name="a_serverWideSettings"></param>
        /// <returns></returns>
        public static string GetSsoDomain(SystemServiceSettings a_instanceSettings, ServerWideInstanceSettings a_serverWideSettings)
        {
            return !string.IsNullOrWhiteSpace(a_instanceSettings.SsoDomain) ? 
                a_instanceSettings.SsoDomain :
                a_serverWideSettings.SsoDomain;
        }

        /// <summary>
        /// Returns an SSO ClientId value based on the value set for the instance, server, or application default (in descending priority)
        /// </summary>
        /// <param name="a_instanceSettings"></param>
        /// <param name="a_serverWideSettings"></param>
        /// <returns></returns>
        public static string GetSsoClientId(SystemServiceSettings a_instanceSettings, ServerWideInstanceSettings a_serverWideSettings)
        {
            return !string.IsNullOrWhiteSpace(a_instanceSettings.SsoClientId) ?
                a_instanceSettings.SsoClientId :
                a_serverWideSettings.SsoClientId;
        }
    }
}
