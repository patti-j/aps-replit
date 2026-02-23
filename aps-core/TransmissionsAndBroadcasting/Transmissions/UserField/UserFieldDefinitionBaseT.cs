using PT.APSCommon;
using PT.Transmissions.User;

namespace PT.Transmissions;

/// <summary>
/// Base object for all UserFieldDefinition related transmissions.
/// </summary>
public abstract class UserFieldDefinitionBaseT : ScenarioBaseT
{
    // I'm pretty sure this isn't necessary since this is an abstract class
    //public const int UNIQUE_ID = 1116;

    #region IPTSerializable Members
    public UserFieldDefinitionBaseT(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }
    #endregion

    protected UserFieldDefinitionBaseT() { }
}