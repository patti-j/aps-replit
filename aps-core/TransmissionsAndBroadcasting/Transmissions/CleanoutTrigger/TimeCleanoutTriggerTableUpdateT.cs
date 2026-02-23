using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class TimeCleanoutTriggerTableUpdateT : TimeCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public TimeCleanoutTriggerTableUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_timeCleanoutTriggerTable = new TimeCleanoutTriggerTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_timeCleanoutTriggerTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1076;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public TimeCleanoutTriggerTableUpdateT() { }

    public TimeCleanoutTriggerTableUpdateT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    private TimeCleanoutTriggerTable m_timeCleanoutTriggerTable = new ();

    public TimeCleanoutTriggerTable TimeCleanoutTriggerTable
    {
        get => m_timeCleanoutTriggerTable;
        set => m_timeCleanoutTriggerTable = value;
    }
}