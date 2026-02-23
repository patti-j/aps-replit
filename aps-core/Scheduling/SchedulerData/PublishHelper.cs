using PT.APSCommon;
using PT.Scheduler;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.SchedulerData;

public class PublishHelper : IPublishHelper
{
    private readonly Publisher m_publisher;

    public PublishHelper(InstanceSettingsEntity a_instanceSettingsEntity, StartupVals a_startupVals)
    {
        m_publisher = new Publisher(a_instanceSettingsEntity, a_startupVals);
    }

    /// <summary>
    /// The error reporter is created during the initialization of the PTSystem object; this PublishHelper is created before that and cannot be moved with current dependencies.
    /// Thus, this must be added after construction.
    /// </summary>
    /// <param name="a_errorReporter"></param>
    public void InitializeErrorReporting(ISystemLogger a_errorReporter)
    {
        m_publisher.InitializeErrorReporting(a_errorReporter);
    }

    public void QueueCopiedScenarioForExport(Scenario a_s, ScenarioDetailExportT a_t)
    {
        m_publisher.AddPublish(a_s, a_t);
    }

    public PublishStatusMessage GetCurrentPublishStatus(BaseId a_scenarioId)
    {
        m_publisher.m_publishStatusMessages.TryGetValue(a_scenarioId, out PublishStatusMessage publishStatusForScenario);

        return publishStatusForScenario ?? new PublishStatusMessage(a_scenarioId);
    }
}