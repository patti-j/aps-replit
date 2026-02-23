using PT.APSCommon;

namespace PT.Transmissions;

public class SetDefaultDispatcherDefinitionOfDefinitionManagerT : DispatcherDefinitionManagerBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 72;

    public override string Description => "Default Optimize Rule was set.";

    #region IPTSerializable Members
    public SetDefaultDispatcherDefinitionOfDefinitionManagerT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            defaultDefinitionId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        defaultDefinitionId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId defaultDefinitionId;

    public SetDefaultDispatcherDefinitionOfDefinitionManagerT() { }

    public SetDefaultDispatcherDefinitionOfDefinitionManagerT(BaseId scenarioId, BaseId defaultDefinitionId)
        : base(scenarioId)
    {
        this.defaultDefinitionId = defaultDefinitionId;
    }
}