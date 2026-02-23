using System.Drawing;

using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class MaterialStatusSettings : ISettingData, ICloneable
{
    public MaterialStatusSettings(IMainForm a_mainForm)
    {
        Script = "";
        Show = true;
        ItemTypeFilterList = new List<ItemDefs.itemTypes>();
        MaterialSourcesAvailableColor = a_mainForm.CurrentBrand.ActiveTheme.MaterialsAvailable;
        MaterialSourcesFirmColor = a_mainForm.CurrentBrand.ActiveTheme.MaterialSourcesFirmColor;
        MaterialSourcesPlannedColor = a_mainForm.CurrentBrand.ActiveTheme.MaterialSourcesPlannedColor;
        MaterialSourcesUnknownColor = a_mainForm.CurrentBrand.ActiveTheme.MaterialSourceUnknownColor;
        MaterialIgnoredConstraintViolationColor = a_mainForm.CurrentBrand.ActiveTheme.MaterialConstraintViolationColor;
        Priority = 5;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
    }

    public MaterialStatusSettings(IReader a_reader)
    {
        ItemTypeFilterList = new List<ItemDefs.itemTypes>();

        if (a_reader.VersionNumber >= 13014)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int val);
                ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
            }

            a_reader.Read(out MaterialSourcesAvailableColor);
            a_reader.Read(out MaterialSourcesFirmColor);
            a_reader.Read(out MaterialSourcesPlannedColor);
            a_reader.Read(out MaterialSourcesUnknownColor);
            a_reader.Read(out MaterialIgnoredConstraintViolationColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else if (a_reader.VersionNumber >= 12007)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int val);
                ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
            }

            a_reader.Read(out MaterialSourcesAvailableColor);
            a_reader.Read(out MaterialSourcesFirmColor);
            a_reader.Read(out MaterialSourcesPlannedColor);
            a_reader.Read(out MaterialSourcesUnknownColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else if (a_reader.VersionNumber >= 11000)
        {
            a_reader.Read(out Script);
            a_reader.Read(out Show);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int val);
                ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
            }

            a_reader.Read(out MaterialSourcesAvailableColor);
            a_reader.Read(out MaterialSourcesFirmColor);
            a_reader.Read(out MaterialSourcesPlannedColor);
            a_reader.Read(out MaterialSourcesUnknownColor);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);

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

        a_writer.Write(MaterialSourcesAvailableColor);
        a_writer.Write(MaterialSourcesFirmColor);
        a_writer.Write(MaterialSourcesPlannedColor);
        a_writer.Write(MaterialSourcesUnknownColor);
        a_writer.Write(MaterialIgnoredConstraintViolationColor);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    #region Property Accessors
    public string Script;
    public bool Show;
    public List<ItemDefs.itemTypes> ItemTypeFilterList;
    public Color MaterialSourcesAvailableColor;
    public Color MaterialSourcesFirmColor;
    public Color MaterialSourcesPlannedColor;
    public Color MaterialSourcesUnknownColor;
    public Color MaterialIgnoredConstraintViolationColor;

    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;
    #endregion

    public int UniqueId => 915;

    public string SettingKey => StaticSettingKey;
    public string Description => "TOOD:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Material Status settings";
    public static string StaticSettingKey => "segmentSettings_MaterialStatus";

    object ICloneable.Clone()
    {
        return Clone();
    }

    public StatusSettings Clone()
    {
        return (StatusSettings)MemberwiseClone();
    }
}