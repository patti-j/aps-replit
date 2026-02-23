using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Advance the Clock of the specified Scenario to the a_newTime.
/// </summary>
public class ScenarioClockAdvanceT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 121;

    #region IPTSerializable Members
    public ScenarioClockAdvanceT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 641)
        {
            reader.Read(out time);
            reader.Read(out adjustDemoData);
            reader.Read(out autoReportProgressOnAllActivities);
            reader.Read(out autoFinishAllActivities);
            m_scenarioId = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 174)
        {
            reader.Read(out time);
            reader.Read(out adjustDemoData);
            reader.Read(out autoReportProgressOnAllActivities);
            reader.Read(out autoFinishAllActivities);
        }
        else if (reader.VersionNumber >= 144)
        {
            reader.Read(out time);
            reader.Read(out adjustDemoData);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out time);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(time);
        writer.Write(adjustDemoData);
        writer.Write(autoReportProgressOnAllActivities);
        writer.Write(autoFinishAllActivities);
        m_scenarioId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public DateTime time;
    public bool adjustDemoData;
    public bool autoReportProgressOnAllActivities;
    public bool autoFinishAllActivities;

    public ScenarioClockAdvanceT() { }

    public ScenarioClockAdvanceT(DateTimeOffset a_newTime)
    {
        if (a_newTime.Offset != TimeSpan.Zero)
        {
            throw new PTValidationException("ScenarioClockAdvanceT should be constructed with utc time");
        }

        time = a_newTime.ToDateTime();
    }

    private BaseId m_scenarioId = BaseId.NULL_ID;

    public BaseId SpecificScenarioId
    {
        get => m_scenarioId;
        set => m_scenarioId = value;
    }

    public bool UseSpecificScenarioId => m_scenarioId != BaseId.NULL_ID;

    public override string Description => string.Format("Scenario Clock Advanced ({0})".Localize(), time.ToDisplayTime());
}