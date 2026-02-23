using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Used by Shop Views and internal ActivityFinish to report material issuing.
/// </summary>
public class MaterialIssue
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 706;

    public MaterialIssue(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            materialRequirementId = new BaseId(reader);
            fromWarehouseId = new BaseId(reader);
            reader.Read(out qtyToIssue);
        }
    }

    public void Serialize(IWriter writer)
    {
        materialRequirementId.Serialize(writer);
        fromWarehouseId.Serialize(writer);
        writer.Write(qtyToIssue);
    }
    #endregion

    public MaterialIssue(BaseId aMaterialRequirementId, BaseId aFromWarehouseId, decimal aQtyToIssue)
    {
        materialRequirementId = aMaterialRequirementId;
        fromWarehouseId = aFromWarehouseId;
        qtyToIssue = aQtyToIssue;
    }

    private readonly BaseId materialRequirementId;

    public BaseId MaterialRequirementId => materialRequirementId;

    private readonly BaseId fromWarehouseId;

    public BaseId FromWarehouseId => fromWarehouseId;

    private readonly decimal qtyToIssue;

    public decimal QtyToIssue => qtyToIssue;
}