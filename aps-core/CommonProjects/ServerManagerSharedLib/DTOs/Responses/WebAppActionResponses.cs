using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    /// <summary>
    /// Classes that represent strongly-typed *outgoing* json blobs to go in the "Parameters" prop of a <see cref="WebApiActionUpdate"/>
    /// Ideally, by defining these here and in the Webapp API repo, we have a clear idea of what params return from the SM when an action is completed, and what to pull out of them.
    /// Classes that represent outgoing actions Params should be added to WebAppActionResponses.cs.
    /// TODO: We should really have a shared library of all these common models between repos
    /// </summary>



    /// <summary>
    /// Returns the full settings data for an instance to save that data to the db.
    /// As such, this can also be used as a "save" for an existing instance. TODO: Rename across all repos to reflect this.
    /// </summary>
    public class WebCreateInstanceResponseParams
    {
        public string PlanningAreaKey { get; set; }
        /// <summary>
        /// Planning area settings as a string json blob. TODO: We should define a non-blob class across our repos
        /// </summary>
        public string PlanningAreaSettings { get; set; }
        public bool Starting { get; set; }
        public string NewVersion { get; set; }
    }

    public class WebInstanceIdentifierResponseParams
    {
        public string PlanningAreaKey { get; set; }
        public string Version { get; set; }
    }

    public class WebUpdateServerSettingsRequestParams
    {
        public ServerSettingsDto ServerSettings { get; set; }
    }

    public class WebUpgradeInstanceResponseParams
    {
        public string InstanceName { get; set; }
        public string OldVersion { get; set; }
        public string NewVersion { get; set; }
        public string UpdatedSettings { get; set; }
    }
}
