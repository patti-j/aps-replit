using PT.Common.Exceptions;

namespace PT.Scheduler.RangeLookup;

public class SetupRangeSet<RangeTy> : IComparer<SetupRange> where RangeTy : SetupRange, IPTSerializable
{
    #region IPTSerializable Members
    public SetupRangeSet(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int nbrSetupRanges;
            reader.Read(out nbrSetupRanges);

            for (int i = 0; i < nbrSetupRanges; ++i)
            {
                SetupRange setupRange;
                int uniqueId;
                reader.Read(out uniqueId);

                if (uniqueId == FromRange.UNIQUE_ID)
                {
                    setupRange = new FromRange(reader);
                }
                else if (uniqueId == ToRange.UNIQUE_ID)
                {
                    setupRange = new ToRange(reader);
                }
                else
                {
                    throw new PTException("4084");
                }

                setupRanges.Add(setupRange);
            }
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        writer.Write(setupRanges.Count);

        for (int i = 0; i < setupRanges.Count; ++i)
        {
            writer.Write(setupRanges[i].UniqueId);
            setupRanges[i].Serialize(writer);
        }
    }

    public const int UNIQUE_ID = 610;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    internal SetupRangeSet() { }

    internal SetupRangeSet(SetupRangeSet<RangeTy> setupRangeSet)
    {
        for (int i = 0; i < setupRangeSet.Count; ++i)
        {
            SetupRange origSR = setupRangeSet[i];
            SetupRange setupRange;

            if (origSR.UniqueId == FromRange.UNIQUE_ID)
            {
                setupRange = new FromRange((FromRange)origSR);
            }
            else if (origSR.UniqueId == ToRange.UNIQUE_ID)
            {
                setupRange = new ToRange((ToRange)origSR);
            }
            else
            {
                throw new PTException("4084");
            }

            setupRanges.Add(setupRange);
        }
    }

    private readonly List<SetupRange> setupRanges = new ();

    public RangeTy Find(decimal value)
    {
        srToFind.SetBinarySearchRange(value);
        int idx = setupRanges.BinarySearch(srToFind, this);

        if (idx >= 0)
        {
            return (RangeTy)setupRanges[idx];
        }

        return null;
    }

    /// <summary>
    /// Used for Binary Search
    /// </summary>
    private readonly SetupRange srToFind = new (0, 0);

    /// <summary>
    /// Do not use this function. It was written for this classes Find method.
    /// </summary>
    public int Compare(SetupRange r1, SetupRange r2)
    {
        // srToFind will either be r1 or r1
        if (ReferenceEquals(srToFind, r1))
        {
            decimal value = r1.Start;
            if (value < r2.Start)
            {
                return -1;
            }

            if (value > r2.End)
            {
                return 1;
            }

            return 0;
        }

        if (ReferenceEquals(srToFind, r2))
        {
            decimal value = r2.Start;
            if (r1.End < value)
            {
                return -1;
            }

            if (r1.Start > value)
            {
                return 1;
            }

            return 0;
        }

        throw new Exception("Finder error.");
    }

    public RangeTy this[int idx] => (RangeTy)setupRanges[idx];

    public int Count => setupRanges.Count;

    internal void Add(RangeTy aRange)
    {
        setupRanges.Add(aRange);
    }

    internal void DataChangesCompleted()
    {
        setupRanges.Sort(SortComparer);
    }

    public int SortComparer(SetupRange r1, SetupRange r2)
    {
        decimal value = r1.Start;

        if (value < r2.Start)
        {
            return -1;
        }

        if (value > r2.Start)
        {
            return 1;
        }

        return 0;
    }

    public override string ToString()
    {
        return string.Format("Contains {0} SetupRanges.", Count);
    }

    internal virtual void Validate()
    {
        PT.APSCommon.Range.PTRangeValidator<decimal> rv = new ();

        for (int i = 0; i < Count; ++i)
        {
            rv.Add(this[i].Start, this[i].End);
        }

        rv.Validate();
    }
}