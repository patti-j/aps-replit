using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Exceptions;

namespace PT.ServerManagerSharedLib.Helpers
{
    public static class InstanceSettingManagerFactory
    {
        public static IMultiInstanceSettingsManager CreateInstanceSettingsManagerForServer(string a_connectionString, string a_name, string a_instanceId, string a_serverAuthToken, string a_env)
        {
            if (!string.IsNullOrEmpty(a_serverAuthToken) && !string.IsNullOrEmpty(a_env))
            {
                var manager = new WebAppMultiInstanceSettingsManager(a_instanceId, a_serverAuthToken, a_env);
                if (manager.EnsureConnection())
                {
                    return manager;
                }
            }
            else
            {
                return new DbInstanceSettingsManager(a_connectionString, a_name);
            }

            throw new ServerManagerException("4501", new object[] { a_name });
        }

        public static IInstanceSettingsManager CreateInstanceSettingsManagerForInstance(string a_connectionString, string a_name, string a_instanceId, string a_instanceAuthToken, string a_env)
        {
            if (!string.IsNullOrEmpty(a_instanceId) && !string.IsNullOrEmpty(a_env))
            {
                var manager = new WebAppInstanceSettingsManager(a_instanceId, a_instanceAuthToken, a_env);
                if (manager.EnsureConnection())
                {
                    return manager;
                }
            }
            else
            {
                return new DbInstanceSettingsManager(a_connectionString, a_name);
            }

            throw new ServerManagerException("4502", new object[] { a_name });
        }
    }
}
