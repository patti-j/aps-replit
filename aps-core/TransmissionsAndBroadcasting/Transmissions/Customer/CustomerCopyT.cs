using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Customer by copying the specified Customer.
/// </summary>
public class CustomerCopyT : CustomerBaseT, IPTSerializable
{
    public override string Description => "Customer copied";

    public new const int UNIQUE_ID = 59;

    #region IPTSerializable Members
    public CustomerCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }

        originalId = new BaseId(reader);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId; //Id of the Customer to copy.

    public CustomerCopyT() { }

    public CustomerCopyT(BaseId scenarioId, BaseId originalId)
        : base(scenarioId)
    {
        this.originalId = originalId;
    }
}