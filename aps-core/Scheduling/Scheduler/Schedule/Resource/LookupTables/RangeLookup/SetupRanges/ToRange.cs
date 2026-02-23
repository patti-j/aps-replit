namespace PT.Scheduler.RangeLookup;

public class ToRange : SetupRange
{
    #region IPTSerializable Members
    public ToRange(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out cost);
            reader.Read(out setupTicks);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(cost);
        writer.Write(setupTicks);
    }

    public new const int UNIQUE_ID = 611;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private void Init(decimal aCost, long aSetupTicks)
    {
        cost = aCost;
        setupTicks = aSetupTicks;
    }

    public ToRange(decimal aStart, decimal aEnd, decimal aCost, long aSetupTicks)
        : base(aStart, aEnd)
    {
        Init(aCost, aSetupTicks);
    }

    public ToRange(Transmissions.SetupToRange aValues)
        : base(aValues.from, aValues.to)
    {
        Init(aValues.cost, aValues.setupTime);
    }

    public ToRange(ToRange aOrig)
        : base(aOrig)
    {
        Init(aOrig.cost, aOrig.setupTicks);
    }

    private decimal cost;

    public decimal Cost => cost;

    private long setupTicks;

    public long SetupTicks => setupTicks;

    public override string ToString()
    {
        return string.Format("Start: {0}; End: {1}; Cost: {2}; Setup Minutes: {3}", Start, End, cost, new TimeSpan(setupTicks).TotalMinutes);
    }
}