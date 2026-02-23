namespace PT.Scheduler;

public class Resource_PathEligibilityInfo
{
    internal Resource_PathEligibilityInfo(InternalResource aRes, List<AlternatePath> aEligiblePaths)
    {
        Resource = aRes;
        EligiblePaths = aEligiblePaths;
    }

    public readonly InternalResource Resource;
    public readonly List<AlternatePath> EligiblePaths;

    public override string ToString()
    {
        string s = Resource + " :: " + EligiblePaths.Count + " Eligible Paths";
        return s;
    }
}