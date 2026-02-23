using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.Common.Exceptions;

namespace PT.Scheduler.MRP;

internal class MrpSupply
{
    private decimal m_unallocatedQty;

    internal decimal UnallocatedQty => m_unallocatedQty;

    protected bool m_originallyLotPegged;
    internal bool OriginallyLotPegged => m_originallyLotPegged;

    internal readonly DateTime SupplyDate;

    internal readonly decimal SupplyQty;

    internal MrpSupply(DateTime a_supplyDate, decimal a_supplyQty)
    {
        SupplyDate = a_supplyDate;
        SupplyQty = a_supplyQty;
        m_unallocatedQty = SupplyQty;
    }

    internal void Allocate(decimal a_qty, MrpDemand a_demand, List<Pair<BaseIdObject, decimal>> a_allocationQtys)
    {
        m_unallocatedQty -= a_qty;

        if (m_unallocatedQty < 0)
        {
            throw new PTException("The unallocated quantity of an existing job has fallen below 0.");
        }

        BaseIdObject demandReason = null;
        if (a_demand is ActivityMrpDemand actDemand)
        {
            demandReason = actDemand.Activity;
        }
        else if (a_demand is SalesOrderMrpDemand soDemand)
        {
            demandReason = soDemand.Distribution;
        }
        else
        {
            return;
        }

        Pair<BaseIdObject, decimal> pair;
        if (a_allocationQtys.Count > 0 && a_allocationQtys[^1].value1 == demandReason)
        {
            pair = a_allocationQtys[^1];
            pair.value2 += a_qty;
        }
        else
        {
            pair = new Pair<BaseIdObject, decimal>(demandReason, a_qty);
            a_allocationQtys.Add(pair);
        }
    }

    private bool needDateSet;

    internal bool NeedDateSet
    {
        get => needDateSet;

        set => needDateSet = true;
    }
}

internal class PurchaseOrderSupply : MrpSupply
{
    internal readonly PurchaseToStock PurchaseOrder;
    
    internal PurchaseOrderSupply(PurchaseToStock a_po, DateTime a_supplyDate, decimal a_poQty) 
        : base(a_supplyDate, a_poQty)
    {
        PurchaseOrder = a_po;
    }
}

internal class ActivitySupply : MrpSupply
{
    internal readonly InternalActivity Activity;

    internal ActivitySupply(InternalActivity a_act, Inventory a_inv, decimal a_supplyQty, DateTime a_supplyDate)
        : base(a_supplyDate, a_supplyQty)
    {
        Activity = a_act;
        string lotCode = a_act.Operation.Products.GetByInventoryId(a_inv.Id)?.LotCode;
        //MRP clears all lot codes for all jobs, regardless of commitment status. It keeps any codes that don't start with MRP.
        // MRP can also repeg that job from existing supplies to keep similar lot pegging. This means that originally lot pegged only applies to 
        // codes that aren't MRP codes.
        if (!string.IsNullOrEmpty(lotCode) && !lotCode.StartsWith("MRP"))
        {
            //This supply already has a lot pegging before MRP processing.
            m_originallyLotPegged = true;
        }
    }
}

