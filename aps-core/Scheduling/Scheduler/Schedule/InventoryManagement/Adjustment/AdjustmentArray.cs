using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.InventoryManagement.Adjustment;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class AdjustmentArray : CustomSortedDictionary<BaseId, Adjustment>, IPTSerializable
{
    private readonly BaseIdGenerator m_adjustmentIdGenerator;

    #region IPTSerializable Members
    internal AdjustmentArray(IReader a_reader, BaseIdGenerator a_adjustmentIdGenerator)
        : base(a_reader)
    {
        m_adjustmentIdGenerator = a_adjustmentIdGenerator;
    }

    internal AdjustmentArray(BaseIdGenerator a_adjustmentIdGenerator)
    {
        m_adjustmentIdGenerator = a_adjustmentIdGenerator;
    }

    public AdjustmentArray(AdjustmentArray a_original)
    {
        if (a_original == null)
        {
            //This can happen if we are copying a cached or simulation array. It just means there aren't any adjustments to add.
            return;
        }
        m_adjustmentIdGenerator = a_original.m_adjustmentIdGenerator;
        Add(a_original);
    }

    /// <summary>
    /// This collection can only be used for storing previously created adjustments from other adjustment arrays
    /// </summary>
    internal AdjustmentArray()
    {
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    protected override void Serialize(IWriter a_writer, Adjustment a_value)
    {
        a_writer.Write(a_value.UniqueId);
        a_value.Serialize(a_writer);
    }

    /// <summary>
    /// Deserializes the correct adjustment
    /// </summary>
    /// <param name="a_reader"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    protected override Adjustment CreateInstance(IReader a_reader)
    {
        a_reader.Read(out int adjustmentClassId);
        switch (adjustmentClassId)
        {
            case PurchaseOrderDiscardAdjustment.UNIQUE_ID:
                return new PurchaseOrderDiscardAdjustment(a_reader);

            case ActivityAdjustment.UNIQUE_ID:
                return new ActivityAdjustment(a_reader);

            case PurchaseOrderAdjustment.UNIQUE_ID:
                return new PurchaseOrderAdjustment(a_reader);

            case MaterialRequirementAdjustment.UNIQUE_ID:
                return new MaterialRequirementAdjustment(a_reader);
            
            case ProductAdjustment.UNIQUE_ID:
                return new ProductAdjustment(a_reader);
            
            case MaterialRequirementLeadTimeAdjustment.UNIQUE_ID:
                return new MaterialRequirementLeadTimeAdjustment(a_reader);
            
            case MaterialRequirementShortageAdjustment.UNIQUE_ID:
                return new MaterialRequirementShortageAdjustment(a_reader);
            
            case ProductionDiscardAdjustment.UNIQUE_ID:
                return new ProductionDiscardAdjustment(a_reader);
            
            case OnHandLotAdjustment.UNIQUE_ID:
                return new OnHandLotAdjustment(a_reader); 
            
            case ProductionDisposalAdjustment.UNIQUE_ID:
                return new ProductionDisposalAdjustment(a_reader);

            case SafetyStockMrpAdjustment.UNIQUE_ID:
                return new SafetyStockMrpAdjustment(a_reader);

            case SalesOrderAdjustment.UNIQUE_ID:
                return new SalesOrderAdjustment(a_reader);

            case SalesOrderMrpDemandAdjustment.UNIQUE_ID:
                return new SalesOrderMrpDemandAdjustment(a_reader);
            
            case SalesOrderDisposalAdjustment.UNIQUE_ID:
                return new SalesOrderDisposalAdjustment(a_reader);
            
            case TransferOrderAdjustment.UNIQUE_ID:
                return new TransferOrderAdjustment(a_reader);
            
            case TransferOrderDiscardAdjustment.UNIQUE_ID:
                return new TransferOrderDiscardAdjustment(a_reader);
            
            case TransferOrderMrpDemandAdjustment.UNIQUE_ID:
                return new TransferOrderMrpDemandAdjustment(a_reader);
            
            case ForecastAdjustment.UNIQUE_ID:
                return new ForecastAdjustment(a_reader);

            case ForecastMrpDemandAdjustment.UNIQUE_ID:
                return new ForecastMrpDemandAdjustment(a_reader);
            
            case LotExpirationAdjustment.UNIQUE_ID:
                return new LotExpirationAdjustment(a_reader);
            
            case LotExpirationDisposalAdjustment.UNIQUE_ID:
                return new LotExpirationDisposalAdjustment(a_reader);
            
            case StorageAreaCleanoutAdjustment.UNIQUE_ID:
                return new StorageAreaCleanoutAdjustment(a_reader);

            default:
                throw new NotSupportedException($"Adjustment Type Id '{adjustmentClassId}' was not found");

        }
    }

    protected override Adjustment CreateInstance(IReader a_reader, BaseIdGenerator a_idGen)
    {
        return CreateInstance(a_reader);
    }

    [Browsable(false)]
    public override int UniqueId => 1207;
    #endregion

    public override string ToString()
    {
        return string.Format("Count={0}; sorted={1}".Localize(), Count, Sorted);
    }

    /// <summary>
    /// Return the setup of positive adjustments.
    /// </summary>
    public AdjustmentArray GetPositiveAdjustments(DateTime aMaxDate)
    {
        return GetAdjustments(true, aMaxDate);
    }

    /// <summary>
    /// Return the set of negative adjustments.
    /// </summary>
    public AdjustmentArray GetNegativeAdjustments(DateTime aMaxDate)
    {
        return GetAdjustments(false, aMaxDate);
    }

    /// <summary>
    /// Get the positive or negative adjustments.
    /// </summary>
    /// <param name="positive">true to get the positive adjustments. False to get the negative adjustments.</param>
    /// <returns>The positive or negative adjustments.</returns>
    private AdjustmentArray GetAdjustments(bool a_positive, DateTime aMaxDate)
    {
        AdjustmentArray aa = new (m_adjustmentIdGenerator);

        for (int i = 0; i < Count; ++i)
        {
            Adjustment a = this[i];
            if (a.AdjDate.Ticks <= aMaxDate.Ticks)
            {
                if (a_positive)
                {
                    if (a.ChangeQty > 0)
                    {
                        aa.Add(a);
                    }
                }
                else
                {
                    if (a.ChangeQty < 0)
                    {
                        aa.Add(a);
                    }
                }
            }
        }

        return aa;
    }

    internal new void Sort()
    {
        base.Sort();
    }

    public IAdjustment[] GetCondensedAdjustments()
    {
        Dictionary<string, AdjustmentCondensed> groupedAdjustments = new Dictionary<string, AdjustmentCondensed>();

        foreach (Adjustment adjustment in this)
        {
            string key = adjustment.Storage?.Lot?.Code ?? string.Empty;
            if (adjustment.HasStorage)
            {
               key += $"{adjustment.Time}-{adjustment.ReasonDescription}-{adjustment.AdjustmentType}-{adjustment.Storage!.StorageArea.ExternalId}";
            }
            else
            {
                key += $"{adjustment.Time}-{adjustment.ReasonDescription}-{adjustment.AdjustmentType}";
            }

            if (!groupedAdjustments.TryGetValue(key, out AdjustmentCondensed groupedAdjustment))
            {
                groupedAdjustments[key] = new AdjustmentCondensed(adjustment);
            }
            else
            {
                groupedAdjustment.ChangeQty += adjustment.ChangeQty;
            }
        }

        IAdjustment[] condensedAdjustments = new IAdjustment[groupedAdjustments.Count];

        int index = 0;
        foreach (AdjustmentCondensed condensedAdjustment in groupedAdjustments.Values)
        {
            condensedAdjustments[index] = (condensedAdjustment);
            index++;
        }
        return condensedAdjustments;
    }

    #region Buckets
    /// <summary>
    /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_bucketSpan"></param>
    /// <param name="a_numberOfBuckets"></param>
    /// <param name="a_bucketTypeToGet"></param>
    /// <param name="a_includePastAdjustments">if true, the first element in returned list will include any adjustments prior to a_startDate</param>
    /// <returns></returns>
    public List<decimal> GetBucketAdjustments(long a_startDate, long a_bucketSpan, int a_numberOfBuckets, BucketTypesToGet a_bucketTypeToGet, bool a_includePastAdjustments)
    {
        List<decimal> buckets = new ();

        if (a_bucketSpan <= 0 || a_numberOfBuckets <= 0 || a_startDate <= 0)
        {
            #if DEBUG
            throw new Exception("The arguments passed to GetBucketAdjustments are invalid. All values must be greater than or equal to zero.");
            #else
                return buckets;
            #endif
        }

        Sort();

        int adjustmentI = 0;
        decimal pastQty = 0;

        // Skip past all the nodes before the first bucket.
        for (; adjustmentI < Count; ++adjustmentI)
        {
            Adjustment adj = this[adjustmentI];

            if (adj.Time >= a_startDate)
            {
                break;
            }

            if (a_includePastAdjustments)
            {
                pastQty += GetAdjChangeQty(a_bucketTypeToGet, adj);
            }
        }

        if (a_includePastAdjustments)
        {
            buckets.Add(pastQty);
        }

        long bucketStart = a_startDate;
        long bucketEnd = bucketStart + a_bucketSpan;

        for (int bucketI = 0; bucketI < a_numberOfBuckets; ++bucketI)
        {
            decimal qty = 0;

            for (; adjustmentI < Count; ++adjustmentI)
            {
                Adjustment adj = this[adjustmentI];

                if (adj.Time >= bucketEnd)
                {
                    break;
                }

                qty += GetAdjChangeQty(a_bucketTypeToGet, adj);
            }

            buckets.Add(qty);

            bucketStart = bucketEnd;
            bucketEnd = bucketStart + a_bucketSpan;
        }

        return buckets;
    }

    /// <summary>
    /// returns adjustment qty if adjustment type matches the a_bucketTypeToGet otherwise returns 0
    /// </summary>
    /// <param name="a_bucketTypeToGet"></param>
    /// <param name="a_adj"></param>
    /// <returns></returns>
    private decimal GetAdjChangeQty(BucketTypesToGet a_bucketTypeToGet, Adjustment a_adj)
    {
        if (
            //(a_bucketTypeToGet == BucketTypesToGet.Forecast && (a_adj.Reason is ForecastShipment || a_adj.IsJobForecast())) ||
            (a_bucketTypeToGet == BucketTypesToGet.JobMaterial && a_adj is ActivityAdjustment matAdj && matAdj.IsJobMaterial()) ||
            (a_bucketTypeToGet == BucketTypesToGet.JobProduct && a_adj is ActivityAdjustment prodAdj && prodAdj.IsJobProduct()) ||
            (a_bucketTypeToGet == BucketTypesToGet.PurchaseToStock && a_adj is PurchaseOrderAdjustment) //||
            //(a_bucketTypeToGet == BucketTypesToGet.SalesOrder && (a_adj.Reason is SalesOrderLineDistribution || a_adj.IsJobSalesOrder())) ||
            //(a_bucketTypeToGet == BucketTypesToGet.TransferIn && a_adj.ChangeQty > 0 && (a_adj.Reason is TransferOrderDistribution || a_adj.IsJobTransferOrder())) ||
            //(a_bucketTypeToGet == BucketTypesToGet.TransferOut && a_adj.ChangeQty < 0 && (a_adj.Reason is TransferOrderDistribution || a_adj.IsJobTransferOrder())) ||
            //(a_bucketTypeToGet == BucketTypesToGet.LotExpirations && a_adj.Reason is Lot && a_adj.ChangeQty < 0) ||
            //(a_bucketTypeToGet == BucketTypesToGet.LotProductions && a_adj.Reason is Lot && a_adj.ChangeQty > 0))
            )
        {
            return a_adj.ChangeQty;
        }

        return 0;
    }

    public enum BucketTypesToGet
    {
        SalesOrder,
        Forecast,
        JobMaterial,
        TransferOut,
        JobProduct,
        PurchaseToStock,
        TransferIn,
        LotExpirations,
        LotProductions
    }
    #endregion
    #region Condensed

    private static List<BaseIdObject> CreateCondensedReasonsArrayList(List<BaseIdObject> a_original)
    {
        List<BaseIdObject> condensed = new ();
        for (int i = 0; i < a_original.Count; ++i)
        {
            if (!condensed.Contains(a_original[i]))
            {
                condensed.Add(a_original[i]);
            }
        }

        return condensed;
    }

    private static readonly object s_adjustmentsLock = new ();

    /// <summary>
    /// Return a version of this AdjustmentArray that only has one adjustment node per point in time. The node represents the final quantity of
    /// material.
    /// </summary>
    /// <param name="a_condenseReasons">Whether to condense all reasons that are at the same time</param>
    /// <param name="a_includeForecasts">Whether to include forecast adjustments in the collection</param>
    /// <returns></returns>
    /// <exception cref="PTException"></exception>
    public CondensedAdjustmentArray Condensed(Inventory a_inv, bool a_condenseReasons, bool a_includeForecasts, DateTime a_clockDate)
    {
        CondensedAdjustmentArray condensed = new ();
        List<BaseIdObject> reasons = new ();
        CondensedAdjustment cndAdj;
        const long c_startingInventoryTicks = 0;

        if (Count == 0)
        {
            reasons.Add(a_inv);
            cndAdj = new CondensedAdjustment(c_startingInventoryTicks, CreateCondensedReasonsArrayList(reasons), a_inv.OnHandQty, 0);
            condensed.Add(cndAdj);
        }
        else
        {
            lock (s_adjustmentsLock)
            {
                Sort();
                IAdjustment prev = new OnHandInventoryMrpAdjustment(a_inv, c_startingInventoryTicks, 0m);

                decimal onHandQty = 0m;

                for (int adjustmentI = 0; adjustmentI < Count; ++adjustmentI)
                {
                    Adjustment adjustment = this[adjustmentI];

                    if (adjustment.AdjDate < a_clockDate)
                    {
                        onHandQty += adjustment.ChangeQty;
                    }
                    else
                    {
                        prev = new OnHandInventoryMrpAdjustment(a_inv, c_startingInventoryTicks, onHandQty);
                        break;
                    }
                }

                reasons = new List<BaseIdObject>();

                for (int adjustmentI = 0; adjustmentI < Count; ++adjustmentI)
                {
                    Adjustment adjustment = this[adjustmentI];
                    if (adjustment.AdjDate < a_clockDate)
                    {
                        continue;
                    }

                    //if (!a_includeForecasts && (adjustment.Reason is ForecastShipment || adjustment is ActivityAdjustment actAdj && actAdj.IsJobForecast()))
                    //{
                    //    continue;
                    //}
#if DEBUG
                    if (adjustment.Time < prev.Time)
                    {
                        throw new PTException("AdjustmentArray.CondensedAdjustmentArray() sorting is mixed up.");
                    }
                    #endif
                    StorageArea currentSa = adjustment?.Storage?.StorageArea;
                    StorageArea prevSA = null;
                    if (prev is Adjustment prevAdjustment && prevAdjustment.HasStorage)
                    {
                        prevSA = prevAdjustment.Storage.StorageArea;
                    }

                    if (adjustment.Time > prev.Time 
                        || prev.AdjustmentType != adjustment.AdjustmentType 
                        || (!a_condenseReasons && (adjustment.GetReason() != prev.GetReason() || currentSa != prevSA)))
                    {
                        cndAdj = new CondensedAdjustment(prev.Time, CreateCondensedReasonsArrayList(reasons), onHandQty, 0);
                        condensed.Add(cndAdj);

                        reasons = new List<BaseIdObject>();
                    }

                    onHandQty += adjustment.ChangeQty;
                    reasons.Add(adjustment.GetReason());
                    prev = adjustment;
                }

                cndAdj = new CondensedAdjustment(prev.Time, CreateCondensedReasonsArrayList(reasons), onHandQty, 0);
                condensed.Add(cndAdj);
            }
        }

        return condensed;
    }

    private string GetActivityAdjustmentLotCode(ActivityAdjustment a_adj, Inventory a_inv)
    {
        InternalActivity act = a_adj.Activity;
        if (act == null)
        {
            return "";
        }

        if (a_adj.ChangeQty > 0) // Product
        {
            for (int i = 0; i < act.Operation.Products.Count; i++)
            {
                Product p = act.Operation.Products[i];
                if (p.Inventory.Id == a_inv.Id)
                {
                    return p.LotCode;
                }
            }
        }
        else if (a_adj.ChangeQty < 0) // Material
        {
            for (int i = 0; i < act.Operation.MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = act.Operation.MaterialRequirements[i];
                if (mr.Item != null && mr.Item.Id == a_inv.Item.Id && mr.Warehouse != null && mr.Warehouse.Id == a_inv.Warehouse.Id)
                {
                    if (mr.GetEligibleLotCount() == 0)
                    {
                        return "";
                    }

                    List<string> codes = new();
                    Dictionary<string, EligibleLot>.Enumerator lotCodeEnumer = mr.GetEligibleLotsEnumerator();
                    while (lotCodeEnumer.MoveNext())
                    {
                        codes.Add(lotCodeEnumer.Current.Key);
                    }

                    return string.Join(",", codes);
                }
            }
        }

        return "";
    }
    #endregion Condensed

    #region Inventory Plan
    /// <summary>
    /// Returns a new adjustment array including current adjustments plus new adjustments for material lead times.
    /// </summary>
    //public List<IAdjustment> AddLeadTimeAdjustments(JobManager a_jobs, Inventory a_inv)
    //{
    //    //Copy to a new unsorted array
    //    List<IAdjustment> newArray = new ();
    //    for (int i = 0; i < Count; ++i)
    //    {
    //        newArray.Add(this[i]);
    //    }

    //    foreach (Job job in a_jobs)
    //    {
    //        if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
    //        {
    //            foreach (InternalOperation baseOp in job.GetOperationsFromCurrentPaths())
    //            {
    //                if (baseOp.Scheduled)
    //                {
    //                    for (int mrI = 0; mrI < baseOp.MaterialRequirements.Count; mrI++)
    //                    {
    //                        MaterialRequirement mr = baseOp.MaterialRequirements[mrI];
    //                        if (!mr.BuyDirect && mr.Item.Id == a_inv.Item.Id && mr.Warehouse == null)
    //                        {
    //                            //NonConstraint materials cause an actualy adjustment, so don't add them twice.
    //                            bool onlyLeadTime = true;
    //                            for (int sourceI = 0; sourceI < mr.SupplySourcesCount; sourceI++)
    //                            {
    //                                MaterialRequirementDefs.SupplySource supplySource = mr.GetSupplySource(sourceI);
    //                                if (!supplySource.OnHandInventory && !supplySource.OnlyInventorysLeadTime)
    //                                {
    //                                    onlyLeadTime = false;
    //                                }
    //                            }

    //                            if (onlyLeadTime && mr.ConstraintType != MaterialRequirementDefs.constraintTypes.NonConstraint)
    //                            {
    //                                newArray.Add(new LeadTimeMrpAdjustment(a_inv, baseOp.ScheduledStartDate.Ticks, -mr.TotalRequiredQty, baseOp));
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    newArray.Sort();
    //    return newArray;
    //}

    /// <summary>
    /// Returns a new adjustment array including current adjustments plus new adjustments for lot expirations and new
    /// adjustments for future (later than clock) lot productions.
    /// </summary>
    public AdjustmentArray GetAdjustmentsOnOrBeforeDate(long a_limitTicks)
    {
        //Copy to a new unsorted array
        AdjustmentArray newArray = new ();
        for (int i = 0; i < Count; ++i)
        {
            Adjustment adjustment = this[i];
            if (adjustment.AdjDate.Ticks <= a_limitTicks)
            {
                newArray.Add(adjustment);
            }
        }

        return newArray;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_bucketSpan"></param>
    /// <param name="a_numberOfBuckets"></param>
    /// <param name="a_includeForecasts"></param>
    /// <param name="a_includePastAdjustments">if true, the first element in returned list will include any adjustments prior to a_startDate</param>
    /// <returns></returns>
    public List<decimal> GetBuckets(Inventory a_inv, long a_startDate, long a_bucketSpan, int a_numberOfBuckets, bool a_includeForecasts, bool a_includePastAdjustments)
    {
        List<decimal> buckets = new ();

        if (a_bucketSpan <= 0 || a_numberOfBuckets <= 0 || a_startDate <= 0)
        {
            #if DEBUG
            throw new Exception("The arguments passed to GetBuckets are invalid. All values must be greater than or equal to zero.");
            #else
                return buckets;
            #endif
        }

        Sort();

        decimal qty = a_inv.OnHandQty;
        int adjustmentI = 0;

        // Skip past all the nodes before the first bucket.
        for (; adjustmentI < Count; ++adjustmentI)
        {
            Adjustment adj = this[adjustmentI];

            if (adj.Time < a_startDate && (a_includeForecasts 
                                           || !(adj.AdjustmentType == InventoryDefs.EAdjustmentType.ForecastDemand 
                                           || adj is ActivityAdjustment actAdj && actAdj.IsJobForecast())))
            {
                qty += adj.ChangeQty;
            }
            else
            {
                break;
            }
        }

        if (a_includePastAdjustments)
        {
            buckets.Add(qty);
        }

        long bucketStart = a_startDate;
        long bucketEnd = bucketStart + a_bucketSpan;

        for (int bucketI = 0; bucketI < a_numberOfBuckets; ++bucketI)
        {
            for (; adjustmentI < Count; ++adjustmentI)
            {
                Adjustment adj = this[adjustmentI];

                if (adj.Time >= bucketEnd)
                {
                    break;
                }

                if (a_includeForecasts 
                    || !(adj.AdjustmentType == InventoryDefs.EAdjustmentType.ForecastDemand 
                    || adj is ActivityAdjustment actAdj && actAdj.IsJobForecast()))
                {
                    qty += adj.ChangeQty;
                }
            }

            buckets.Add(qty);

            bucketStart = bucketEnd;
            bucketEnd = bucketStart + a_bucketSpan;
        }

        return buckets;
    }
    #endregion

    internal void RestoreReferences(ScenarioDetail a_sd)
    {
        foreach (Adjustment adjustment in this)
        {
            adjustment.RestoreReferences(a_sd);
        }
    }

    internal AdjustmentArray ShallowCopy()
    {
        AdjustmentArray newArray = new AdjustmentArray(m_adjustmentIdGenerator);
        foreach (Adjustment adjustment in this)
        {
            newArray.Add(adjustment);
        }

        return newArray;
    }
}