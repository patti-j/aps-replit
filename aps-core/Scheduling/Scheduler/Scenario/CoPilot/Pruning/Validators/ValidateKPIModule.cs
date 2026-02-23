using PT.Common.Threading;
using PT.Transmissions;

namespace PT.Scheduler.CoPilot.Pruning.Validators;

internal class ValidateKpiModule : IPruneScenarioValidationModule
{
    private readonly ScenarioBaseT m_transmission;
    private Exception m_exception;
    private bool m_kpisFailed;
    private ValidatorManager.EValidationResult m_result;

    internal ValidateKpiModule(ScenarioBaseT a_transmission)
    {
        m_transmission = a_transmission;
    }

    public bool Initialize(Scenario a_scenario)
    {
        try
        {
            using (ObjectAccess<ScenarioEvents> se = a_scenario.ScenarioEventsLock.EnterWrite())
            {
                se.Instance.SimulationProgressEvent += new ScenarioEvents.SimulationProgressDelegate(HandleSimulationProgressEventInit);
            }

            a_scenario.Receive(m_transmission);
            using (ObjectAccess<ScenarioEvents> se = a_scenario.ScenarioEventsLock.EnterWrite())
            {
                se.Instance.SimulationProgressEvent -= new ScenarioEvents.SimulationProgressDelegate(HandleSimulationProgressEventInit);
            }

            return m_kpisFailed;
        }
        catch (Exception e)
        {
            m_exception = e;
            return false;
        }
    }

    private void HandleSimulationProgressEventInit(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simtype, ScenarioBaseT a_t, long a_simNbr, decimal a_percentcomplete, SimulationProgress.Status a_status)
    {
        if (a_status == SimulationProgress.Status.PostSimulationWorkComplete)
        {
            try
            {
                a_sd.Scenario.CalculateKPIs(a_sd, a_simtype, a_t);
                m_kpisFailed = false;
            }
            catch (Exception e)
            {
                m_exception = e;
                m_kpisFailed = true;
            }
        }
    }

    private void HandleSimulationProgressEventValidate(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simtype, ScenarioBaseT a_t, long a_simNbr, decimal a_percentcomplete, SimulationProgress.Status a_status)
    {
        if (a_status == SimulationProgress.Status.PostSimulationWorkComplete)
        {
            try
            {
                a_sd.Scenario.CalculateKPIs(a_sd, a_simtype, a_t);
                m_result = ValidatorManager.EValidationResult.NoError;
            }
            catch (Exception e)
            {
                if (e.GetType().FullName == m_exception.GetType().FullName)
                {
                    if (e.Message.StartsWith("ERROR ") && m_exception.Message.StartsWith("ERROR "))
                    {
                        //They both have a PT error code
                        if (e.Message.Substring(0, 12) == m_exception.Message.Substring(0, 12))
                        {
                            //Same error
                            if (m_exception.InnerException != null)
                            {
                                if (e.InnerException == null)
                                {
                                    m_result = ValidatorManager.EValidationResult.NewError;
                                }

                                //Both have an inner exception
                                if (m_exception.InnerException.Message == e.InnerException.Message)
                                {
                                    m_result = ValidatorManager.EValidationResult.ErrorValidated;
                                }
                            }
                        }
                    }
                    else
                    {
                        //This might be the best we can do. Possibly check stack trace, but that might not be consistent. Assume this is the same error if the messages match.
                        if (e.Message == m_exception.Message)
                        {
                            m_result = ValidatorManager.EValidationResult.ErrorValidated;
                        }
                    }
                }
                else
                {
                    m_result = ValidatorManager.EValidationResult.NewError;
                }
            }
        }
    }

    public ValidatorManager.EValidationResult Validate(Scenario a_scenario)
    {
        try
        {
            m_result = ValidatorManager.EValidationResult.Undefined;
            using (ObjectAccess<ScenarioEvents> se = a_scenario.ScenarioEventsLock.EnterWrite())
            {
                se.Instance.SimulationProgressEvent += new ScenarioEvents.SimulationProgressDelegate(HandleSimulationProgressEventValidate);
            }

            a_scenario.Receive(m_transmission);
            using (ObjectAccess<ScenarioEvents> se = a_scenario.ScenarioEventsLock.EnterWrite())
            {
                se.Instance.SimulationProgressEvent -= new ScenarioEvents.SimulationProgressDelegate(HandleSimulationProgressEventValidate);
            }

            return m_result;
        }
        catch (Exception e)
        {
            return ValidatorManager.EValidationResult.NewError;
        }
    }
}