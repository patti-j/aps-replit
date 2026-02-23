namespace PT.Scheduler;

public partial class StorageAreaConnectorCollection
{
    internal void ResetSimulationStateVariables()
    {
        foreach (StorageAreaConnector connector in this)
        {
            connector.ResetSimulationStateVariables();
        }
    }

    internal void ResetAllocation()
    {
        foreach (StorageAreaConnector connector in this)
        {
            connector.FlowRangeConstraint.ResetAllocation();
        }
    }
}

