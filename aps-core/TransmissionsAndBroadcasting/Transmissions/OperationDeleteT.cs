using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Operation
/// </summary>
public class OperationDeleteT : OperationIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 817;

    public OperationDeleteT(BaseId a_scenarioId, BaseId a_jobId, BaseId a_manufacturingOrderId, BaseId a_operationId)
        : base(a_scenarioId, a_jobId, a_manufacturingOrderId, a_operationId) { }

    public OperationDeleteT() { }
    public OperationDeleteT(IReader a_reader) : base(a_reader) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;

    public override string Description => "Operation deleted";
}