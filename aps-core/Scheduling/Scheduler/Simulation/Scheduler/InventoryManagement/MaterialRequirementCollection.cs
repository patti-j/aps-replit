using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of MaterialRequirements.
/// </summary>
public partial class MaterialRequirementsCollection
{
    internal void ResetSimulationStateVariables(ScenarioDetail a_sd, BaseOperation a_operation)
    {
        m_waitingOnMaterial = false;
        m_materialConstraintsEventScheduled = false;

        for (int mrI = 0; mrI < Count; ++mrI)
        {
            MaterialRequirement mr = this[mrI];
            mr.ResetSimulationStateVariables(a_sd, a_operation);
        }
    }

    private bool m_waitingOnMaterial;

    /// <summary>
    /// This is set to true if the operation is being constrained by material.
    /// </summary>
    internal bool WaitingOnMaterial
    {
        get => m_waitingOnMaterial;

        set => m_waitingOnMaterial = value;
    }

    private bool m_materialConstraintsEventScheduled;

    /// <summary>
    /// This value is set to true the first time an activity of an operation requests that material constraints be taken into consideration.
    /// </summary>
    internal bool MaterialConstraintsEventScheduled
    {
        get => m_materialConstraintsEventScheduled;

        set => m_materialConstraintsEventScheduled = value;
    }

    internal void IgnoreConstraintsForFrozenSpan()
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].SetConstraintIgnoredForFrozenSpan();
        }
    }

    internal void RestoreConstraintsAfterFrozenSpan()
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].RestoreConstraintAfterFrozenSpan();
        }
    }

    /// <summary>
    /// Obtain the release time of the latest constraining material requirement.
    /// </summary>
    /// <param name="now">The scheduler's current clock.</param>
    /// <returns>The latest release time of constraining material requirements.</returns>
    internal long GetLastestConstrainingBuyDirectMaterialRequirement(long a_now)
    {
        long latestReleaseDate;
        GetLastestConstrainingBuyDirectMaterialRequirement(a_now, out latestReleaseDate);
        return latestReleaseDate;
    }

    /// <summary>
    /// Obtain the latest material requirement release date among all non-issued material requirements.
    /// </summary>
    /// <param name="now">The current system clock time.</param>
    /// <param name="latestBuyDirectReleaseDate">The calculated release time of the latest buy direct material constraint.</param>
    /// <returns>null if there are no non-issued material constraints that are of type Constraint or ConfirmedConstraint; otherwise the most constraining material is returned.</returns>
    internal MaterialRequirement GetLastestConstrainingBuyDirectMaterialRequirement(long a_now, out long o_latestBuyDirectReleaseDate)
    {
        o_latestBuyDirectReleaseDate = 0;
        MaterialRequirement latestMR = null;

        for (int i = 0; i < Count; ++i)
        {
            MaterialRequirement mr = this[i];

            if (mr.RequirementType == MaterialRequirementDefs.requirementTypes.BuyDirect && !mr.IssuedComplete)
            {
                switch (mr.ConstraintType)
                {
                    case MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate:
                        if (mr.LatestSourceDate > o_latestBuyDirectReleaseDate)
                        {
                            o_latestBuyDirectReleaseDate = mr.LatestSourceDate;
                            latestMR = mr;
                        }

                        break;

                    case MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate:
                    {
                        long leadTimeAvailablility = mr.LeadTimeSpan.Ticks + a_now;
                        long earliestMaterialAvailableTime = Math.Min(leadTimeAvailablility, mr.LatestSourceDate);
                        if (earliestMaterialAvailableTime > o_latestBuyDirectReleaseDate)
                        {
                            o_latestBuyDirectReleaseDate = earliestMaterialAvailableTime;
                            latestMR = mr;
                        }
                    }
                        break;
                }
            }
        }

        return latestMR;
    }

    internal MaterialRequirement GetLastestConstrainingMaterialRequirement(long a_now)
    {
        long latestReleaseDate;
        MaterialRequirement mr = GetLastestConstrainingBuyDirectMaterialRequirement(a_now, out latestReleaseDate);
        return mr;
    }

    internal void AdjustQtys(decimal a_ratio, decimal a_newRequiredMOQty, ScenarioOptions a_so)
    {
        for (int i = 0; i < Count; ++i)
        {
            MaterialRequirement mr = this[i];
            mr.AdjustQty(a_ratio, a_newRequiredMOQty, a_so);
        }
    }

    /// <summary>
    /// This should be called prior to attempting to allocate material for an activity.
    /// </summary>
    internal void IssuedQtyAllocationInitialization()
    {
        foreach (MaterialRequirement mr in m_materialRequirementList)
        {
            mr.ResetUnallocatedIssuedQty();
        }
    }

    /// <summary>
    /// Whether any MaterialRequirements are waiting on EligibleLots.
    /// MaterialRequirements with EligibleLots requirements are released
    /// after the material with the EligibleLot code becomes available.
    /// </summary>
    /// <returns></returns>
    internal bool HasReqWithEligLots()
    {
        bool ret = false;
        foreach (MaterialRequirement mr in this)
        {
            if (mr.WaitingOnEligibleLots)
            {
                ret = true;
                break;
            }
        }

        return ret;
    }

    /// <summary>
    /// Whether all the MaterialRequirements have been released.
    /// </summary>
    internal bool Released
    {
        get
        {
            foreach (MaterialRequirement mr in this)
            {
                if (!mr.Released)
                {
                    return false;
                }
            }

            if (WaitingOnMaterial)
            {
                return false;
            }

            return true;
        }
    }
}
