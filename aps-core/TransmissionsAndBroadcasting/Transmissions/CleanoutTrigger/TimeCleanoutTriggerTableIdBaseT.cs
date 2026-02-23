using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

public abstract class TimeCleanoutTriggerTableIdBaseT : TimeCleanoutTriggerTableBaseT
{
    #region IPTSerializable Members
    public TimeCleanoutTriggerTableIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            id = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        id.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1073;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected TimeCleanoutTriggerTableIdBaseT() { }

    protected TimeCleanoutTriggerTableIdBaseT(BaseId scenarioId, BaseId tableId)
        : base(scenarioId)
    {
        id = tableId;
    }

    private readonly BaseId id;

    public BaseId Id => id;
}