using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

/// <summary>
/// Create a new table.
/// </summary>
public class TimeCleanoutTriggerTableNewT : TimeCleanoutTriggerTableBaseT
{
    #region IPTSerializable Members
    public TimeCleanoutTriggerTableNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12305)
        {
            cleanoutTriggerTable = new TimeCleanoutTriggerTable(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        cleanoutTriggerTable.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1063;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public TimeCleanoutTriggerTableNewT() { }

    public TimeCleanoutTriggerTableNewT(BaseId scenarioId)
        : base(scenarioId) { }

    private TimeCleanoutTriggerTable cleanoutTriggerTable = new ();

    public TimeCleanoutTriggerTable CleanoutTriggerTable
    {
        get => cleanoutTriggerTable;
        set => cleanoutTriggerTable = value;
    }

    public override string Description => "Fixed Time Cleanout Trigger Table created";
}