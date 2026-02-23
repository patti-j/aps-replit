using PT.APSCommon;

namespace PT.Transmissions.Game;

/// <summary>
/// Signals the start of a new game, based on the game source scenario provided
/// </summary>
public class NewGameT : ScenarioBaseT
{
    //TODO: set
    public static readonly int UNIQUE_ID = 792;

    #region IPTSerializable Members
    public NewGameT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 493)
        {
            OriginalId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        OriginalId.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId OriginalId;

    public NewGameT() { }

    public NewGameT(BaseId a_originalId)
    {
        OriginalId = a_originalId;
    }
}