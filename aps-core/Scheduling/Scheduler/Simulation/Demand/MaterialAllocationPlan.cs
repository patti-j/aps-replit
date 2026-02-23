using PT.SchedulerDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Scheduler.Simulation.Demand;

internal class MaterialAllocationPlan
{
    private readonly StorageAreaConnector m_connector;
    private List<StorageArea> m_storageAreas = new();
    private List<ItemStorage> m_orderedItemStorage = new();
    private long m_connectorAvailableTicks;
    internal long RetryDate => m_connectorAvailableTicks;

    internal StorageAreaConnector Connector => m_connector;

    internal MaterialAllocationPlan()
    {

    }

    internal MaterialAllocationPlan(StorageAreaConnector a_connector, long a_connectorAvailableTicks)
    {
        m_connector = a_connector;
        m_connectorAvailableTicks = a_connectorAvailableTicks;
    }

    internal MaterialAllocationPlan(StorageAreaConnector a_connector)
    {
        m_connector = a_connector;
    }

    internal bool Empty => m_storageAreas.Count == 0;

    internal List<ItemStorage> AvailableStorage => m_orderedItemStorage;

    internal void AddStorageArea(StorageArea a_storageArea)
    {
        m_storageAreas.Add(a_storageArea);
    }

    internal decimal MaxAvailableQty;

    internal bool HasExactSupplySource;

    internal void OrderStorageAreas(BaseDemandProfile a_materialDemand, ScenarioDetail a_sd)
    {
        List<ItemStorage> orderedStorageAreas = new();
        // Create a list of eligible tank inventories and quantities available at the simulation clock.
        List<ItemStorage> exactSupplySources = new List<ItemStorage>();

        a_materialDemand.ResetForAllocation();
        foreach (StorageArea storageArea in m_storageAreas)
        {
            decimal totalAvailableQty = storageArea.CalculateAvailableQtyForDemand(a_materialDemand, a_sd);
            if (totalAvailableQty == a_materialDemand.RemainingQty)
            {
                exactSupplySources.Add(storageArea.Storage[a_materialDemand.Item.Id]);
                HasExactSupplySource = true;
                //Update the total available qty
                MaxAvailableQty += totalAvailableQty;
                continue;
            }

            if (totalAvailableQty >= a_materialDemand.RemainingQty || (totalAvailableQty > 0 && a_materialDemand.AllowPartialSupply))
            {
                orderedStorageAreas.Add(storageArea.Storage[a_materialDemand.Item.Id]);
                //Update the total available qty
                MaxAvailableQty += totalAvailableQty;
            }
        }

        //TODO: Improve performance of Sort so it doesn't have to do so many lookups
        //Sort the supplying tanks based on the demands material allocation
        switch (a_materialDemand.MaterialAllocation)
        {
            case ItemDefs.MaterialAllocation.NotSet:
                break;
            case ItemDefs.MaterialAllocation.UseOldestFirst:
                // Sort the tank resource by processing finish date so tanks that were completed first are allocated first. 
                // This frees them up to handle additional work and prevents material from being unused for a long time.
                // Before this change a customer was experiencing tank blocks that extended to the end of the planning horizon.
                // Other tanks that were producing the same required material that appeared earlier in the tank list and were
                // always being selected first so the later tanks never had their material used.
                exactSupplySources = exactSupplySources.OrderBy(itemStorage => itemStorage.QtyProfile.First.Date).ThenBy(storage => storage.Inventory.LeadTimeTicks).ToList();
                orderedStorageAreas = orderedStorageAreas.OrderBy(itemStorage => itemStorage.QtyProfile.First.Date).ThenBy(storage => storage.Inventory.LeadTimeTicks).ToList();
                break;
            case ItemDefs.MaterialAllocation.UseNewestFirst:
                // Sort the tank resource by processing finish date so tanks that were completed last are allocated first. 
                // This respects the demands preference. For example using newer material to avoid using remainders
                exactSupplySources = exactSupplySources.OrderByDescending(itemStorage => itemStorage.QtyProfile.Last.Date).ThenBy(storage => storage.Inventory.LeadTimeTicks).ToList();
                orderedStorageAreas = orderedStorageAreas.OrderByDescending(itemStorage => itemStorage.QtyProfile.Last.Date).ThenBy(storage => storage.Inventory.LeadTimeTicks).ToList();

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        exactSupplySources.AddRange(orderedStorageAreas);
        m_orderedItemStorage = exactSupplySources;
    }

    internal void AllocatePlan(BaseDemandProfile a_materialDemand, ScenarioDetail a_sd)
    {
        bool someQtyAllocated;
        do
        {
            someQtyAllocated = false;
            //We have at least one storage area available.
            // Use the first tank that has enough material.
            foreach (ItemStorage itemStorage in AvailableStorage)
            {
                //This is where we perform the actual allocation on the Supply and Demand profiles.
                //The demand profile should track the supply source/type of the qty it gets from the supply node
                //It continues through available SAs until it finds enough material, or has run out of sources
                someQtyAllocated |= itemStorage.AllocateSupply(a_materialDemand, a_sd);

                if (a_materialDemand.Satisfied)
                {
                    return;
                }

                if (!a_materialDemand.AllowMultiStorageAreaSupply)
                {
                    return;
                }
            }
        } while (someQtyAllocated);
    }

    internal long CalculateAvailableDate(BaseDemandProfile a_materialDemand, ScenarioDetail a_sd, out BaseSupplyProfile o_testAvailableProfile)
    {
        a_materialDemand.ResetForAllocation();

        long earliestMaterialAvailableDate = PTDateTime.MaxDateTimeTicks;

        o_testAvailableProfile = new BaseSupplyProfile();
        foreach (StorageArea storageArea in m_storageAreas)
        {
            ItemStorage storage = storageArea.Storage[a_materialDemand.Item];

            //Copy to the test profile
            foreach (QtySupplyNode node in storage.QtyProfile)
            {
                QtySupplyNode copyNode = new (node);
                o_testAvailableProfile.Insert(copyNode);
            }
        }

        //Find when this demand could be satisfied, ignoring the demand node dates
        long availableDate = o_testAvailableProfile.CalculateAvailableDateForDemand(a_materialDemand, a_sd);
        if (availableDate != PTDateTime.InvalidDateTime.Ticks)
        {
            earliestMaterialAvailableDate = Math.Min(availableDate, earliestMaterialAvailableDate);
        }

        if (earliestMaterialAvailableDate != PTDateTime.MaxDateTimeTicks)
        {
            return earliestMaterialAvailableDate;
        }

        return PTDateTime.InvalidDateTime.Ticks;
    }
}

