using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.Data
{
    /// <summary>
    /// Manages instance-level settings for all instances across the server.
    /// This is expected to require privileges outside of what an individual instance can provide.
    /// </summary>
    public interface IMultiInstanceSettingsManager : IInstanceSettingsManager
    {
        public bool DeleteInstance(string a_instanceIdentifier, string a_instanceName, string a_instanceVersion);

        public bool DeleteAllInstances();

        IEnumerable<APSInstanceEntity> GetInstances();

        public void SaveInstances(IEnumerable<APSInstanceEntity> a_instances);

        public void SaveServerwideSettings(ServerSettingsDto a_settings);

        public void SetAsBackup(APSInstanceEntity newVersion, APSInstanceEntity oldVersion);

        //public void BackUpSettings(string a_backupPostfix);

    }
}
