using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Confirming or Unconfirming Operations.
/// </summary>
public class ScenarioDetailConfirmOperationConstraintsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 125;

    #region IPTSerializable Members
    public ScenarioDetailConfirmOperationConstraintsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            operations = new OperationKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        operations.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailConfirmOperationConstraintsT() { }

    public ScenarioDetailConfirmOperationConstraintsT(BaseId scenarioId, OperationKeyList operations, bool confirm)
        : base(scenarioId)
    {
        this.operations = operations;
    }

    private readonly OperationKeyList operations;

    public OperationKeyList Operations => operations;

    public override string Description => "Operation confirmed";
}