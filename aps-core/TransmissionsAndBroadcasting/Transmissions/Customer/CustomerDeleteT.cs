using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Customer.
/// </summary>
public class CustomerDeleteT : CustomerIdBaseT
{
    public override string Description => "Customer deleted";

    public new const int UNIQUE_ID = 62;

    #region IPTSerializable Members
    public CustomerDeleteT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CustomerDeleteT() { }

    public CustomerDeleteT(BaseId a_scenarioId, IEnumerable<BaseId> a_customerIds)
        : base(a_scenarioId, a_customerIds) { }
}