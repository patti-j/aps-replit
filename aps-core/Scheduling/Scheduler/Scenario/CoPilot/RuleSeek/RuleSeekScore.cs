namespace PT.Scheduler.CoPilot.RuleSeek;

/// <summary>
/// Simulation result score that stores the KPI score and the rule sets that were used and changed to get that score.
/// </summary>
public class RuleSeekScore
{
    public RuleSeekScore(decimal a_scoreValue, bool a_lowerIsBetter, params BalancedCompositeDispatcherDefinition[] a_rules)
    {
        RuleSet = new List<BalancedCompositeDispatcherDefinition>();
        if (a_rules != null)
        {
            RuleSet.AddRange(a_rules);
        }

        Score = a_scoreValue;
        LowerIsBetter = a_lowerIsBetter;
    }

    public decimal Score;
    public bool LowerIsBetter; //based on the original KPI value.
    public List<BalancedCompositeDispatcherDefinition> RuleSet;
}