using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.Common.PTMath;
using PT.Scheduler.Demand;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

using static System.Runtime.InteropServices.JavaScript.JSType;
using static PT.ERPTransmissions.WarehouseT;
using static PT.SchedulerDefinitions.MainResourceDefs;

namespace PT.Scheduler;

/// <summary>
/// A location where inventories are stored.  Each Warehouse holds Inventories for various Items and can be accessed by various Plants.
/// </summary>
public partial class Warehouse
{
    /// <summary>
    /// Reset any allocations for before attempting to schedule a new activity
    /// </summary>
    internal void ResetAllocation()
    {
        m_storageAreaConnectors.ResetAllocation();
        m_storageAreas.ResetAllocation();
    }

    /// <summary>
    /// Consumes and allocates any remainder inventory
    /// </summary>
    internal void ZeroInventory(long a_simClock, ScenarioDetail a_sd, BaseIdObject a_reason)
    {
        //TODO: Storage
        using IEnumerator<Inventory> etr = Inventories.GetEnumerator();
        while (etr.MoveNext())
        {
            Inventory inv = etr.Current;
            //inv.ZeroInventory(a_simClock, a_sd, a_reason);
        }
    }

    internal virtual void ResetSimulationStateVariables(long a_clock, ScenarioDetail a_sd)
    {
        Inventories.ResetSimulationStateVariables(a_clock, a_sd);
        foreach (StorageArea storageArea in m_storageAreas)
        {
            storageArea.ResetSimulationStateVariables();
        }

        m_storageAreaConnectors.ResetSimulationStateVariables();
    }

    internal void SimulationInitialization(long a_clockDate, ref List<QtyToStockEvent> r_retryEvents)
    {
        m_storageAreas.SimulationInitialization(m_storageAreaConnectors);
        m_inventories.SimulationInitialization(a_clockDate, ref r_retryEvents);
    }

    internal void SimulationComplete()
    {
        m_storageAreas.SimulationComplete();
    }

    internal void AllocateForecasts()
    {
        Inventories.AllocateForecasts();
    }

    /// <summary>
    /// This function should be called after all changes to demands have been completed by a transmission.
    /// [DaysOnHand:Synchronization:3]
    /// </summary>
    /// <param name="a_sm"></param>
    internal void ProcessDemandChanges(SalesOrderManager a_sm)
    {
        Inventories.ProcessDemandChanges(a_sm);
    }

    internal Lot StoreOperationProducts(long a_simClock, InternalActivity a_act, Product a_product, Resource a_primaryResource, SupplyProfile a_productQtyProfile, List<EventBase> a_newEvents)
    {
        // The inventory where the product will be stored. This could be in a warehouse or a tank.
        Inventory inventory = Inventories[a_product.Item.Id];

        //Create Lot first so we can add adjustments when the product is stored
        Lot newProductionLot = inventory.CreateLot(inventory, a_productQtyProfile, a_product.PartialProductionLotId);

        a_productQtyProfile.StoreProduct(a_act, a_simClock, a_product, newProductionLot, a_newEvents);
        //TODO: Use storage area connectors to filter out available storage. 

        return newProductionLot;
    }

    /// <summary>
    /// Receive material from a transfer order.
    /// </summary>
    /// <param name="a_tod">The transfer of material.</param>
    /// <param name="a_qty">The amount of material going into inventory.</param>
    internal Lot StoreTransferOrder(TransferOrderDistribution a_tod, decimal a_qty, long a_simClock, ScenarioOptions a_roundingOptions)
    {
        TransferOrderSupplyProfile toSupply = new (a_tod, a_qty, a_tod.ToInventory);

        decimal toQty = toSupply.RemainingQty;

        QtySupplyNode newNode = toSupply.AddToEnd(a_tod.ScheduledReceiveDateTicks, a_tod, toQty);

        if (a_tod.ToStorageArea != null)
        {
            ItemStorage storage = a_tod.ToStorageArea.Storage[a_tod.ToInventory.Item.Id];
            (decimal availableQty, decimal availableQtyAfterDisposal) = storage.CalculateRemainingStorageCapacity(toSupply.Last.Date);

            return StoreTransferOrderInSa(a_tod, a_simClock, a_roundingOptions, toQty, toSupply, availableQtyAfterDisposal, availableQty, newNode, storage);
        }
        else
        {
            List<(ItemStorage sa, decimal availableQty, decimal availableQtyAfterDisposal)> availableStorage = new ();
            foreach (StorageArea storageArea in a_tod.ToWarehouse.StorageAreas)
            {
                if (storageArea.CanStoreItem(a_tod.Item.Id))
                {
                    ItemStorage storage = storageArea.Storage[a_tod.Item.Id];
                    (decimal availableQty, decimal availableQtyAfterDisposal) = storage.CalculateRemainingStorageCapacity(toSupply.Last.Date);
                    if (availableQty > 0 || availableQtyAfterDisposal > 0)
                    {
                        availableStorage.Add((storage, availableQty, availableQtyAfterDisposal));
                    }
                }
            }

            (ItemStorage sa, decimal bestAvailableQty, decimal bestAvailableQtyAfterDisposal) = availableStorage.OrderByDescending(s => s.availableQty).ThenByDescending(s => s.availableQtyAfterDisposal).First();
            return StoreTransferOrderInSa(a_tod, a_simClock, a_roundingOptions, toQty, toSupply, bestAvailableQtyAfterDisposal, bestAvailableQty, newNode, sa);
        }
    }

    private Lot StoreTransferOrderInSa(TransferOrderDistribution a_tod, long a_simClock, ScenarioOptions a_roundingOptions, decimal a_toQty, TransferOrderSupplyProfile a_toSupply, decimal a_availableQtyAfterDisposal, decimal a_availableQty, QtySupplyNode a_newNode, ItemStorage a_storage)
    {
        decimal qtyToStore;
        if (a_tod.OverrideStorageConstraint)
        {
            //Store it anyways
            qtyToStore = a_toQty;
        }
        else
        {
            if (a_toSupply.RequireEmptyStorageArea && !a_tod.ToStorageArea.Empty)
            {
                //Check for disposal
                if (a_availableQtyAfterDisposal > 0)
                {
                    //we can discard the current qty first
                    //TODO: storage.DisposeAll(a_simClock, a_pts);
                    if (a_availableQtyAfterDisposal <= a_toQty)
                    {
                        decimal qtyToDiscard = a_toQty - a_availableQtyAfterDisposal;
                        a_toSupply.DiscardExcess(a_simClock, a_tod, qtyToDiscard);
                        qtyToStore = a_toQty - qtyToDiscard;
                    }
                    else
                    {
                        //Otherwise store full qty
                        qtyToStore = a_toQty;
                    }
                }
                else
                {
                    //Discard everything, no room
                    a_toSupply.DiscardExcess(a_simClock, a_tod, a_toQty);
                    qtyToStore = 0m;
                }
            }
            else
            {
                if (a_availableQty < a_toQty)
                {
                    //We are constrained by space, discard the excess but create an excess adjustment
                    decimal excess = a_toQty - a_availableQty;
                    a_toSupply.DiscardExcess(a_simClock, a_tod, excess);
                    qtyToStore = a_availableQty;
                }
                else
                {
                    //Store everything
                    qtyToStore = a_toQty;
                }
            }
        }


        if (!a_roundingOptions.IsApproximatelyZero(qtyToStore))
        {
            //Create Lot first so we can add adjustments when the product is stored
            Lot purchaseLot = a_tod.ToInventory.CreateLot(a_tod.ToInventory, a_toSupply, BaseId.NULL_ID);
            a_newNode.SetLot(purchaseLot);
            a_storage.StoreProduct(a_tod, a_newNode, qtyToStore, a_tod);
            return purchaseLot;
            //TODO: Disposal
        }

        return null;
    }
    

    internal bool AllowsUnconstrainedStorage(Item a_item)
    {
        foreach (StorageArea storageArea in StorageAreas)
        {
            ItemStorage itemStorage = storageArea.Storage[a_item];
            if (itemStorage != null)
            {
                if (itemStorage.MaxQty <= 0)
                {
                    //This one is not constrained
                    return true;
                }
            }
        }

        return false;
    }

    internal struct StorageAllocationResult
    {
        private readonly HashSet<ItemStorage> m_retryStorageAreas;
        private readonly HashSet<ItemStorage> m_usableItemStorage;
        internal bool Success;
        internal long RetryDate;
        internal bool Retry => RetryDate > 0 && RetryDate < PTDateTime.MaxDateTimeTicks - 2;
        internal HashSet<ItemStorage> RetryStorageAreas => m_retryStorageAreas;
        internal HashSet<ItemStorage> UsableItemStorage => m_usableItemStorage;

        //TODO: This is not an ideal solution
        //Special constant to indicate retry at next online retry
        internal static long RetryAtNextOnlineRetryConst = PTDateTime.MaxDateTimeTicks - 1;
        /// <summary>
        /// Whether to ignore the specific time, and just retry at the next online time (don't remove from dispatchers)
        /// </summary>
        internal bool RetryAtNextOnline => RetryDate == RetryAtNextOnlineRetryConst;

        internal StorageAllocationResult(HashSet<ItemStorage> a_retryStorageAreas, HashSet<ItemStorage> a_usableItemStorage)
        {
            m_retryStorageAreas = a_retryStorageAreas;
            m_usableItemStorage = a_usableItemStorage;
        }

        internal static StorageAllocationResult SuccessResult = new StorageAllocationResult() { Success = true };
    }

    internal StorageAllocationResult AllocateStorage(ProductSupplyProfile a_supplyProfile, Resource a_primaryResource, ScenarioDetail a_sd, InternalActivity a_act, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ref bool r_moQtyChanged)
    {
        //TODO: Loop through all SA and calculate how much qty can be stored there.
        //TODO: Return the max qty we can store across all available SAs.
        //TODO: Somehow check for true/false, could be outside this function. If so, rename

        (List<StoragePlan> storagePlans, HashSet<ItemStorage> retrySAs, HashSet<ItemStorage> usableItemStorage) = GenerateStoragePlans(a_supplyProfile, a_primaryResource, a_act, a_si, a_sd.SimClock, out long minSaAvailableDate);

        StorageAllocationResult failResult = new (retrySAs, usableItemStorage) { RetryDate = minSaAvailableDate };
        if (storagePlans.Count == 0)
        {
            return failResult;
        }

        //If any plan is an exact match, allocate that
        foreach (StoragePlan plan in storagePlans)
        {
            if (plan.AvailableQty == a_supplyProfile.RemainingQty)
            {
                plan.AllocatePlan(a_supplyProfile);
                return StorageAllocationResult.SuccessResult;
            }
        }

        //If any plan is an exact match after drainage, allocate that
        foreach (StoragePlan plan in storagePlans)
        {
            if (plan.AvailableQtyIfDrained == a_supplyProfile.RemainingQty)
            {
                plan.AllocatePlan(a_supplyProfile);
                return StorageAllocationResult.SuccessResult;
            }
        }

        //If any plan is unconstrained, allocate that
        foreach (StoragePlan plan in storagePlans)
        {
            if (plan.FullyAvailable)
            {
                plan.AllocatePlan(a_supplyProfile);
                return StorageAllocationResult.SuccessResult;
            }
        }

        //Store the earliest connector conflict. Even if another plan has a connector, it may not be usable for another reason
        if (storagePlans.Where(s => s.RetryDate > 0).MinBy(s => s.RetryDate) is StoragePlan retryPlan)
        {
            failResult.RetryDate = Math.Min(retryPlan.RetryDate, minSaAvailableDate);
        }

        //Verify and remove any invalid plans
        // If all plans are invalid, use the first available date to try again
        StoragePlan[] availablePlans = storagePlans.Where(p => p.RetryDate == 0).ToArray();
        if (availablePlans.Length != storagePlans.Count)
        {
            if (availablePlans.Length == 0)
            {
                //All invalid
                return failResult;
            }
        }

        //If none of the storage plans have an exact fit and the MO is configured to resize, use the Resizing feature.
        //Use Available plans going forward, since there is at least one
        //TODO: Add a setting for resize up since it could schedule without resizing.
        if (a_act.ManufacturingOrder.ResizeForStorage)
        {
            ProductRule pr = a_sd.ProductRuleManager.GetProductRule(a_primaryResource.Id, a_supplyProfile.Inventory.Item.Id, a_act.Operation.ProductCode);

            decimal maxQty = a_primaryResource.MaxQty;
            bool useMaxQuantity = a_primaryResource.MaxQuantityConstrained;
            if (pr != null && pr.UseMaxQty)
            {
                maxQty = pr.MaxQty;
                useMaxQuantity = pr.MaxQty != 0m && pr.MaxQty != decimal.MaxValue;
            }

            //1. No resource max -> Resize up to plan with the largest MaxAvailableQty
            StoragePlan maxQtyPlan = availablePlans.MaxBy(s => s.AvailableQty);
            decimal resizeQty = maxQtyPlan.AvailableQty;
            if (useMaxQuantity)
            {
                //2. If resource max -> Resize up to max of resource and largest plan available qty\
                resizeQty = Math.Min(resizeQty, maxQty);

                if (resizeQty == a_act.ManufacturingOrder.RequiredQty)
                {
                    maxQtyPlan.AllocatePlan(a_supplyProfile);
                    return StorageAllocationResult.SuccessResult;
                }
            }

            decimal productQty = a_act.Operation.Products.PrimaryProduct.TotalOutputQty;

            decimal resizeRatio = resizeQty / productQty;

            if (resizeRatio != 1m)
            {
                //resize up
                if (resizeQty > productQty && a_act.CanResizeForStorage(a_sd, resizeQty, a_primaryResource, a_si, a_ccp))
                {
                    if (!a_act.ManufacturingOrder.AdjustQtyRequiredByRatioForStorage(resizeRatio, a_primaryResource, a_sd.ProductRuleManager))
                    {
                        a_act.ManufacturingOrder.AdjustToOriginalQty(null, a_sd.ProductRuleManager);
                        return failResult;
                    }

                    r_moQtyChanged = true; //We resized, retry allocate
                    return StorageAllocationResult.SuccessResult;
                }

                //3. If largest maxAvailableQty > qty to schedule, no resize, use the plan
                if (resizeQty >= a_supplyProfile.RemainingQty)
                {
                    maxQtyPlan.AllocatePlan(a_supplyProfile);
                    return StorageAllocationResult.SuccessResult;
                }

                //Try resizing down
                decimal minQty = a_primaryResource.MinQty;
                bool useMinQty = a_primaryResource.MinQuantityConstrained; //Only split if there is a min value being used. 0 is not set

                if (pr != null && pr.UseMinQty)
                {
                    minQty = pr.MinQty;
                    useMinQty = pr.MinQty != 0m && pr.MinQty != decimal.MinValue;
                }

                if (useMinQty)
                {
                    //5. If largest MaxAvailableQty < min, don't resize, don't allocate
                    if (resizeQty < minQty)
                    {
                        return failResult;
                    }
                }

                //6. If resource min -> Resize down to largest maxAvailableQty.
                if (a_act.CanResizeForStorage(a_sd, resizeQty, a_primaryResource, a_si, a_ccp))
                {
                    if (!a_act.ManufacturingOrder.AdjustQtyRequiredByRatioForStorage(resizeRatio, a_primaryResource, a_sd.ProductRuleManager))
                    {
                        a_act.ManufacturingOrder.AdjustToOriginalQty(null, a_sd.ProductRuleManager);
                        return failResult;
                    }

                    r_moQtyChanged = true;
                    return StorageAllocationResult.SuccessResult;
                }

                //6. Return false if no allocation
                return failResult;
            }
        }

        decimal maxAvailableQty = availablePlans.Max(s => Math.Max(s.AvailableQty, s.AvailableQtyIfDrained));
        if (maxAvailableQty < a_supplyProfile.RemainingQty)
        {
            //Although some storage areas may be available, there is no combination to supply the full required qty.
            return failResult;
        }

        //Default to the largest available
        StoragePlan defaultPlan = availablePlans.MaxBy(p => p.AvailableQty);
        if (a_supplyProfile.RemainingQty >= defaultPlan.AvailableQty)
        {
            // we have to drain first.
            //TODO: Pick best plan?
            defaultPlan = availablePlans.MaxBy(p => p.AvailableQtyIfDrained);
        }

        defaultPlan.AllocatePlan(a_supplyProfile);

        return StorageAllocationResult.SuccessResult;
    }

    /// <summary>
    /// Generates a set of storage plans and identifies storage areas that are available or require retry for a given
    /// product supply profile and resource at a specified clock date.
    /// </summary>
    /// <remarks>This method evaluates available storage connectors and areas based on the provided supply
    /// profile and resource constraints. It prioritizes connectors and considers both single and multiple connector
    /// scenarios to maximize storage allocation opportunities. The method does not guarantee that any plan is feasible;
    /// callers should validate the returned plans as needed.</remarks>
    /// <param name="a_supplyProfile">The product supply profile containing requirements and constraints for storage allocation.</param>
    /// <param name="a_primaryResource">The primary resource for which storage plans are to be generated.</param>
    /// <param name="a_activity">The internal activity associated with the storage allocation process.</param>
    /// <param name="a_si">The schedulable information used to determine required capacity and allocation timing.</param>
    /// <param name="a_clockDate">The clock date, in ticks, representing the point in time for evaluating storage availability.</param>
    /// <param name="o_minRetryDate">When this method returns, contains the earliest date, in ticks, at which a retry for storage allocation may be
    /// attempted. Set to <see langword="long.MaxValue"/> if no retry is possible.</param>
    /// <returns>A tuple containing a list of feasible storage plans, a set of item storage areas that require retry, and a set
    /// of item storage areas that are currently usable. The returned collections may be empty if no suitable storage
    /// plans or areas are found.</returns>
    private (List<StoragePlan>, HashSet<ItemStorage>, HashSet<ItemStorage>) GenerateStoragePlans(ProductSupplyProfile a_supplyProfile, Resource a_primaryResource, InternalActivity a_activity, SchedulableInfo a_si, long a_simClock, out long o_minRetryDate)
    {
        o_minRetryDate = long.MaxValue;
        List<ItemStorage.StorageAvailability> availableStorage = new ();

        //TODO: Order the connectors by best storage fit. We need to TestAllocate SA on each connector. 1 at a time, then by combination until enough material is found

        HashSet<ItemStorage> retrySAs = new ();
        HashSet<ItemStorage> usableItemStorage = new ();
        //First step is to prioritize connectors
        List<StoragePlan> availablePlans = new ();
        List<StorageAreaConnector> storageAreaConnectors = m_storageAreaConnectors.GetStorageConnectorsForResourceIn(a_primaryResource).ToList();
        if (storageAreaConnectors.Count < 2) //This means we have a simple plan with no combinations
        {
            StoragePlan storagePlan;
            if (storageAreaConnectors.Count == 0)
            {
                //No connectors, so no constraints
                if (a_supplyProfile.RequiredStorageArea != null)
                {
                    CalcStorageAreaAvailability(a_supplyProfile, a_supplyProfile.RequiredStorageArea, availableStorage, a_primaryResource, retrySAs, usableItemStorage, ref o_minRetryDate);
                }
                else
                {
                    //Check all storage areas
                    foreach (StorageArea storageArea in StorageAreas)
                    {
                        CalcStorageAreaAvailability(a_supplyProfile, storageArea, availableStorage, a_primaryResource, retrySAs, usableItemStorage, ref o_minRetryDate);
                    }
                }

                if (availableStorage.Count != 0)
                {
                    storagePlan = new StoragePlan(availableStorage);
                    availablePlans.Add(storagePlan); //We will check later if this plan is feasible
                }
            }
            else /*storageAreaConnectors.Count == 1*/
            {
                //We have a single plan for storage. 
                StorageAreaConnector connector = storageAreaConnectors[0];
                if (connector.FlowRangeConstraint.VerifyAllocationRange(a_supplyProfile.GetUsageRange(), out long intersectionEndTicks))
                {
                    if (a_supplyProfile.RequiredStorageArea != null)
                    {
                        if (connector.StorageAreasIn.Contains(a_supplyProfile.RequiredStorageArea)) //If this has a required SA not for this connector, skip it
                        {
                            CalcStorageAreaAvailability(a_supplyProfile, a_supplyProfile.RequiredStorageArea, availableStorage, a_primaryResource, retrySAs, usableItemStorage, ref o_minRetryDate);
                        }
                    }
                    else
                    {
                        foreach (StorageArea storageArea in connector.StorageAreasIn)
                        {
                            CalcStorageAreaAvailability(a_supplyProfile, storageArea, availableStorage, a_primaryResource, retrySAs, usableItemStorage, ref o_minRetryDate);
                        }
                    }

                    if (availableStorage.Count != 0)
                    {
                        storagePlan = new StoragePlan(availableStorage, connector);
                        availablePlans.Add(storagePlan); //We will check later if this plan is feasible
                    }
                }
                else
                {
                    availablePlans.Add(new StoragePlan(connector, intersectionEndTicks)); //We will check later if this plan is feasible
                }
            }
        }
        else
        {
            //There are multiple connectors. This means we have multiple plans for 
            foreach (StorageAreaConnector connector in storageAreaConnectors)
            {
                if (!connector.FlowRangeConstraint.VerifyAllocationRange(a_supplyProfile.GetUsageRange(), out long intersectionEndTicks))
                {
                    //This connector is allocated for another storage transfer and cannot be used for this one.
                    availablePlans.Add(new StoragePlan(connector, intersectionEndTicks)); //We will check later if this plan is feasible
                    continue;
                }

                availableStorage.Clear();

                if (a_supplyProfile.RequiredStorageArea != null)
                {
                    if (connector.StorageAreasIn.Contains(a_supplyProfile.RequiredStorageArea)) //If this has a required SA not for this connector, skip it
                    {
                        CalcStorageAreaAvailability(a_supplyProfile, a_supplyProfile.RequiredStorageArea, availableStorage, a_primaryResource, retrySAs, usableItemStorage, ref o_minRetryDate);
                    }
                }
                else
                {
                    //Check all storage areas
                    foreach (StorageArea storageArea in connector.StorageAreasIn)
                    {
                        CalcStorageAreaAvailability(a_supplyProfile, storageArea, availableStorage, a_primaryResource, retrySAs, usableItemStorage, ref o_minRetryDate);
                    }
                }

                if (availableStorage.Count != 0)
                {
                    StoragePlan storagePlan = new StoragePlan(availableStorage, connector);
                    availablePlans.Add(storagePlan); //We will check later if this plan is feasible
                }

                //Although some storage areas may be available, there is no combination to supply the full required qty.
            }
        }
        
        return (availablePlans, retrySAs, usableItemStorage);
    }

    private static void SetEarliestRetryDate(long a_retryDate, ref long r_existingRetryDate)
    {
        if (r_existingRetryDate == long.MaxValue || a_retryDate > r_existingRetryDate)
        {
            r_existingRetryDate = a_retryDate;
        }
    }

    private static EStorageAvailabilityResult CalcStorageAreaAvailability(SupplyProfile a_supplyProfile, StorageArea a_storageArea, List<ItemStorage.StorageAvailability> a_availableStorage, InternalResource a_primaryResource, HashSet<ItemStorage> a_retrySAs, HashSet<ItemStorage> a_usableItemStorage, ref long r_minRetryDate)
    {
        if (!a_storageArea.Storage.TryGetValue(a_supplyProfile.Inventory.Item.Id, out ItemStorage itemStorage))
        {
            //This storage area cannot store this item
            return EStorageAvailabilityResult.NoItemStorage;
        }

        IInterval storageInterval = a_supplyProfile.GetUsageRange();
        if (!a_storageArea.ValidateInFlow(storageInterval, out long saAvailableTicks))
        {
            if (saAvailableTicks > 0)
            {
                SetEarliestRetryDate(saAvailableTicks, ref r_minRetryDate);
            }
            return EStorageAvailabilityResult.NotAvailable;
        }

        if (a_availableStorage.Any(a => a.ItemStorage == itemStorage))
        {
            //This storage area is already being used by another connector
            return EStorageAvailabilityResult.NotAvailable;
        }

        ItemStorage.StorageAvailability availability = itemStorage.InitStorageAvailability(a_supplyProfile.Last.Date, true);

        //Requires empty storage check
        //TODO: Make sure we drain at the last qty node, not the time of scheduling. No need to keep the material around.
        if (a_supplyProfile.RequireEmptyStorageArea && !a_storageArea.Empty)
        {
            bool validate = true;
            ItemStorage lastStorage = a_storageArea.LastStorage;
            if (lastStorage.Item == a_supplyProfile.Inventory.Item)
            {
                //Note, it's possible that due to expiration, the LastStorage is empty and will not have nodes.
                if (lastStorage.QtyProfile.HasNodes && lastStorage.QtyProfile.Last.LastQtyNode.SourceLot.Source is Product lastProduct && 
                    a_supplyProfile.Source is Product thisProduct)
                {
                    if (lastProduct == thisProduct)
                    {
                        //The same product is already stored. However we much check if a split can store in the same SA
                        if (a_supplyProfile.AllowSameLotInNonEmptyStorageArea // (operation setting allows it)
                            || lastStorage.QtyProfile.Last.LastQtyNode.SourceLot.LotSource == ItemDefs.ELotSource.PartialProduction) // Or it's not a split, and it's previous production from this source
                        {

                            //This supply has already stored in this SA, so we don't need to constraint they empty SA constraint.
                            validate = false;
                        }
                    }
                }
            }

            if (validate)
            {
                //We can't store here by default, check for drainage of the stored item
                foreach (ItemStorage storedItem in a_storageArea.GetStoredItems())
                {
                    ItemStorage.StorageAvailability currentStorageAvailability = storedItem.InitStorageAvailability(a_supplyProfile.Last.Date, false);
                    if (!currentStorageAvailability.CanBeDrained && currentStorageAvailability.CurrentQty != 0m)
                    {
                        //Can't use this SA, the current storage cannot be drained
                        a_retrySAs.AddIfNew(itemStorage);
                        return EStorageAvailabilityResult.RequiresEmptyStorageArea;
                    }

                    availability.RequiresDrainingStorage(currentStorageAvailability);
                }
            }
        }

        //TODO: Calculate cleanout and make sure it fits between last usage/empty and first supply date.
        //TODO: Allocate the cleanout so we can create adjustments if the activity is scheduled.

        //SingleItemStorage check
        if (!a_storageArea.CanAddStorage(a_supplyProfile))
        {
            //We can't store here by default, check for drainage of the stored item
            //There can be only one
            ItemStorage currentStorage = a_storageArea.GetSingleStoredItem();
            ItemStorage.StorageAvailability currentStorageAvailability = currentStorage.InitStorageAvailability(a_supplyProfile.Last.Date, false);
            if (!currentStorageAvailability.CanBeDrained && currentStorageAvailability.CurrentQty != 0m)
            {
                //Can't use this SA, the current storage cannot be drained
                a_retrySAs.AddIfNew(itemStorage);
                return EStorageAvailabilityResult.RequiresSingleItemStorage;
            }

            availability.RequiresDrainingStorage(currentStorageAvailability);
        }
        else if (a_supplyProfile.RemainingQty > availability.QtyAvailable && a_supplyProfile.RemainingQty <= availability.QtyAvailableIfDrained)
        {
            //TODO: This use case may not really make sense
            //We need to drain (only this ItemStorage) first.
            availability.RequiresDrainingStorage(availability);
        }

        //Check whether the SA cleanout if sa is now empty or requires drainage
        if (a_storageArea.Resource != null && (availability.CurrentQty == 0 || availability.RequiresDisposal))
        {
            //First determine if we need to incur a cleanout between these products. If so, determine if the required cleanout can fit.
            if (a_storageArea.Resource.ItemCleanoutTable != null && a_storageArea.LastStorage != null && a_storageArea.LastStorage.QtyProfile.HasNodes)
            {
                //Determine the previously run item
                string lastStoredItemExternalId = a_storageArea.LastStorage.Inventory.Item.ExternalId;
                string itemExternalIdToStore = a_supplyProfile.Inventory.Item.ExternalId;

                a_storageArea.Resource.ItemCleanoutTable.LookupCleanout(lastStoredItemExternalId, itemExternalIdToStore, out long cleanSpan, out decimal cleanCost);
                if (cleanSpan > 0) //There must be a previous storage to track cleanout start
                {
                    //We have a cleanout to do before we can store this item
                    long cleanoutStartTicks = a_storageArea.LastStorage.QtyProfile.Last.LastQtyNode.LastConsumptionDate;
                    FindCapacityResult capacityResult = a_storageArea.Resource.FindCapacity(cleanoutStartTicks, cleanSpan, false, null, usageEnum.Clean, false, false, string.Empty, false, InternalActivityDefs.peopleUsages.UseSpecifiedNbr, 1m, out bool _);
                    if (capacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
                    {
                        if (capacityResult.NextStartTime > 0)
                        {
                            //The clean does not fit
                            SetEarliestRetryDate(capacityResult.NextStartTime, ref r_minRetryDate);
                            return EStorageAvailabilityResult.RequiresClean;
                        }
                        else
                        {
                            //TODO: SA Cleans. This SA requires a clean, however there isn't enough space to add one.
                            // Likely, the SA goes offline before a clean could be finished.
                            // For now, we just schedule a clean on offline capacity so the job doesn't fail, and the issue is visible.
                            availability.RequiresStorageClean(new Interval(cleanoutStartTicks, cleanoutStartTicks + cleanSpan));
                        }
                    }
                    else if (capacityResult.FinishDate > storageInterval.StartTicks)
                    {
                        //The clean does not fit
                        SetEarliestRetryDate(capacityResult.FinishDate, ref r_minRetryDate);
                        return EStorageAvailabilityResult.RequiresClean;
                    }
                    else
                    {
                        //We need to allocate the clean
                        availability.RequiresStorageClean(new Interval(cleanoutStartTicks, capacityResult.FinishDate));
                    }
                }
            }

            long storageIntervalDuration = Math.Max(1, storageInterval.Duration);
            FindCapacityResult findCapacityResult = a_storageArea.Resource.FindCapacity(storageInterval.StartTicks, storageIntervalDuration, false, null, usageEnum.Unspecified, true, false, string.Empty, false, InternalActivityDefs.peopleUsages.UseSpecifiedNbr, 1m, out bool _);
            if (findCapacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                if (findCapacityResult.NextStartTime > 0)
                {
                    //We need to find this retry time using the primary resource capacity
                    LinkedListNode<ResourceCapacityInterval> containingOrPlanningHorizon = a_primaryResource.FindContainingCapacityIntervalNode(findCapacityResult.NextStartTime, null);
                    if (containingOrPlanningHorizon.Value.Contains(findCapacityResult.NextStartTime))
                    {
                        //The primary is online at this same time
                        SetEarliestRetryDate(findCapacityResult.NextStartTime, ref r_minRetryDate);
                    }
                    else
                    {
                        //Otherwise, use the next online interval start. Add one minute buffer to back calculate doesn't go back to the previous interval end
                        LinkedListNode<ResourceCapacityInterval> forward = a_primaryResource.FindFirstOnlineRCIForward(findCapacityResult.NextStartTime);
                        SetEarliestRetryDate(forward.Value.StartDate + TimeSpan.FromMinutes(1).Ticks, ref r_minRetryDate);
                    }
                }
                else
                {
                    SetEarliestRetryDate(StorageAllocationResult.RetryAtNextOnlineRetryConst, ref r_minRetryDate);
                }

                return EStorageAvailabilityResult.NotAvailable;
            }
        }

        availability.CalcMaxStorageCapacityAvailable();

        a_usableItemStorage.Add(itemStorage);

        if (availability.MaxUsableCapacityAvailable > 0m)
        {
            a_availableStorage.Add(availability);
            return EStorageAvailabilityResult.Available;
        }

        return EStorageAvailabilityResult.NotAvailable;
    }

    [Flags]
    private enum EStorageAvailabilityResult
    {
        Available = 0,
        NotAvailable = 1,
        NoItemStorage = 2,
        RequiresEmptyStorageArea = 4,
        RequiresSingleItemStorage  = 8,
        RequiresClean = 16,
    }

    internal Lot StorePurchaseOrder(PurchaseToStock a_pts, long a_simClock, ScenarioOptions a_roundingOptions)
    {
        PurchaseOrderSupplyProfile poSupply = new (a_pts, a_pts.Inventory);
        decimal poQty = a_pts.QtyRemaining;
        
        QtySupplyNode newNode = poSupply.AddToEnd(a_pts.ScheduledReceiptDateTicks, a_pts, poQty);

        if (a_pts.StorageArea != null)
        {
            decimal qtyToStore;

            ItemStorage storage = a_pts.StorageArea.Storage[a_pts.Inventory.Item.Id];
            (decimal availableQty, decimal availableQtyAfterDisposal) = storage.CalculateRemainingStorageCapacity(poSupply.Last.Date);

            if (a_pts.OverrideStorageConstraint)
            {
                //Store it anyways
                qtyToStore = poQty;
            }
            else
            {
                if (a_pts.RequireEmptyStorageArea && !a_pts.StorageArea.Empty)
                {
                    decimal qtyToDispose = 0m;
                    //Check for disposal
                    if (availableQtyAfterDisposal > 0)
                    {
                        //we can discard the current qty first
                        //TODO: storage.DisposeAll(a_simClock, a_pts);
                        if (availableQtyAfterDisposal <= poQty)
                        {
                            decimal qtyToDiscard = poQty - availableQtyAfterDisposal;
                            poSupply.DiscardExcess(a_simClock, a_pts, qtyToDiscard);
                            qtyToStore = poQty - qtyToDiscard;
                        }
                        else
                        {
                            //Otherwise store full qty
                            qtyToStore = poQty;
                        }
                    }
                    else
                    {
                        //Discard everything, no room
                        poSupply.DiscardExcess(a_simClock, a_pts, poQty);
                        qtyToStore = 0m;
                    }
                }
                else
                {
                    if (availableQty < poQty)
                    {
                        //We are constrained by space, discard the excess but create an excess adjustment
                        decimal excess = poQty - availableQty;
                        poSupply.DiscardExcess(a_simClock, a_pts, excess);
                        qtyToStore = availableQty;
                    }
                    else
                    {
                        //Store everything
                        qtyToStore = poQty;
                    }
                }
            }


            if (!a_roundingOptions.IsApproximatelyZero(qtyToStore))
            {
                //Create Lot first so we can add adjustments when the product is stored
                Lot purchaseLot = a_pts.Inventory.CreateLot(a_pts.Inventory, poSupply, BaseId.NULL_ID);
                newNode.SetLot(purchaseLot);
                storage.StoreProduct(a_pts, newNode, qtyToStore, a_pts);
                return purchaseLot;
                //TODO: Disposal
            }
        }

        return null;
    }

    private class StoragePlan
    {
        private decimal m_availableQty;
        private decimal m_availableQtyIfDrained;
        private readonly List<ItemStorage.StorageAvailability> m_availableStorage;
        private readonly StorageAreaConnector m_connector;
        private long m_connectorAvailableTicks;
        internal long RetryDate => m_connectorAvailableTicks;
        internal bool RequiresDrainage;

        internal StoragePlan(List<ItemStorage.StorageAvailability> a_availableStorage, StorageAreaConnector a_connector)
        {
            m_availableStorage = new List<ItemStorage.StorageAvailability>();
            m_connector = a_connector;

            //TODO: Order available storage by production rules, default is largest
            a_availableStorage.Sort((a, b) => b.MaxUsableCapacityAvailable.CompareTo(a.MaxUsableCapacityAvailable));

            LimitAvailability(a_availableStorage, a_connector.StorageInFlowLimit);
        }

        internal StoragePlan(List<ItemStorage.StorageAvailability> a_availableStorage)
        {
            m_availableStorage = new List<ItemStorage.StorageAvailability>();

            //TODO: Order available storage by production rules, default is largest
            a_availableStorage.Sort((a, b) => b.MaxUsableCapacityAvailable.CompareTo(a.MaxUsableCapacityAvailable));

            LimitAvailability(a_availableStorage, int.MaxValue);
        }

        internal StoragePlan(StorageAreaConnector a_connector, long a_nextAvailableAttempt)
        {
            m_availableStorage = new List<ItemStorage.StorageAvailability>();
            m_connector = a_connector;
            m_connectorAvailableTicks = a_nextAvailableAttempt;
        }

        private void LimitAvailability(List<ItemStorage.StorageAvailability> a_availableStorage, int a_inFlowLimit)
        {
            int remainingInFlow = a_inFlowLimit == 0 ? int.MaxValue : a_inFlowLimit;
            foreach (ItemStorage.StorageAvailability availability in a_availableStorage)
            {
                if (remainingInFlow <= 0)
                {
                    //Limit SA usages by inflow limit on the connector
                    break;
                }

                m_availableStorage.Add(availability);
                remainingInFlow--;

                if (m_availableQty < ItemStorage.StorageAvailability.c_maxStorageCapacity)
                {
                    if (availability.Unconstrained)
                    {
                        m_availableQty = ItemStorage.StorageAvailability.c_maxStorageCapacity;
                        m_availableQtyIfDrained = ItemStorage.StorageAvailability.c_maxStorageCapacity;
                    }
                    else
                    {
                        m_availableQty = Math.Min(m_availableQty + availability.QtyAvailable, ItemStorage.StorageAvailability.c_maxStorageCapacity);
                        m_availableQtyIfDrained = Math.Min(m_availableQtyIfDrained + availability.QtyAvailableIfDrained, ItemStorage.StorageAvailability.c_maxStorageCapacity);
                    }
                }
            }
        }

        internal decimal AvailableQty => m_availableQty;

        internal bool FullyAvailable => m_availableQty >= ItemStorage.StorageAvailability.c_maxStorageCapacity;

        internal decimal AvailableQtyIfDrained => m_availableQtyIfDrained;

        internal List<ItemStorage.StorageAvailability> OrderedStorageAvailabilities => m_availableStorage;

        internal void AllocatePlan(SupplyProfile a_profile)
        {
            IInterval profileUsageRange = a_profile.GetUsageRange();
            if (m_connector != null)
            {
                m_connector.FlowRangeConstraint.AllocateUsage(profileUsageRange);
                a_profile.Connector = m_connector;
            }

            //TODO: Order available storage by production rules
            foreach (ItemStorage.StorageAvailability availability in m_availableStorage)
            {
                if (availability.RequiresDisposal)
                {
                    //Drain the SA
                    foreach (ItemStorage.StorageAvailability storageToDispose in availability.StorageToDispose)
                    {
                        a_profile.AllocateDisposal(storageToDispose.CurrentQty, storageToDispose.ItemStorage);
                    }
                }

                if (availability.RequiresCleanout)
                {
                    //Requires a cleanout
                    a_profile.AllocateCleanout(availability.RequiredCleanout, availability.ItemStorage);
                }

                availability.ItemStorage.AllocateStorage(a_profile);
                if (a_profile.SupplyAllocated)
                {
                    a_profile.AllocateFinalSupply();
                    return;
                }
            }
        }
    }
}