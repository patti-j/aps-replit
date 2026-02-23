namespace PT.Scheduler.Demand;

public partial class SalesOrderLine
{
    internal void ResetSimulationStateVariables()
    {
        for (int i = 0; i < LineDistributions.Count; ++i)
        {
            SalesOrderLineDistribution dist = LineDistributions[i];
            dist.ResetSimulationStateVariables();
        }
    }
}