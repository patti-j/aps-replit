using System.Collections;
using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Extensions;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation;

public partial class MRSupply : IEnumerable<Adjustment>
{
    internal MRSupply(BaseIdGenerator a_sdIdGen)
    {
        m_supplyAdjustments = new AdjustmentArray(a_sdIdGen);
    }
    //public Dictionary<long, MRSupplyNode> SupplySet => m_nodes;

    IEnumerator<Adjustment> IEnumerable<Adjustment>.GetEnumerator()
    {
        return m_supplyAdjustments.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return m_supplyAdjustments.GetEnumerator();
    }

    public override string ToString()
    {
        return string.Format("{0} Inventories, activities, or purchases.".Localize(), m_supplyAdjustments.Count);
    }

    internal DateTime GetSupplyDateTime(InternalActivity a_ia)
    {
        DateTime baseFinishDate;

        if (a_ia.Scheduled)
        {
            baseFinishDate = a_ia.Batch.PostProcessingEndDateTime;
        }
        else if (a_ia.ReportedFinishDateSet)
        {
            baseFinishDate = a_ia.ReportedFinishDate;
        }
        else
        {
            return PTDateTime.MinDateTime;
        }

        if (a_ia.Operation.Products.Count > 0)
        {
            baseFinishDate = baseFinishDate.AddTicks(a_ia.Operation.Products[0].MaterialPostProcessingTicks);
        }

        return baseFinishDate;
    }

    internal DateTime GetSupplyDateTime(PurchaseToStock a_pts)
    {
        return a_pts.AvailableDate;
    }

    internal DateTime GetSupplyDateTime(TransferOrderDistribution a_orderDemand)
    {
        return a_orderDemand.ScheduledReceiveDate;
    }

    /// <summary>
    /// Returns latest availability datetime based on supply source data
    /// </summary>
    /// <param name="a_iOp"></param>
    /// <param name="a_sd"></param>
    /// <returns></returns>
    internal DateTime GetLatestSupplySourceDate(InternalOperation a_iOp, ScenarioDetail a_sd)
    {
        DateTime latestDate = PTDateTime.MinDateTime;

        foreach (Adjustment adjustment in m_supplyAdjustments)
        {
            DateTime supplyDate;
            if (adjustment is ActivityAdjustment actAdjustment)
            {
                supplyDate = GetSupplyDateTime(actAdjustment.Activity);

                if (a_sd != null && a_sd.ExtensionController.RunAutoJoinExtension)
                {
                    DateTime? updatedDate = a_sd.ExtensionController.CalculateLatestMaterialAvailability(a_sd, actAdjustment, actAdjustment.Activity);
                    if (updatedDate.HasValue)
                    {
                        supplyDate = updatedDate.Value;
                    }
                }

                if (supplyDate > latestDate)
                {
                    latestDate = supplyDate;
                }
            }
            else if (adjustment is PurchaseOrderAdjustment poAdjustment)
            {
                supplyDate = GetSupplyDateTime(poAdjustment.PurchaseOrder);
                if (supplyDate > latestDate)
                {
                    latestDate = supplyDate;
                }
            }
            //TODO: SA
            //else if (adjustment is TransferOrderDistribution orderDemand)
            //{
            //    supplyDate = GetSupplyDateTime(orderDemand);
            //    if (supplyDate > latestDate)
            //    {
            //        latestDate = supplyDate;
            //    }
            //}
        }

        return latestDate;
    }

    /// <summary>
    /// Returns latest availability datetime based on supply source data
    /// </summary>
    /// <returns></returns>
    internal DateTime GetEarliestSupplySourceDate()
    {
        DateTime earliestDate = PTDateTime.MaxDateTime;

        foreach (Adjustment adjustment in m_supplyAdjustments)
        {
            DateTime supplyDate;
            if (adjustment is ActivityAdjustment actAdjustment)
            {
                supplyDate = GetSupplyDateTime(actAdjustment.Activity);
                if (supplyDate != PTDateTime.MinDateTime && supplyDate < earliestDate)
                {
                    earliestDate = supplyDate;
                }
            }
            else if (adjustment is PurchaseOrderAdjustment poAdjustment)
            {
                supplyDate = GetSupplyDateTime(poAdjustment.PurchaseOrder);
                if (supplyDate < earliestDate)
                {
                    earliestDate = supplyDate;
                }
            }
            //TODO: SA
            //else if (node.Supply is TransferOrderDistribution orderDemand)
            //{
            //    supplyDate = GetSupplyDateTime(orderDemand);
            //    if (supplyDate < earliestDate)
            //    {
            //        earliestDate = supplyDate;
            //    }
            //}
        }

        return earliestDate;
    }

    internal Inventory? GetLatestSupplyInventory(ScenarioDetail a_sd)
    {
        DateTime latestDate = PTDateTime.MinDateTime;
        Inventory latestSupplyInventory = null;

        foreach (Adjustment adjustment in m_supplyAdjustments)
        {
            DateTime supplyDate;
            if (adjustment is ActivityAdjustment actAdjustment)
            {
                supplyDate = GetSupplyDateTime(actAdjustment.Activity);

                if (a_sd != null && a_sd.ExtensionController.RunAutoJoinExtension)
                {
                    DateTime? updatedDate = a_sd.ExtensionController.CalculateLatestMaterialAvailability(a_sd, actAdjustment, actAdjustment.Activity);
                    if (updatedDate.HasValue)
                    {
                        supplyDate = updatedDate.Value;
                    }
                }

                if (supplyDate > latestDate)
                {
                    latestDate = supplyDate;
                    latestSupplyInventory = actAdjustment.Inventory;
                }
            }
            else if (adjustment is PurchaseOrderAdjustment poAdjustment)
            {
                supplyDate = GetSupplyDateTime(poAdjustment.PurchaseOrder);
                if (supplyDate > latestDate)
                {
                    latestDate = supplyDate;
                    latestSupplyInventory = poAdjustment.Inventory;
                }
            }
            //TODO: SA
            //else if (adjustment is TransferOrderDistribution orderDemand)
            //{
            //    supplyDate = GetSupplyDateTime(orderDemand);
            //    if (supplyDate > latestDate)
            //    {
            //        latestDate = supplyDate;
            //    }
            //}
        }
        return latestSupplyInventory;
    }

    /// <summary>
    /// Consolidates multiple transfer order adjustments into a more readable description
    /// </summary>
    public class CondensedSupplyDescription
    {
        internal InternalActivity SourceActivity;
        internal PurchaseToStock SourcePO;
        internal Inventory SourceInventory;
        internal TransferOrderDistribution SourceTransfer;
        public long FirstTime;
        internal long LastTime;
        private StorageArea StorageArea;
        internal decimal Qty;
        internal bool Expired;

        public CondensedSupplyDescription(InternalActivity a_act)
        {
            SourceActivity = a_act;
        }

        public CondensedSupplyDescription(PurchaseToStock a_po)
        {
            SourcePO = a_po;
        }

        public CondensedSupplyDescription(Inventory a_inv)
        {
            SourceInventory = a_inv;
        }

        public CondensedSupplyDescription(TransferOrderDistribution a_tod)
        {
            SourceTransfer = a_tod;
        }

        public void Add(Adjustment a_adj)
        {
            if (FirstTime == 0)
            {
                FirstTime = a_adj.Time;
            }
            
            Qty += a_adj.Qty;
            if (a_adj.Time > LastTime)
            {
                LastTime = a_adj.Time;
            }

            StorageArea = a_adj.Storage?.StorageArea;
            Expired = a_adj.Expired;
        }

        public string GetDescription()
        {
            StringBuilder sb = new ();
            if (SourceActivity != null)
            {
                InternalOperation io = SourceActivity.Operation;
                ManufacturingOrder mo = io.ManufacturingOrder;
                Job job = mo.Job;

                if (FirstTime == LastTime)
                {
                    sb.AppendFormat("{0} on {1} from Job '{2}'".Localize(), Qty, new DateTime(FirstTime).ToDisplayTime().ToDateTime(), job.Name);
                }
                else
                {
                    sb.AppendFormat("{0} over {1} from {2} to {3} from Job '{4}'".Localize(), Qty, TimeSpan.FromTicks(LastTime - FirstTime).ToReadableStringHourPrecision(), new DateTime(FirstTime).ToDisplayTime().ToDateTime(), new DateTime(LastTime).ToDisplayTime().ToDateTime(), job.Name);
                }

                if (job.ManufacturingOrders.Count > 1)
                {
                    sb.AppendFormat(", MO '{0}'".Localize(), mo.Name);
                }

                if (mo.OperationManager.Count > 1)
                {
                    sb.AppendFormat(", Op '{0}'".Localize(), io.Name);
                }

                if (io.Activities.Count > 1)
                {
                    sb.AppendFormat(", Activity '{0}' on {1}".Localize(), SourceActivity.Id, SourceActivity.ResourcesUsed);
                }
                else
                {
                    sb.AppendFormat(" on Resource(s): {0}".Localize(), io.ResourcesUsed);
                }

                sb.AppendFormat(" from Storage: '{0}'".Localize(), StorageArea.Name);

                if (Expired)
                {
                    sb.Append(". Expired");
                }
            }
            else if (SourcePO != null)
            {
                if (FirstTime == LastTime)
                {
                    sb.AppendFormat("{0} on {1} from Purchase '{2}'".Localize(), Qty, new DateTime(FirstTime).ToDisplayTime().ToDateTime(), SourcePO.Name);
                }
                else
                {
                    sb.AppendFormat("{0} over {1} from {2} to {3} from Purchase '{4}'".Localize(), Qty, TimeSpan.FromTicks(LastTime - FirstTime).ToReadableStringHourPrecision(), new DateTime(FirstTime).ToDisplayTime().ToDateTime(), new DateTime(LastTime).ToDisplayTime().ToDateTime(), SourcePO.Name);
                }

                sb.AppendFormat(" from Storage: '{0}'".Localize(), StorageArea.Name);
                if (Expired)
                {
                    sb.Append(". Expired");
                }
            }
            else if (SourceTransfer != null)
            {
                if (FirstTime == LastTime)
                {
                    sb.AppendFormat("{0} on {1} from Transfer '{2}'".Localize(), Qty, new DateTime(FirstTime).ToDisplayTime().ToDateTime(), SourceTransfer.TransferOrder.Name);
                }
                else
                {
                    sb.AppendFormat("{0} over {1} from {2} to {3} from Transfer '{4}'".Localize(), Qty, TimeSpan.FromTicks(LastTime - FirstTime).ToReadableStringHourPrecision(), new DateTime(FirstTime).ToDisplayTime().ToDateTime(), new DateTime(LastTime).ToDisplayTime().ToDateTime(), SourceTransfer.TransferOrder.Name);
                }

                sb.AppendFormat(" from Storage: '{0}'".Localize(), StorageArea.Name);
                if (Expired)
                {
                    sb.Append(". Expired");
                }
            }
            else if (SourceInventory != null)
            {
                if (FirstTime == LastTime)
                {
                    sb.AppendFormat("{0} from Warehouse1 '{1}'".Localize(), Qty, SourceInventory.Warehouse.Name);
                }
                else
                {
                    sb.AppendFormat("{0} over {1} from {2} to {3} from Warehouse '{4}'".Localize(), Qty, TimeSpan.FromTicks(LastTime - FirstTime).ToReadableStringHourPrecision(), new DateTime(FirstTime).ToDisplayTime().ToDateTime(), new DateTime(LastTime).ToDisplayTime().ToDateTime(), SourceInventory.Warehouse.Name);
                }

                if (StorageArea != null)
                {
                    sb.AppendFormat(" from Storage: '{0}'".Localize(), StorageArea.Name);
                }
                else
                {
                    //Lead time
                    sb.AppendFormat(" from Lead Time".Localize());
                }
            }

            return sb.ToString();
        }
    }

    public string GetDescription()
    {
        Dictionary<(BaseId, BaseId), CondensedSupplyDescription> supplyDescriptions = new ();

        foreach (Adjustment adjustment in m_supplyAdjustments)
        {
            if (adjustment.HasLotStorage)
            {
                //This adjustment pulled from a known source
                Lot sourceLot = adjustment.Storage.Lot;
                BaseId storageAreaId = adjustment.Storage.StorageArea.Id;
                if (sourceLot.Reason is InternalActivity activity)
                {
                    CondensedSupplyDescription desc;
                    if (!supplyDescriptions.TryGetValue((activity.Id, storageAreaId), out desc))
                    {
                        desc = new CondensedSupplyDescription(activity);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((activity.Id, storageAreaId), desc);
                        continue;
                    }
                    
                    desc.Add(adjustment);
                }
                else if (sourceLot.Reason is PurchaseToStock po)
                {
                    if (!supplyDescriptions.TryGetValue((po.Id, storageAreaId), out CondensedSupplyDescription desc))
                    {
                        desc = new CondensedSupplyDescription(po);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((po.Id, storageAreaId), desc);
                        continue;
                    }

                    desc.Add(adjustment);
                }
                else if (sourceLot.Reason is TransferOrderDistribution orderDemand)
                {
                    if (!supplyDescriptions.TryGetValue((orderDemand.Id, storageAreaId), out CondensedSupplyDescription tODesc))
                    {
                        tODesc = new CondensedSupplyDescription(orderDemand);
                        tODesc.Add(adjustment);
                        supplyDescriptions.Add((orderDemand.Id, storageAreaId), tODesc);
                        continue;
                    }

                    tODesc.Add(adjustment);
                }
                else
                {
                    Inventory inv = adjustment.Inventory;

                    CondensedSupplyDescription desc;
                    if (!supplyDescriptions.TryGetValue((inv.Id, storageAreaId), out desc))
                    {
                        desc = new CondensedSupplyDescription(inv);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((inv.Id, storageAreaId), desc);
                        continue;
                    }

                    desc.Add(adjustment);
                }
            }
            else
            {
                //Could be leadtime
                if (adjustment is MaterialRequirementLeadTimeAdjustment)
                {
                    Inventory inv = adjustment.Inventory;

                    CondensedSupplyDescription desc;
                    if (!supplyDescriptions.TryGetValue((inv.Id, BaseId.NULL_ID), out desc))
                    {
                        desc = new CondensedSupplyDescription(inv);
                        desc.Add(adjustment);
                        supplyDescriptions.Add((inv.Id, BaseId.NULL_ID), desc);
                        continue;
                    }

                    desc.Add(adjustment);
                }
            }
        }

        StringBuilder sb = new ();

        supplyDescriptions.Values.OrderByDescending(d => d.FirstTime).ToList().ForEach(desc =>
        {
            if (sb.Length > 0)
            {
                sb.Append("; ");
            }
            sb.Append(desc.GetDescription());
        });

        return sb.ToString();
    }

    internal decimal GetSupplyFromActivities
    {
        get
        {
            decimal qty = 0;
            foreach (Adjustment adjustment in m_supplyAdjustments)
            {
                if (adjustment is MaterialRequirementAdjustment mrAdjustment)
                {
                    if (mrAdjustment.HasLotStorage)
                    {
                        if (mrAdjustment.Storage.Lot.Reason is InternalActivity)
                        {
                            qty += adjustment.ChangeQty;
                        }
                    }

                }
            }

            return decimal.Abs(qty);
        }
    }

    internal decimal GetSupplyFromPurchases
    {
        get
        {
            decimal qty = 0;
            foreach (Adjustment adjustment in m_supplyAdjustments)
            {
                if (adjustment is MaterialRequirementAdjustment mrAdjustment)
                {
                    if (mrAdjustment.HasLotStorage)
                    {
                        if (mrAdjustment.Storage.Lot.Reason is PurchaseToStock)
                        {
                            qty += adjustment.ChangeQty;
                        }
                    }

                }
            }

            return decimal.Abs(qty);
        }
    }

    internal decimal GetSupplyFromInventories
    {
        get
        {
            decimal qty = 0;
            foreach (Adjustment adjustment in m_supplyAdjustments)
            {
                if (adjustment.HasLotStorage && adjustment.Storage.Lot.LotSource == ItemDefs.ELotSource.Inventory)
                {
                    qty += adjustment.ChangeQty;
                }
            }

            return decimal.Abs(qty);
        }
    }

    internal decimal GetSupplyFromAllSources
    {
        get
        {
            decimal qty = 0;
            foreach (Adjustment adjustment in m_supplyAdjustments)
            {
                qty += adjustment.ChangeQty;
            }

            return decimal.Abs(qty);
        }
    }

    /// <summary>
    /// Returns true if all SupplyNodes are Inventory
    /// </summary>
    internal bool OnlySupplyingFromInventory
    {
        get
        {
            foreach (Adjustment adjustment in m_supplyAdjustments)
            {
                if (adjustment.Storage == null || adjustment.Storage.Lot.LotSource != ItemDefs.ELotSource.Inventory)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Returns a list of supplying warehouse names
    /// </summary>
    internal IEnumerable<string> GetSuppliedWarehouseNames
    {
        get
        {
            foreach (Adjustment adjustment in m_supplyAdjustments)
            {
                if (adjustment.HasLotStorage && adjustment.Storage.Lot.LotSource == ItemDefs.ELotSource.Inventory)
                {
                    yield return adjustment.Inventory.Warehouse.Name;
                }
            }
        }
    }
    
    /// <summary>
    /// Returns a list of supplying storage area names
    /// </summary>
    internal IEnumerable<string> GetSuppliedStorageAreaNames
    {
        get
        {
            if (m_supplyAdjustments.Count > 0)
            {
                HashSet<BaseId> storageAreaIds = new HashSet<BaseId>();
                foreach (Adjustment adjustment in m_supplyAdjustments)
                {
                    if (adjustment.HasStorage && storageAreaIds.AddIfNew(adjustment.Storage!.StorageArea.Id))
                    {
                        yield return adjustment.Storage.StorageArea.Name;
                    }
                }
            }
        }
    }

    /// <summary>
    /// returns true if any of the material sources are Estimate or Planned (or not Firm for POs)
    /// </summary>
    /// <returns></returns>
    internal bool AreAnySuppliesPlanned()
    {
        foreach (Adjustment adjustment in m_supplyAdjustments)
        {
            if (!adjustment.HasLotStorage)
            {
                continue;
            }

            if (adjustment.Storage.Lot.Reason is PurchaseToStock po && !po.Firm)
            {
                return true;
            }

            if (adjustment.Storage.Lot.Reason is InternalActivity act && act.Operation.ManufacturingOrder.Job.Commitment < JobDefs.commitmentTypes.Firm)
            {
                return true;
            }
        }

        return false;
    }

    public bool Empty => m_supplyAdjustments == null || m_supplyAdjustments.Count == 0;

    /// <summary>
    /// Whether this requirement sources from the provided sources.
    /// </summary>
    /// <param name="a_exclusive">Only these sources are used</param>
    /// <param name="a_inclusive">Whether all of these sources are used</param>
    /// <param name="a_sources">The adjustment types to check</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool SourcesFrom(bool a_exclusive, bool a_inclusive, params InventoryDefs.EAdjustmentType[] a_sources)
    {
        HashSet<InventoryDefs.EAdjustmentType> adjustmentTypes = a_sources.ToHashSet();
        HashSet<InventoryDefs.EAdjustmentType> usedTypes = null;
        
        if (a_inclusive)
        {
            usedTypes = a_sources.ToHashSet();
        }
        
        foreach (Adjustment adj in this)
        {
            if (adjustmentTypes.Contains(adj.AdjustmentType))
            {
                if (a_inclusive)
                {
                    usedTypes.Remove(adj.AdjustmentType);
                }
                else if (!a_exclusive)
                {
                    //Not exclusive or inclusive so as long as one of the types match, it's true
                    return true;
                }
            }
            else if (a_exclusive)
            {
                return false;
            }
        }

        if (a_inclusive)
        {
            return usedTypes.Count == 0; //all of the types were used
        }

        return true;
    }

    //Sort the data so we don't have to worry about sort timing when accessing the collection first after loading.
    internal void AfterRestoreAdjustmentReferences()
    {
        m_supplyAdjustments.Sort();
    }
}