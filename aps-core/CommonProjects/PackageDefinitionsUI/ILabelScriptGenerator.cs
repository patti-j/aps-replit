using PT.Scheduler;

namespace PT.PackageDefinitionsUI;

public interface ILabelScriptGenerator
{
    string GetLabelText(ScenarioDetail a_sd, Resource a_scheduledResource, InternalActivity a_internalActivity, string a_prioritySettingsScript);
    string GetLabelText(ScenarioDetail a_sd, StorageArea a_storageArea, string a_prioritySettingsScript);
}