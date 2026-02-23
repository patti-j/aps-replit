using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.Data
{
    /// <summary>
    /// Allows access for a single PT instance's settings data
    /// </summary>
    public interface IInstanceSettingsManager
    {

        #region Setup

        public void EnsureDbVersion();

        public void CreateInstanceConnectionFile(APSInstanceEntity a_instance, string a_connectionString);

        #endregion

        public bool EnsureConnection();

        public APSInstanceEntity GetInstance(string a_instanceName, string a_instanceVersion);
        public ServerWideInstanceSettings GetServerSettings();
        public void SaveInstance(APSInstanceEntity a_instance);
        public StartupVals GetStartupVals(string a_instanceName, string a_instanceVersion);

        public ErpDatabase GetErpDatabaseSettings(string a_instanceName, string a_instanceVersion);

        public string GetCertificateThumbprint(string a_instanceName, string a_instanceVersion);

        [Obsolete("The instance doesn't need this (it only consumed DiagnosticsEnabled and ServicePaths, now moved to startupvals) but can't be removed until all devs on hydrogen are using webapp")]
        public InstanceSettingsEntity GetInstanceSettingsEntity(string a_instanceName, string a_instanceVersion);

        public string GetServerManagerFolder(string a_instanceName, string a_instanceVersion);

    }
}
