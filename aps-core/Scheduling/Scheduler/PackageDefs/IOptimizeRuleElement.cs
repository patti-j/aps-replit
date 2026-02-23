using PT.PackageDefinitions;

namespace PT.Scheduler.PackageDefs;

public interface IOptimizeRuleScoreElement : IOptimizeRuleElement
{
    /// <summary>
    /// Takes into account factor logic, Points, and a category multiplier calculate a score for this activity on the specified resource.
    /// </summary>
    /// <param name="a_act">The activity attempting to schedule</param>
    /// <param name="a_res">Resource the activity would be scheduling on</param>
    /// <param name="a_sd">Data Model</param>
    /// <param name="a_simClock">Schedule starting point</param>
    /// <returns>Total score taking into account all parameters and initialized settings</returns>
    decimal GetScore(InternalActivity a_act, Resource a_res, ScenarioDetail a_sd, long a_simClock);

    PackageEnums.ESequencingFactorCalculationDependency DependencyType { get; }

    PackageEnums.ESequencingFactorComplexity Complexity { get; }
}

public interface IEarlyWindowBufferOptimizeRuleElement
{
    /// <summary>
    /// Gets the penalty growth factor applied to early window sequencing. Incurred penalty can decline linearly,
    /// exponentially or logarithmically as the release date approaches the end of the early window.
    /// </summary>
    PackageEnums.ESequencingFactorEarlyWindowPenaltyScale EarlyWindowPenaltyScale { get; }

    /// <summary>
    /// This represents the maximum penalty incurred when the release date is at the start of the early window.
    /// </summary>
    decimal MaxPenalty { get; }

    /// <summary>
    /// This represents the minimum penalty incurred when the release date is at the end of the early window.
    /// </summary>
    decimal MinPenalty { get; }

    /// <summary>
    /// Applies a penalty to the specified score if the simulation clock falls within the early window period.
    /// </summary>
    /// <param name="a_score">The original score to which the penalty may be applied.</param>
    /// <param name="a_simClock">The current simulation clock.</param>
    /// <param name="a_earlyWindowStart">The start time of the early window period, inclusive. Must be less than or equal to <paramref
    /// name="a_earlyWindowEnd"/>.</param>
    /// <param name="a_earlyWindowEnd">The end time of the early window period, inclusive. Must be greater than or equal to <paramref
    /// name="a_earlyWindowStart"/>.</param>
    /// <returns>The adjusted score after applying the early window penalty if applicable. Returns the original score if no
    /// penalty is applied.</returns>
    decimal ApplyEarlyWindowPenalty(decimal a_score, long a_simClock, long a_earlyWindowStart, long a_earlyWindowEnd);
}

/// <summary>
/// This element overrides composite scores.  
/// </summary>
public interface IOverrideOptimizeRuleElement : IOptimizeRuleScoreElement { }

/// <summary>
/// This optimize rule can be configured by the user. The points and settings are configurable.
/// The Name and Description are shown to the user.
/// </summary>
public interface IWeightedOptimizeRuleElement : IMinimumScoreOptimizeRuleElement
{
    string Name { get; }

    string Description { get; }

    /// <summary>
    /// Category for grouping and scaling. For example On-Time Delivery
    /// </summary>
    string Category { get; }

    /// <summary>
    /// An explanation for the user on how the score gets calculated
    /// </summary>
    string CalculationExplanation { get; }

    /// <summary>
    /// The base Points used in calculating a sequence score
    /// This is set based on optimize rule settings before calculation. It is only set once
    /// </summary>
    decimal Points { set; }

    /// <summary>
    /// The current category multiplier.
    /// Default usage is a multiplier from 0 to 100 times
    /// Custom scaling can be implemented in the calculation,
    /// for example if the score should go negative instead of approaching zero
    /// The value will be set to decimal.MinValue if there is no scaling to be used
    /// </summary>
    decimal CategoryMultiplier { set; }

    /// <summary>
    /// Whether this sequencing factor should be used in calculating the scores
    /// If false, GetScore will not be called.
    /// Typically this will be whether the EffectivePoints of the factor != zero
    /// </summary>
    bool CalculateScores { get; }

    /// <summary>
    /// Return the effective Points taking into account Points and the CategoryMultiplier
    /// </summary>
    /// <returns></returns>
    decimal CalculateEffectivePoints(decimal a_basePoints, decimal a_categoryMultiplier);
}

/// <summary>
/// SetResMultiplier will be called with the value to use to adjust based on other resource scores
/// </summary>
public interface IAlternateResourceScoreElement : IOptimizeRuleScoreElement
{
    //This is set based on optimize rule settings before calculation. It is only set once
    decimal ResourceMultiplier { get; set; }
}

/// <summary>
/// This optimize rule can be configured by the user. The points and settings are configurable.
/// The Name and Description are shown to the user.
/// </summary>
public interface IConfigurableOptimizeRuleElement : IWeightedOptimizeRuleElement
{
    //Custom settings applicable for this optimize rule. This object can be created and changed from a UI element. 
    SettingData Settings { set; }
    void InitializeSettings();
}

public interface IMinimumScoreOptimizeRuleElement : IOptimizeRuleScoreElement
{
    decimal MinimumScore { get; set; }
    bool UseMinimumScore { get; set; }
}

/// <summary>
/// This sequencing factor is determined by Operation Attributes.
/// The Name and Description are shown to the user.
/// </summary>
public interface IAttributeOptimizeRuleElement : IConfigurableOptimizeRuleElement
{
    //Custom settings applicable for this optimize rule. This object can be created and changed from a UI element. 
    void SetComplexity(int a_attributeCount);
}

public static class OptimizeRuleConstraints
{
    public static readonly decimal MaxOptimizeRuleScore = 100000000000000000000m;
    public static readonly decimal MinOptimizeRuleScore = -MaxOptimizeRuleScore;
}