using System;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class InstanceUsersConnected
    {
        public string InstanceName { get; set; }
        public int Users { get; set; }

        public InstanceUsersConnected(string instanceName, int usersConnected)
        {
            InstanceName = instanceName;
            Users = usersConnected;
        }
    }
}
