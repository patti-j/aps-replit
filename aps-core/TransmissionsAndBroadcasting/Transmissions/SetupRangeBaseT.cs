using PT.APSCommon;

namespace PT.Transmissions;

//575-581
public abstract class SetupRangeBaseT : ScenarioIdBaseT
{
    protected SetupRangeBaseT() { }

    internal SetupRangeBaseT(BaseId aScenarioId, BaseId aFromToRangeId)
        : base(aScenarioId)
    {
        fromToRangeId = aFromToRangeId;
    }

    #region IPTSerializable
    public SetupRangeBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            fromToRangeId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        fromToRangeId.Serialize(writer);
    }

    public const int UNIQUE_ID = 575;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly BaseId fromToRangeId;

    public BaseId FromToRangeId => fromToRangeId;
}