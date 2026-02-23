using PT.APSCommon;

namespace PT.Transmissions;

public class SetupRangeDeleteT : SetupRangeBaseT
{
    #region IPTSerializable Members
    public SetupRangeDeleteT(IReader reader)
        : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 576;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SetupRangeDeleteT() { }

    public SetupRangeDeleteT(BaseId scenarioId, BaseId aFromToRangeId)
        : base(scenarioId, aFromToRangeId) { }

    public override string Description => "Attribute Number Range Table deleted";
}