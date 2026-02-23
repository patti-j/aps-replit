using PT.SchedulerDefinitions;

using static PT.PackageDefinitions.PackageEnums;

namespace PT.Scheduler;

//TODO: Remove this class and create IGanttEligibilityIndicatorElements
/// <summary>
/// Each eligible resource only appears in this set once.
/// </summary>
public class PlantEligibilityInfo
{
    internal PlantEligibilityInfo() { }

    /// <summary>
    /// The resources eligible as the same path.
    /// </summary>
    public readonly List<Resource_PathEligibilityInfo> EligibleAsSame = new ();

    /// <summary>
    /// The resources eligible as a different path.
    /// </summary>
    public readonly List<Resource_PathEligibilityInfo> EligibleAsDifferent = new ();

    /// <summary>
    /// The resources eligible as the same path and a different path.
    /// </summary>
    public readonly List<Resource_PathEligibilityInfo> EligibleAsSameAndDifferent = new ();

    private readonly Dictionary<TripleLongKey, EBlockEligibilityTypes> eligibilityDictionary = new ();

    public EBlockEligibilityTypes GetEligibilityType(BaseResource ir)
    {
        EBlockEligibilityTypes et;
        if (eligibilityDictionary.TryGetValue(CreateTLK(ir), out et))
        {
            return et;
        }

        return EBlockEligibilityTypes.NotEligible;
    }

    /// <summary>
    /// This function must be called after the lists have been populated otherwise GetEligibilityType() won't work.
    /// </summary>
    internal void ConstructionComplete()
    {
        InitEligibility(EligibleAsSame, EBlockEligibilityTypes.SamePath);
        InitEligibility(EligibleAsDifferent, EBlockEligibilityTypes.DifferentPath);
        InitEligibility(EligibleAsSameAndDifferent, EBlockEligibilityTypes.SameAndDifferentPath);
    }

    private void InitEligibility(List<Resource_PathEligibilityInfo> eil, EBlockEligibilityTypes type)
    {
        for (int i = 0; i < eil.Count; ++i)
        {
            Resource_PathEligibilityInfo ei = eil[i];
            eligibilityDictionary.Add(CreateTLK(ei.Resource), type);
        }
    }

    private TripleLongKey CreateTLK(BaseResource res)
    {
        return new TripleLongKey(res.Id.Value, res.Department.Id.Value, res.Department.Plant.Id.Value);
    }
}