using PT.APSCommon;

namespace PT.Transmissions;

public class ScenarioClearUndoSetsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 904;
    public override string Description => "Cleared Undo Sets";

    #region IPTSerializable Members
    public ScenarioClearUndoSetsT(IReader reader) : base(reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioClearUndoSetsT() { }

    public ScenarioClearUndoSetsT(BaseId a_scenarioId) : base(a_scenarioId)
    {
        ReplayForUndoRedo = false;
    }

    public ScenarioClearUndoSetsT(ScenarioIdBaseT a_t) : base(a_t)
    {
        ReplayForUndoRedo = false;
    }
}