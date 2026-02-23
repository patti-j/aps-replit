using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new PTAttribute by copying the specified PTAttribute.
/// </summary>
public class PTAttributeCopyT : PTAttributeBaseT
{
    public override string Description => "PTAttribute copied";

    public new const int UNIQUE_ID = 987;

    #region IPTSerializable Members
    public PTAttributeCopyT(IReader a_reader)
        : base(a_reader)
    {
        OriginalId = new BaseId(a_reader);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        OriginalId.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId OriginalId; //Id of the PTAttribute to copy.

    public PTAttributeCopyT() { }

    public PTAttributeCopyT(BaseId a_scenarioId, BaseId a_originalId)
        : base(a_scenarioId)
    {
        OriginalId = a_originalId;
    }
}