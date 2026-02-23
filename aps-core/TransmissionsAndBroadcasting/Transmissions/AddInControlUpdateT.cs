using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class AddInControlUpdateT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public AddInControlUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 375)
        {
            _list = new AddInControllerList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        _list.Serialize(writer);
    }

    public const int UNIQUE_ID = 721;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public AddInControlUpdateT() { }

    public AddInControlUpdateT(BaseId scenarioId)
        : base(scenarioId) { }

    private readonly AddInControllerList _list = new ();

    public AddInControllerList AddInControllerList => _list;

    public override string Description => "Extension settings modified";
}