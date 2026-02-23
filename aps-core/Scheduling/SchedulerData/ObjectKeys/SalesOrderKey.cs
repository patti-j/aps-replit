using PT.APSCommon;
using PT.Common.ObjectHelpers;
using PT.Scheduler;
using PT.Scheduler.Demand;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct SalesOrderKey : IEquatable<SalesOrderKey>
{
    public readonly BaseId SalesOrderId;
    public readonly BaseId SoLineId;
    public readonly BaseId SoLineDistributionId;

    public SalesOrderKey(BaseId a_salesOrder, BaseId a_soLineId, BaseId a_soLineDistributionId)
    {
        SalesOrderId = a_salesOrder;
        SoLineId = a_soLineId;
        SoLineDistributionId = a_soLineDistributionId;
    }

    public SalesOrder GetSalesOrder(ScenarioDetail a_sd)
    {
        return a_sd.SalesOrderManager.GetById(SalesOrderId);
    }

    public bool Equals(SalesOrderKey a_other)
    {
        return SoLineDistributionId.Value == a_other.SoLineDistributionId.Value && SoLineId.Value == a_other.SoLineId.Value && SalesOrderId.Value == a_other.SalesOrderId.Value;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is SalesOrderKey key)
        {
            return Equals(key);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCodeHelper.GetHashCode(SalesOrderId, SoLineId, SoLineDistributionId);
    }
}