namespace PT.Scheduler;

partial class BaseIdObjectManager<T>
{
    internal virtual int SimilarityComparison(BaseIdObjectManager<T> a_bm)
    {
        return Count.CompareTo(a_bm.Count);
    }
}