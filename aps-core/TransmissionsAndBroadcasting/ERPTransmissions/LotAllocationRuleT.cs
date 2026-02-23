using PT.Transmissions;

namespace PT.ERPTransmissions;

public class LotAllocationRuleT : ERPMaintenanceTransmission<LotAllocationRuleT.LotAllocationRule>
{
    #region Serialization
    public LotAllocationRuleT(IReader reader)
        : base(reader)
    {
        int count;
        reader.Read(out count);
        for (int i = 0; i < count; ++i)
        {
            LotAllocationRule rule = new (reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        //Deprecated
    }

    public new const int UNIQUE_ID = 757;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public LotAllocationRuleT() { }

    public class LotAllocationRule : PTObjectBase
    {
        public LotAllocationRule(IReader reader)
            : base(reader)
        {
            LotAllocationRuleValues deprecatedValues = new (reader);
        }
    }
}