using System.Drawing;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class CommitmentSettings : ISettingData
{
    public string SettingKey => StaticSettingKey;
    public string Description => "TOOD:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Commitment settings";

    public CommitmentSettings(IMainForm a_mainForm)
    {
        Script = "";
        Show = false;
        IDynamicSkin theme = a_mainForm.CurrentBrand.ActiveTheme;
        EstimateColor = theme.CommitmentEstimate;
        PlannedColor = theme.CommitmentPlanned;
        FirmColor = theme.CommitmentFirm;
        ReleasedColor = theme.CommitmentReleased;

        Priority = 2;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
    }

    public CommitmentSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out EstimateColor);
            a_reader.Read(out PlannedColor);
            a_reader.Read(out FirmColor);
            a_reader.Read(out ReleasedColor);

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
        a_writer.Write(EstimateColor);
        a_writer.Write(PlannedColor);
        a_writer.Write(FirmColor);
        a_writer.Write(ReleasedColor);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    public string Script;
    public bool Show;
    public Color EstimateColor;
    public Color PlannedColor;
    public Color FirmColor;
    public Color ReleasedColor;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;

    public int UniqueId => 913;
    public static string StaticSettingKey => "segmentSettings_Commitment";
}