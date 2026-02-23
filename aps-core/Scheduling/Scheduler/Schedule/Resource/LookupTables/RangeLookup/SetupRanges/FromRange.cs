namespace PT.Scheduler.RangeLookup;

public class FromRange : SetupRange
{
    #region IPTSerializable Members
    public FromRange(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            setupRangeSet = new SetupRangeSet<ToRange>(a_reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        setupRangeSet.Serialize(writer);
    }

    public new const int UNIQUE_ID = 612;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    public FromRange(decimal aStart, decimal aEnd)
        : base(aStart, aEnd)
    {
        setupRangeSet = new SetupRangeSet<ToRange>();
    }

    public FromRange(Transmissions.SetupRangeUpdate aValues)
        : base(aValues.start, aValues.end)
    {
        setupRangeSet = new SetupRangeSet<ToRange>();
        for (int toRangeI = 0; toRangeI < aValues.toRanges.Count; ++toRangeI)
        {
            Add(new ToRange(aValues.toRanges[toRangeI]));
        }

        setupRangeSet.DataChangesCompleted();
    }

    internal FromRange(FromRange origFromRange)
        : base(origFromRange)
    {
        setupRangeSet = new SetupRangeSet<ToRange>(origFromRange.setupRangeSet);
    }

    internal void Add(ToRange aToRange)
    {
        setupRangeSet.Add(aToRange);
    }

    internal void DataChangesCompleted()
    {
        setupRangeSet.DataChangesCompleted();
    }

    private readonly SetupRangeSet<ToRange> setupRangeSet;

    public override string ToString()
    {
        return string.Format("From: {0}; To: {1}; With {2} ToRanges.", Start, End, setupRangeSet.Count);
    }

    public ToRange Find(decimal value)
    {
        return setupRangeSet.Find(value);
    }

    public ToRange this[int idx] => setupRangeSet[idx];

    public int Count => setupRangeSet.Count;

    internal void Validate()
    {
        setupRangeSet.Validate();
    }
}