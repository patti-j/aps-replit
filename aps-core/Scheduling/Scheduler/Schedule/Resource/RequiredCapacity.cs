using PT.Scheduler.Simulation;

namespace PT.Scheduler;

public class RequiredCapacity
{
    public RequiredCapacity(
        RequiredSpanPlusClean a_cleanBeforeSpan,
        RequiredSpanPlusSetup a_setupSpan,
        RequiredSpan a_processingSpan,
        RequiredSpan a_postProcessingSpan,
        RequiredSpan a_storageSpan,
        RequiredSpanPlusClean a_cleanSpan
    )
    {
        CleanBeforeSpan = a_cleanBeforeSpan;
        SetupSpan = a_setupSpan;
        ProcessingSpan = a_processingSpan;
        PostProcessingSpan = a_postProcessingSpan;
        StorageSpan = a_storageSpan;
        CleanAfterSpan = a_cleanSpan;
    }

    internal RequiredCapacity(SchedulableInfo a_si)
    {
        CleanBeforeSpan = a_si.m_requiredAdditionalCleanBeforeSpan;
        SetupSpan = a_si.m_requiredSetupSpan;
        ProcessingSpan = a_si.m_requiredProcessingSpan;
        StorageSpan = a_si.m_requiredStorageSpan;
        PostProcessingSpan = a_si.m_requiredPostProcessingSpan;
    }

    public RequiredSpanPlusClean CleanBeforeSpan { get; set; }

    public RequiredSpanPlusSetup SetupSpan { get; set; }

    public RequiredSpan ProcessingSpan { get; set; }

    public RequiredSpan PostProcessingSpan { get; set; }

    public RequiredSpan StorageSpan { get; set; }

    public RequiredSpanPlusClean CleanAfterSpan { get; set; }

    public long TotalRequiredCapacity()
    {
        return CleanBeforeSpan.TimeSpanTicks + SetupSpan.TimeSpanTicks + ProcessingSpan.TimeSpanTicks + PostProcessingSpan.TimeSpanTicks + StorageSpan.TimeSpanTicks + CleanAfterSpan.TimeSpanTicks;
    }

    public override string ToString()
    {
        return string.Format("SetupZeroLength={0}; CleanBefore={9}; Setup={1}; ProcessingZeroLength={2}; Processing={3}; PostProcessingZeroLength={4}; PostProcessing={5}; StorageSpan={6}; CleanZeroLength={7}; Clean={8}", SetupSpan.Overrun, SetupSpan.TimeSpan, ProcessingSpan.Overrun, ProcessingSpan.TimeSpan, PostProcessingSpan.Overrun, PostProcessingSpan.TimeSpan, StorageSpan.TimeSpan, CleanAfterSpan.Overrun, CleanAfterSpan.TimeSpan, CleanBeforeSpan.TimeSpan);
    }
}

/// <summary>
/// Summary description for RequiredCapacity.
/// </summary>
public class RequiredCapacityPlus : RequiredCapacity
{
    public RequiredCapacityPlus(
        RequiredSpanPlusClean a_cleanBeforeSpan,
        RequiredSpanPlusSetup a_setupSpan,
        RequiredSpan a_processingSpan,
        RequiredSpan a_postProcessingSpan,
        RequiredSpan a_storageSpan,
        RequiredSpanPlusClean a_cleanAfterSpan, //This would only be set if the next block is known when calculating
        long a_requiredNumberOfCycles,
        decimal a_requiredQty)
        : base(
            a_cleanBeforeSpan,
            a_setupSpan,
            a_processingSpan,
            a_postProcessingSpan,
            a_storageSpan,
            a_cleanAfterSpan)
    {
        RequiredNumberOfCycles = a_requiredNumberOfCycles;
        RequiredQty = a_requiredQty;
    }

    /// <summary>
    /// The number of cycles that need to be run.
    /// </summary>
    public long RequiredNumberOfCycles { internal set;  get; }

    /// <summary>
    /// The quantity of material that needs to be made.
    /// </summary>
    public decimal RequiredQty { internal set;  get; }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new ();

        AddTime(CleanBeforeSpan.TimeSpanTicks, "Clean Before", sb);
        AddTime(SetupSpan.TimeSpanTicks, "Setup", sb);
        AddTime(ProcessingSpan.TimeSpanTicks, "Processing", sb);
        AddTime(PostProcessingSpan.TimeSpanTicks, "PostProcessing", sb);
        AddTime(CleanAfterSpan.TimeSpanTicks, "Clean", sb);
        AddTime(TotalRequiredCapacity(), "TotalTime", sb);

        sb.AppendFormat("Cycles={0}; ", RequiredNumberOfCycles);
        sb.AppendFormat("Qty={0}; ", RequiredQty);

        return sb.ToString();
    }

    private void AddTime(long a_time, string a_name, System.Text.StringBuilder a_sb)
    {
        if (a_time > 0)
        {
            a_sb.AppendFormat("{0}={1}; ", a_name, DateTimeHelper.PrintTimeSpan(a_time));
        }
    }

    public static readonly RequiredCapacityPlus s_notInit = new (RequiredSpanPlusClean.s_notInit
        , RequiredSpanPlusSetup.s_notInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit, 0, 0m);
}