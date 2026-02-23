using System.Drawing;

using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Interfaces;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class ProcessSettings : ISettingData
{
    public ProcessSettings(IMainForm a_mainForm)
    {
        Script = "";
        ProcessStoragePostScript = "";
        ProcessStorageScript = "";
        ProcessCleanScript = "";
        ProcessPostScript = "";
        ProcessRunScript = "";
        ProcessSetupScript = "";
        Show = true;
        UseSetupCodeColors = false;
        IDynamicSkin theme = a_mainForm.CurrentBrand.ActiveTheme;
        SetupColor = theme.SettingUpColor;
        RunColor = theme.RunningColor;
        PostProcessingColor = theme.PostProcessingColor;
        CleanColor = theme.CleanColor;
        StorageColor = theme.StorageColor;
        StoragePostProcessingColor = theme.StoragePostProcessColor;
        StorageExpirationColor = theme.StorageExpirationColor;
        Priority = 4;
        MinHeight = 0;
        ProportionalHeightWeight = 25;
    }

    public ProcessSettings(IReader a_reader) : this(a_reader, null) { }

    public ProcessSettings(IReader a_reader, IMainForm a_mainForm)
    {
        if (a_reader.VersionNumber >= 12559)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out Script);
            a_reader.Read(out ProcessStoragePostScript);
            a_reader.Read(out ProcessStorageScript);
            a_reader.Read(out ProcessCleanScript);
            a_reader.Read(out ProcessPostScript);
            a_reader.Read(out ProcessRunScript);
            a_reader.Read(out ProcessSetupScript);
            a_reader.Read(out SetupColor);
            a_reader.Read(out RunColor);
            a_reader.Read(out StorageColor);
            a_reader.Read(out PostProcessingColor);
            a_reader.Read(out CleanColor);
            a_reader.Read(out StoragePostProcessingColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else if (a_reader.VersionNumber >= 12317)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out Script);
            a_reader.Read(out ProcessStoragePostScript);
            a_reader.Read(out ProcessStorageScript);
            a_reader.Read(out ProcessCleanScript);
            a_reader.Read(out ProcessPostScript);
            a_reader.Read(out ProcessRunScript);
            a_reader.Read(out ProcessSetupScript);
            a_reader.Read(out SetupColor);
            a_reader.Read(out RunColor);
            a_reader.Read(out StorageColor);
            a_reader.Read(out PostProcessingColor);
            a_reader.Read(out CleanColor);
            a_reader.Read(out StoragePostProcessingColor);
            a_reader.Read(out Color _);
            a_reader.Read(out Color _);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            a_reader.Read(out Script);
            a_reader.Read(out ProcessStoragePostScript);
            a_reader.Read(out ProcessStorageScript);
            a_reader.Read(out ProcessCleanScript);
            a_reader.Read(out ProcessPostScript);
            a_reader.Read(out ProcessRunScript);
            a_reader.Read(out ProcessSetupScript);
            a_reader.Read(out bool show);
            a_reader.Read(out bool useSetupCodeColors);
            a_reader.Read(out SetupColor);
            a_reader.Read(out RunColor);
            a_reader.Read(out StorageColor);
            a_reader.Read(out PostProcessingColor);
            a_reader.Read(out CleanColor);
            a_reader.Read(out StoragePostProcessingColor);

            a_reader.Read(out Priority);
            a_reader.Read(out MinHeight);
            a_reader.Read(out ProportionalHeightWeight);

            Show = show;
            UseSetupCodeColors = useSetupCodeColors;
            ShowTankPlot = true;
        }
        else
        {
            CleanColor = a_mainForm != null ? a_mainForm.CurrentBrand.ActiveTheme.CleanColor : Color.Empty;
            if (a_reader.VersionNumber >= 11000)
            {
                a_reader.Read(out Script);
                a_reader.Read(out ProcessStoragePostScript);
                a_reader.Read(out ProcessStorageScript);
                a_reader.Read(out ProcessPostScript);
                a_reader.Read(out ProcessRunScript);
                a_reader.Read(out ProcessSetupScript);
                a_reader.Read(out bool show);
                a_reader.Read(out bool useSetupCodeColors);
                a_reader.Read(out SetupColor);
                a_reader.Read(out RunColor);
                a_reader.Read(out StorageColor);
                a_reader.Read(out PostProcessingColor);
                a_reader.Read(out StoragePostProcessingColor);

                a_reader.Read(out Priority);
                a_reader.Read(out MinHeight);
                a_reader.Read(out ProportionalHeightWeight);

                Show = show;
                UseSetupCodeColors = useSetupCodeColors;
                ShowTankPlot = true;
            }
            else
            {
                a_reader.Read(out Script);
                a_reader.Read(out ProcessStoragePostScript);
                a_reader.Read(out ProcessStorageScript);
                a_reader.Read(out ProcessPostScript);
                a_reader.Read(out ProcessRunScript);
                a_reader.Read(out ProcessSetupScript);
                a_reader.Read(out bool show);
                a_reader.Read(out bool useSetupCodeColors);

                Show = show;
                UseSetupCodeColors = useSetupCodeColors;
                ShowTankPlot = true;
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(Script);
        a_writer.Write(ProcessStoragePostScript);
        a_writer.Write(ProcessStorageScript);
        a_writer.Write(ProcessCleanScript);
        a_writer.Write(ProcessPostScript);
        a_writer.Write(ProcessRunScript);
        a_writer.Write(ProcessSetupScript);
        a_writer.Write(SetupColor);
        a_writer.Write(RunColor);
        a_writer.Write(StorageColor);
        a_writer.Write(PostProcessingColor);
        a_writer.Write(CleanColor);
        a_writer.Write(StoragePostProcessingColor);

        a_writer.Write(Priority);
        a_writer.Write(MinHeight);
        a_writer.Write(ProportionalHeightWeight);
    }

    #region Property Accessors
    public string Script;
    public string ProcessStoragePostScript;
    public string ProcessStorageScript;
    public string ProcessPostScript;
    public string ProcessCleanScript = "";
    public string ProcessRunScript;
    public string ProcessSetupScript;

    public Color SetupColor;
    public Color RunColor;
    public Color StorageColor;
    public Color PostProcessingColor;
    public Color CleanColor;
    public Color StoragePostProcessingColor;
    public Color StorageExpirationColor;
    public uint Priority;
    public uint MinHeight;
    public uint ProportionalHeightWeight;

    private BoolVector32 m_bools;

    private const short c_showIdx = 0;
    private const short c_useSetupCodeColors = 1;
    private const short c_showTankPlotIdx = 2;

    public bool Show
    {
        get => m_bools[c_showIdx];
        set => m_bools[c_showIdx] = value;
    }

    public bool UseSetupCodeColors
    {
        get => m_bools[c_useSetupCodeColors];
        set => m_bools[c_useSetupCodeColors] = value;
    }

    public bool ShowTankPlot
    {
        get => m_bools[c_showTankPlotIdx];
        set => m_bools[c_showTankPlotIdx] = value;
    }
    #endregion

    public static string StaticSettingKey => "segmentSettings_Process";

    public int UniqueId => 916;
    public string SettingKey => StaticSettingKey;
    public string Description => "TOOD:";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.SegmentSettings;
    public string SettingCaption => "Process settings";
}