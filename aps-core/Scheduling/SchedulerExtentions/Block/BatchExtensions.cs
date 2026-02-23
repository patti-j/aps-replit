using PT.Scheduler;

namespace PT.SchedulerExtensions.Block;

public static class BatchExtensions
{
    public static TimeSpan CalendarSetupSpan(this Batch a_batch)
    {
        return TimeSpan.FromTicks(a_batch.SetupEndTicks - a_batch.StartTicks);
    }

    public static TimeSpan CalendarRunSpan(this Batch a_batch)
    {
        return TimeSpan.FromTicks(a_batch.ProcessingEndTicks - a_batch.SetupEndTicks);
    }

    public static TimeSpan CalendarPostProcessingSpan(this Batch a_batch)
    {
        return TimeSpan.FromTicks(a_batch.PostProcessingEndTicks - a_batch.ProcessingEndTicks);
    }

    public static TimeSpan CalendarCleanSpan(this Batch a_batch)
    {
        return a_batch.CleanEndDateTime - a_batch.PostProcessingEndDateTime;
    }

    public static TimeSpan CalendarStorageSpan(this Batch a_batch)
    {
        return a_batch.StorageSpan.TimeSpan;
    }

    public static TimeSpan Duration(this Batch a_batch)
    {
        return TimeSpan.FromTicks(a_batch.EndTicks - a_batch.StartTicks);
    }
}