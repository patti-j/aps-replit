namespace PT.Scheduler;

public partial class ItemManager
{
    /// <summary>
    /// Assign each item's StageArray index value to a unique value starting from 0.
    /// These values are used by ScenarioDetail as an index into an array for each item.
    /// </summary>
    internal void SimulationStageStageArrayInitialization()
    {
        for (int itemI = 0; itemI < Count; ++itemI)
        {
            GetByIndex(itemI).StageArrayIndex = itemI;
        }
    }

    /// <summary>
    /// Reset the simulation state variables of the items.
    /// </summary>
    internal void ResetSimulationStateVariables()
    {
        for (int itemI = 0; itemI < Count; ++itemI)
        {
            Item item = GetByIndex(itemI);
            item.ResetSimulationStateVariables();
        }
    }
}