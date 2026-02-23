using PT.APSCommon;

namespace PT.Transmissions;

public class SetupRangeCopyT : SetupRangeBaseT
{
    #region IPTSerializable Members
    public SetupRangeCopyT(IReader reader)
        : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 578;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SetupRangeCopyT() { }

    public SetupRangeCopyT(BaseId scenarioId, BaseId aFromToRangeId)
        : base(scenarioId, aFromToRangeId) { }

    public override string Description => "Attribute Number Range Table updated";
}