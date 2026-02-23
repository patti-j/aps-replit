using PT.APSCommon;
using PT.ERPTransmissions;
using PT.Transmissions;

namespace PT.Transmissions2;

public class ScenarioDetailUserFieldDefinitionT : UserFieldDefinitionT, IPTSerializable
{
    public override string Description => "User Field Definitions updated.";

    public ScenarioDetailUserFieldDefinitionT()
    {
    }

    #region IPTSerializable Members
    public ScenarioDetailUserFieldDefinitionT(IReader reader)
        : base(reader)
    {
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1099;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}