using PT.Common.Debugging;

namespace PT.SchedulerDefinitions;

public class PTDatabaseHelper
{
    private readonly TimeZoneInfo m_adjustToTimeZone;

    public PTDatabaseHelper(TimeZoneInfo a_adjustToTimeZone, bool a_publishAllData, DateTime a_maxPublishDate, bool a_detailedSchedulingPermission)
    {
        m_adjustToTimeZone = a_adjustToTimeZone;
        PublishAllData = a_publishAllData;
        MaxPublishDate = a_maxPublishDate;
        DetailedSchedulingPermission = a_detailedSchedulingPermission;
    }

    public bool PublishAllData { get; }

    public DateTime MaxPublishDate { get; }

    public bool DetailedSchedulingPermission { get; }
    public Dictionary<string, IUserFieldDefinition> UserFieldDefinitions { get; set; }

    [Obsolete("Use DateTimeOffset instead")]
    public DateTime AdjustPublishTime(DateTime a_datetime)
    {
        try
        {
            if (a_datetime.Ticks < PTDateTime.MinDateTimeTicks || a_datetime.Ticks > PTDateTime.MaxDateTimeTicks)
            {
                DebugException.ThrowInTest("a_datetime is out of bounds. This DateTime shouldn't be deserialized as an out of bounds value.");
                //TODO: This typically happens because some value was by default set to an obsolete PtDateTime constant.
                //      We technically should go into the code fix these assignments as we find them,
                //      but it's a fairly delicate situation as there could be code that depends
                //      on the value being the obsolete constant. Better not to break things for now. 
                return a_datetime;
            }

            DateTimeOffset dateTimeOffset = new (a_datetime, TimeSpan.Zero);
            return AdjustPublishTime(dateTimeOffset).ToDateTime();
        }
        catch
        {
            return a_datetime;
        }
    }

    public DateTimeOffset AdjustPublishTime(DateTimeOffset a_datetime)
    {
        if (a_datetime <= PTDateTime.MinValue)
        {
            return new DateTime(PTDateTime.MinValue.Ticks, DateTimeKind.Utc);
        }

        if (a_datetime >= PTDateTime.MaxValue)
        {
            return new DateTime(PTDateTime.MaxValue.Ticks, DateTimeKind.Utc);
        }

        DateTimeOffset publishDateTime = TimeZoneInfo.ConvertTime(a_datetime, m_adjustToTimeZone);

        if (!DetailedSchedulingPermission)
        {
            publishDateTime = publishDateTime.ToDateNoTime();
        }

        return publishDateTime;
    }

    // Removed as we transition to just having a scenario "Production" flag
    //public string GetScenarioTypeString(ScenarioTypes a_ssType)
    //{
    //    const string SCENARIO_TYPE_LIVE = "Live";
    //    const string SCENARIO_TYPE_PUBLISHED = "Published";
    //    const string SCENARIO_TYPE_WHATIF = "What-If";
    //    const string SCENARIO_TYPE_RULESEEK = "CoPilot";
    //    const string SCENARIO_TYPE_INSERTJOBS = "InsertJobs";

    //    switch (a_ssType)
    //    {
    //        case ScenarioTypes.Live:
    //            return SCENARIO_TYPE_LIVE;
    //        case ScenarioTypes.Published:
    //            return SCENARIO_TYPE_PUBLISHED;
    //        case ScenarioTypes.RuleSeek:
    //            return SCENARIO_TYPE_RULESEEK;
    //        case ScenarioTypes.InsertJobs:
    //            return SCENARIO_TYPE_INSERTJOBS;
    //        default:
    //            return SCENARIO_TYPE_WHATIF;
    //    }
    //}
    public string GetTimeZoneId()
    {
        return m_adjustToTimeZone.Id;
    }
}

public interface IUserFieldDefinition
{
    string Name { get; }
    string ExternalId { get; }
    bool Publish { get; }
    UserField.UDFTypes UDFDataType { get; }
}