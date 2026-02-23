using PT.APSCommon;
using PT.Scheduler;

namespace PT.PackageDefinitionsUI;

public interface IImpactAnalyzer
{
    /// <summary>
    /// Recalculates and stores all of the impact property values
    /// </summary>
    /// <param name="a_scenarioDetail"></param>
    void SimulationComplete(ScenarioDetail a_scenarioDetail);

    /// <summary>
    /// The data has been reset, likely due to an Undo/Redo or new scenario
    /// </summary>
    /// <param name="a_sd"></param>
    void ResetData(BaseId a_scenarioId);

    void InitializeScenario(ScenarioDetail a_sd);
}