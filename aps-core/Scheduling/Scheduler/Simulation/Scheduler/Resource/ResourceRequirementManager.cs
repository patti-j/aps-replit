using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Contains a list of ResourceRequirements
/// </summary>
public partial class ResourceRequirementManager
{
    #region Eligibility. Step 2. Eligibility across all resource requirements. That is, a filtering out of plants not able to satisfy all resource requirements.
    /// <summary>
    /// Step 2 of Eligibility. The resources that are eligible at the plant-operation level; where it can be made and what can satisfy the resource requirements.
    /// There is a one to one relationship between each resource requirement
    /// and each entry in this list.
    /// Each entry in this list describes the resources
    /// within a plant that are capable of satisfying the resource requirement.
    /// Across all the sets in this list every set contains entries
    /// for the exact same plants as every other set. So if you choose to
    /// satisfy any resource requirement from any plant in a set you can
    /// rest assured that there are adequate resources in that plant to satisfy
    /// all the resource requirements and the plants are eligible for the operation
    /// based on the MO's set of eligible plants.
    /// </summary>
    private readonly ResReqsPlantResourceEligibilitySets m_eligibleResources = new ();

    /// <summary>
    /// Step 2 of Eligibility. The resources that are eligible at the plant-operation level; where it can be made and what can satisfy the resource requirements.
    /// There is a one to one relationship between each resource requirement
    /// and each entry in this list.
    /// Each entry in this list describes the resources
    /// within a plant that are capable of satisfying the resource requirement.
    /// Across all the sets in this list every set contains entries
    /// for the exact same plants as every other set. So if you choose to
    /// satisfy any resource requirement from any plant in a set you can
    /// rest assured that there are adequate resources in that plant to satisfy
    /// all the resource requirements and the plants are eligible for the operation
    /// based on the MO's set of eligible plants.
    /// </summary>
    internal ResReqsPlantResourceEligibilitySets EligibleResources => m_eligibleResources;

    /// <summary>
    /// Step 2 of Eligibility.
    /// Determine the plants that are capable of satisfying all resource requirements.
    /// Make sure there are adequate numbers of resources within the plant.
    /// Invariants:
    /// 1. There must be at least 1 ResourceRequirement in this object.
    /// </summary>
    internal void CreateEffectiveResourceEligibilitySet(ProductRuleManager a_productRuleManager)
    {
        m_eligibleResources.Clear();

        // Determine eligibility across all resource requirements.
        // This is performing Step 1 of determining eligibility.
        foreach (ResourceRequirement rr in m_resourceRequirements)
        {
            rr.CreateResourceEligibilitySet(a_productRuleManager);
        }

        // These are the plants that are able to satisfy this set of requirements.
        BaseIdList effectivePlantIds = new ();

        // Populate the effectivePlants set with all the plants in the first ResourceRequirement.
        ResourceRequirement firstRR = GetByIndex(0);

        using (SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = firstRR.EligibleResources.GetEnumerator())
        {
            while (ersEtr.MoveNext())
            {
                BaseId plantId = ersEtr.Current.Key;
                effectivePlantIds.Add(plantId);
            }
        }

        // Filter the effective plant ids down to the plants that are able to satisfy all of the
        // resource requirements.
        foreach (ResourceRequirement rr in m_resourceRequirements)
        {
            for (int i = effectivePlantIds.Count - 1; i >= 0; i--)
            {
                BaseId effectivePlantId = effectivePlantIds.ElementAt(i);
                if (!rr.EligibleResources.Contains(effectivePlantId))
                {
                    effectivePlantIds.Remove(effectivePlantId);
                }
            }
        }

        // Filter the plant list down to the plants that have adequate numbers of required resources.
        // Some operations have multiple resource requirements. It is possible that an operation
        // will require more of a resource type than resources available.
        if (m_resourceRequirements.Count > 1)
        {
            for (int i = effectivePlantIds.Count - 1; i >= 0; i--)
            {
                BaseId effectivePlantId = effectivePlantIds.ElementAt(i);
                // Create an array of eligible resources for each resource requirement for the current plant.
                List<Tuple<ResourceRequirement, EligibleResourceSet>> eligibleResourcesForResReqAtPlant = new ();

                foreach (ResourceRequirement rr in m_resourceRequirements)
                {
                    EligibleResourceSet eligibleResources = rr.GetEligibleResourcesForPlant(effectivePlantId);
                    //Make a copy of the eligibility set so it is not modified when checking for adequate resources
                    EligibleResourceSet eligibleResourceSetCopy = new (eligibleResources);
                    Tuple<ResourceRequirement, EligibleResourceSet> t = new (rr, eligibleResourceSetCopy);
                    eligibleResourcesForResReqAtPlant.Add(t);
                }

                // Verify there are enough resources, taking resource requirement overlap nto consideration, to satisfy the 
                // requirements of this resource requirement.
                // Start from the requirement with the smallest number of eligible resources, pick off resources one by one
                // for each requirement to see if there are any shortages.
                if (!HasAdequateResources(eligibleResourcesForResReqAtPlant))
                {
                    effectivePlantIds.Remove(effectivePlantId);
                }
            }
        }

        // Compute eligibility across plants.
        // The first step is to eliminate plants that can't satisfy all the
        // resource requirements of this set of resource requirements.
        // The end result is an array with a corresponding entry for each resource requirement where
        // each element in the set defines the set of eligible resources in each plant.
        foreach (ResourceRequirement rr in m_resourceRequirements)
        {
            PlantResourceEligibilitySet effectivePlantResourceEligibilitySet = new ();
            foreach (BaseId effectivePlantId in effectivePlantIds)
            {
                EligibleResourceSet eligibleResourceSet = rr.EligibleResources[effectivePlantId];

                if (eligibleResourceSet != null)
                {
                    effectivePlantResourceEligibilitySet.Add(effectivePlantId, eligibleResourceSet);
                }
            }

            m_eligibleResources.Add(effectivePlantResourceEligibilitySet);
        }
    }

    /// <summary>
    /// Requirements of an operation may vy for the same resources. You can use this function to determine whether a plant has adequate
    /// resource to satisfy all its resource requirements.
    /// invariants:
    /// 1. Every Eligible Resource set contains at least 1 resource.
    /// </summary>
    /// <param name="eligibleResourcesForResReqAtPlant">An array of eligible resource sets.</param>
    /// <returns>false if the plant lacks adequate resources</returns>
    private bool HasAdequateResources(List<Tuple<ResourceRequirement, EligibleResourceSet>> a_eligibleResourcesForResReqAtPlant)
    {
        while (a_eligibleResourcesForResReqAtPlant.Count > 1)
        {
            int minEligibleResourceIdx = FindIndexOfResourceRequirementWithSmallestNumberOfEligibleResources(a_eligibleResourcesForResReqAtPlant);
            EligibleResourceSet eligibleResourceSet = a_eligibleResourcesForResReqAtPlant[minEligibleResourceIdx].Item2;
            ResourceRequirement rr1 = a_eligibleResourcesForResReqAtPlant[minEligibleResourceIdx].Item1;
            InternalResource claimedResource = null;

            // Check whether any Infinite resource can satisfy this requirement.
            if (eligibleResourceSet.ContainsInfiniteCapacityResource() || eligibleResourceSet.ContainsMultitaskingCapacityResource())
            {
                // Do nothing.
            }
            else
            {
                // Claim the first resource;The choice doesn't matter; can be any.
                claimedResource = eligibleResourceSet[0];
            }

            a_eligibleResourcesForResReqAtPlant.RemoveAt(minEligibleResourceIdx);

            // Check whether any of the resource requirements have zero resources left.
            for (int eligibleResourcesForResReqAtPlantI = 0; eligibleResourcesForResReqAtPlantI < a_eligibleResourcesForResReqAtPlant.Count; ++eligibleResourcesForResReqAtPlantI)
            {
                eligibleResourceSet = a_eligibleResourcesForResReqAtPlant[eligibleResourcesForResReqAtPlantI].Item2;
                ResourceRequirement rr2 = a_eligibleResourcesForResReqAtPlant[eligibleResourcesForResReqAtPlantI].Item1;

                // Remove the claimed resource from the set of eligible resource (it may or may not be a part of the eligible set).
                if (claimedResource != null)
                {
                    if (MainResourceDefs.Usage.Intersects(rr1.UsageStart, rr1.UsageEnd, rr2.UsageStart, rr2.UsageEnd))
                    {
                        eligibleResourceSet.Remove(claimedResource);
                    }
                }

                if (eligibleResourceSet.Count == 0)
                {
                    // Inadequate resources.
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Given an array of eligible resource sets find the index of the resource with the smallest number of eligible resources.
    /// </summary>
    /// <param name="eligibleResourcesForResReqAtPlant"></param>
    /// <returns>The index of the array element with the least number of eligible resources in it. In the event of a tye the index of the first tying resource requirement is returned.</returns>
    private int FindIndexOfResourceRequirementWithSmallestNumberOfEligibleResources(List<Tuple<ResourceRequirement, EligibleResourceSet>> a_eligibleResourcesForResReqAtPlant)
    {
        int minIdx = int.MaxValue;
        int min = int.MaxValue;

        for (int i = 0; i < a_eligibleResourcesForResReqAtPlant.Count; ++i)
        {
            EligibleResourceSet eligibleResourceSet = a_eligibleResourcesForResReqAtPlant[i].Item2;

            if (eligibleResourceSet.Count < min)
            {
                min = eligibleResourceSet.Count;
                minIdx = i;
            }
        }

        return minIdx;
    }
    #endregion

    #region Similarity
    public int SimilarityComparison(ResourceRequirementManager a_rrm)
    {
        if (Count < a_rrm.Count)
        {
            return -1;
        }

        if (Count > a_rrm.Count)
        {
            return 1;
        }

        int v;

        for (int i = 0; i < Count; ++i)
        {
            if ((v = GetByIndex(i).SimilarityComparison(a_rrm.GetByIndex(i))) != 0)
            {
                return v;
            }
        }

        return 0;
    }

    internal void DetermineDifferences(ResourceRequirementManager a_rrm, System.Text.StringBuilder a_warnings)
    {
        if (Count != a_rrm.Count)
        {
            a_warnings.Append("the operations being compared have different numbers of resource requirements;");
            return;
        }

        for (int rrI = 0; rrI < Count; ++rrI)
        {
            ResourceRequirement rr = GetByIndex(rrI);
            ResourceRequirement compareRR = a_rrm.FindIdenticalRR(rr);

            if (compareRR == null)
            {
                a_warnings.Append("the resource requirements don't match;");
                return;
            }
        }
    }

    private ResourceRequirement FindIdenticalRR(ResourceRequirement a_rr)
    {
        ResourceRequirement rr = null;

        for (int i = 0; i < Count; ++i)
        {
            rr = GetByIndex(i);

            if (a_rr.SimilarityComparison(rr) == 0)
            {
                return rr;
            }
        }

        return null;
    }

    internal void ClearEffectiveResourceEligibilitySet()
    {
        m_eligibleResources.Clear();
        foreach (ResourceRequirement rr in m_resourceRequirements)
        {
            rr.EligibleResources.Clear();
        }
    }
    #endregion

    /// <summary>
    /// Returns true if there is more than one type of UsageStart among the ResourceRequirements.
    /// </summary>
    /// <returns></returns>
    internal bool HasMixOfUsageStarts()
    {
        MainResourceDefs.usageEnum firstUsage = GetByIndex(0).UsageStart;

        for (int i = 1; i < Count; ++i)
        {
            ResourceRequirement rr = GetByIndex(i);
            if (rr.UsageStart != firstUsage)
            {
                return true;
            }
        }

        return false;
    }
}