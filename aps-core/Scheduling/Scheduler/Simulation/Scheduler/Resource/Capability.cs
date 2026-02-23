namespace PT.Scheduler;

partial class Capability
{
    #region Similiarity
    internal int SimilarityComparison(Capability a_c)
    {
        return Id.Value.CompareTo(a_c.Id.Value);
    }
    #endregion
}