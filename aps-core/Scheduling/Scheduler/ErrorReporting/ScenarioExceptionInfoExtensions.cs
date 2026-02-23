using PT.Common.File;

namespace PT.Scheduler.ErrorReporting;

public static class ScenarioExceptionInfoExtensions
{
    public static ScenarioExceptionInfo Create(this ScenarioExceptionInfo a_sei, ScenarioDetail a_sd)
    {
        DateTimeOffset creationDateTime = PTDateTime.MinValue;
        try
        {
            using (a_sd.Scenario.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, AutoExiter.THREAD_TRY_WAIT_MS))
            {
                creationDateTime = ss.CreationDateTime.ToDateTimeOffset(TimeZoneInfo.Utc);
            }
        }
        catch (AutoTryEnterException) { }


        a_sei.Initialize(a_sd.Scenario.Name, a_sd.Scenario.Type.ToString(), creationDateTime);
        return a_sei;
    }

    public static void Create(this ScenarioExceptionInfo a_sei, Scenario a_s)
    {
        DateTimeOffset creationDateTime = PTDateTime.MinValue;
        try
        {
            using (a_s.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, AutoExiter.THREAD_TRY_WAIT_MS))
            {
                creationDateTime = ss.CreationDateTime.ToDateTimeOffset(TimeZoneInfo.Utc);
            }
        }
        catch (AutoTryEnterException) { }


        a_sei.Initialize(a_s.Name, a_s.Type.ToString(), creationDateTime);
    }
}