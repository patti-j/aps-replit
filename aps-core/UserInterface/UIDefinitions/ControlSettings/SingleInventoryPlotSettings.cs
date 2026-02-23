using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class SingleInventoryPlotSettings : ISettingData
{
    #region IPTSerializable Members
    private BoolVector32 m_bools;

    public SingleInventoryPlotSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 607)
        {
            m_bools = new BoolVector32(a_reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
    }

    public int UniqueId => 0; //TODO
    #endregion

    public SingleInventoryPlotSettings()
    {
        m_bools = new BoolVector32();
        ShowOnHandPlot = true;
        ShowBelowSafetyPlot = true;
        ShowShortagePlot = true;
        ShowSafetyStockLine = true;
        ShowFirstShortageLine = true;
        ShowOverMaxPlot = true;
        ShowAdjustmentsAfterPlanningHorizon = true;
        ShowFirstDisposalLine = true;
    }

    private const short c_showOnHandPlotIdx = 0;
    private const short c_showBelowSafetyPlotIdx = 1;
    private const short c_showShortagePlotIdx = 2;
    private const short c_showSafetyStockLineIdx = 3;
    private const short c_showFirstShortageLineIdx = 4;
    private const short c_showOverMaxPlotIdx = 5;
    private const short c_showMaxInventoryLineIdx = 6;
    private const short c_showAdjustmentsAfterPlanningHorizon = 7;
    private const short c_showFirstDisposalLineIdx = 8;

    public bool ShowOnHandPlot
    {
        get => m_bools[c_showOnHandPlotIdx];
        set => m_bools[c_showOnHandPlotIdx] = value;
    }

    public bool ShowBelowSafetyPlot
    {
        get => m_bools[c_showBelowSafetyPlotIdx];
        set => m_bools[c_showBelowSafetyPlotIdx] = value;
    }

    public bool ShowShortagePlot
    {
        get => m_bools[c_showShortagePlotIdx];
        set => m_bools[c_showShortagePlotIdx] = value;
    }

    public bool ShowOverMaxPlot
    {
        get => m_bools[c_showOverMaxPlotIdx];
        set => m_bools[c_showOverMaxPlotIdx] = value;
    }

    public bool ShowSafetyStockLine
    {
        get => m_bools[c_showSafetyStockLineIdx];
        set => m_bools[c_showSafetyStockLineIdx] = value;
    }

    public bool ShowMaxInventoryLine
    {
        get => m_bools[c_showMaxInventoryLineIdx];
        set => m_bools[c_showMaxInventoryLineIdx] = value;
    }

    public bool ShowFirstShortageLine
    {
        get => m_bools[c_showFirstShortageLineIdx];
        set => m_bools[c_showFirstShortageLineIdx] = value;
    }
    public bool ShowFirstDisposalLine
    {
        get => m_bools[c_showFirstDisposalLineIdx];
        set => m_bools[c_showFirstDisposalLineIdx] = value;
    }

    public bool ShowAdjustmentsAfterPlanningHorizon
    {
        get => m_bools[c_showAdjustmentsAfterPlanningHorizon];
        set => m_bools[c_showAdjustmentsAfterPlanningHorizon] = value;
    }

    public string SettingKey => "SingleInventoryPlotSettings";
    public string SettingCaption => "Inventory Plot Settings";
    public string Description => "Stores plot layout";
    public string SettingsGroup => SettingGroupConstants.InventoryPlanSettings;
    public string SettingsGroupCategory => SettingGroupConstants.InventoryPlanSettings;
}