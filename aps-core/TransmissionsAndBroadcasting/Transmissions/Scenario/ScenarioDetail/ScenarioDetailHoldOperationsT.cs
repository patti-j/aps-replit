using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Holding or Unholding a list of Operations.
/// </summary>
public class ScenarioDetailHoldOperationsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 275;

    #region IPTSerializable Members
    public ScenarioDetailHoldOperationsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out holdit);
            reader.Read(out holdReason);

            reader.Read(out holdUntilDate);

            operations = new OperationKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(holdit);
        writer.Write(holdReason);

        writer.Write(holdUntilDate);

        operations.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailHoldOperationsT() { }

    public ScenarioDetailHoldOperationsT(BaseId scenarioId, OperationKeyList operations, bool holdit, DateTime holdUntilDate, string holdReason)
        : base(scenarioId)
    {
        this.operations = operations;
        this.holdit = holdit;
        this.holdUntilDate = holdUntilDate;
        this.holdReason = holdReason;
    }

    private readonly OperationKeyList operations;

    public OperationKeyList Operations => operations;

    private readonly bool holdit;

    public bool Holdit => holdit;

    private readonly DateTime holdUntilDate;

    public DateTime HoldUntilDate => holdUntilDate;

    private readonly string holdReason;

    public string HoldReason => holdReason;

    public override string Description => "Operation Hold status updated";
}