using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the BalancedCompositeDispatcherDefinition (and all of its Resources).
/// </summary>
public class BalancedCompositeDispatcherDefinitionDeleteT : BalancedCompositeDispatcherDefinitionIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 25;

    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinitionDeleteT(IReader reader)
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

    public BalancedCompositeDispatcherDefinitionDeleteT() { }

    public BalancedCompositeDispatcherDefinitionDeleteT(BaseId scenarioId, BaseId balancedCompositeDispatcherDefinitionId)
        : base(scenarioId, balancedCompositeDispatcherDefinitionId) { }

    public override string Description => "Optimize Rule deleted";
}