using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Common.Extensions;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.APIs;

internal class TriggerExport : ImportExportActionBase
{
    internal ApsWebServiceResponseBase BeginExport(long a_scenarioId, EExportDestinations a_publishDestination, BaseId a_instigator, ApiLogger a_apiLogger)
    {
        try
        {
            m_scenarioId = BaseId.NULL_ID;
            while (true)
            {
                try
                {
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 5000))
                    {
                        if (a_scenarioId == long.MinValue)
                        {
                            Scenario liveScenario = sm.GetFirstProductionScenario();
                            m_scenarioId = liveScenario.Id;

                            using (liveScenario.AutoEnterScenarioEvents(out ScenarioEvents se))
                            {
                                se.PublishStatusEvent += SeOnPublishStatusEvent;
                            }
                        }
                        else
                        {
                            //Publish by scenario id
                            for (int i = 0; i < sm.LoadedScenarioCount; i++)
                            {
                                Scenario s = sm.GetByIndex(i);
                                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                                {
                                    if (ss.Id.ToBaseType() == a_scenarioId)
                                    {
                                        m_scenarioId = ss.Id;
                                        using (s.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                                        {
                                            se.PublishStatusEvent += SeOnPublishStatusEvent;
                                        }
                                    }
                                }
                            }
                        }

                        if (m_scenarioId == BaseId.NULL_ID)
                        {
                            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.InvalidScenarioId);
                        }

                        break;
                    }
                }
                catch (AutoTryEnterException e)
                {
                    //Do Nothing
                    Thread.Sleep(100);
                }
            }

            ScenarioDetailExportT exportT = new (m_scenarioId, a_publishDestination);
            exportT.Instigator = a_instigator;
            string broadcastArgs = $"ScenarioId: {exportT.scenarioId} | Instigator = {exportT.Instigator}";
            a_apiLogger.LogBroadcast("exportT", broadcastArgs);

            SystemController.ClientSession.SendClientAction(exportT);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.SuccessWithoutValidation);
        }
        catch (Exception e)
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Failure, e.GetExceptionFullMessage());
        }
    }

    private void SeOnPublishStatusEvent(PublishStatusMessageT a_t)
    {
        if (a_t is PublishStatusMessageT messageT)
        {
            lock (m_dataLock) //Need to lock here because the await loop could check status at any time.
            {
                if (messageT.ProgressStep == PublishStatuses.EPublishProgressStep.Complete || messageT.ProgressStep == PublishStatuses.EPublishProgressStep.Canceled || messageT.ProgressStep == PublishStatuses.EPublishProgressStep.Error)
                {
                    m_finished = true;
                    m_started = false;
                }
                else if (messageT.ProgressStep == PublishStatuses.EPublishProgressStep.Started)
                {
                    m_finished = false;
                    m_started = true;
                    m_exceptions = null;
                }

                if (messageT.Exceptions.Count > 0)
                {
                    m_exceptions = messageT.Exceptions;
                }
            }
        }
    }

    protected override void DeInitializeListeners()
    {
        if (m_scenarioId != BaseId.NULL_ID)
        {
            while (true)
            {
                try
                {
                    using (SystemController.Sys.AutoEnterRead(m_scenarioId, out Scenario s))
                    {
                        using (s.AutoEnterScenarioEvents(out ScenarioEvents se))
                        {
                            se.PublishStatusEvent -= SeOnPublishStatusEvent;
                            return;
                        }
                    }
                }
                catch (AutoTryEnterException)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}