using System.Drawing;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class BuffersSettings : ISettingData
{
    public BuffersSettings(IMainForm a_mainForm)
    {
        Script = "";
        Show = false;
        ShowCurrentDrumPenetration = true;
        IDynamicSkin theme = a_mainForm.CurrentBrand.ActiveTheme;
        OkColor = theme.OkColor;
        WarningColor = theme.WarningColor;
        CriticalColor = theme.CriticalAlert;
        LateColor = theme.LateColor;

        Priority = 7;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
    }

    public BuffersSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out ShowCurrentDrumPenetration);
            a_reader.Read(out OkColor);
            a_reader.Read(out WarningColor);
            a_reader.Read(out CriticalColor);
            a_reader.Read(out LateColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out ShowCurrentDrumPenetration);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Script);
        a_writer.Write(Show);
        a_writer.Write(ShowCurrentDrumPenetration);
        a_writer.Write(OkColor);
        a_writer.Write(WarningColor);
        a_writer.Write(CriticalColor);
        a_writer.Write(LateColor);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    #region Property Accessors
    public string Script;
    public bool Show;
    public bool ShowCurrentDrumPenetration;
    public Color OkColor;
    public Color WarningColor;
    public Color CriticalColor;
    public Color LateColor;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;
    #endregion

    public int UniqueId => 914;
    public string SettingKey => StaticSettingKey;
    public string Description => "TOOD:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Buffer settings";
    public static string StaticSettingKey => "segmentSettings_Buffers";
}