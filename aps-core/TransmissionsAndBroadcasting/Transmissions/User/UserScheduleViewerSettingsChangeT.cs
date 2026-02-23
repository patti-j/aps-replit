using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for changing a User's Block Label Settings.
/// </summary>
public class UserScheduleViewerSettingsChangeT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 162;

    #region IPTSerializable Members
    public UserScheduleViewerSettingsChangeT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            settings = new ScheduleViewerSettings(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        settings.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScheduleViewerSettings settings;

    public UserScheduleViewerSettingsChangeT() { }

    public UserScheduleViewerSettingsChangeT(BaseId userId, ScheduleViewerSettings settings)
        : base(userId)
    {
        this.settings = settings;
    }
}