using PT.Scheduler.Schedule.Demand;

namespace PT.Scheduler.Demand;

public partial class SalesOrder
{
    internal void ResetSimulationStateVariables()
    {
        for (int i = 0; i < SalesOrderLines.Count; ++i)
        {
            SalesOrderLine line = SalesOrderLines[i];
            line.ResetSimulationStateVariables();
        }
    }

    /// <summary>
    /// Add the lot codes used as eligible lots for sales order line distributions to a set of lot codes.
    /// </summary>
    /// <param name="a_eligibleLots"></param>
    internal void GetEligibleLotCodes(EligibleLots a_eligibleLots)
    {
        foreach (SalesOrderLine sol in SalesOrderLines)
        {
            foreach (SalesOrderLineDistribution sod in sol)
            {
                sod.AddEligibleLotCodes(a_eligibleLots);
            }
        }
    }
}