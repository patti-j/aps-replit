using PT.APSCommon;

namespace PT.Scheduler;

public struct FactorScore : IComparable, IPTSerializable
{
    internal FactorScore(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12433)
        {
            a_reader.Read(out FactorName);
            a_reader.Read(out Score);
            a_reader.Read(out PctOfTotal);
            m_resId = new BaseId(a_reader);
        }
        else
        {
            a_reader.Read(out FactorName);
            a_reader.Read(out Score);
            a_reader.Read(out PctOfTotal);
        }
    }

    internal FactorScore(FactorScore a_score, decimal a_percentOfTotal, BaseId a_resId)
    {
        FactorName = a_score.FactorName;
        Score = a_score.Score;
        PctOfTotal = a_percentOfTotal;
        m_resId = a_resId;
    }

    internal FactorScore(string a_factorName, decimal a_score, decimal a_percentOfTotal, BaseId a_resId)
    {
        FactorName = a_factorName;
        Score = a_score;
        PctOfTotal = a_percentOfTotal;
        m_resId = a_resId;
    }

    public readonly string FactorName;
    public readonly decimal Score;
    public decimal PctOfTotal;
    public BaseId ResId => m_resId;

    private readonly BaseId m_resId;

    public static int Compare(FactorScore a_a, FactorScore a_b)
    {
        //Sorting by percent, large to small.
        if (a_a.PctOfTotal < a_b.PctOfTotal)
        {
            return 1;
        }

        if (a_a.PctOfTotal == a_b.PctOfTotal)
        {
            return 0;
        }

        return -1;
    }

    int IComparable.CompareTo(object obj)
    {
        FactorScore fs = (FactorScore)obj;
        return Compare(this, fs);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(FactorName);
        a_writer.Write(Score);
        a_writer.Write(PctOfTotal);
        m_resId.Serialize(a_writer);
    }

    public int UniqueId => 921;
}