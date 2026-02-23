using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Don't be confused about this. It has nothing to do with overlap or material overlap or
/// keeping track of inventory. It is used only to plot and track inventory levels in UI and reports.
/// </summary>
public partial class Adjustment
{
    internal Adjustment(Adjustment a_original)
    {
        m_time = a_original.m_time;
        m_changeQty = a_original.m_changeQty;
        m_inventory = a_original.Inventory;
        m_storage = a_original.Storage;
    }

    internal Adjustment(Inventory a_inv, long a_time, decimal a_changeQty)
    {
        #if DEBUG
        // Adjustments must be one of the following types:
        //if (a_reason is not (InternalActivity or BaseOperation or PurchaseToStock or Inventory or Demand.ForecastShipment or Demand.SalesOrderLineDistribution or Demand.TransferOrderDistribution or Scheduler.Lot))
        //{
        //    throw new DebugException("Invalid Adjustment reason type specified in constructor.");
        //}
        #endif
        m_inventory = a_inv;
        m_time = a_time;
        m_changeQty = a_changeQty;
        Id = BaseId.NEW_OBJECT_ID;
    }

    internal Adjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment)
    {
        #if DEBUG
        // Adjustments must be one of the following types:
        //if (a_reason is not (InternalActivity or BaseOperation or PurchaseToStock or Inventory or Demand.ForecastShipment or Demand.SalesOrderLineDistribution or Demand.TransferOrderDistribution or Scheduler.Lot))
        //{
        //    throw new DebugException("Invalid Adjustment reason type specified in constructor.");
        //}
        #endif
        m_inventory = a_inv;
        m_time = a_time;
        m_changeQty = a_changeQty;
        m_storage = a_storageAdjustment;
        Id = BaseId.NEW_OBJECT_ID;
    }

    private decimal m_changeQty;

    /// <summary>
    /// The amount of material produced or consumed.
    /// </summary>
    public decimal ChangeQty => m_changeQty;

    private long m_time;

    /// <summary>
    /// The time the material is consumed.
    /// </summary>
    public long Time => m_time;

    /// <summary>
    /// The time the material is consumed.
    /// </summary>
    public DateTime AdjDate => new (m_time);

    private Inventory m_inventory;

    public Inventory Inventory => m_inventory;

    protected InventoryDefs.EAdjustmentType m_type;

    public InventoryDefs.EAdjustmentType AdjustmentType
    {
        get => m_type;
    }

    public virtual BaseIdObject GetReason() => m_inventory;

    public bool HasStorage => m_storage != null;
    public bool HasLotStorage => m_storage?.Lot != null;

    /// <summary>
    /// if Reason is a Demand. It returns the priority for that demand. Otherwise 5
    /// </summary>
    /// <returns></returns>
    public virtual int ReasonPriority
    {
        get => 
        //TODO: 
        //if (Reason is Demand.SalesOrderLineDistribution)
        //{
        //    return ((Demand.SalesOrderLineDistribution)Reason).Priority;
        //}

        //if (Reason is Demand.ForecastShipment)
        //{
        //    return ((Demand.ForecastShipment)Reason).Forecast.Priority;
        //}

        //if (Reason is Demand.TransferOrderDistribution)
        //{
        //    return ((Demand.TransferOrderDistribution)Reason).TransferOrder.Priority;
        //}

        //if (Reason is Inventory)
        //{
        //    return ((Inventory)Reason).SafetyStockJobPriority;
        //}

        5; //TODO: Why?
    }
}
