using PT.Transmissions;

namespace PT.UIDefinitions.Interfaces;

public interface IAutoSaveSettingsManager
{
    void AddPendingChanges(string a_instigatingEntityName, Func<PTTransmission> a_autoSaveDelegate);
}