using PT.APSCommon;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

public interface IPublishHelper
{
    void InitializeErrorReporting(ISystemLogger a_errorReporter);
    void QueueCopiedScenarioForExport(Scenario a_s, ScenarioDetailExportT a_t);
    PublishStatusMessage GetCurrentPublishStatus(BaseId a_scenarioId);
}