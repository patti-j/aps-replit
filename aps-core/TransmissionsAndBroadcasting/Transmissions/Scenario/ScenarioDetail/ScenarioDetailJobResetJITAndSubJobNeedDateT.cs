using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Resetting the JIT and Sub-Job Need dates for a list of Jobs.
/// </summary>
public class ScenarioDetailJobResetJITAndSubJobNeedDateT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 179;

    #region IPTSerializable Members
    public ScenarioDetailJobResetJITAndSubJobNeedDateT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_bools = new BoolVector32(a_reader);
            m_selectedMOs = new MOKeyList(a_reader);
            a_reader.Read(out short datePointEnum);
            m_subJobNeedDateResetPoint = (ScenarioOptions.ESubJobNeedDateResetPoint)datePointEnum;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_selectedMOs.Serialize(a_writer);
        a_writer.Write((short)m_subJobNeedDateResetPoint);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailJobResetJITAndSubJobNeedDateT() { }

    public ScenarioDetailJobResetJITAndSubJobNeedDateT(BaseId a_scenarioId, MOKeyList a_moKeyList, bool a_resetSubJobNeedDates, ScenarioOptions.ESubJobNeedDateResetPoint a_subJobNeedDateResetPoint)
        : base(a_scenarioId)
    {
        if (a_moKeyList == null || a_moKeyList.Count == 0)
        {
            RecalculateAllSettings = true;
            m_selectedMOs = new MOKeyList();
        }
        else
        {
            ResetSubJobNeedDates = a_resetSubJobNeedDates;
            m_selectedMOs = a_moKeyList;
        }

        m_subJobNeedDateResetPoint = a_subJobNeedDateResetPoint;
    }

    private readonly MOKeyList m_selectedMOs;
    private readonly ScenarioOptions.ESubJobNeedDateResetPoint m_subJobNeedDateResetPoint;

    public ScenarioOptions.ESubJobNeedDateResetPoint SubJobNeedDateResetPoint => m_subJobNeedDateResetPoint;

    public MOKeyList ManufacturingOrders => m_selectedMOs;

    private const short c_resetSubJobNeedDatesIdx = 0;
    private const short c_recalculateAllSettingsForAllJobs = 1;

    public bool ResetSubJobNeedDates
    {
        get => m_bools[c_resetSubJobNeedDatesIdx];
        private set => m_bools[c_resetSubJobNeedDatesIdx] = value;
    }

    public bool RecalculateAllSettings
    {
        get => m_bools[c_recalculateAllSettingsForAllJobs];
        private set => m_bools[c_recalculateAllSettingsForAllJobs] = value;

    }

    private BoolVector32 m_bools;
    
    #region IHistory Members
    public override string Description
    {
        get
        {
            if (RecalculateAllSettings)
            {
                return "Reset the JIT and Sub-MO need date of all Jobs".Localize();
            }
            
            if (ResetSubJobNeedDates)
            {
                return m_selectedMOs.Count == 1 ? "Job JIT and Sub-Job Need date reset" : string.Format("Reset the JIT and Sub-MO need date of ({0}) Jobs".Localize(), m_selectedMOs.Count);
            }

            return m_selectedMOs.Count == 1 ? "Job JIT Date reset" : string.Format("Reset JIT date of ({0}) Jobs".Localize(), m_selectedMOs.Count);
        }
    }
    #endregion
}
