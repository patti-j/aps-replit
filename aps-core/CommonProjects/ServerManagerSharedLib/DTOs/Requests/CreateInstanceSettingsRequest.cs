using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    /// <summary>
    /// Request for updating instance settings, to be saved in the instance settings database.
    /// While Server Manager used to handle this, the API now resides on the instance in core.
    /// </summary>
    public class CreateInstanceSettingsRequest
    {
        /// <summary>
        /// Contains all instance-level settings.
        /// </summary>
        public APSInstanceEntity InstanceSettings { get; set; }

        /// <summary>
        /// Contains settings that have historically been determined by the Server Manager managing the instance, rather than controllable by the instance itself.
        /// Unlike the settings contained in <see cref="SaveInstanceRequest.InstanceSettings"/>, these may automatically be updated if a managing server updates its values.
        /// This should be populated when creating a new instance, but can be null otherwise.
        /// </summary>
        public ServerWideInstanceSettings ServerWideInstanceSettings { get; set; } = null;
    }
}
