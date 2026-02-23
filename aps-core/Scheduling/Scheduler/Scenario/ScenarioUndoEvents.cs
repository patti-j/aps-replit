using PT.APSCommon;
using PT.Transmissions;

namespace PT.Scheduler;

public class ScenarioUndoEvents
{
    public delegate void UndoSetChangedDelegate(BaseId a_scenarioId, ScenarioBaseT a_T);

    public event UndoSetChangedDelegate UndoSetChangedEvent;

    internal void FireUndoSetChangedEvent(BaseId a_scenarioId, ScenarioBaseT a_T)
    {
        UndoSetChangedEvent?.Invoke(a_scenarioId, a_T);
    }

    public delegate void UndoBeginDelegate(BaseId a_scenarioId, bool a_isUndo, string a_description);

    public event UndoBeginDelegate UndoBeginEvent;

    internal void FireUndoBeginEvent(BaseId a_scenarioId, bool a_isUndo, string a_description)
    {
        UndoBeginEvent?.Invoke(a_scenarioId, a_isUndo, a_description);
    }

    public delegate void UndoEndDelegate(ScenarioDetail a_sd, UserManager a_userManager, bool a_success = true);

    public event UndoEndDelegate UndoEndEvent;

    internal void FireUndoEndEvent(ScenarioDetail a_sd, UserManager a_userManager, bool a_success)
    {
        UndoEndEvent?.Invoke(a_sd, a_userManager, a_success);
    }
}