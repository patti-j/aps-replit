namespace PT.Common.Testing;

public class TimingSet
{
    private readonly List<Timing> times;
    private Timing current;
    private bool skip;

    public TimingSet(bool aSkipFirst)
    {
        skip = aSkipFirst;
        times = new List<Timing>();
    }

    public int Count => times.Count;

    public void Start()
    {
        current = new Timing();
        current.Start();
    }

    private Timing min;
    private Timing max;

    private long total;
    private long average;
    private long minMaxDiff;

    public void Stop()
    {
        current.Stop();

        if (skip)
        {
            skip = false;
        }
        else
        {
            if (min == null)
            {
                min = current;
                max = current;
            }

            if (current < min)
            {
                min = current;
            }

            if (current > max)
            {
                max = current;
            }

            times.Add(current);
            total += current.Length;
            average = total / times.Count;
            minMaxDiff = max.Length - min.Length;
        }

        current = null;
    }

    public Timing Min => min;

    public Timing Max => max;

    public TimeSpan Average => new (average);

    public TimeSpan MinMaxDiff => new (minMaxDiff);

    public TimeSpan Total => new (total);

    public override string ToString()
    {
        return string.Format("Min={0}; Max={1}; Avg={2}; MinMaxDiff={3}; Count={4}; TotalTime={5}", Min, Max, Average, MinMaxDiff, Count, Total);
    }

    #region 1/27/2011. This stuff can be deleted if you no longer find you need to write the timing data out to file. The simulation timing data is now stored in ScenarioDetail and can be accessed through the APS command prompt.
    //public void AppendSimulationResultsToDisk(string description, bool printSeparator)
    //{
    //    Timing.AppendWriteResultsToDisk(Timing.SIMULATION_FILE, ToString(), description, printSeparator);
    //}

    //public void WriteSimulationResultsToDisk(string description, bool printSeparator)
    //{
    //    Timing.WriteResultsToDisk(Timing.SIMULATION_FILE, ToString(), description, printSeparator);
    //}

    //public static void DeleteSimulationFile()
    //{
    //    Timing.DeleteTimingFile(Timing.SIMULATION_FILE);
    //}
    #endregion
}