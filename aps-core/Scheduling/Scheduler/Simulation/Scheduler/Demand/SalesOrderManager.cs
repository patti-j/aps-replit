using PT.Scheduler.Schedule.Demand;

namespace PT.Scheduler.Demand;

public partial class SalesOrderManager
{
    internal List<SalesOrderLineDistribution> GetDistributionsList()
    {
        //Add all the line distributions to a list sorted by priority and sub-sorted by date that aren't on hold or closed. Set default Warehouse or make null.
        List<SalesOrderLineDistribution> distributions = new ();

        for (int soI = 0; soI < Count; ++soI)
        {
            SalesOrder so = this[soI];
            if (!so.Cancelled)
            {
                for (int lineI = 0; lineI < so.SalesOrderLines.Count; ++lineI)
                {
                    SalesOrderLine line = so.SalesOrderLines[lineI];

                    for (int detailI = 0; detailI < line.LineDistributions.Count; ++detailI)
                    {
                        SalesOrderLineDistribution dist = line.LineDistributions[detailI];
                        if (!dist.Closed && !dist.Hold)
                        {
                            distributions.Add(dist);
                        }
                    }
                }
            }
        }

        distributions.Sort();
        return distributions;
    }

    internal void ResetSimulationStateVariables()
    {
        for (int i = 0; i < Count; ++i)
        {
            SalesOrder so = this[i];
            so.ResetSimulationStateVariables();
        }
    }

    /// <summary>
    /// Add all the lot codes of lots used as eligible lots to a HashSet<string>
    /// </summary>
    /// <param name="">The hashset to.</param>
    internal void AddEligibleLotCodes(EligibleLots a_eligibleLotCodes)
    {
        IEnumerator<SalesOrder> etr = GetEnumerator();
        while (etr.MoveNext())
        {
            etr.Current.GetEligibleLotCodes(a_eligibleLotCodes);
        }
    }

    internal void SimulationActionComplete()
    {
        foreach (SalesOrder salesOrder in this)
        {
            foreach (SalesOrderLine line in salesOrder.SalesOrderLines)
            {
                foreach (SalesOrderLineDistribution dist in line)
                {
                    dist.SimulationActionComplete();
                }
            }
        }
    }
}