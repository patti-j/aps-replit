using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Holding or Unholding a list of MOs.
/// </summary>
public class ScenarioDetailHoldMOsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 274;

    #region IPTSerializable Members
    public ScenarioDetailHoldMOsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out holdit);
            reader.Read(out holdReason);

            reader.Read(out holdUntilDate);

            mos = new MOKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(holdit);
        writer.Write(holdReason);

        writer.Write(holdUntilDate);

        mos.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailHoldMOsT() { }

    public ScenarioDetailHoldMOsT(BaseId scenarioId, MOKeyList mos, bool holdit, DateTime holdUntilDate, string holdReason)
        : base(scenarioId)
    {
        this.mos = mos;
        this.holdit = holdit;
        this.holdUntilDate = holdUntilDate;
        this.holdReason = holdReason;
    }

    private readonly MOKeyList mos;

    public MOKeyList MOs => mos;

    private readonly bool holdit;

    public bool Holdit => holdit;

    private readonly DateTime holdUntilDate;

    public DateTime HoldUntilDate => holdUntilDate;

    private readonly string holdReason;

    public string HoldReason => holdReason;

    public override string Description => "ManufacturingOrder Hold status updated";
}