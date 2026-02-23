using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all BalancedCompositeDispatcherDefinitions in the specified Scenario (and all of their Resources).
/// </summary>
public class BalancedCompositeDispatcherDefinitionDeleteAllT : BalancedCompositeDispatcherDefinitionBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 24;

    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinitionDeleteAllT(IReader reader)
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

    public BalancedCompositeDispatcherDefinitionDeleteAllT() { }
    public BalancedCompositeDispatcherDefinitionDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Optimize Rules deleted";
}