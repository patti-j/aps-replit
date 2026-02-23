using PT.APSCommon;

namespace PT.Transmissions;

public class ScenarioUndoCheckpointT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 139;

    #region IPTSerializable Members
    public ScenarioUndoCheckpointT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 103)
        {
            // The values in VersionNumber 102 were moved into ScenarioChecksumT.
        }
        else if (reader.VersionNumber >= 102)
        {
            double dTemp;
            reader.Read(out dTemp);
            reader.Read(out dTemp);

            int iTemp;
            reader.Read(out iTemp);
        }
        else if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioUndoCheckpointT() { }

    /// <summary>
    /// Signal that an undo checkpoint needs to be created. And send over the server's schedule checksums.
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="aStartAndEndSums">The sum of every block's start and end time.</param>
    /// <param name="aResourceJobOperationCombos">For every block, the sum of Resource*Job*Operation</param>
    /// <param name="aBlockCount">The number of blocks in the schedule.</param>
    public ScenarioUndoCheckpointT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Create undo checkpoint";
}