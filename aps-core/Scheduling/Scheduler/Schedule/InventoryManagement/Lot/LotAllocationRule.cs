using PT.ERPTransmissions;

namespace PT.Scheduler;

public class LotAllocationRule : BaseObject
{
    internal LotAllocationRule(IReader reader)
        : base(reader)
    {
        LotAllocationRuleValues ruleValuesDeprecated = new (reader);
    }

    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public override string DefaultNamePrefix => "LotAllocationRule";
}