using PT.Database;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.Block.UsageDetails;

public class BlockIntervalDetails
{
    internal void Clear()
    {
        _postProcessingEnd = InitDateValue.Ticks;
        _postProcessingStart = InitDateValue.Ticks;
        _processingEnd = InitDateValue.Ticks;
        _processingStart = InitDateValue.Ticks;
        _qty = 0;
        _setupEnd = InitDateValue.Ticks;
        _setupStart = InitDateValue.Ticks;
    }

    private DateTime InitDateValue => new (0);

    /// <summary>
    /// The start of the first segment, Setup, Processing or Post Processing.
    /// </summary>
    public DateTime ScheduledStart
    {
        get
        {
            if (SetupStart.Ticks != InitDateValue.Ticks)
            {
                return SetupStart;
            }

            if (ProcessingStart.Ticks != InitDateValue.Ticks)
            {
                return ProcessingStart;
            }

            return PostProcessingStart;
        }
    }

    /// <summary>
    /// The end of the latest segment, PostProcessing, Processing, or Setup.
    /// </summary>
    public DateTime ScheduledEnd
    {
        get
        {
            if (PostProcessingEnd.Ticks != InitDateValue.Ticks)
            {
                return PostProcessingEnd;
            }

            if (ProcessingEnd.Ticks != InitDateValue.Ticks)
            {
                return ProcessingEnd;
            }

            return SetupEnd;
        }
    }

    private long _setupStart;

    internal long SetupStartDate
    {
        get => _setupStart;
        set => _setupStart = value;
    }

    public DateTime SetupStart => new (SetupStartDate);

    private long _setupEnd;

    internal long SetupEndDate
    {
        get => _setupEnd;
        set => _setupEnd = value;
    }

    public DateTime SetupEnd => new (SetupEndDate);

    private long _processingStart;

    internal long ProcessingStartDate
    {
        get => _processingStart;
        set => _processingStart = value;
    }

    public DateTime ProcessingStart => new (ProcessingStartDate);

    private long _processingEnd;

    internal long ProcessingEndDate
    {
        get => _processingEnd;
        set => _processingEnd = value;
    }

    public DateTime ProcessingEnd => new (ProcessingEndDate);

    private long _postProcessingStart;

    internal long PostProcessingStartDate
    {
        get => _postProcessingStart;
        set => _postProcessingStart = value;
    }

    public DateTime PostProcessingStart => new (PostProcessingStartDate);

    private long _postProcessingEnd;

    internal long PostProcessingEndDate
    {
        get => _postProcessingEnd;
        set => _postProcessingEnd = value;
    }

    public DateTime PostProcessingEnd => new (PostProcessingEndDate);

    private decimal _qty;

    public decimal Qty
    {
        get => _qty;
        internal set => _qty = value;
    }

    public void PtDbPopulate(ref PtDbDataSet dataSet, ResourceBlock block, int index, PtDbDataSet.JobResourceBlocksRow jobBlocksRow, PTDatabaseHelper a_dbHelper)
    {
        block.ScheduledResource.FindOnlineInvervalCoveringPoint(
            ScheduledStart, 
            out DateTime shiftStart, 
            out DateTime shiftEnd, 
            out string shiftName, 
            out string shiftDescription, 
            out decimal shiftNbrOfPeople, 
            out string shiftType
            );


        dataSet.JobResourceBlockIntervals.AddJobResourceBlockIntervalsRow(
            jobBlocksRow.PublishDate,
            jobBlocksRow.InstanceId,
            block.Activity.Operation.ManufacturingOrder.Job.Id.ToBaseType(),
            block.Activity.Operation.ManufacturingOrder.Id.ToBaseType(),
            block.Activity.Operation.Id.ToBaseType(),
            block.Activity.Id.ToBaseType(),
            block.Id.ToBaseType(),
            index,
            _qty,
            a_dbHelper.AdjustPublishTime(shiftStart),
            a_dbHelper.AdjustPublishTime(shiftEnd),
            shiftName,
            shiftDescription,
            shiftNbrOfPeople,
            shiftType,
            a_dbHelper.AdjustPublishTime(SetupStart),
            a_dbHelper.AdjustPublishTime(SetupEnd),
            a_dbHelper.AdjustPublishTime(ProcessingStart),
            a_dbHelper.AdjustPublishTime(ProcessingEnd),
            a_dbHelper.AdjustPublishTime(PostProcessingStart),
            a_dbHelper.AdjustPublishTime(PostProcessingEnd),
            a_dbHelper.AdjustPublishTime(ScheduledStart),
            a_dbHelper.AdjustPublishTime(ScheduledEnd)
        );
    }

    public override string ToString()
    {
        return string.Format("{0} ** Setup {1} -> {2} ** Proc {3} -> {4} Qty {5} ** Post Proc {6} -> {7}",
            base.ToString(),
            _setupStart,
            _setupEnd,
            _processingStart,
            _processingEnd,
            _qty,
            _postProcessingStart,
            _postProcessingEnd
        );
    }
}