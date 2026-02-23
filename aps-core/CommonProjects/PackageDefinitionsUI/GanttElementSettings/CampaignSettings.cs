using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class SimplifyGanttCampaignSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 910;

    public string SettingKey => "SimplifyGanttSettings_Campaign";
    public string Description => "TODO:"; //TODO: implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SimplifyGanttSettings;
    public string SettingCaption => "Campaign settings";

    #region IPTSerializable Members
    public SimplifyGanttCampaignSettings(IReader reader)
    {
        reader.Read(out alternateCampaignColorsByItemGroup);
    }

    public SimplifyGanttCampaignSettings() { }

    public SimplifyGanttCampaignSettings(ScheduleViewerSettings.SimplifyGanttSettings a_settings)
    {
        alternateCampaignColorsByItemGroup = a_settings.AlternateCampaignColorsByItemGroup;
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(alternateCampaignColorsByItemGroup);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public bool alternateCampaignColorsByItemGroup;

    public bool AlternateCampaignColorsByItemGroup
    {
        get => alternateCampaignColorsByItemGroup;
        set => alternateCampaignColorsByItemGroup = value;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    public SimplifyGanttCampaignSettings Clone()
    {
        return (SimplifyGanttCampaignSettings)MemberwiseClone();
    }
}