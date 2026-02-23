using System.Drawing;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class StatusSettings : ISettingData, ICloneable
{
    public StatusSettings(IMainForm a_mainForm)
    {
        Script = "";
        Show = true;
        ItemTypeFilterList = new List<ItemDefs.itemTypes>();

        IDynamicSkin theme = a_mainForm.CurrentBrand.ActiveTheme;

        OnHoldColor = theme.OnHoldColor;
        WaitingColor = theme.WaitingColor;
        SettingUpColor = theme.SettingUpColor;
        RunningColor = theme.RunningColor;
        ReadyColor = theme.ReadyColor;
        StartedColor = theme.StartedColor;
        PostProcessingColor = theme.PostProcessingColor;
        TransferringColor = theme.TransferringColor;
        PausedColor = theme.PausedColor;
        Priority = 5;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
    }

    public StatusSettings(IReader a_reader)
    {
        ItemTypeFilterList = new List<ItemDefs.itemTypes>();

        if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int val);
                ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
            }

            a_reader.Read(out OnHoldColor);
            a_reader.Read(out WaitingColor);
            a_reader.Read(out SettingUpColor);
            a_reader.Read(out RunningColor);
            a_reader.Read(out ReadyColor);
            a_reader.Read(out StartedColor);
            a_reader.Read(out PostProcessingColor);
            a_reader.Read(out TransferringColor);
            a_reader.Read(out PausedColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int val);
                ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Script);
        a_writer.Write(Show);
        a_writer.Write(ItemTypeFilterList.Count);
        foreach (ItemDefs.itemTypes itemType in ItemTypeFilterList)
        {
            a_writer.Write((int)itemType);
        }

        a_writer.Write(OnHoldColor);
        a_writer.Write(WaitingColor);
        a_writer.Write(SettingUpColor);
        a_writer.Write(RunningColor);
        a_writer.Write(ReadyColor);
        a_writer.Write(StartedColor);
        a_writer.Write(PostProcessingColor);
        a_writer.Write(TransferringColor);
        a_writer.Write(PausedColor);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    #region Property Accessors
    public string Script;
    public bool Show;
    public List<ItemDefs.itemTypes> ItemTypeFilterList;
    public Color OnHoldColor;
    public Color WaitingColor;
    public Color SettingUpColor;
    public Color RunningColor;
    public Color ReadyColor;
    public Color StartedColor;
    public Color PostProcessingColor;
    public Color TransferringColor;
    public Color PausedColor;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;
    #endregion

    public int UniqueId => 915;

    public string SettingKey => StaticSettingKey;
    public string Description => "TOOD:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Status settings";
    public static string StaticSettingKey => "segmentSettings_Status";

    object ICloneable.Clone()
    {
        return Clone();
    }

    public StatusSettings Clone()
    {
        return (StatusSettings)MemberwiseClone();
    }
}