using PT.APSCommon;

namespace PT.Scheduler.Simulation.Scheduler.InventoryManagement;

/// <summary>
/// This tracks the on-hand and expected allocations during the material allocation phase for a specific demand.
/// Most demands will have only a single InventoryAllocationSim to track allocated qty. In the case where a mr has
/// multiple material requirements for the same item/inventory, this allows tracking the individual allocations while attempting to find
/// material required for all MRs.
/// </summary>
internal class InventoryAllocationSimCollection
{
    private readonly Dictionary<BaseId, InventoryAllocationSim> m_simAllocations = new ();

    internal void Combine(InventoryAllocationSimCollection a_otherAllocations)
    {
        foreach ((BaseId key, InventoryAllocationSim value) in a_otherAllocations.m_simAllocations)
        {
            if (m_simAllocations.TryGetValue(key, out InventoryAllocationSim existingAllocation))
            {
                existingAllocation.Combine(value);
            }
            else
            {
                m_simAllocations.Add(key, value);
            }
        }
    }

    internal InventoryAllocationSim GetOrCreateNew(BaseId a_demandId)
    {
        if (m_simAllocations.TryGetValue(a_demandId, out InventoryAllocationSim existingAllocations))
        {
            return existingAllocations;
        }

        InventoryAllocationSim newAllocation = new (a_demandId);
        m_simAllocations.Add(a_demandId, newAllocation);
        return newAllocation;
    }

    internal bool ResetForDemand(BaseId a_demandId)
    {
        return m_simAllocations.Remove(a_demandId);
    }
}

internal class InventoryAllocationSim
{
    /// <summary>
    /// The quantity allocated against on-hand inventory and expected on-hand up to the simulation clock.
    /// </summary>
    internal decimal QtyAllocated;

    /// <summary>
    /// The quantity allocated from material expected to be available or from overlap (in the future quantity profile).
    /// </summary>
    internal decimal QtyAllocatedFromExpected;

    internal BaseId DemandId;

    internal InventoryAllocationSim(BaseId a_demandId)
    {
        DemandId = a_demandId;
    }

    public void Combine(InventoryAllocationSim a_otherAllocations)
    {
        QtyAllocated += a_otherAllocations.QtyAllocated;
        QtyAllocatedFromExpected += a_otherAllocations.QtyAllocatedFromExpected;
    }

    public void Reset()
    {
        QtyAllocated = 0m;
        QtyAllocatedFromExpected = 0m;
    }
}