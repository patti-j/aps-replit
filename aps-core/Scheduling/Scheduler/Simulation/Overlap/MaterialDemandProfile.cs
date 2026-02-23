using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.Common.PTMath;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation;
internal abstract class BaseDemandProfile : QtyProfile<QtyDemandNode>
{
    internal override QtyDemandNode CreateQtyNode(QtyDemandNode a_node)
    {
        return new QtyDemandNode(a_node);
    }

    internal override QtyProfile<QtyDemandNode> CopyProfile()
    {
        throw new NotImplementedException();
    }

    internal BaseDemandProfile(Item a_item)
    {
        m_item = a_item;
    }
    
    protected readonly Item m_item;
    
    internal Item Item => m_item;

    internal decimal TotalRequiredQty;
    internal decimal RemainingQty;
    internal bool Satisfied => RemainingQty <= 0;
    internal bool AllowPartialSupply;
    public long FirstDemandDate => First.Date;
    internal bool AllowMultiStorageAreaSupply;

    //Connector used to allocate the material
    internal StorageAreaConnector Connector { get; set; }

    internal IEnumerable<Inventory> AllocatedInventories
    {
        get
        {
            //if (SingleNode)
            //{
            //    LinkedList<QtyDemandNode> nodes = First.QtyNodes;
            //    foreach (Inventory inv in nodes.First.Value.AllocatedInventories)
            //    {
            //        yield return inv;
            //    }
            //}
            //else
            {
                HashSet<BaseId> invs = new();
                foreach (QtyDemandNode node in this)
                {
                    foreach (Inventory inv in node.AllocatedInventories)
                    {
                        if (invs.AddIfNew(inv.Id))
                        {
                            yield return inv;
                        }
                    }
                }
            }
        }
    }

    internal override void Allocate(decimal a_allocationQty, QtyNode a_supplyNode, QtyNode a_demandNode)
    {
        QtyDemandNode demandNode = (QtyDemandNode)a_demandNode;

        QtySupplyNode qtySupplyNode = (QtySupplyNode)a_supplyNode;
        demandNode.AllocateSupply(a_allocationQty, qtySupplyNode);
        RemainingQty -= a_allocationQty;

        base.Allocate(a_allocationQty, a_supplyNode, a_demandNode);
    }

    internal long GetLatestMaterialUseDate()
    {
        long latestAllocationDate = PTDateTime.MinDateTimeTicks;
        foreach (QtyDemandNode qtyNode in this)
        {
            if (qtyNode.LatestAllocationDate > latestAllocationDate)
            {
                latestAllocationDate = qtyNode.LatestAllocationDate;
            }
        }

        return latestAllocationDate;
    }

    protected ItemDefs.MaterialAllocation m_materialAllocation;
    public ItemDefs.MaterialAllocation MaterialAllocation => m_materialAllocation;

    internal virtual bool IsSupplySourceEligible(QtySupplyNode a_node, ScenarioDetail a_sd)
    {
        return true;
    }

    protected StorageArea LastStorageAreaSupply;
    private long m_lastStorageAllocationTime; //store as the start of usage if we continue storing in multiple storage areas
    internal void TrackStorageAreaSupply(long a_date, StorageArea a_storageArea)
    {
        if (LastStorageAreaSupply != null && LastStorageAreaSupply != a_storageArea)
        {
            //We are now pulling from a different SA
            LastStorageAreaSupply.AllocateUsage(a_date);
            a_storageArea.AllocateUsage(m_lastStorageAllocationTime);
        }
        
        a_storageArea.AllocateUsage(a_date);
        m_lastStorageAllocationTime = a_date;
        LastStorageAreaSupply = a_storageArea;
    }

    internal override void ResetForAllocation()
    {
        LastStorageAreaSupply = null;
        base.ResetForAllocation();
    }
}

//Used by SO and TO
internal abstract class WarehouseDemandProfile : BaseDemandProfile
{
    private readonly bool m_mustSupplyFromSpecificWarehouse;
    private readonly Warehouse m_warehouse;

    internal WarehouseDemandProfile(Item a_item, bool a_mustSupplyFromSpecificWarehouse, Warehouse a_warehouse, DemandSource a_demandSource)
        : base(a_item)
    {
        m_mustSupplyFromSpecificWarehouse = a_mustSupplyFromSpecificWarehouse;
        m_warehouse = a_warehouse;
        m_demandSource = a_demandSource;
    }

    internal bool MustSupplyFromWarehouse => m_mustSupplyFromSpecificWarehouse;
    internal Warehouse SupplyWarehouse => m_warehouse;
    
    private readonly DemandSource m_demandSource;
    internal DemandSource DemandSource => m_demandSource;

    internal abstract void GenerateProfile(ScenarioDetail a_sd);

    internal abstract void UpdateActualAvailableTicks(long a_matlAvailTicks);

    internal void ConsumeAllocations(long a_simClock, IEnumerable<ItemStorage> a_itemStorages, List<EventBase> a_futureConsumptionEvents)
    {
        long lastAdjustmentDate = Last.Date;
        foreach (QtyDemandNode qtyNode in this)
        {
            qtyNode.ConsumeAllocatedQty(a_simClock, a_futureConsumptionEvents);
        }

        //Create a disposal check at the end of this demands allocations in case the storage should be emptied
        foreach (ItemStorage itemStorage in a_itemStorages)
        {
            if (itemStorage.DisposeImmediately)
            {
                FutureStorageDisposalEvent potentialDisposal = new(lastAdjustmentDate, itemStorage, m_demandSource);
                a_futureConsumptionEvents.Add(potentialDisposal);
            }
        }
    }
}

internal class MaterialDemandProfile : BaseDemandProfile
{
    private readonly MaterialRequirement m_mr;
    private readonly InternalActivity m_act;
    internal bool AllowMultiWarehouseSupply;

    private decimal m_issuedQty;
    private readonly MaterialRequirementDemandSource m_demandSource;

    internal MaterialRequirement MR => m_mr;
    private MaterialRequirementDefs.constraintTypes m_constraintType;
    internal bool NonConstraint => m_mr.NonConstraint;
    internal bool CanUseLeadTime => NonConstraint || m_constraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate;
    internal InternalActivity Activity => m_act;
    internal MaterialRequirementDemandSource DemandSource => m_demandSource;
    internal bool AllowExpiredSupply => m_mr.AllowExpiredSupply;

    internal MaterialDemandProfile(MaterialRequirement a_mr, InternalActivity a_act, ScenarioDetail a_sd) 
        : base(a_mr.Item)
    {
        m_mr = a_mr;
        m_act = a_act;
        AllowPartialSupply = a_mr.AllowPartialSupply;
        AllowMultiWarehouseSupply = AllowPartialSupply && a_mr.MultipleWarehouseSupplyAllowed;
        AllowMultiStorageAreaSupply = AllowPartialSupply && a_mr.MultipleStorageAreaSupplyAllowed;
        m_demandSource = new MaterialRequirementDemandSource(a_act, a_mr);
        m_materialAllocation = m_mr.MaterialAllocation;
        m_scenarioOptions = a_sd.ScenarioOptions;

        ResetForAllocation();
    }

    private long m_lastUsageDate; //For tracking SA usage
    internal void GenerateProfile(ScenarioDetail a_sd, SchedulableInfo a_si, Resource a_primaryResource)
    {
        decimal remainingQty;
        switch (m_mr.MaterialUsedTiming)
        {
            case MaterialRequirementDefs.EMaterialUsedTiming.SetupStart:
                AddRemainingNode(a_si.m_scheduledStartDate);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.DuringSetup:
                m_act.Operation.CalculateMaterialConsumptionRangeIntoQtyProfile(this, a_primaryResource, a_si.m_ocp.SetupProfile, a_si.m_setupFinishDate);
                m_lastUsageDate = a_si.m_setupFinishDate;
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.ProductionStart:
                remainingQty = AddFirstSetupTransfer(a_si.m_scheduledStartDate);
                AddRemainingNode(a_si.m_setupFinishDate, remainingQty);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle:
                m_lastUsageDate = a_si.m_productionFinishDate;
                a_primaryResource.CalculateCycleStartTimes(m_act, a_si, out CycleAdjustmentProfile ccp);
                if (ccp.Count > 0)
                {
                    if (ccp.Count == 1)
                    {
                        long firstRequiredDate = m_mr.RequireFirstTransferAtSetup ? a_si.m_scheduledStartDate : a_si.m_setupFinishDate;
                        QtyDemandNode qtyNodeTemp = CreateQtyNode(firstRequiredDate, RemainingQty);
                        AddToEnd(qtyNodeTemp);
                    }
                    else
                    {
                        remainingQty = RemainingQty;
                        decimal materialPerRequiredQty = a_sd.ScenarioOptions.RoundQty(remainingQty / ccp.Count);
                        decimal transferQty = m_mr.Item.TransferQty;
                        bool useTransferQty = transferQty > 0m;
                        decimal qtyRemainingFromTransfer = 0m;
                        
                        for (var i = 0; i < ccp.Count; i++)
                        {
                            CycleAdjustment cycleStart = ccp[i];

                            if (useTransferQty)
                            {
                                if (qtyRemainingFromTransfer < materialPerRequiredQty)
                                {
                                    //We need more qty, at least one transfer

                                    decimal nextTransferQty = transferQty;

                                    if (materialPerRequiredQty > transferQty)
                                    {
                                        //We might need more than one transfer
                                        decimal currentDemandQty = materialPerRequiredQty - qtyRemainingFromTransfer;
                                        if (currentDemandQty > transferQty)
                                        {
                                            //pull enough by transfer qty to get enough for the next cycle
                                            nextTransferQty = Math.Ceiling(currentDemandQty / transferQty) * transferQty;
                                        }
                                        else
                                        {
                                            //one transfer is enough with the remainder we already have
                                        }
                                    }
                                    else
                                    {
                                        // A single transfer is enough to supply at least one cycle
                                    }

                                    if (remainingQty < nextTransferQty)
                                    {
                                        //Last transfer
                                        QtyDemandNode qtyNodeTemp = CreateQtyNode(cycleStart.Date, remainingQty);
                                        AddToEnd(qtyNodeTemp);
                                        break;
                                    }
                                    else
                                    {
                                        QtyDemandNode qtyNodeTemp = CreateQtyNode(cycleStart.Date, nextTransferQty);
                                        AddToEnd(qtyNodeTemp);
                                        qtyRemainingFromTransfer += nextTransferQty;
                                    }
                                }

                                //Update qty for this cycle
                                remainingQty -= materialPerRequiredQty;
                                qtyRemainingFromTransfer -= materialPerRequiredQty;
                            }
                            else
                            {
                                //Every cycle is a demand node
                                decimal completionQty = materialPerRequiredQty;
                                if (i == ccp.Count - 1) //We are on the last cycle. Handle Remainders
                                {
                                    //help prevent remainders or rounding issues
                                    completionQty = remainingQty;
                                }

                                QtyDemandNode qtyNodeTemp = CreateQtyNode(cycleStart.Date, completionQty);
                                AddToEnd(qtyNodeTemp);
                                remainingQty -= completionQty;
                            }
                        }

                        if (HasNodes && m_mr.RequireFirstTransferAtSetup)
                        {
                            //Update the first node to be required at setup
                            First.QtyNodes.First.Value.Date = a_si.m_scheduledStartDate;
                        }
                    }
                }

                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.FirstAndLastProductionCycle:
                m_lastUsageDate = a_si.m_productionFinishDate;
                long firstCycleStart = a_si.m_setupFinishDate;
                long lastCycleStart = a_primaryResource.CalculateLastCycleStartTime(m_act, a_si);

                if (firstCycleStart >= lastCycleStart)
                {
                    //a single cycle, use the start of production
                    long firstRequiredDate = m_mr.RequireFirstTransferAtSetup ? a_si.m_scheduledStartDate : a_si.m_setupFinishDate;
                    AddRemainingNode(firstRequiredDate);
                }
                else
                {
                    decimal materialPerCycleQty = a_sd.ScenarioOptions.RoundQty(RemainingQty / a_si.m_requiredNumberOfCycles);
                    if (m_mr.Item.TransferQty > materialPerCycleQty)
                    {
                        materialPerCycleQty = m_mr.Item.TransferQty;
                    }

                    decimal firstCycleQty = Math.Min(RemainingQty, materialPerCycleQty);

                    //One transfer at the first cycle
                    long firstRequiredDate = m_mr.RequireFirstTransferAtSetup ? a_si.m_scheduledStartDate : a_si.m_setupFinishDate;
                    QtyDemandNode firstCycleNode = CreateQtyNode(firstRequiredDate, firstCycleQty);
                    AddToEnd(firstCycleNode);

                    remainingQty = RemainingQty - firstCycleQty;
                    //Use end of second to last interval for the start date of last interval
                    AddRemainingNode(lastCycleStart, remainingQty);
                }
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.LastProductionCycle:
                remainingQty = AddFirstSetupTransfer(a_si.m_scheduledStartDate);
                long lastCycleStart1 = a_primaryResource.CalculateLastCycleStartTime(m_act, a_si);
                AddRemainingNode(lastCycleStart1, remainingQty);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.PostProcessingStart:
                remainingQty = AddFirstSetupTransfer(a_si.m_scheduledStartDate);
                AddRemainingNode(a_si.m_productionFinishDate, remainingQty);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.PostProcessingEnd:
                remainingQty = AddFirstSetupTransfer(a_si.m_scheduledStartDate);
                AddRemainingNode(a_si.m_postProcessingFinishDate, remainingQty);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!HasNodes || a_sd.EndOfPlanningHorizon <= First.Date)
        {
            //Past planning horizon, or no demand
            m_constraintType = MaterialRequirementDefs.constraintTypes.NonConstraint;
        }
        else
        {
            m_constraintType = m_mr.ConstraintType;
        }
    }

    /// <summary>
    /// Create an initial node if required at the beginning of setup
    /// </summary>
    /// <returns>The RemainingQty</returns>
    private decimal AddFirstSetupTransfer(long a_setupStartTicks)
    {
        if (m_mr.RequireFirstTransferAtSetup)
        {
            decimal firstTransfer = 0m;
            if (m_mr.Item.TransferQty > 0)
            {
                firstTransfer = Math.Min(m_mr.Item.TransferQty, RemainingQty);
            }
            else if (RemainingQty > 1m)
            {
                firstTransfer = 1m;
            }
            else if (RemainingQty > 0m)
            {
                firstTransfer = RemainingQty;
            }

            if (firstTransfer > 0)
            {
                QtyDemandNode setupNode = CreateQtyNode(a_setupStartTicks, firstTransfer);
                AddToEnd(setupNode);
                return RemainingQty - firstTransfer;
            }
        }

        return RemainingQty;
    }

    private void AddRemainingNode(long a_dateTicks)
    {
        AddRemainingNode(a_dateTicks, RemainingQty);
    }

    private void AddRemainingNode(long a_dateTicks, decimal a_qty)
    {
        if (a_qty > 0)
        {
            QtyDemandNode endOfPostNode = CreateQtyNode(a_dateTicks, a_qty);
            AddToEnd(endOfPostNode);
        }
    }

    internal bool InitIssuedQty(ScenarioOptions a_options)
    {
        if (m_mr.UnConsumedIssuedQty > 0)
        {
            if (m_mr.UnConsumedIssuedQty >= TotalRequiredQty)
            {
                // The material requirement has already been satisfied.
                m_mr.AllocateIssuedQty(TotalRequiredQty);

                return true;
            }

            //Subtract issued
            TotalRequiredQty -= m_mr.UnConsumedIssuedQty;

            //Now round the qty since it wasn't fully satisfied
            TotalRequiredQty = a_options.RoundQty(TotalRequiredQty);

            if (TotalRequiredQty <= 0)
            {
                //After applying issued qty, there is nothing left to allocate
                return true;
            }

            m_issuedQty = m_mr.UnConsumedIssuedQty;
            m_mr.AllocateRemainingIssuedQty();
        }

        return false;
    }

    internal override bool IsSupplySourceEligible(QtySupplyNode a_node, ScenarioDetail a_sd)
    {
        if (a_node.SupplySource == null)
        {
            //TODO: This shouldn't happen. Fix this
            return true;
        }

        if (a_sd.ExtensionController.RunEligibleMaterialExtension) // a_act might be null when finding qty for thing such as TransferOrders.
        {
            if (a_node.SupplySource is Lot supplyLot)
            {
                bool? lotEligCust = a_sd.ExtensionController.IsEligibleMaterialSource(a_sd, FirstDemandDate, m_mr, m_act, a_node.SourceLot);
                if (lotEligCust.HasValue)
                {
                    return lotEligCust.Value;
                }
            }
            else if (a_node.SourceLot.Reason is InternalActivity activity)
            {
                bool? lotEligCust = a_sd.ExtensionController.IsEligibleMaterialSource(a_sd, FirstDemandDate, m_mr, m_act, activity, a_node.SourceLot);
                if (lotEligCust.HasValue)
                {
                    return lotEligCust.Value;
                }
            }
            else if (a_node.SourceLot.Reason is PurchaseToStock po)
            {
                bool? lotEligCust = a_sd.ExtensionController.IsEligibleMaterialSource(a_sd, FirstDemandDate, m_mr, m_act, po, a_node.SourceLot);
                if (lotEligCust.HasValue)
                {
                    return lotEligCust.Value;
                }
            }
        } 
        
        if (!AllowExpiredSupply && a_node.Expired)
        {
            return false;
        }

        //Check lot tracking:
        if (!m_mr.IsLotElig(a_node.SupplySource.SupplyLot, a_sd.UsedAsEligibleLotsLotCodes))
        {
            return false;
        }

        if (!m_mr.ShelfLifeRequirement.IsUsable(a_node.SupplySource.SupplyLot, a_node.Date))
        {
            return false;
        }

        if (m_mr.WearRequirement.MaxWearAmount < a_node.SupplySource.SupplyLot.WearData.WearAmount)
        {
            return false;
        }

        //if (a_matlAllocation != null)
        //{
        //    IMaterialAllocation ima = a_matlAllocation;
        //    if (ima != null && SourceQtyDataReason != null)
        //    {
        //        SourceQtyData sqd = SourceQtyDataReason.SourceQtyData;
        //        if (ima.MaterialSourcing == SchedulerDefinitions.ItemDefs.MaterialSourcing.Exclusive && sqd.QtyConsumed)
        //        {
        //            eligibleMatl = false;
        //        }

        //        if (ima.MinSourceQty > 0 && sqd.AvailableQty < ima.MinSourceQty)
        //        {
        //            // The material source is ineligible because it has less than the required minimum amount of material the requirement allows its sources of material to have. 
        //            eligibleMatl = false;
        //        }

        //        if (ima.MaxSourceQty > 0 && sqd.AvailableQty > ima.MaxSourceQty)
        //        {
        //            eligibleMatl = false;
        //        }
        //    }
        //}

        return true;
    }



    internal void AllocateRemainingDemandFromLeadTime(Inventory a_inv, long a_leadTimeAvailability)
    {
        foreach (QtyDemandNode node in this)
        {
            if (node.UnallocatedQty > 0m)
            {
                if (a_leadTimeAvailability <= node.Date)
                {
                    RemainingQty -= node.UnallocatedQty;
                    node.AllocateRemainingSupplyFromLeadTime(a_inv);
                }
                else if (!AllowMultiWarehouseSupply)
                {
                    //If this node cannot use this lead time, then we don't need to check the others since it all has to come from a single warehouse.
                    break;
                }
            }
        }
    }

    internal void AllocateRemainingDemandFromPastPlanningHorizonIfPossible(Inventory a_inv, long a_planningHorizon)
    {
        foreach (QtyDemandNode node in this)
        {
            if (node.UnallocatedQty > 0m)
            {
                if (a_planningHorizon <= node.Date)
                {
                    RemainingQty -= node.UnallocatedQty;
                    node.AllocateShortage(a_inv, true);
                }
                else if (!AllowMultiWarehouseSupply)
                {
                    //If this node cannot use this lead time, then we don't need to check the others since it all has to come from a single warehouse.
                    break;
                }
            }
        }
    }

    internal QtyDemandNode CreateQtyNode(long a_time, decimal a_qty)
    {
        QtyDemandNode demandNode = new (a_time, m_act, m_mr, a_qty);
        return demandNode;
    }

    internal void ConsumeAllocations(long a_simClock, List<EventBase> a_futureConsumptionEvents)
    {
        long lastAdjustmentDate = Last.Date;
        foreach (QtyDemandNode qtyNode in this)
        {
            qtyNode.ConsumeAllocatedQty(a_simClock, a_futureConsumptionEvents, m_act);
        }
        
        //Create a disposal check at the end of this demands allocations in case the storage should be emptied
        foreach (ItemStorage itemStorage in ItemStorageCache)
        {
            if (itemStorage.DisposeImmediately)
            {
                FutureStorageDisposalEvent potentialDisposal = new (lastAdjustmentDate, itemStorage, m_demandSource);
                a_futureConsumptionEvents.Add(potentialDisposal);
            }
        }

        //Track usage
        foreach (ItemStorage itemStorage in ItemStorageCache)
        {
            long usageEnd = itemStorage.StorageArea.ScheduleUsage();
            if (usageEnd > 0)
            {
                a_futureConsumptionEvents.Add(new StorageAreaUsageEvent(usageEnd, itemStorage.StorageArea, false));
            }
        }

        //Lock the connector during the scheduled period
        long? connectorEnd = Connector?.FlowRangeConstraint.ScheduleUsage();
        if (connectorEnd is > 0)
        {
            a_futureConsumptionEvents.Add(new StorageAreaUsageEvent(connectorEnd.Value, Connector));
        }
    }

    /// <summary>
    /// Creates a shortage adjustment for all remaining demand nodes that have not been fully allocated
    /// </summary>
    /// <param name="a_warehouses"></param>
    internal void AllocateShortage(WarehouseArrayList a_warehouses)
    {
        Inventory inventoryForShortage = null;
        //'Satisfied' by non-constraint
        if (m_mr.Warehouse != null)
        {
            inventoryForShortage = m_mr.Warehouse.Inventories[m_mr.Item.Id];
        }
        else
        {
            foreach (Warehouse warehouse in a_warehouses)
            {
                inventoryForShortage = warehouse.Inventories[m_mr.Item.Id];
                if (inventoryForShortage != null)
                {
                    break;
                }
            }
        }

        if (inventoryForShortage != null)
        {
            foreach (QtyDemandNode node in this)
            {
                if (node.UnallocatedQty >= 0m)
                {
                    RemainingQty -= node.UnallocatedQty;
                    node.AllocateShortage(inventoryForShortage, false);
                }
            }
        }
    }

    internal List<ItemStorage> ItemStorageCache;

    /// <summary>
    /// Stores the available item storage for use after consumption
    /// </summary>
    /// <param name="a_itemStorageSources"></param>
    internal void CacheItemStorageSources(List<ItemStorage> a_itemStorageSources)
    {
        ItemStorageCache = a_itemStorageSources;
    }

    private IInterval m_usageDuration;
    private ScenarioOptions m_scenarioOptions;

    internal IInterval GetUsageRange(SchedulableInfo a_si)
    {
        if (m_usageDuration != null)
        {
            return m_usageDuration;
        }

        //Calculate based on product availability
        switch (m_mr.MaterialUsedTiming)
        {
            case MaterialRequirementDefs.EMaterialUsedTiming.DuringSetup:
                if (a_si.m_setupFinishDate - a_si.m_scheduledStartDate > 0)
                {
                    m_usageDuration = new Interval(a_si.m_scheduledStartDate, a_si.m_setupFinishDate);
                }
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle:
                if (a_si.m_productionFinishDate - a_si.m_setupFinishDate > 0)
                {
                    long usageStart = a_si.m_setupFinishDate;
                    if (MR.RequireFirstTransferAtSetup)
                    {
                        usageStart = a_si.m_scheduledStartDate;
                    }
                    m_usageDuration = new Interval(usageStart, a_si.m_productionFinishDate);
                }
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.FirstAndLastProductionCycle:
                if (a_si.m_productionFinishDate - a_si.m_setupFinishDate > 0)
                {
                    long usageStart = a_si.m_setupFinishDate;
                    if (MR.RequireFirstTransferAtSetup)
                    {
                        usageStart = a_si.m_scheduledStartDate;
                    }
                    m_usageDuration = new Interval(usageStart, a_si.m_productionFinishDate);
                }
                break;
        }

        if (m_usageDuration == null)
        {
            //We didn't set it from an above usage, use the default range
            m_usageDuration = new Interval(First.Date, Last.Date);
        }

        return m_usageDuration;
    }

    internal RequiredCapacity GetRequiredCapacity(SchedulableInfo a_si)
    {
        RequiredCapacity capacity = null;
        if (m_mr.MaterialUsedTiming == MaterialRequirementDefs.EMaterialUsedTiming.DuringSetup)
        {
            //Nothing to backcalculate here
        }
        else if (m_mr.MaterialUsedTiming == MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle || m_mr.MaterialUsedTiming == MaterialRequirementDefs.EMaterialUsedTiming.FirstAndLastProductionCycle)
        {
            capacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
        }
        else
        {
            #if DEBUG
            if (capacity == null)
            {
                throw new DebugException("A SAConnector was not available due to intersecting transfer time but the MR was not configured to use material transfer time");
            }
            #endif
        }

        return capacity;
    }

    internal override void ResetForAllocation()
    {
        TotalRequiredQty = m_act.m_mrQtys[m_mr.Id];
        if (m_mr.RequirementType == MaterialRequirementDefs.requirementTypes.BuyDirect)
        {
            // No inventory planning is done for BuyDirect requirements.
            RemainingQty = 0;
        }
        else if (m_mr.IssuedComplete)
        {
            // The mateiral has already been issued to the activity.
            RemainingQty = 0;
        }
        else
        {
            InitIssuedQty(m_scenarioOptions);
            RemainingQty = TotalRequiredQty;
        }

        base.ResetForAllocation();
    }

    internal void AllocateLastStorageAreaNode()
    {
        if (m_lastUsageDate != 0 && LastStorageAreaSupply != null)
        {
            LastStorageAreaSupply.AllocateUsage(m_lastUsageDate);
        }
    }
}