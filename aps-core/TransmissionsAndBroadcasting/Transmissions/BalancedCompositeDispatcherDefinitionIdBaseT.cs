using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all BalancedCompositeDispatcherDefinition related transmissions.
/// </summary>
public abstract class BalancedCompositeDispatcherDefinitionIdBaseT : BalancedCompositeDispatcherDefinitionBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 26;

    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinitionIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            balancedCompositeDispatcherDefinitionId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        balancedCompositeDispatcherDefinitionId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId balancedCompositeDispatcherDefinitionId;

    protected BalancedCompositeDispatcherDefinitionIdBaseT() { }

    public BalancedCompositeDispatcherDefinitionIdBaseT(BaseId scenarioId, BaseId balancedCompositeDispatcherDefinitionId)
        : base(scenarioId)
    {
        this.balancedCompositeDispatcherDefinitionId = balancedCompositeDispatcherDefinitionId;
    }
}