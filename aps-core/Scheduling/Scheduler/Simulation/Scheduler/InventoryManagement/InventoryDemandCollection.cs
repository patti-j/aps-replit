using System.Collections;

using PT.APSCommon;
using PT.Common.Debugging;

namespace PT.Scheduler.Simulation.Scheduler.InventoryManagement;

/// <summary>
/// A collection of all material allocations. Each set of allocations is separated by demand.
/// For example if there are multiple material requirements in an operation, each mr will have it's own set of allocations.
/// Each allocation represents a material source for the demand. Multiple allocations for a single mr means there are multiple sources to supply the required material.
/// </summary>
internal class InventoryDemandCollection : IEnumerable<InventoryDemandAllocations>
{
    //TODO: Convert to dictionary
    private readonly List<InventoryDemandAllocations> m_demandAllocationsCollection = new ();

    internal void Add(InventoryAllocation a_newAllocation)
    {
        foreach (InventoryDemandAllocations demandAllocations in m_demandAllocationsCollection)
        {
            if (demandAllocations.DemandId == a_newAllocation.MR.Id)
            {
                //Add a new allocation for this demand
                demandAllocations.Add(a_newAllocation);
                return;
            }
        }

        //Add a new DemandAllocation
        InventoryDemandAllocations newDemand = new ();
        newDemand.Add(a_newAllocation);
        m_demandAllocationsCollection.Add(newDemand);
    }

    internal void Add(InventoryDemandAllocations a_newAllocation)
    {
        #if DEBUG
        if (m_demandAllocationsCollection.Any(x => x.DemandId == a_newAllocation.DemandId))
        {
            throw new DebugException("Why are we adding duplicates?");
        }
        #endif
        m_demandAllocationsCollection.Add(a_newAllocation);
    }

    internal void Clear()
    {
        foreach (InventoryDemandAllocations demandAllocations in m_demandAllocationsCollection)
        {
            demandAllocations.Clear();
        }

        m_demandAllocationsCollection.Clear();
    }

    internal void ReplaceAllocations(InventoryDemandAllocations a_replacementAllocation)
    {
        foreach (InventoryDemandAllocations demandAllocations in m_demandAllocationsCollection)
        {
            if (demandAllocations.DemandId == a_replacementAllocation.DemandId)
            {
                demandAllocations.Clear();
                foreach (InventoryAllocation allocation in a_replacementAllocation)
                {
                    demandAllocations.Add(allocation);
                }

                break;
            }
        }
    }

    internal bool Empty => m_demandAllocationsCollection.Count == 0;

    internal int Count => m_demandAllocationsCollection.Count;

    public IEnumerator<InventoryDemandAllocations> GetEnumerator()
    {
        return m_demandAllocationsCollection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public InventoryDemandAllocations GetAllocationsByDemand(BaseId a_mrId)
    {
        foreach (InventoryDemandAllocations demandAllocations in m_demandAllocationsCollection)
        {
            if (demandAllocations.DemandId == a_mrId)
            {
                return demandAllocations;
            }
        }

        throw new DebugException("Missing demand allocations");
    }

    public InventoryDemandAllocations GetOnlyDemandAllocations()
    {
        #if DEBUG
        if (m_demandAllocationsCollection.Count != 1)
        {
            throw new DebugException("There are not exactly one set of allocations");
        }
        #endif

        return m_demandAllocationsCollection[0];
    }

    public bool RemoveDemandAllocations(BaseId a_mrId)
    {
        for (int i = m_demandAllocationsCollection.Count - 1; i >= 0; i--)
        {
            if (m_demandAllocationsCollection[i].DemandId == a_mrId)
            {
                m_demandAllocationsCollection.RemoveAt(i);
                return true;
            }
        }

        return false;
    }
}

internal class InventoryDemandAllocations : IEnumerable<InventoryAllocation>
{
    internal BaseId DemandId;
    private readonly List<InventoryAllocation> m_demandAllocations = new ();

    internal void Add(InventoryAllocation a_newAllocation)
    {
        #if DEBUG
        if (m_demandAllocations.Count > 0 && DemandId != a_newAllocation.MR.Id)
        {
            throw new DebugException("Invalid allocation");
        }
        #endif
        DemandId = a_newAllocation.MR.Id;
        m_demandAllocations.Add(a_newAllocation);
    }

    internal void Clear()
    {
        m_demandAllocations.Clear();
    }

    public InventoryAllocation GetLastAllocation()
    {
        return m_demandAllocations[^1];
    }

    internal BaseIdObject GetDemand()
    {
        //This is generic so that this class can be used for other demands (like SOs) in the future
        return m_demandAllocations[0].MR;
    }

    internal MaterialRequirement GetMrDemand()
    {
        return m_demandAllocations[0].MR;
    }

    public IEnumerator<InventoryAllocation> GetEnumerator()
    {
        return m_demandAllocations.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}