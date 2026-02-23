using System.Drawing;

using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class ProgressSegmentSettings : ISettingData
{
    public ProgressSegmentSettings(IMainForm a_mainForm)
    {
        Script = "";
        Show = true;

        Priority = 8;
        MinHeight = 0;
        ProportionalHeightWeight = 25;

        OkColor = a_mainForm.CurrentBrand.ActiveTheme.OkColor;
        WarningColor = a_mainForm.CurrentBrand.ActiveTheme.WarningAlert;
    }

    public ProgressSegmentSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);

            a_reader.Read(out OkColor);
            a_reader.Read(out WarningColor);
        }
        else
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Script);
        a_writer.Write(Show);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);

        a_writer.Write(OkColor);
        a_writer.Write(WarningColor);
    }

    public string Script;
    public bool Show;
    public Color OkColor;
    public Color WarningColor;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;

    public int UniqueId => 912;

    public string SettingKey => StaticSettingKey;
    public string Description => "TOOD:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Progress settings";
    public static string StaticSettingKey => "segmentSettings_Progress";
}