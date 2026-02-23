namespace PT.Transmissions;

public class SetupToRange
{
    public SetupToRange(decimal aFrom, decimal aTo, decimal aCost, long aSetupTime)
    {
        from = aFrom;
        to = aTo;
        cost = aCost;
        setupTime = aSetupTime;
    }

    public SetupToRange(IReader reader)
    {
        reader.Read(out from);
        reader.Read(out to);
        reader.Read(out cost);
        reader.Read(out setupTime);
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(from);
        writer.Write(to);
        writer.Write(cost);
        writer.Write(setupTime);
    }

    public decimal from, to;
    public decimal cost;
    public long setupTime;
}