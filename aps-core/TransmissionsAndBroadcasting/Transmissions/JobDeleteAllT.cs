using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all Jobs in the specified Scenario .
/// </summary>
public class JobDeleteAllT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 77;

    #region IPTSerializable Members
    public JobDeleteAllT(IReader reader)
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

    public JobDeleteAllT() { }

    public JobDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Jobs deleted";
}