namespace PT.Transmissions;

public class SetupRangeUpdate
{
    public SetupRangeUpdate(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out start);
            reader.Read(out end);

            int count;
            reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                toRanges.Add(new SetupToRange(reader));
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(start);
        writer.Write(end);

        writer.Write(toRanges.Count);
        for (int i = 0; i < toRanges.Count; ++i)
        {
            toRanges[i].Serialize(writer);
        }
    }

    public SetupRangeUpdate(decimal aStart, decimal aEnd)
    {
        start = aStart;
        end = aEnd;
    }

    public decimal start;
    public decimal end;

    public List<SetupToRange> toRanges = new ();

    private void AddToRange(decimal from, decimal to, decimal cost, long setupTime)
    {
        toRanges.Add(new SetupToRange(from, to, cost, setupTime));
    }
}