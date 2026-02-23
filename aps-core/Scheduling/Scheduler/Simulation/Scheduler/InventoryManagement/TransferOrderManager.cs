using PT.Scheduler.Schedule.Demand;

namespace PT.Scheduler.Demand;

public partial class TransferOrderManager
{
    /// <summary>
    /// Copy the EligibleLots of this transferOrder to an EligibleLots object.
    /// </summary>
    /// <param name="a_eligibleLots"></param>
    internal void AddEligibleLotCodes(EligibleLots a_eligibleLots)
    {
        IEnumerator<TransferOrder> etr = GetEnumerator();
        while (etr.MoveNext())
        {
            IEnumerator<TransferOrderDistribution> etrTod = etr.Current.Distributions.GetEnumerator();
            while (etrTod.MoveNext())
            {
                etrTod.Current.EligibleLots.AddEligibleLotCodes(a_eligibleLots);
            }
        }
    }

    internal void ResetSimulationStateVariables()
    {
        foreach (TransferOrder transferOrder in this)
        {
            transferOrder.ResetSimulationStateVariables();
        }
    }

    internal void SimulationActionComplete()
    {
        foreach (TransferOrder to in this)
        {
            foreach (TransferOrderDistribution dist in to.Distributions)
            {
                dist.SimulationActionComplete();
            }
        }
    }
}