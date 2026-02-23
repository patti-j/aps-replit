using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

/// <summary>
/// Updates Lot allocation rules in a particular Scenario.
/// </summary>
public class ScenarioDetailLotAllocationRuleT : LotAllocationRuleT
{
    #region IPTSerializable Members
    public ScenarioDetailLotAllocationRuleT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            BaseId deprecated = new (reader);
        }
    }

    public new const int UNIQUE_ID = 762;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLotAllocationRuleT() { }
    public override string Description => "Lot allocation rules updated";
}