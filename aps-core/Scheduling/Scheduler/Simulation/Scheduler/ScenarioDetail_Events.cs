using PT.Transmissions;

namespace PT.Scheduler;

public partial class ScenarioDetail
{
    private void FireSimulationValidationFailureEvent(SimulationValidationException a_e, ScenarioBaseT a_t)
    {
        if (a_t != null)
        {
            ScenarioEvents se;

            using (_scenario.AutoEnterScenarioEvents(out se))
            {
                se.FireSimulationValidationFailureEvent(this, a_e, a_t);
            }
        }
    }

    private void FireExportScenarioEvent(ScenarioDetailExportT a_t)
    {
        ScenarioEvents se;
        using (_scenario.AutoEnterScenarioEvents(out se))
        {
            ScenarioSummary ss;
            using (_scenario.AutoEnterScenarioSummary(out ss))
            {
                se.FireExportScenarioEvent(this, a_t, ss);
            }
        }
    }

    private void FireCapacityIntervalsPurgedEvent(DateTime a_purgeTime)
    {
        ScenarioEvents se;
        using (_scenario.AutoEnterScenarioEvents(out se))
        {
            se.FireCapacityIntervalsPurgedEvent(a_purgeTime);
        }
    }

    #region EXPEDITE
    private void ExpediteComplete(ScenarioBaseT a_T)
    {
        m_simulationProgress.PostSimulationWorkComplete();
    }

    /// <summary>
    /// Perform some handling of a SimulationValidationException. Update SimulationProgress and fire event.
    /// </summary>
    private void HandleExpediteValidationException(ScenarioBaseT a_T, SimulationType simType, SimulationValidationException e)
    {
        SimulationProgress.FireSimulationProgressEvent(this, simType, a_T, SimulationProgress.Status.Terminated, m_nbrOfSimulationsSinceStartup);
        FireSimulationValidationFailureEvent(e, a_T);
    }

    /// <summary>
    /// Perform some handling of an Exception that occurred during an expedite. Update SimulationProgress.
    /// </summary>
    private void HandleExpediteException(ScenarioBaseT a_T, SimulationType simType)
    {
        SimulationProgress.FireSimulationProgressEvent(this, simType, a_T, SimulationProgress.Status.Exception, m_nbrOfSimulationsSinceStartup);
    }
    #endregion
}