using System.Drawing;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class TimingSettings : ISettingData
{
    public TimingSettings(IMainForm a_mainForm)
    {
        Script = "";
        Show = true;

        IDynamicSkin theme = a_mainForm.CurrentBrand.ActiveTheme;

        TooEarlyColor = theme.TooEarlyColor;
        OnTimeColor = theme.OnTimeColor;
        AlmostLateColor = theme.AlmostLateColor;
        LateColor = theme.LateColor;

        CapacityBottleneckColor = theme.BottleneckCapacityColor;
        MaterialBottleneckColor = theme.BottleneckMaterialColor;
        ReleaseBottleneckColor = theme.BottleneckReleaseColor;

        Priority = 1;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
    }

    public TimingSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out TooEarlyColor);
            a_reader.Read(out OnTimeColor);
            a_reader.Read(out AlmostLateColor);
            a_reader.Read(out LateColor);
            a_reader.Read(out CapacityBottleneckColor);
            a_reader.Read(out MaterialBottleneckColor);
            a_reader.Read(out ReleaseBottleneckColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
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
        a_writer.Write(TooEarlyColor);
        a_writer.Write(OnTimeColor);
        a_writer.Write(AlmostLateColor);
        a_writer.Write(LateColor);
        a_writer.Write(CapacityBottleneckColor);
        a_writer.Write(MaterialBottleneckColor);
        a_writer.Write(ReleaseBottleneckColor);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    public int UniqueId => 906;

    #region Property Accessors
    public string Script;
    public bool Show;
    public Color TooEarlyColor;
    public Color OnTimeColor;
    public Color AlmostLateColor;
    public Color LateColor;
    public Color CapacityBottleneckColor;
    public Color MaterialBottleneckColor;
    public Color ReleaseBottleneckColor;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;
    #endregion

    public string SettingKey => StaticSettingKey;
    public string Description => "TODO:"; //TODO: Implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Timing settings";
    public static string StaticSettingKey => "segmentSettings_Timing";
}