using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using System.Text;

namespace PT.Scheduler;

public class ActivityAdjustment : Adjustment
{
    #region IPTSerializable Members
    internal ActivityAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1226;

    private RestoreInfo m_restoreInfo;
    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        RestoreInfo info = new(m_activity);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly ActivityKey ActivityKey;

        internal RestoreInfo(IReader a_reader)
        {
            ActivityKey = new ActivityKey(a_reader);
        }

        internal RestoreInfo(InternalActivity a_activity)
        {
            ActivityKey = a_activity.CreateActivityKey();
        }

        internal void Serialize(IWriter a_writer)
        {
            ActivityKey.Serialize(a_writer);
        }
    }

    #endregion IPTSerializable Members

    internal ActivityAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, InternalActivity a_activity)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment)
    {
        m_activity = a_activity;
    }

    private InternalActivity m_activity;

    public InternalActivity Activity => m_activity;

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);

        m_activity = a_sd.JobManager.FindActivity(m_restoreInfo.ActivityKey);
        m_restoreInfo = null;
    }

    public override int ReasonPriority => m_activity.Operation.ManufacturingOrder.Job.Priority;

    public override BaseIdObject GetReason() => m_activity;

    /// <summary>
    /// For negative Adjustments that are Activities, this is the earlier of the JIT Start Date and the Adjustment Time.
    /// For all others, it's equivalent to the Adjustment Time.
    /// </summary>
    public DateTime JitAdjustedUsage
    {
        get
        {
            if (ChangeQty < 0)
            {
                if (m_activity.Scheduled)
                {
                    return new DateTime(Math.Min(m_activity.ScheduledStartDate.Ticks, m_activity.DbrJitStartTicks));
                }
            }

            return AdjDate;
        }
    }

    /// <summary>
    /// Returns true if Reason is Activity and change quantity greater than 0.
    /// </summary>
    /// <returns></returns>
    public bool IsJobProduct()
    {
        if (Activity.Operation.Job.Classification == JobDefs.classifications.TransferOrder)
        {
            return false;
        }

        return ChangeQty > 0;
    }

    /// <summary>
    /// Returns true if Reason is Activity, change qty less than zero and the Activity belongs to a Job with Classification of SalesOrder.
    /// </summary>
    /// <returns></returns>
    public bool IsJobSalesOrder()
    {
        return ChangeQty < 0 && Activity.Operation.Job.Classification == JobDefs.classifications.SalesOrder;
    }

    /// <summary>
    /// Returns true if Reason is Activity, change qty less than zero and the Activity belongs to a Job with Classification of Forecast.
    /// </summary>
    /// <returns></returns>
    public bool IsJobForecast()
    {
        return ChangeQty < 0 && Activity.Operation.Job.Classification == JobDefs.classifications.Forecast;
    }

    /// <summary>
    /// Returns true if Reason is Activity, change qty less than zero and the Activity belongs to a Job with Classification of Forecast.
    /// </summary>
    /// <returns></returns>
    public bool IsJobTransferOrder()
    {
        return Activity.Operation.Job.Classification == JobDefs.classifications.TransferOrder;
    }

    /// <summary>
    /// Returns true if Reason is Activity and change quantity less than 0 and Job Classification is not Sales Order.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsJobMaterial()
    {
        if (ChangeQty >= 0)
        {
            return false;
        }

        switch (Activity.Operation.Job.Classification)
        {
            case JobDefs.classifications.Forecast:
            case JobDefs.classifications.TransferOrder:
            case JobDefs.classifications.SalesOrder:
                return false;
        }

        return true;
    }

    public MaterialRequirement GetAdjMaterialRequirement(BaseId a_itemId)
    {
        if (ChangeQty >= 0)
        {
            return null; // not the right type of adjustment
        }

        for (int i = 0; i < Activity.Operation.MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = Activity.Operation.MaterialRequirements[i];
            if (mr.Item?.Id == a_itemId)
            {
                return mr;
            }
        }

        return null;
    }

    public Product GetAdjProduct(BaseId a_itemId)
    {
        if (ChangeQty < 0)
        {
            return null; // not the right type of adjustment
        }

        for (int i = 0; i < Activity.Operation.Products.Count; i++)
        {
            Product product = Activity.Operation.Products[i];
            if (product.Item.Id == a_itemId)
            {
                return product;
            }
        }

        return null;
    }

    public override string ReasonDescription
    {
        get
        {
            StringBuilder sb = new();

                InternalOperation io = Activity.Operation;
                ManufacturingOrder mo = io.ManufacturingOrder;
                Job job = mo.Job;
                string prefix = "Job".Localize();

                if (job.Classification == JobDefs.classifications.SalesOrder)
                {
                    prefix = "Sales Order".Localize() + " " + prefix;
                }
                else if (job.Classification == JobDefs.classifications.Forecast)
                {
                    prefix = "Forecast Demand".Localize() + " " + prefix;
                }
                else if (job.Classification == JobDefs.classifications.TransferOrder)
                {
                    prefix = "Transfer".Localize() + " " + prefix;
                }

                GetNameOrExternalIdMessage(sb, prefix, job);

                if (job.Customers.Count > 0)
                {
                    sb.AppendFormat("Customers: ".Localize());

                    string customers = job.Customers.GetCustomerExternalIdsList();
                    sb.Append(customers);
                }

                if (job.ManufacturingOrders.Count > 1)
                {
                    GetNameOrExternalIdMessage(sb, "; " + "MO".Localize(), mo);

                    if (mo.Product != null && mo.Product.Name.Length > 0)
                    {
                        sb.AppendFormat("; " + "Product: {0}  {1}".Localize(), mo.Product, job.ProductDescription);
                    }
                }
                else
                {
                    if (job.Product.Length > 0)
                    {
                        if (string.IsNullOrEmpty(job.ProductDescription))
                        {
                            sb.AppendFormat("; " + "Product: {0}".Localize(), job.Product);
                        }
                        else
                        {
                            sb.AppendFormat("; " + "Product: {0}  {1}".Localize(), job.Product, job.ProductDescription);
                        }
                    }
                    else if (mo.GetMaterialRequirements().Count > 0)
                    {
                        string materialRequirementExternalIds = string.Empty;
                        foreach (MaterialRequirement materialRequirement in mo.GetMaterialRequirements())
                        {
                            materialRequirementExternalIds += materialRequirement.ExternalId + "; ";
                        }
                        materialRequirementExternalIds = materialRequirementExternalIds.Remove(materialRequirementExternalIds.Length - 2);
                        sb.AppendFormat("; " + "Material Requirements: {0}".Localize(), materialRequirementExternalIds);
                    }
                }

                if (mo.CurrentPath.OpsProducingProductsCount() > 1)
                {
                    GetNameOrExternalIdMessage(sb, "; " + "Op".Localize(), io);
                }

                return sb.ToString();
        }
    }
}
