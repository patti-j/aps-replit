using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Contains a set of FinishedPredecessorMOReleaseInfos.
/// </summary>
public class FinishedPredecessorMOReleaseInfoManager : IPTSerializable
{
    #region IPTSerializable Members
    public FinishedPredecessorMOReleaseInfoManager(IReader reader)
    {
        if (reader.VersionNumber >= 60)
        {
            int count;

            reader.Read(out count);

            for (int i = 0; i < count; ++i)
            {
                FinishedPredecessorMOReleaseInfo ri = new (reader);
                finishedPredecessorMOReleaseInfos.Add(ri);
            }

            CalculateMaximumReleaseTicks();
        }
        else
        {
            throw new PTException("Deserialization version is too low for FinishedPredecessorMOReleaseInfoManager.");
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(Count);

        for (int i = 0; i < Count; ++i)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId =>
        // TODO:  Add FinishedPredecessorMOReleaseInfoManager.UniqueId getter implementation
        0;
    #endregion

    public FinishedPredecessorMOReleaseInfoManager() { }

    private readonly List<FinishedPredecessorMOReleaseInfo> finishedPredecessorMOReleaseInfos = new ();

    public int Count => finishedPredecessorMOReleaseInfos.Count;

    /// <summary>
    /// If you update an elements in this collection you need to call CalculateMaximumReleaseTicks() to make
    /// sure the right maxiumum date has been calculated.
    /// </summary>
    public FinishedPredecessorMOReleaseInfo this[int index] => finishedPredecessorMOReleaseInfos[index];

    internal void Add(ManufacturingOrder aPredMO, long aReadyTicks)
    {
        FinishedPredecessorMOReleaseInfo ri = new (aPredMO.Job.ExternalId, aPredMO.ExternalId, aReadyTicks);
        Add(ri);
    }

    internal void Add(FinishedPredecessorMOReleaseInfo aRI)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            FinishedPredecessorMOReleaseInfo ri = this[i];

            if (ri.SameMO(aRI))
            {
                Delete(ri);
            }
        }

        finishedPredecessorMOReleaseInfos.Add(aRI);
        CalculateMaximumReleaseTicks();
    }

    internal void Delete(FinishedPredecessorMOReleaseInfo info)
    {
        finishedPredecessorMOReleaseInfos.Remove(info);
        CalculateMaximumReleaseTicks();
    }

    private long maximumReleaseTicks;

    /// <summary>
    /// O(1).
    /// The maximum release ticks of all the FinishedPredecessorMOReleaseInfo objects in the collection.
    /// </summary>
    internal long MaximumReleaseTicks => maximumReleaseTicks;

    /// <summary>
    /// This function must be called when a FinishedPredecessorMOReleaseInfo is added, updated or deleted.
    /// </summary>
    internal void CalculateMaximumReleaseTicks()
    {
        maximumReleaseTicks = 0;

        for (int i = 0; i < Count; ++i)
        {
            FinishedPredecessorMOReleaseInfo ri = this[i];

            if (ri.ReadyDateTicks > maximumReleaseTicks)
            {
                maximumReleaseTicks = ri.ReadyDateTicks;
            }
        }
    }
}