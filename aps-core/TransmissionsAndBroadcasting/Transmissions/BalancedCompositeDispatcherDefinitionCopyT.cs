using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new BalancedCompositeDispatcherDefinition by copying the specified BalancedCompositeDispatcherDefinition.
/// </summary>
public class BalancedCompositeDispatcherDefinitionCopyT : BalancedCompositeDispatcherDefinitionBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 22;

    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinitionCopyT(IReader reader)
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

    public BaseId originalId; //Id of the BalancedCompositeDispatcherDefinition to copy.

    public BalancedCompositeDispatcherDefinitionCopyT() { }

    public BalancedCompositeDispatcherDefinitionCopyT(BaseId scenarioId, BaseId originalId)
        : base(scenarioId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Optimize Rule copied";
}