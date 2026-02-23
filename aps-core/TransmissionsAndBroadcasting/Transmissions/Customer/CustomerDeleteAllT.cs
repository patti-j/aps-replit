using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all Customers in the specified Scenario (and all of their Resources).
/// </summary>
public class CustomerDeleteAllT : CustomerBaseT, IPTSerializable
{
    public override string Description => "All Customers deleted";

    public new const int UNIQUE_ID = 61;

    #region IPTSerializable Members
    public CustomerDeleteAllT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CustomerDeleteAllT() { }

    public CustomerDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }
}