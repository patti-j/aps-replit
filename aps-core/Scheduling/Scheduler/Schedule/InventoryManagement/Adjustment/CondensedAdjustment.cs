using System.Drawing;
using System.Text;

using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for CondensedAdjustment.
/// </summary>
public class CondensedAdjustment
{
    internal CondensedAdjustment(long a_time, List<BaseIdObject> a_reasons, decimal a_onHandQty, decimal a_storageAreaLevel, string a_lotCode = "", string a_storageArea = "")
    {
        m_time = a_time;
        m_reasons = a_reasons;
        onHandQty = a_onHandQty;
        m_lotCode = a_lotCode;
        m_storageArea = a_storageArea;
        m_storageAreaLevel = a_storageAreaLevel;
    }

    private readonly decimal onHandQty;

    /// <summary>
    /// The new projected on hand qty at that time after the event.
    /// </summary>
    public decimal OnHandQty => onHandQty;

    private readonly long m_time;

    /// <summary>
    /// The time the material is consumed.
    /// </summary>
    public long Time => m_time;


    private string m_lotCode = string.Empty;
    // The LotCode of each adjustments in the condensed, concatenated into a string
    public string LotCode => m_lotCode;
    
    private string m_storageArea = string.Empty;
    public string StorageArea => m_storageArea;
    private readonly decimal m_storageAreaLevel;

    /// <summary>
    /// The new projected on hand qty at that time after the event.
    /// </summary>
    public decimal StorageAreaLevel => m_storageAreaLevel;
    public DateTime AdjDate => new (m_time);

    public List<BaseIdObject> Reasons => m_reasons;

    private readonly List<BaseIdObject> m_reasons;

    /// <summary>
    /// The reason for the adjustment to inventory.
    /// The object will either be a PurchaseToStock or InternalActivity.
    /// </summary>
    public string Reason()
    {
        StringBuilder sb = new ();

        for (int i = 0; i < m_reasons.Count; ++i)
        {
            if (i > 0)
            {
                sb.Append(" ");
            }

            //TODO: SA
            //string reason = Adjustment.GetAdjustmentReason(m_reasons[i]);

            //if (m_reasons[i] is AdjustmentExpirable)
            //{
            //    reason += string.Format("{0} Expired".Localize(), reason);
            //}

            //sb.AppendFormat("[{0}]", reason);
        }

        return sb.ToString();
    }

    /// <summary>
    /// returns true if this condensed adjustment's reason is Inventory and that inventory is in a ResourceWarehouse (tank)
    /// At this point, this means that this condensed adjustment is either for the initial on-hand inventory or for the
    /// emptying tank adjustment.
    /// </summary>
    /// <returns></returns>
    public bool IsReasonTankInventory()
    {
        foreach (object o in m_reasons)
        {
            if (o is Inventory inv && inv.Warehouse is ResourceWarehouse)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if a_reason is Activity and change quantity greater than 0.
    /// </summary>
    /// <returns></returns>
    private bool IsJobProduct(BaseIdObject a_reason)
    {
        bool jobProduct = a_reason is BaseActivity && onHandQty > 0;
        if (jobProduct && a_reason is InternalActivity)
        {
            InternalActivity act = a_reason as InternalActivity;
            if (act.Operation.Job.Classification == JobDefs.classifications.TransferOrder)
            {
                return false;
            }
        }

        return jobProduct;
    }

    /// <summary>
    /// Returns true if a_reason is Activity, change qty less than zero and the Activity belongs to a Job with Classification of SalesOrder.
    /// </summary>
    /// <returns></returns>
    private bool IsJobSalesOrder(BaseIdObject a_reason)
    {
        InternalActivity act = a_reason as InternalActivity;
        return act != null && onHandQty < 0 && act.Operation.Job.Classification == JobDefs.classifications.SalesOrder;
    }

    /// <summary>
    /// Returns true if a_reason is Activity, change qty less than zero and the Activity belongs to a Job with Classification of Forecast.
    /// </summary>
    /// <returns></returns>
    private bool IsJobForecast(BaseIdObject a_reason)
    {
        InternalActivity act = a_reason as InternalActivity;
        return act != null && onHandQty < 0 && act.Operation.Job.Classification == JobDefs.classifications.Forecast;
    }

    /// <summary>
    /// Returns true if a_reason is Activity, change qty less than zero and the Activity belongs to a Job with Classification of Forecast.
    /// </summary>
    /// <returns></returns>
    private static bool IsJobTransferOrder(BaseIdObject a_reason)
    {
        InternalActivity act = a_reason as InternalActivity;
        return act != null && act.Operation.Job.Classification == JobDefs.classifications.TransferOrder;
    }

    /// <summary>
    /// Returns true if a_reason is Activity and change quantity less than 0 and Job Classification is not Sales Order.
    /// </summary>
    /// <returns></returns>
    private bool IsJobMaterial(BaseIdObject a_reason)
    {
        InternalActivity act = a_reason as InternalActivity;
        if (act == null || onHandQty >= 0)
        {
            return false;
        }

        switch (act.Operation.Job.Classification)
        {
            case JobDefs.classifications.Forecast:
            case JobDefs.classifications.TransferOrder:
            case JobDefs.classifications.SalesOrder:
                return false;
        }

        return true;
    }

    public string GetReasonDescription(BaseIdObject a_reason, bool a_includeForecastInNetInv = false)
    {
        string description = string.Empty;
        if (a_reason == null)
        {
            return description;
        }

        if (IsJobForecast(a_reason) && a_includeForecastInNetInv)
        {
            description = "Job Forecast".Localize();
        }
        else if (IsJobTransferOrder(a_reason))
        {
            description = "Job Transfer".Localize();
        }
        else if (IsJobSalesOrder(a_reason))
        {
            description = "Job Sales Order".Localize();
        }
        else if (IsJobMaterial(a_reason))
        {
            description = "Job Material".Localize();
        }
        else if (IsJobProduct(a_reason))
        {
            description = "Job Product".Localize();
        }

        return description;
    }

    #region Debug
    public override string ToString()
    {
        return string.Format("{0}; Qty={1}; Reasons={2}", Time, onHandQty, Reason());
    }
    #endregion
}