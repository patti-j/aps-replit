namespace PT.SchedulerDefinitions;

/// <summary>
/// This class holds the calculated values used to determine the maximum scenarios that can be running at once.
/// Predetermined values can be set using a 0 - 10 scale, or specified values can be set
/// </summary>
[Obsolete("This class has been deprecated and only exists for compatibility. The new class, CoPilotPerformanceValuesServer, is defined in the ServerManagerSharedLib.")]
public class CoPilotPerformanceValuesDeprecated
{
    private int m_settingValue;
    private int m_maxSimulations;
    private int m_cpuLimitPercentageTotal;
    private int m_cpuLimitPercentageGalaxy;
    private double m_cpuProcessorsScalar;
    private double m_memoryScalar;
    private int m_updateInterval;
    private int m_dispatchInterval;

    public const int UNIQUE_ID = 773;

    #region IPTSerializable Members
    internal CoPilotPerformanceValuesDeprecated(IReader reader)
    {
        #region 453
        if (reader.VersionNumber >= 453)
        {
            reader.Read(out m_settingValue);
            if (m_settingValue == -1)
            {
                //Custom values are being used.
                reader.Read(out m_maxSimulations);
                reader.Read(out m_cpuLimitPercentageGalaxy);
                reader.Read(out m_cpuLimitPercentageTotal);
                reader.Read(out m_memoryScalar);
                reader.Read(out m_cpuProcessorsScalar);
                reader.Read(out m_updateInterval);
                reader.Read(out m_dispatchInterval);
            }
            else
            {
                //Recreate values
                InitSettings(m_settingValue);
            }
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_settingValue);
        if (m_settingValue == -1)
        {
            //Custom values are being used.
            writer.Write(m_maxSimulations);
            writer.Write(m_cpuLimitPercentageGalaxy);
            writer.Write(m_cpuLimitPercentageTotal);
            writer.Write(m_memoryScalar);
            writer.Write(m_cpuProcessorsScalar);
            writer.Write(m_updateInterval);
            writer.Write(m_dispatchInterval);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public CoPilotPerformanceValuesDeprecated()
    {
        InitSettings(7);
    }

    #region MEMBERS
    /// <summary>
    /// 0 - 10 for predetermined settings. 0 is one simulation, 10 is maximum practical simulations.
    /// -1 for custom settings that can be set to specific values.
    /// </summary>
    public int Value => m_settingValue;

    /// <summary>
    /// CPU % at which no more simulations will be created
    /// </summary>
    public int CpuLimitPercentTotal => m_cpuLimitPercentageTotal;

    /// <summary>
    /// CPU % used by Galaxy System Process at which no more simulations will be created
    /// </summary>
    public int CpuLimitPercentTotalGalaxy => m_cpuLimitPercentageGalaxy;

    /// <summary>
    /// Maximum simulations that will be running at once regardless of any other settings.
    /// </summary>
    public int MaxSimulations => m_maxSimulations;

    //Represents the number to multiply scenario size in order to determine minimum memory requirement.
    public double MemoryScalar => m_memoryScalar;

    //Represents timer interval (in ms) used to calculate the status check in the simulation manager.
    //The Simulation Manager will evaluate current system CPU and memory usages. 
    public int UpdateInterval => m_updateInterval;

    //The interval (in ms) in between broadcasting new RuleSeek results.
    public int DispatchInterval => m_dispatchInterval;
    #endregion

    /// <summary>
    /// Set default values based on a 0 - 10 scale.
    /// Currently, the way the switch is set up, it is possible for a lower case to use more cpu power than a higher case.
    /// This depends on the number of CPU cores on the hardware.
    /// </summary>
    /// <param name="a_value">Predefined Settings Value</param>
    private void InitSettings(int a_value)
    {
        m_settingValue = a_value;
        m_cpuProcessorsScalar = .125 * a_value; //8 is all cores
        m_memoryScalar = 20 / Math.Max(1, a_value);

        switch (a_value)
        {
            case 0:
                m_maxSimulations = 1;
                m_cpuLimitPercentageTotal = 100;
                m_cpuLimitPercentageGalaxy = 100;
                m_updateInterval = 10000;
                m_dispatchInterval = 20000;
                break;
            case 1:
                m_maxSimulations = 2;
                m_cpuLimitPercentageTotal = 100;
                m_cpuLimitPercentageGalaxy = 100;
                m_updateInterval = 10000;
                m_dispatchInterval = 20000;
                break;
            case 2:
                m_maxSimulations = 4;
                m_cpuLimitPercentageTotal = 100;
                m_cpuLimitPercentageGalaxy = 45;
                m_updateInterval = 20000;
                m_dispatchInterval = 20000;
                break;
            case 3:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 100;
                m_cpuLimitPercentageGalaxy = 50;
                m_updateInterval = 12000;
                m_dispatchInterval = 20000;
                break;
            case 4:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 100;
                m_cpuLimitPercentageGalaxy = 50;
                m_updateInterval = 12000;
                m_dispatchInterval = 15000;
                break;
            case 5:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 100;
                m_cpuLimitPercentageGalaxy = 50;
                m_updateInterval = 10000;
                m_dispatchInterval = 15000;
                break;
            case 6:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 70;
                m_cpuLimitPercentageGalaxy = 60;
                m_updateInterval = 10000;
                m_dispatchInterval = 12000;
                break;
            case 7:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 75;
                m_cpuLimitPercentageGalaxy = 70;
                m_updateInterval = 7000;
                m_dispatchInterval = 10000;
                break;
            case 8:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 80;
                m_cpuLimitPercentageGalaxy = 80;
                m_updateInterval = 5000;
                m_dispatchInterval = 8000;
                break;
            case 9:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 90;
                m_cpuLimitPercentageGalaxy = 85;
                m_updateInterval = 3000;
                m_dispatchInterval = 8000;
                break;
            case 10:
                m_maxSimulations = (int)(m_cpuProcessorsScalar * Environment.ProcessorCount);
                m_cpuLimitPercentageTotal = 90;
                m_cpuLimitPercentageGalaxy = 90;
                m_updateInterval = 1000;
                m_dispatchInterval = 5000;
                break;
        }
    }
}