using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.PTMath;
using PT.Scheduler.Demand;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.Scheduler.Simulation.Scheduler.InventoryManagement;

namespace PT.Scheduler;

/// <summary>
/// Inventory object data that is related to item storage. The Inventory object contains planning properties while this class contains storage and scheduling fields.
/// </summary>
public partial class ItemStorage : IPTSerializable
{
    /// <summary>
    /// New object Constructor
    /// </summary>
    //internal ItemStorage(ItemStorage a_sourceStorage)
    //{
    //    m_availableQtyProfile = a_sourceStorage.m_availableQtyProfile.CopyProfile();
    //    m_simAllocations = a_sourceStorage.m_simAllocations;
    //    m_qtyProfile = a_sourceStorage.m_qtyProfile.CopyProfile();
    //    SimQty = a_sourceStorage.SimQty;
    //    m_simQtyOnHandAtCurrentClock = a_sourceStorage.m_simQtyOnHandAtCurrentClock;

    //}

    /// <summary>
    /// A deep copy add to this inventory. Any objects from the source are deep copied instead of being referenced by this inventory.
    /// </summary>
    /// <param name="a_sourceInv">Any objects from this source are deep copied instead of being referenced by this inventory.</param>
    internal void Add(ItemStorage a_sourceInv)
    {
        //QtyProfile availCopy = a_sourceInv.m_availableQtyProfile.CopyProfile();
        //m_availableQtyProfile.Merge(availCopy);
        m_simAllocations.Combine(a_sourceInv.m_simAllocations);
        QtyProfile<QtySupplyNode> futureProfileCopy = a_sourceInv.m_qtyProfile.CopyProfile();
        m_qtyProfile.Merge(futureProfileCopy);
        m_simQtyOnHandAtCurrentClock += a_sourceInv.m_simQtyOnHandAtCurrentClock;
    }

    internal void Remove(long a_simClock, QtyNode a_node, Lot a_lot)
    {
        //if (a_node.m_remainingQty > 0)
        //{
        //    // There's uncertainty about which profile the node belongs to, so an attempt is made to remove it from both.
        //    //TODO: why is there uncertainty? I have confirmed an expirable shelf life node can be in either profile when the ShelfLifeEvent event is processed.
        //    if (m_availableQtyProfile.Remove(a_node))
        //    {
        //        SimQty -= a_node.m_remainingQty;
        //    }
        //    else
        //    {
        //        m_qtyProfile.Remove(a_node);
        //    }

        //    AdjustmentExpirable adj = new(a_simClock, a_node.Lot, -a_node.m_remainingQty);
        //    a_lot.Adjustments.Add(adj);
        //}
    }

    /// <summary>
    /// Add a quantity profile to the expected inventory.
    /// </summary>
    /// <param name="a_reason"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_qtyProfile"></param>
    /// <param name="a_materialSource">The source of the material, but not necessarily the reason. For example the source could be a Product while the reason is the activity</param>
    /// <param name="a_newProductionLot"></param>
    internal void StoreProduct(BaseIdObject a_reason, QtySupplyNode a_storageNode, decimal a_qty, BaseIdObject a_materialSource)
    {
        QtySupplyNode storageNode = a_storageNode.BreakOff(a_qty);

        m_qtyProfile.Merge(storageNode);
        StorageAdjustment storageAdjustment = new StorageAdjustment(storageNode.SourceLot, StorageArea);

        //TODO: Consider moving this into ISupplySource so each object can generate it's related adjustments
        if (a_reason is InternalActivity act)
        {
            Product product = (Product)a_materialSource;

            if (storageNode.AvailableDate != storageNode.ProductionDate)
            {
                //Add the storage only component
                ProductAdjustment adjustment = new ProductAdjustment(Inventory, storageNode.ProductionDate, a_qty, storageAdjustment, act, product);
                adjustment.FlagStorageOnly();
                storageNode.SourceLot.AddAdjustment(adjustment);

                ProductAdjustment availableAdjustment = new ProductAdjustment(Inventory, storageNode.AvailableDate, a_qty, storageAdjustment, act, product);
                storageNode.SourceLot.AddAdjustment(availableAdjustment);
                availableAdjustment.FlagAvailable();
            }
            else
            {
                //Add the standard stored and available adjustment
                ProductAdjustment availableAdjustment = new ProductAdjustment(Inventory, storageNode.AvailableDate, a_qty, storageAdjustment, act, product);
                storageNode.SourceLot.AddAdjustment(availableAdjustment);
            }
        }
        else if (a_reason is PurchaseToStock po)
        {
            if (storageNode.AvailableDate != storageNode.ProductionDate)
            {
                //Add the storage only component
                PurchaseOrderAdjustment adjustment = new PurchaseOrderAdjustment(Inventory, storageNode.ProductionDate, a_qty, storageAdjustment, po);
                adjustment.FlagStorageOnly();
                storageNode.SourceLot.AddAdjustment(adjustment);

                PurchaseOrderAdjustment availableAdjustment = new PurchaseOrderAdjustment(Inventory, storageNode.AvailableDate, a_qty, storageAdjustment, po);
                storageNode.SourceLot.AddAdjustment(availableAdjustment);
                availableAdjustment.FlagAvailable();
            }
            else
            {
                //Add the standard stored and available adjustment
                PurchaseOrderAdjustment availableAdjustment = new PurchaseOrderAdjustment(Inventory, storageNode.AvailableDate, a_qty, storageAdjustment, po);
                storageNode.SourceLot.AddAdjustment(availableAdjustment);
            }
        }
        else if (a_reason is TransferOrderDistribution tod)
        {
            //Add the standard stored and available adjustment
            TransferOrderAdjustment availableAdjustment = new TransferOrderAdjustment(Inventory, storageNode.AvailableDate, a_qty, storageAdjustment, tod);
            storageNode.SourceLot.AddAdjustment(availableAdjustment);
        }

        m_storageArea.FlagStorage(this);
    }

    /// <summary>
    /// Store material from an on-hand lot
    /// </summary>
    /// <param name="a_lot"></param>
    /// <param name="a_onHandQty"></param>
    internal void StoreOnHandLot(Lot a_lot, decimal a_onHandQty)
    {
        QtySupplyNode qtySupplyNode = new (a_lot.ProductionTicks, a_lot.ProductionTicks, a_lot, a_lot, a_onHandQty);
        qtySupplyNode.SetLot(a_lot);
        m_qtyProfile.Merge(qtySupplyNode);

        OnHandLotAdjustment onHandLotAdjustment = new (Inventory, a_lot.ProductionTicks, a_onHandQty, new StorageAdjustment(a_lot, StorageArea));
        a_lot.AddAdjustment(onHandLotAdjustment);
        
        m_storageArea.FlagStorage(this);
    }

    internal void ClearOnHandLot()
    {
        //TODO: what should we do about the m_qtyProfile node
        m_storageArea.FlagStorageRemoval(this);
    }
    
    /// <summary>
    /// This value only refers to the amount of remaining on-hand at the simulation clock. The quantiy can include
    /// on-hand, expected on-hand from production, and POs.
    /// </summary>
    private decimal m_simQtyOnHandAtCurrentClock;
    
    internal void SimulationComplete()
    {

    }

    internal void SimulationStageInitialization()
    {

    }


    /// <summary>
    /// Profile of all qty nodes
    /// </summary>
    private StorageProfile m_qtyProfile;

    /// <summary>
    /// Profile of inventory potentially including available and future additions to inventory from things such as production and Purchases to Stock.
    /// As the simulation proceeds attempts to allocate inventory will result in inventory up to and including the simulation time being
    /// transferred to the AvailableQtyProfile.
    /// </summary>
    internal StorageProfile QtyProfile => m_qtyProfile;

    public bool Empty => m_qtyProfile.Qty > 0;

    /// <summary>
    /// 
    /// </summary>
    /// <returns>(CurrentQtyAvailable, QtyAvailableIfDrained)</returns>
    internal (decimal, decimal) CalculateRemainingStorageCapacity(long a_materialDemandDate)
    {
        if (UnConstrained)
        {
            return (decimal.MaxValue, 0);
        }

        decimal currentQty = m_qtyProfile.CalculateProjectedQty(a_materialDemandDate);
        decimal qtyIfDrained = 0m;

        if (currentQty <= m_disposalQty)
        {
            //We can dispose of this current qty, so return max as available
            qtyIfDrained = m_maxQty;
        }

        return (m_maxQty - currentQty, qtyIfDrained);
    } 

    private readonly InventoryAllocationSimCollection m_simAllocations = new();

    /// <summary>
    /// Call this right before the start of a simulation.
    /// </summary>
    internal void ResetSimulationStateVariables()
    {
        m_qtyProfile = new StorageProfile(Inventory, this);
        m_retryStorageEvents.Clear();

        //m_inventoryLevelCalculated = false;

        //if (m_demandTotals == null)
        //{
        //    // [DaysOnHand:Synchronization]
        //    // If demand totals haven't beeen created yet, create them now.
        //    // Usually they will have been created by a previous simulation and
        //    // kept up to date as changes to demand have occurred.
        //    CalcDemandTotals(a_sd.SalesOrderManager);
        //}

        ////m_daysOnHandStartRangeEtr = m_demandTotals.GetEnumerator();
        ////m_daysOnHandEndRangeEtr = m_demandTotals.GetEnumerator();
        //a_sd.InitializeCache(ref m_requirementAdjustmentCache);
    }

    /// <summary>
    /// These lots have expired and can no longer be used by any source (No Demand can use expired material for this item)
    /// </summary>
    /// <param name="a_lotIds"></param>
    internal void ExpireLots(long a_simClock, HashSet<BaseId> a_lotIds)
    {
        QtySupplyNode[] currentNodes = m_qtyProfile.ToArray();
        Lot lastRemovedLot = null;
        foreach (QtySupplyNode qtySupplyNode in currentNodes)
        {
            Lot sourceLot = qtySupplyNode.SupplySource.SupplyLot;
            if (a_lotIds.Contains(sourceLot.Id))
            {
                qtySupplyNode.ResetForAllocation(); //To get the unallocated Qty
                if (qtySupplyNode.UnallocatedQty > 0)
                {
                    sourceLot.GenerateExpirationAdjustment(-qtySupplyNode.UnallocatedQty, StorageArea);
                    lastRemovedLot = sourceLot;
                }

                if (!sourceLot.Item.SaveExpiredMaterial)
                {
                    m_qtyProfile.Remove(qtySupplyNode);
                }
            }
        }

        //Check for disposal
        if (lastRemovedLot != null && DisposeImmediately)
        {
            InitStorageAvailability(a_simClock, false);
            if (m_simStorageAvailability.CanBeDrained)
            {
                DisposeDueToExpiration(lastRemovedLot, m_simStorageAvailability.CurrentQty);
            }
            else if (m_simStorageAvailability.CurrentQty == 0)
            {
                m_storageArea.FlagStorageRemoval(this);
            }
        }
    }

    internal decimal CalculateAvailableQtyForDemand(BaseDemandProfile a_demandProfile, ScenarioDetail a_sd)
    {
        m_qtyProfile.ResetForAllocation();
        a_demandProfile.ResetForAllocation();

        //Now calculate qty based on usable nodes (they are all eligible).
        decimal totalQty = 0m;
        IEnumerator<QtySupplyNode> supplyEnumerator = m_qtyProfile.CreateDirectionalEnumerator(a_demandProfile.First.Date, a_demandProfile.MaterialAllocation);
        if (!supplyEnumerator.MoveNext())
        {
            //Somehow there are no nodes??
            return 0m;
        }
        QtySupplyNode supplyNode = supplyEnumerator.Current;

        using IEnumerator<QtyNode> demandEnumerator = a_demandProfile.GetEnumerator();

        while (demandEnumerator.MoveNext())
        {
            QtyNode demandNode = demandEnumerator.Current;
            while (demandNode.UnallocatedQty > 0m)
            {
                decimal requiredQty = demandNode.UnallocatedQty;

                if(supplyNode.AvailableDate <= demandNode.Date)
                {
                    decimal supplyNodeUnallocatedQty = supplyNode.UnallocatedQty;

                    if (a_demandProfile.IsSupplySourceEligible(supplyNode, a_sd))
                    {
                        //We can use this node
                        if (supplyNodeUnallocatedQty >= requiredQty)
                        {
                            supplyNode.TestAllocate(requiredQty);
                            demandNode.TestAllocate(requiredQty);
                            totalQty += requiredQty;
                            continue; //We satisfied the demand node, and can use more on the next demand node
                        }

                        if (supplyNodeUnallocatedQty > 0)
                        {
                            //We can't supply the full required qty, so use what is remaining
                            totalQty += supplyNodeUnallocatedQty;
                            supplyNode.TestAllocate(supplyNodeUnallocatedQty);
                            demandNode.TestAllocate(supplyNodeUnallocatedQty);
                        }
                    }
                }
                else
                {
                    //We can't satisfy this demand node, but it's possible a later demand node could use this material, so continue to the next demand node
                    break;
                }

                //Move to the next supply node
                if (!supplyEnumerator.MoveNext())
                {
                    //This demand node was not satisfied, return what we were able to use
                    return totalQty;
                }

                supplyNode = supplyEnumerator.Current;
            }
        }

        //TODO: This is not great for performance just to see if there is excess qty. It's only used to prioritize SA selection. Possibly just get total qty and avoid checking if Eligible
        //Now add up any excess.
        supplyEnumerator.Reset();
        while (supplyEnumerator.MoveNext())
        {
            supplyNode = supplyEnumerator.Current;
            if (supplyNode.UnallocatedQty > 0m && supplyNode.AvailableDate <= a_sd.SimClock)
            {
                if (a_demandProfile.IsSupplySourceEligible(supplyNode, a_sd))
                {
                    totalQty += supplyNode.UnallocatedQty;
                }
            }
        }

        return totalQty;
    }

    /// <summary>
    /// Actually go through and update 
    /// </summary>
    /// <param name="a_demandProfile"></param>
    internal bool AllocateSupply(BaseDemandProfile a_demandProfile, ScenarioDetail a_sd)
    {
        bool someQtyAllocated = false;
        //Now calculate qty based on usable nodes (they are all eligible).
        m_qtyProfile.ResetForAllocation(); //TODO: What about operations with MR for the same item?
        IEnumerator<QtySupplyNode> supplyEnumerator = m_qtyProfile.CreateDirectionalEnumerator(a_demandProfile.First.Date, a_demandProfile.MaterialAllocation);
        supplyEnumerator.MoveNext();
        QtySupplyNode supplyNode = supplyEnumerator.Current;

        using IEnumerator<QtyNode> demandEnumerator = a_demandProfile.GetEnumerator();
        //long firstAllocation = int.MaxValue;
        //long lastAllocation = 0;

        while (demandEnumerator.MoveNext())
        {
            QtyNode demandNode = demandEnumerator.Current;
            while (demandNode.UnallocatedQty > 0m)
            {
                if (supplyNode.AvailableDate <= demandNode.Date)
                {
                    if (a_demandProfile.IsSupplySourceEligible(supplyNode, a_sd)) //TODO, cache whether the node is eligible
                    {
                        if (supplyNode.UnallocatedQty > demandNode.UnallocatedQty)
                        {
                            a_demandProfile.Allocate(demandNode.UnallocatedQty, supplyNode, demandNode);
                            a_demandProfile.TrackStorageAreaSupply(demandNode.Date, StorageArea);
                            //firstAllocation = Math.Min(firstAllocation, demandNode.Date);
                            //lastAllocation = Math.Max(lastAllocation, demandNode.Date);
                            someQtyAllocated = true;
                            continue; //We satisfied the demand node, and can use more on the next demand node
                        }

                        if (supplyNode.UnallocatedQty > 0)
                        {
                            //We can't supply the full required qty, so use what is remaining
                            a_demandProfile.Allocate(supplyNode.UnallocatedQty, supplyNode, demandNode);
                            a_demandProfile.TrackStorageAreaSupply(demandNode.Date, StorageArea);
                            //firstAllocation = Math.Min(firstAllocation, demandNode.Date);
                            //lastAllocation = Math.Max(lastAllocation, demandNode.Date);
                            someQtyAllocated = true;
                        }
                    }
                }

                //Move to the next supply node
                supplyNode = supplyEnumerator.MoveNext() ? supplyEnumerator.Current : null;

                if (supplyNode == null)
                {
                    //This demand node was not satisfied
                    return someQtyAllocated; //new Interval(firstAllocation, lastAllocation);
                }
            }
        }

        return someQtyAllocated; //new Interval(firstAllocation, lastAllocation);
    }

    internal void DisposeDueToExpiration(Lot a_lot, decimal a_decimal)
    {
        a_lot.GenerateExpirationDisposalAdjustment(a_decimal, StorageArea);
        
        m_qtyProfile.Clear();
        m_storageArea.FlagStorageRemoval(this);
    }

    internal void DisposeAll(BaseIdObject a_reason, Lot a_newLot)
    {
        if (m_qtyProfile.HasNodes)
        {
            long lastProductionDate = m_qtyProfile.Last.Date;
            foreach (QtySupplyNode node in m_qtyProfile)
            {
                if (node.UnallocatedQty <= 0)
                {
                    continue;
                }

                Lot sourceLot = node.SourceLot;
                Adjustment adjustment;
                if (a_reason is InternalActivity act)
                {
                    adjustment = new ProductionDisposalAdjustment(Inventory, lastProductionDate, -node.UnallocatedQty, new StorageAdjustment(a_newLot, StorageArea), act);
                }
                else if (a_reason is PurchaseToStock po)
                {
                    adjustment = new PurchaseOrderDisposalAdjustment(Inventory, lastProductionDate, -node.UnallocatedQty, new StorageAdjustment(a_newLot, StorageArea), po);
                }
                else
                {
                    throw new PTValidationException("Invalid disposal reason");
                }

                sourceLot.AddAdjustment(adjustment);
            }
        }

        m_qtyProfile.Clear();
        m_storageArea.FlagStorageRemoval(this);
    }

    internal void AllocateStorage(SupplyProfile a_productQtyProfile)
    {
        a_productQtyProfile.ResetForAllocation();
        foreach (QtySupplyNode qtyNode in a_productQtyProfile)
        {
            decimal qtyToStore = qtyNode.UnallocatedQty;
            if (!m_simStorageAvailability.Unconstrained)
            {
                if (m_simStorageAvailability.CanBeDrained)
                {
                    qtyToStore = Math.Min(m_simStorageAvailability.QtyAvailableIfDrained, qtyToStore);
                }
                else
                {
                    qtyToStore = Math.Min(m_simStorageAvailability.QtyAvailable, qtyToStore);
                }
            }

            if (qtyToStore <= 0m)
            {
                //Allocated all storage demand.
                continue;
            }

            if (m_simStorageAvailability.CanBeDrained && (qtyToStore > m_simStorageAvailability.QtyAvailable || a_productQtyProfile.RequireEmptyStorageArea))
            {
                //We must drain first
                a_productQtyProfile.AllocateDisposal(m_simStorageAvailability.CurrentQty, this);
                m_simStorageAvailability.UpdateQty(0m);
            }

            a_productQtyProfile.AllocateSupply(qtyToStore, qtyNode, this);

            if (!m_simStorageAvailability.Unconstrained && m_simStorageAvailability.QtyAvailable == 0m)
            {
                //Allocated all capacity
                return;
            }
        }
    }

    private StorageAvailability m_simStorageAvailability;

    /// <summary>
    /// Create and cache simulation storage availability
    /// </summary>
    /// <param name="a_materialSupplyDate">Only qty before this date matters</param>
    /// <param name="a_qtyLimitOnly">Whether to skip on-hand qty calculations to save performance. This is good if we are only checking qty constraints</param>
    /// <returns></returns>
    internal StorageAvailability InitStorageAvailability(long a_materialSupplyDate, bool a_qtyLimitOnly)
    {
        m_qtyProfile.ResetForAllocation();

        m_simStorageAvailability = new StorageAvailability(this);

        if (a_qtyLimitOnly && m_simStorageAvailability.Unconstrained)
        {
            //No need to calculate on-hand since we only care about max qty constraint
            return m_simStorageAvailability;
        }

        decimal currentQty = m_qtyProfile.CalculateProjectedQty(a_materialSupplyDate);
        m_simStorageAvailability.UpdateQty(currentQty);

        return m_simStorageAvailability;
    }

    internal class StorageAvailability
    {
        internal const decimal c_maxStorageCapacity = 7922816251426433759M; //decimal.MaxValue / 1^10


        internal StorageAvailability(ItemStorage a_itemStorage)
        {
            ItemStorage = a_itemStorage;
            m_maxStorage = a_itemStorage.MaxQty > 0m ? a_itemStorage.MaxQty : c_maxStorageCapacity;
            m_disposalQty = a_itemStorage.DisposalQty;

            if (m_maxStorage <= 0)
            {
                Unconstrained = true;
            }
        }

        internal readonly ItemStorage ItemStorage;

        internal bool Unconstrained;
        internal decimal QtyAvailable;
        internal decimal QtyAvailableIfDrained;
        internal bool CanBeDrained;
        internal decimal MaxUsableCapacityAvailable;
        internal decimal CurrentQty;
        private List<StorageAvailability> m_storageToDispose;
        internal List<StorageAvailability> StorageToDispose => m_storageToDispose;
        internal bool RequiresDisposal => m_storageToDispose != null;

        internal bool RequiresCleanout => m_requiredCleanout != null;
        internal IInterval RequiredCleanout => m_requiredCleanout;
        private IInterval m_requiredCleanout;

        /// <summary>
        /// This Storage is only valid if all previous storage in the Storage Area is drained
        /// </summary>
        /// <param name="a_currentStorageAvailability"></param>
        internal void RequiresDrainingStorage(StorageAvailability a_currentStorageAvailability)
        {
            if (m_storageToDispose == null)
            {
                m_storageToDispose = new ();
            }

            m_storageToDispose.Add(a_currentStorageAvailability);
        }

        /// <summary>
        /// This Storage is only valid if all previous storage in the Storage Area is drained
        /// </summary>
        /// <param name="a_currentStorageAvailability"></param>
        internal void RequiresStorageClean(IInterval a_cleanoutInterval)
        {
            m_requiredCleanout = a_cleanoutInterval;
        }

        private decimal m_maxStorage;
        private decimal m_disposalQty;

        internal void CalcMaxStorageCapacityAvailable()
        {
            if (Unconstrained)
            {
                MaxUsableCapacityAvailable = c_maxStorageCapacity;
            }
            else if (CanBeDrained)
            {
                MaxUsableCapacityAvailable = QtyAvailableIfDrained;
            }
            else
            {
                MaxUsableCapacityAvailable = QtyAvailable;
            }
        }

        internal void UpdateQty(decimal a_currentQty)
        {
            decimal qtyIfDrained = 0m;

            if (a_currentQty <= m_disposalQty && a_currentQty > 0m)
            {
                //We can dispose of this current qty, so return max as available
                qtyIfDrained = m_maxStorage > 0m ? m_maxStorage : c_maxStorageCapacity;
            }

            CurrentQty = a_currentQty;
            QtyAvailable = m_maxStorage - a_currentQty;
            QtyAvailableIfDrained = qtyIfDrained;
            CanBeDrained = qtyIfDrained > 0m;
        }

        internal void UpdateForAllocation(decimal a_qtyToStore)
        {
            if (Unconstrained)
            {
                //No need to update availability since it's unlimited
                return;
            }

            UpdateQty(m_maxStorage - QtyAvailable + a_qtyToStore);
        }
    }

    internal List<(decimal, QtyNode)> CheckForImmediateDisposal(long a_lastWithdrawTicks)
    {
        if (DisposeImmediately)
        {
            if (m_qtyProfile.ValidateDisposalQty(m_disposalQty, a_lastWithdrawTicks))
            {
                //We need to dispose since after allocations we are under the drainage qty.
                return m_qtyProfile.DisposeAllAtOrBefore(a_lastWithdrawTicks);
            }
        }

        return null;
    }

    /// <summary>
    /// Check current storage level. If all the material has been removed, notify storage area that no material is left.
    /// </summary>
    internal bool CheckForEmpty(long a_simClock)
    {
        foreach (QtySupplyNode node in m_qtyProfile)
        {
            if (node.m_currentQty > 0m)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The activities that failed to schedule due to insufficient storage
    /// </summary>
    private readonly List<EventBase> m_retryStorageEvents = new();

    internal void AddRetryStorageEvent(EventBase a_retryEvent)
    {
        m_retryStorageEvents.Add(a_retryEvent);
    }

    /// <summary>
    /// Clear the retry set. You should do this after you've added the events back to the event queue.
    /// </summary>
    internal void ClearRetryStorageEventSet()
    {
        m_retryStorageEvents.Clear();
    }

    /// <summary>
    /// Get an enumerator to the retry events.
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<EventBase> GetRetryStorageEventSet()
    {
        return m_retryStorageEvents;
    }

    internal void GenerateCleanoutAdjustment(IInterval a_cleanout)
    {
        Inventory.AddScheduleAdjustment(new StorageAreaCleanoutAdjustment(Inventory, a_cleanout.StartTicks, new StorageAdjustment(null, StorageArea), true));
        Inventory.AddScheduleAdjustment(new StorageAreaCleanoutAdjustment(Inventory, a_cleanout.EndTicks, new StorageAdjustment(null, StorageArea), false));
    }
}