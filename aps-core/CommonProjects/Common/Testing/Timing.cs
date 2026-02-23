namespace PT.Common.Testing;

public class Timing
{
    private bool startSet;
    private long startTime;

    public Timing() { }

    public Timing(bool start)
    {
        StartConstructorHandling(start);
    }

    public Timing(bool start, string name)
    {
        _name = name;
        StartConstructorHandling(start);
    }

    private void StartConstructorHandling(bool start)
    {
        if (start)
        {
            Start();
        }
    }

    public void Start()
    {
        startSet = true;
        stopSet = false;
        startTime = DateTime.Now.Ticks;
    }

    private bool stopSet;
    private long stopTime;

    private readonly string _name;

    public string Name => _name;

    public void Stop()
    {
        if (!stopSet)
        {
            stopTime = DateTime.Now.Ticks;
            stopSet = true;
            length = stopTime - startTime;
        }
    }

    private long length;

    public long Length
    {
        get
        {
            if (!startSet)
            {
                throw new Exception("Timer not started");
            }

            if (!stopSet)
            {
                return DateTime.Now.Ticks - startTime;
            }

            return length;
        }
    }

    public static bool operator <(Timing left, Timing right)
    {
        return left.Length < right.Length;
    }

    public static bool operator <=(Timing left, Timing right)
    {
        return left.Length <= right.Length;
    }

    public static bool operator >(Timing left, Timing right)
    {
        return left.Length > right.Length;
    }

    public static bool operator >=(Timing left, Timing right)
    {
        return left.Length >= right.Length;
    }

    public override string ToString()
    {
        TimeSpan lengthTS = new (Length);
        int hours = (int)lengthTS.TotalHours;
        string hoursString = hours.ToString();
        if (hoursString.Length == 1)
        {
            hoursString = "0" + hours;
        }

        return hoursString + lengthTS.ToString(@"\:mm\:ss\.fffffff");
    }

    #region 1/27/2011. Most of this stuff can be deleted if you no longer find you need to write simulation timing data to file. The simulation timing data is now stored in ScenarioDetail and can be accessed through the APS command prompt.
    //public const string DEFAULT_DIRECTORY = "C:\\_A_TIMING";

    ///// <summary>
    ///// Must be called prior to using the other static members of this class that write to file. This function makes sure directory C:\\_A_TIMING exists
    ///// </summary>
    //public static void InitTimingDirectory()
    //{
    //    if (!System.IO.Directory.Exists(DEFAULT_DIRECTORY))
    //    {
    //        System.IO.Directory.CreateDirectory(DEFAULT_DIRECTORY);
    //    }
    //}

    //internal const string SIMULATION_FILE = DEFAULT_DIRECTORY + "\\Simulation.txt";

    //internal static void AppendWriteResultsToDisk(string file, string timingMsg, string description, bool printSeparator)
    //{
    //    System.IO.StreamWriter sw = System.IO.File.AppendText(file);

    //    try
    //    {
    //        sw.WriteLine(description.PadLeft(30) + timingMsg.ToString().PadLeft(19) + (" at " + DateTime.Now).PadLeft(25));

    //        if (printSeparator)
    //        {
    //            sw.WriteLine("*******************************************************************************");
    //        }
    //    }
    //    finally
    //    {
    //        sw.Flush();
    //        sw.Close();
    //    }
    //}

    //internal static void WriteResultsToDisk(string file, string timingMsg, string description, bool printSeparator)
    //{
    //    DeleteTimingFile(file);
    //    AppendWriteResultsToDisk(file, timingMsg, description, printSeparator);
    //}

    //internal static void DeleteTimingFile(string file)
    //{
    //    if (System.IO.File.Exists(file))
    //    {
    //        System.IO.File.Delete(file);
    //    }
    //}

    //public void AppendSimulationResultsToDisk(string description, bool printSeparator)
    //{
    //    Stop();
    //    AppendWriteResultsToDisk(SIMULATION_FILE, ToString(), description, printSeparator);
    //}

    //public void WriteSimulationResultsToDisk(string description, bool printSeparator)
    //{
    //    Stop();
    //    WriteResultsToDisk(SIMULATION_FILE, ToString(), description, printSeparator);
    //}

    //public static void DeleteSimulationFile()
    //{
    //    DeleteTimingFile(SIMULATION_FILE);
    //}
    #endregion
}