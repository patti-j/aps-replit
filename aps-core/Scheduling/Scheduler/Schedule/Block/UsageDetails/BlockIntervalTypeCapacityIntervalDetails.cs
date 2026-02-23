namespace PT.Scheduler.Schedule.Block.UsageDetails;

public class BlockIntervalTypeCapacityIntervalDetails
{
    internal BlockIntervalTypeCapacityIntervalDetails(BlockIntervalType intervalType, int capacityIntervalId, long aStartDate, long aStopDate, decimal aQty)
    {
        _intervalType = intervalType;
        _capacityIntervalId = capacityIntervalId;

        _startDate = aStartDate;
        _endDate = aStopDate;

        _qty = aQty;
    }

    public readonly BlockIntervalType _intervalType;
    public readonly int _capacityIntervalId;
    public readonly long _startDate;
    public readonly long _endDate;
    public readonly decimal _qty;

    public override string ToString()
    {
        return string.Format("{0} ** {1} ** {2} -> {3} ** Q {4} ** {5}", base.ToString(), _intervalType, _startDate, _endDate, _qty, _capacityIntervalId);
    }
}