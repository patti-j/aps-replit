// Enable the defintion to disable the changes made for this task. This can be deleted after testing is complete.
// The changes delay the release of operations whose material requirements eligible lots aren't available yet.
//#define tfstask10688Disable

using PT.APSCommon;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Specifies a material needed to perform an Operation.
/// Material Requirements are used for purchased materials and stocked manufactured materials (but not for Predecessor Manufacturing Orders).
/// Although it appears you can delete this class and collapse the functionality into MaterialRequirement you may end up needing it when you
/// update the code to specify the Source and Supply at the Activity level.
/// </summary>
public partial class MaterialRequirement
{
    private readonly MRSupply m_mrSupply;

    public MRSupply MRSupply => m_mrSupply;

    /// <summary>
    /// A simulation only value. The operation this MaterialRequirement is for.
    /// </summary>
    internal InternalOperation Operation { get; private set; }

    internal void ResetSimulationStateVariables(ScenarioDetail a_sd, BaseOperation a_operation)
    {
        m_simBools.Clear();
        Operation = (InternalOperation)a_operation;
        ResetUncomsumedIssuedQty();
        InternalOperation operation = a_operation as InternalOperation;

        if (operation != null && operation.Activities.Count > 1)
        {
            for (int activityI = 0; activityI < operation.Activities.Count; ++activityI)
            {
                InternalActivity activity = operation.Activities.GetByIndex(activityI);
                decimal fractionOfStartQty = activity.FractionOfStartQty;
                if (activity.InProductionOrPostProcessing)
                {
                    decimal materialRatio = TotalRequiredQty / operation.RequiredFinishQty;
                    decimal issuedQty = a_sd.ScenarioOptions.RoundQty(materialRatio * activity.RequiredFinishQty);
                    UnConsumedIssuedQty = UnConsumedIssuedQty - issuedQty;
                }
            }

            UnConsumedIssuedQty = Math.Max(0, UnConsumedIssuedQty);
        }

        m_mrSupply.ResetSimulationStateVariables();

        // Add MinAgeTicks to Item if there is one.
        if (ShelfLifeRequirement.MinAgeTicks > 0)
        {
            Item.AddMinAge(ShelfLifeRequirement.MinAgeTicks);
        }

        if (m_eligibleLots != null && m_eligibleLots.Count > 0)
        {
            // Don't wait on  eligiblelots if the operation is
            // in production or is IssuedComplete. In either
            // case there's no need to wait since the material 
            // must already be available. 
            if (!Operation.InProduction() && !IssuedComplete && NonConstraint && a_sd.ExtensionController.RunEligibleMaterialExtension)
            {
                WaitingOnEligibleLots = true;
            }
        }
    }
    //*******************************************************************
    // Issued Qty code:
    // Allocation and Consumption.
    // Allocate:step 1 of the scheduling Matl search processs. Try to find some unused issued quantity for the activity.
    //  activities get access to issued quantity on a first come first serve basis because Issued Qty isn't specified per activity.
    // Consume: step 2 of scheduling matl process. When an activity it scheduled, the issued qty allocated for it is consumed
    //  making it unavailable for other activities that need to schedule.
    //*******************************************************************

    private BoolVector32 m_simBools;

    private const int c_WaitingOnEligibleLotIdx = 0;

    /// <summary>
    /// This is true if this MaterialRequirement requires material from a specific lot
    /// and the lot hasn't become available yet.
    /// </summary>
    internal bool WaitingOnEligibleLots
    {
        // This is being used to disable the code changes for 10688.
        // To reenable them, delete this conditional and return
        // the line below.
        // Note, there are 2 conditionals like this in the project; the other being in ScenarioDetail.QtyToStockEventReceived
        get
        {
            #if tfstask10688Disable
return false;
            #else
            return m_simBools[c_WaitingOnEligibleLotIdx];
            #endif
        }
        set { m_simBools[c_WaitingOnEligibleLotIdx] = value; }
    }

    /// <summary>
    /// The amount of issued quantity that hasn't been consumed by a scheduled activity.
    /// </summary>
    internal decimal UnConsumedIssuedQty { get; private set; }

    /// <summary>
    /// Reset allocation issued quantity.
    /// This tracking is done while attempting to satisfy the material quantity requirments
    /// for an MR.
    /// </summary>
    internal void ResetUncomsumedIssuedQty()
    {
        UnConsumedIssuedQty = m_issuedQty;
    }

    /// <summary>
    /// Consume some issued qty for a scheduled activity.
    /// </summary>
    /// <param name="a_qty"></param>
    internal void ConsumeIssuedQty(decimal a_qty)
    {
        UnConsumedIssuedQty -= a_qty;
    }

    /// <summary>
    /// The amount of available issued quantity.
    /// </summary>
    internal decimal UnallocattedIssuedQty { get; private set; }

    /// <summary>
    /// Consume all of the remaining issued quantity. Call this version of the consume function if there will be no
    /// Remaining issued quantity left after allocating it to an activity.
    /// </summary>
    internal void AllocateRemainingIssuedQty()
    {
        UnallocattedIssuedQty = 0;
    }

    /// <summary>
    /// Prior to attempting to satisfy material requirements for an activity, this function must be called.
    /// </summary>
    internal void ResetUnallocatedIssuedQty()
    {
        UnallocattedIssuedQty = UnConsumedIssuedQty;
    }

    /// <summary>
    /// Consume some of the remaining issues quantity.
    /// </summary>
    /// <param name="a_qty"></param>
    internal void AllocateIssuedQty(decimal a_qty)
    {
        UnConsumedIssuedQty -= a_qty;
    }

    /// <summary>
    /// This value is based on evenly spreading the total required quantity across the total number of cycles that will be run. The total number of cycles
    /// is based on the Required Start Quantity.
    /// </summary>
    /// <param name="ro"></param>
    /// <returns></returns>
    internal decimal DetermineUsageQtyPerCycle(ResourceOperation a_ro, decimal a_nbrOfCycles)
    {
        decimal usageQtyPerCycle = TotalRequiredQty / a_nbrOfCycles;
        return usageQtyPerCycle;
    }

    internal void AdjustQty(decimal a_ratio, decimal a_newRequiredMOQty, ScenarioOptions a_so)
    {
        if (FixedQty)
        {
            return; // this MR is protected against adjustments
        }

        TotalRequiredQty = a_so.RoundQtyWithZeroCheck(TotalRequiredQty * a_ratio, a_newRequiredMOQty);
        TotalCost = TotalCost * a_ratio;
    }

    /// <summary>
    /// Whether a lot can be used to satisfy this material requirement.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <returns></returns>
    internal bool IsLotElig(Lot a_lot, EligibleLots a_fullPeggedLotsCollection)
    {
        EligibleLot el = m_eligibleLots.Get(a_lot.Code);

        bool containsEligibleLot = el != null;

        //This demand is pegged to the lot, use it
        if (containsEligibleLot)
        {
            return true;
        }

        //This lot is pegged to something, only the pegged demands can use it
        if (a_lot.LimitMatlSrcToEligibleLots)
        {
            return false;
        }

        EligibleLot fullRequirers = a_fullPeggedLotsCollection.Get(a_lot.Code);

        if (fullRequirers != null && fullRequirers.RequirerCount > 1)
        {
            //Some other demand still needs to use this lot
            return false;
        }

        //No other demands need this lot
        if (m_eligibleLots.Count > 0)
        {
            // This MR is limited to specific lot codes. The code of the lot must be
            // in the eligible lot set.
            return false;
        }

        //This doesn't have any required lots and no other demands require this lot code
        return true;
    }

    /// <summary>
    /// Whether the MaterialRequirement must be supplied by the lots specified as Eligible Lots.
    /// </summary>
    public bool MustUseEligLot => m_eligibleLots.Count > 0;

    /// <summary>
    /// Whether the lot can be used to satisfy this requirement.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool IsUsable(Lot a_lot, object args)
    {
        bool usable = true;
        //if (UsabilityRequirement != null)
        //{
        //    usable = UsabilityRequirement.IsUsable(a_lot, args);
        //}

        return usable;
    }

    /// <summary>
    /// Whether the MaterialRequirement has been released.
    /// </summary>
    internal bool Released
    {
        get
        {
            if (WaitingOnEligibleLots)
            {
                return false;
            }

            return true;
        }
    }

    public void LeadTimeEvent()
    {
        if (ConstraintType != MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate)
        {
            WaitingOnEligibleLots = false;
        }
    }

    internal void LinkAdjustment(MaterialRequirementAdjustment a_adjustment)
    {
        m_mrSupply.AddAdjustment(a_adjustment);
    }

    internal void SimulationActionComplete()
    {
        //Sort adjustments now so when accessed they don't sort and get collection modified exceptions
        MRSupply.SortAdjustments();
    }
}