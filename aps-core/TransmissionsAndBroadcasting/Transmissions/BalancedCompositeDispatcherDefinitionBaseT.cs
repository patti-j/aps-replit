using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all BalancedCompositeDispatcherDefinition related transmissions.
/// </summary>
public abstract class BalancedCompositeDispatcherDefinitionBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 20;

    #region PT Serialization
    public BalancedCompositeDispatcherDefinitionBaseT(IReader reader)
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

    protected BalancedCompositeDispatcherDefinitionBaseT() { }

    public BalancedCompositeDispatcherDefinitionBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}