namespace PT.PackageDefinitions;

public class BaseSequencingFactor
{
    private decimal m_points;

    public decimal Points
    {
        set => m_points = value;
    }

    private decimal m_categoryMultiplier;

    public decimal CategoryMultiplier
    {
        set => m_categoryMultiplier = value;
    }

    private decimal m_effectivePoints;

    protected decimal EffectivePoints => CalculateEffectivePoints(m_points, m_categoryMultiplier);
    
    public bool CalculateScores => EffectivePoints != decimal.Zero;

    private decimal m_minScore = 0;
    public decimal MinimumScore
    {
        get => m_minScore;
        set => m_minScore = value;
    }

    public bool UseMinimumScore { get; set; }

    public decimal MaxPenalty { get; set; }
    public decimal MinPenalty { get; set; }

    public PackageEnums.ESequencingFactorEarlyWindowPenaltyScale EarlyWindowPenaltyScale { get; set; }

    /// <summary>
    /// Calculates the effective points of a sequencing factor, which is used in
    /// determining the schedule. Our goal is to modify the impact of the category
    /// on the effective points of the sequencing factor so the idea is
    /// that -100 means decreasing it by 100%, 0 means no change, 100 means double (100% increase),
    /// 200 means triple, and so on until 1000. We do not want the sign of effective points
    /// to be different than the base points.
    /// </summary>
    /// <param name="a_basePoints">Range is any real numbers</param>
    /// <param name="a_categoryMultiplier">Range for this is -100 to 1000</param>
    /// <returns></returns>
    public decimal CalculateEffectivePoints(decimal a_basePoints, decimal a_categoryMultiplier)
    {
        // These conditions are here so that we don't flip the sign of the Effective Points.
        // For example, if base point is -100 and the category multiplier is also -100,
        // then doing plain multiplication would result in a positive effective point.
        // Our goal is to modify the impact of the category on the effective points of the
        // sequencing factor so the idea of is that -100 means decreasing it by 100%.
        // Then also 0 means no change, and 100 means we double the value. 
        if (a_categoryMultiplier < -100)
        {
            // This situation is not handled in the condition below and will cause unwanted
            // behavior, specifically, flipping the sign of the points.
            // The control that passes values into this function has been changed to only
            // pass in valid multiplier, but this is just here to make things robust. 
            m_effectivePoints = 0;
        }
        else if (a_categoryMultiplier < 0)
        {
            // We want to multiply by the complement (I think this is the word for it) here since
            // we're really decreasing the value by a percent when a_categoryMultiplier is negative
            m_effectivePoints = a_basePoints * ((100 - Math.Abs(a_categoryMultiplier)) / 100);
        }
        else
        {
            m_effectivePoints = a_basePoints + a_basePoints * (a_categoryMultiplier / 100);
        }

        return Math.Round(m_effectivePoints, 0, MidpointRounding.AwayFromZero);
    }

    public decimal ApplyEarlyWindowPenalty(decimal a_score, long a_simClock, long a_earlyWindowStart, long a_earlyWindowEnd)
    {
        if (EarlyWindowPenaltyScale == PackageEnums.ESequencingFactorEarlyWindowPenaltyScale.Disabled 
            || a_simClock < a_earlyWindowStart 
            || a_simClock > a_earlyWindowEnd)
        {
            return a_score;
        }

        long windowSizeTicks = a_earlyWindowEnd - a_earlyWindowStart;
        long position = a_simClock - a_earlyWindowStart;

        double elapsedPercentage = Math.Clamp(position / (double)windowSizeTicks, 0 , 1);
        double weightedPercentage = GetWeightedPercentage(elapsedPercentage);

        //weighted = 0 -> MaxPenalty
        //weighted = 1 -> MaxPenalty - MaxPenalty + MinPenalty = MinPenalty
        decimal weightedPenalty = (Math.Abs(a_score) * (MaxPenalty + (MinPenalty - MaxPenalty) * (decimal)weightedPercentage));
        return a_score - weightedPenalty;
    }

    private double GetWeightedPercentage(double a_elapsedPercentage)
    {
        const double c_baseValue = 10.0;
        switch (EarlyWindowPenaltyScale)
        {
            case PackageEnums.ESequencingFactorEarlyWindowPenaltyScale.Linear:
                return a_elapsedPercentage;
            case PackageEnums.ESequencingFactorEarlyWindowPenaltyScale.Logarithmic:
                
                // Logarithmic growth from 0 to 1 (-- log_10(1 + (10 - 1) * percentage elapsed) --)
                return Math.Log(1 + (c_baseValue - 1) * a_elapsedPercentage, c_baseValue);
            case PackageEnums.ESequencingFactorEarlyWindowPenaltyScale.Exponential:

                // Exponential growth from 0 to 1 (-- [10^(percentage elapsed) - 1] / (10 - 1) --)
                return (Math.Pow(c_baseValue, a_elapsedPercentage) - 1) / (c_baseValue - 1);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static string CategoryOnTimeDelivery => "On-Time Delivery";
    public static string CategoryProductivity => "Productivity";
    public static string CategoryFinancial => "Financial";
    public static string CategoryAttributes => "Attributes";
    public static string CategoryPriority => "Priority";
    public static string CategoryAvailability => "Availability";
    public static string CategoryResourceSelection => "ResourceSelection";
}

public class BaseAttributeSequencingFactor : BaseSequencingFactor
{
    protected PackageEnums.ESequencingFactorComplexity m_complexity;
    public void SetComplexity(int a_attributesCount)
    {
        if (a_attributesCount < 2)
        {
            m_complexity = PackageEnums.ESequencingFactorComplexity.Simple;
        }
        else if (a_attributesCount < 4)
        {
            m_complexity = PackageEnums.ESequencingFactorComplexity.Moderate;
        }
        else
        {
            m_complexity = PackageEnums.ESequencingFactorComplexity.Complex;
        }
    }
}