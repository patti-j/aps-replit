using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new CapacityInterval by copying the specified CapacityInterval.
/// </summary>
public class CapacityIntervalCopyT : CapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 39;

    #region IPTSerializable Members
    public CapacityIntervalCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId; //Id of the CapacityInterval to copy.

    public CapacityIntervalCopyT() { }

    public CapacityIntervalCopyT(BaseId scenarioId, BaseId originalId)
        : base(scenarioId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Capacity copied";
}