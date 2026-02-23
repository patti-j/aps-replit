namespace PT.Scheduler.RangeLookup;

public class SetupRange : IPTSerializable
{
    #region IPTSerializable Members
    public SetupRange(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out start);
            reader.Read(out end);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        writer.Write(start);
        writer.Write(end);
    }

    public const int UNIQUE_ID = -1;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    private void Init(decimal aStart, decimal aEnd)
    {
        start = aStart;
        end = aEnd;
    }

    public SetupRange(decimal aStart, decimal aEnd)
    {
        Init(aStart, aEnd);
    }

    internal SetupRange(SetupRange aOrig)
    {
        Init(aOrig.start, aOrig.end);
    }

    /// <summary>
    /// This was added for BinarySearch in SetupRangeSet. In this case we aren't hunting for a range
    /// we are seeking a range that contains a value.
    /// </summary>
    /// <param name="aValue"></param>
    internal void SetBinarySearchRange(decimal aValue)
    {
        Init(aValue, aValue);
    }

    private decimal start;

    public decimal Start => start;

    private decimal end;

    public decimal End => end;
}