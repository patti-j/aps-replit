namespace PT.UIDefinitions.ControlSettings;

/// <summary>
/// Stores user settings for the InventoryPlan
/// </summary>
public class InventoryPlanSettings
{
    #region IPTSerializable Members
    private BoolVector32 m_bools;

    public InventoryPlanSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 614)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_ganttMaterialsIdx);
            a_reader.Read(out m_ganttProductsIdx);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_ganttMaterialsIdx);
        a_writer.Write(m_ganttProductsIdx);
    }
    #endregion

    public InventoryPlanSettings()
    {
        m_bools = new BoolVector32();
    }

    private const short c_includeForecastsIdx = 0;

    public bool IncludeForecasts
    {
        get => m_bools[c_includeForecastsIdx];
        set => m_bools[c_includeForecastsIdx] = value;
    }

    private short m_ganttMaterialsIdx;

    public short GanttMaterialsIdx
    {
        get => m_ganttMaterialsIdx;
        set => m_ganttMaterialsIdx = value;
    }

    private short m_ganttProductsIdx;

    public short GanttProductsIdx
    {
        get => m_ganttProductsIdx;
        set => m_ganttProductsIdx = value;
    }
}