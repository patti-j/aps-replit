using System;
using PT.Common;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class ServiceStatus
    {
        #region Status

        public int UsersOnline { get; set; }
        public DateTime LastLogon { get; set; }
        public DateTime StartTime { get; set; } = PTDateTime.MinDateTime;
        public DateTime LastActionTime { get; set; }
        
        public EServiceState State { get; set; }

        public enum EServiceState
        {
            Stopped,
            Stopping,
            Idle,
            Started,
            Active,
            Starting,
            Unknown
        }

        #endregion

        #region Identifiers
        // These are returned to verify the response comes from the expected instance
        // (ie, not an older version/ different instance using the an old instance's port).
        public Version ProductVersion { get; set; }
        public string InstanceName { get; set; }
        
        #endregion
    }
}
