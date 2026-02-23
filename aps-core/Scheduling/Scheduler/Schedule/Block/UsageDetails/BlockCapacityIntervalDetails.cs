namespace PT.Scheduler.Schedule.Block.UsageDetails;

public class BlockCapacityIntervalDetails
{
    internal BlockCapacityIntervalDetails(List<BlockIntervalTypeCapacityIntervalDetails> scheduleDetails)
    {
        _scheduleDetails = scheduleDetails;
    }

    private readonly List<BlockIntervalTypeCapacityIntervalDetails> _scheduleDetails;

    private int _nextIndex;

    public bool GetNextCapacityIntervalDetails(BlockIntervalDetails cid)
    {
        cid.Clear();

        if (_nextIndex >= _scheduleDetails.Count)
        {
            return false;
        }

        BlockIntervalTypeCapacityIntervalDetails scheduleDetails = _scheduleDetails[_nextIndex];
        SetCapacityIntervalDetailsValues(cid, scheduleDetails);

        for (int i = _nextIndex; i < _scheduleDetails.Count; ++i)
        {
            if (scheduleDetails._capacityIntervalId == _scheduleDetails[i]._capacityIntervalId)
            {
                SetCapacityIntervalDetailsValues(cid, _scheduleDetails[i]);
            }
            else
            {
                break;
            }
        }

        return true;
    }

    private void SetCapacityIntervalDetailsValues(BlockIntervalDetails cid, BlockIntervalTypeCapacityIntervalDetails scheduleDetails)
    {
        ++_nextIndex;

        switch (scheduleDetails._intervalType)
        {
            case BlockIntervalType.Setup:
                cid.SetupStartDate = scheduleDetails._startDate;
                cid.SetupEndDate = scheduleDetails._endDate;
                break;

            case BlockIntervalType.Processing:
                cid.ProcessingStartDate = scheduleDetails._startDate;
                cid.ProcessingEndDate = scheduleDetails._endDate;
                cid.Qty = scheduleDetails._qty;
                break;

            case BlockIntervalType.PostProcessing:
                cid.PostProcessingStartDate = scheduleDetails._startDate;
                cid.PostProcessingEndDate = scheduleDetails._endDate;
                break;
        }
    }
}