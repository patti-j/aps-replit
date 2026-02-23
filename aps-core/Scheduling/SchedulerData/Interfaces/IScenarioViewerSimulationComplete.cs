using PT.Scheduler;
using PT.Transmissions;

namespace SchedulerData.Interfaces;

public delegate void SimulationCompleteHandler(ScenarioBaseT a_t, ScenarioDetail a_sd);

public interface IScenarioViewerSimulationComplete
{
    event SimulationCompleteHandler SimulationComplete;
}