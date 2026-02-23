using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Initiates an Optimization using the specified OptimizeSettings.
/// </summary>
public class ScenarioDetailOptimizeT : ScenarioIdBaseT, IPTSerializable, IRequiresAuthorization
{
    public const int UNIQUE_ID = 131;

    #region IPTSerializable Members
    public ScenarioDetailOptimizeT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12543)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_specificPlantId = new BaseId(a_reader);

            if (!UseScenarioOptimizeSettings)
            {
                m_optimizeSettings = new OptimizeSettings(a_reader);
            }
            
            a_reader.Read(out bool optimizeSelectedJobs);
            if (optimizeSelectedJobs)
            {
                m_selectedJobBaseIdList = new BaseIdList(a_reader);
            }
        } 
        else if (a_reader.VersionNumber >= 12031)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_specificPlantId = new BaseId(a_reader);

            if (!UseScenarioOptimizeSettings)
            {
                m_optimizeSettings = new OptimizeSettings(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_specificPlantId = new BaseId(a_reader);
        }
        else if (a_reader.VersionNumber >= 748)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_specificPlantId = new BaseId(a_reader);

            if (!UseScenarioOptimizeSettings)
            {
                m_optimizeSettings = new OptimizeSettings(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 618)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_specificPlantId = new BaseId(a_reader);
        }
        else
        {
            m_bools = new BoolVector32();
            if (a_reader.VersionNumber >= 77)
            {
                new OptimizeSettings(a_reader);
                bool useScenarioSettings;
                a_reader.Read(out useScenarioSettings);
                m_bools[c_useScenarioOptimizeSettingsIdx] = useScenarioSettings;
            }

            #region Version 1
            else if (a_reader.VersionNumber >= 1)
            {
                new OptimizeSettings(a_reader);
            }
        }
        #endregion
    }

    private BoolVector32 m_bools;

    private const short c_useScenarioOptimizeSettingsIdx = 0;
    private const short c_specificStartTimeSetIdx = 2;
    private const short c_specificPlantSetIdx = 3;
    private const short c_runMrpIdx = 4;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write(SpecificStartTime);
        SpecificPlantId.Serialize(a_writer);

        m_optimizeSettings?.Serialize(a_writer);

        bool optimizeSelectedJobs = m_selectedJobBaseIdList != null;
        a_writer.Write(optimizeSelectedJobs);

        if (optimizeSelectedJobs)
        {
            m_selectedJobBaseIdList.Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailOptimizeT() { }

    public ScenarioDetailOptimizeT(BaseId scenarioId, OptimizeSettings a_optimizeSettings, bool a_runMRP, BaseIdList a_selectedJobBaseIdList = null)
        : base(scenarioId)
    {
        m_bools = new BoolVector32();
        m_bools[c_useScenarioOptimizeSettingsIdx] = a_optimizeSettings == null;
        m_bools[c_runMrpIdx] = a_runMRP;
        m_optimizeSettings = a_optimizeSettings;
        m_selectedJobBaseIdList = a_selectedJobBaseIdList;
    }

    private DateTime m_specificStartTime;

    public DateTime SpecificStartTime
    {
        get => m_specificStartTime;
        set
        {
            m_specificStartTime = value;
            m_bools[c_specificStartTimeSetIdx] = true;
        }
    }

    public bool SpecificStartTimeSet => m_bools[c_specificStartTimeSetIdx];

    /// <summary>
    /// If true then the ScenarioDetail's OptimizeSettings should be used and updated.  Otherwise, the User's OptimizeSettings should be used and updated.
    /// </summary>
    public bool UseScenarioOptimizeSettings => m_bools[c_useScenarioOptimizeSettingsIdx];

    public bool RunMRP => m_bools[c_runMrpIdx];

    private readonly OptimizeSettings m_optimizeSettings;
    private readonly BaseIdList m_selectedJobBaseIdList;
    public BaseIdList SelectedJobIdList => m_selectedJobBaseIdList;
    public OptimizeSettings OptimizeSettings => m_optimizeSettings;

    private BaseId m_specificPlantId = BaseId.NULL_ID;

    public BaseId SpecificPlantId
    {
        get => m_specificPlantId;
        set
        {
            m_specificPlantId = value;
            m_bools[c_specificPlantSetIdx] = true;
        }
    }

    public bool SpecificPlantIdSet => m_bools[c_specificPlantSetIdx];

    public override string Description
    {
        get
        {
            if (RunMRP)
            {
                return UseScenarioOptimizeSettings ? "MPS/MRP (Scenario Options)".Localize() : "MPS/MRP (User Options)".Localize();
            }

            if (SpecificPlantIdSet)
            {
                return UseScenarioOptimizeSettings ? "Plant Optimized (Scenario Options)".Localize() : "Plant Optimized (User Options)".Localize();
            }

            return UseScenarioOptimizeSettings ? "Optimize (Scenario Options)".Localize() : "Optimize (User Options)".Localize();
        }
    }

    public IEnumerable<string> Permissions => new[] { UserPermissionKeys.Scheduling };
}