using System.Drawing;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class SimplifyGanttStorageAreaSettings : ISettingData, ICloneable
{
    public const int UNIQUE_ID = 1908;
    public static string Key = "SimplifyGanttSettings_GanttStorage";
    public string SettingKey => Key;
    public string Description => "TODO:"; //TODO: implement this
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SimplifyGanttSettings;
    public string SettingCaption => "Gantt Storage settings";

    private BoolVector32 m_flags;

    #region IPTSerializable Members
    public SimplifyGanttStorageAreaSettings(IReader a_reader) : this(a_reader, null){}
    public SimplifyGanttStorageAreaSettings(IReader a_reader, IMainForm a_mainForm)
    {
        m_flags = new BoolVector32(a_reader);

        if (a_reader.VersionNumber >= 13000)
        {
            a_reader.Read(out StorageAreaColor);
            a_reader.Read(out ItemStorageColor);
            a_reader.Read(out StorageCleanoutColor);
            a_reader.Read(out StorageAreaScript);
            a_reader.Read(out ItemStorageScript);
        }
        else // Hydrogen
        {
            a_reader.Read(out StorageAreaColor);
            a_reader.Read(out ItemStorageColor);
            a_reader.Read(out StorageAreaScript);
            a_reader.Read(out ItemStorageScript);
        }
        
        if (a_mainForm != null)
        {
            if (StorageAreaColor.ToArgb() == Color.Empty.ToArgb())
            {
                StorageAreaColor = a_mainForm.CurrentBrand.ActiveTheme.StorageColor;
            }

            if (ItemStorageColor.ToArgb() == Color.Empty.ToArgb())
            {
                ItemStorageColor = a_mainForm.CurrentBrand.ActiveTheme.StoragePlotColor;
            }

            if (StorageCleanoutColor.ToArgb() == Color.Empty.ToArgb())
            {
                StorageCleanoutColor = a_mainForm.CurrentBrand.ActiveTheme.CleanColor;
            }
        }
    }

    public SimplifyGanttStorageAreaSettings()
    {
        StorageAreaScript = "";
        ItemStorageScript = "";
        Show = true;
    }
    public SimplifyGanttStorageAreaSettings(IMainForm a_mainForm)
    {
        StorageAreaScript = "";
        ItemStorageScript = "";
        Show = true;
        StorageAreaColor = a_mainForm.CurrentBrand.ActiveTheme.StorageColor;
        ItemStorageColor = a_mainForm.CurrentBrand.ActiveTheme.StoragePlotColor;
        StorageCleanoutColor = a_mainForm.CurrentBrand.ActiveTheme.CleanColor;
    }

    public void Serialize(IWriter writer)
    {
        m_flags.Serialize(writer);
        writer.Write(StorageAreaColor);
        writer.Write(ItemStorageColor);
        writer.Write(StorageCleanoutColor);
        writer.Write(StorageAreaScript);
        writer.Write(ItemStorageScript);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Gantt Storage Area Properties
    private const int c_showIdx = 0;

    public bool Show
    {
        get => m_flags[c_showIdx];
        set => m_flags[c_showIdx] = value;
    }
    public string StorageAreaScript = string.Empty;
    public string ItemStorageScript = string.Empty;

    public Color StorageAreaColor;
    public Color ItemStorageColor;
    public Color StorageCleanoutColor;
    #endregion

    object ICloneable.Clone()
    {
        return Clone();
    }

    public SimplifyGanttStorageAreaSettings Clone()
    {
        return (SimplifyGanttStorageAreaSettings)MemberwiseClone();
    }
}