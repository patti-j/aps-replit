using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public class TimeCleanoutTriggerTableCopyT : TimeCleanoutTriggerTableIdBaseT
{
    #region IPTSerializable Members
    public TimeCleanoutTriggerTableCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1074;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public TimeCleanoutTriggerTableCopyT() { }

    public TimeCleanoutTriggerTableCopyT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId, tableId) { }

    public override string Description => "Fixed Time Cleanout Trigger Table copied";
}