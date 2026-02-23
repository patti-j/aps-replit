using PT.Scheduler.MRP;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.Customizations.MRP;

public static class MrpExtensionDefaults
{
    public static DateTime? ExistingOrderNeedDateResetDefault(ScenarioDetail a_sd, Job a_existingJob, decimal a_supplyQty, decimal a_unallocatedQty, MrpDemand a_demandAdjustment, DateTime a_demandAdjustmentDate)
    {
        if (a_existingJob.NeedDateTime != a_demandAdjustmentDate)
        {
            if (a_existingJob.Commitment != JobDefs.commitmentTypes.Released && a_existingJob.Commitment != JobDefs.commitmentTypes.Firm && a_unallocatedQty == a_supplyQty) //Material has not been allocated to another demand
            {
                return a_demandAdjustmentDate;
            }
        }

        return null;
    }

    public static bool CanExistingSupplySatisfyDemand(ScenarioDetail a_sd, Job a_potentialSupplyJob, DateTime a_demandAdjustmentDate)
    {
        if ((a_potentialSupplyJob.Commitment == JobDefs.commitmentTypes.Released || a_potentialSupplyJob.Commitment == JobDefs.commitmentTypes.Firm) && a_potentialSupplyJob.NeedDateTime > a_demandAdjustmentDate)
        {
            //This is a released or firm job. It's possible that it is late and can't satisfy the demand, however if it is planned (ie NeedDate) before the required date, peg this job anyways
            return false;
        }

        return true;
    }
}