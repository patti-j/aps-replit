using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class AttributeSegmentSettings : ISettingData
{
    public string Script;
    public bool Show;
    public bool ShowNameAttribute;
    public bool ShowCodeAttribute;
    public bool ShowNumberAttribute;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;

    public AttributeSegmentSettings()
    {
        Script = "";
        Show = false;
        ShowNameAttribute = true;
        ShowCodeAttribute = true;
        ShowNumberAttribute = true;

        Priority = 3;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
    }

    public AttributeSegmentSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12303)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out ShowNameAttribute);
            a_reader.Read(out ShowCodeAttribute);
            a_reader.Read(out ShowNumberAttribute);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else if (a_reader.VersionNumber >= 12046)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out bool ShowProductColorAttribute);
            a_reader.Read(out bool ShowSetupColorAttribute);
            a_reader.Read(out bool ShowCustomAttributes);
            a_reader.Read(out bool ShowJobColorAttribute);
            a_reader.Read(out ShowNameAttribute);
            a_reader.Read(out ShowCodeAttribute);
            a_reader.Read(out ShowNumberAttribute);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out bool ShowProductColorAttribute);
            a_reader.Read(out bool ShowSetupColorAttribute);
            a_reader.Read(out bool ShowCustomAttributes);
            a_reader.Read(out bool ShowJobColorAttribute);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out bool ShowProductColorAttribute);
            a_reader.Read(out bool ShowSetupColorAttribute);
            a_reader.Read(out bool ShowCustomAttributes);
            a_reader.Read(out bool ShowJobColorAttribute);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Script);
        a_writer.Write(Show);
        a_writer.Write(ShowNameAttribute);
        a_writer.Write(ShowCodeAttribute);
        a_writer.Write(ShowNumberAttribute);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    public int UniqueId => 911;

    public string SettingKey => "segmentSettings_Attribute";
    public string Description => "TODO:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Attribute Segments";
}