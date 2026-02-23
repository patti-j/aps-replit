using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Summary description for ScenarioDetailCompressT.
/// </summary>
public class ScenarioDetailCompressT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 124;

    #region IPTSerializable Members
    public ScenarioDetailCompressT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12204)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_plantId = new BaseId(a_reader);

            if (!UseScenarioCompressSettings)
            {
                m_compressSettings = new OptimizeSettings(a_reader);
                if (a_reader.VersionNumber < 12204)
                {
                    //StartTime for compress used to be the end time
                    m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                }
            }

            m_limitToResOverrideList = new BaseIdList(a_reader);
        }
        else if (a_reader.VersionNumber >= 12031)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_plantId = new BaseId(a_reader);

            if (!UseScenarioCompressSettings)
            {
                m_compressSettings = new OptimizeSettings(a_reader);
                if (a_reader.VersionNumber < 12204)
                {
                    //StartTime for compress used to be the end time
                    m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                }
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_plantId = new BaseId(a_reader);
        }
        else if (a_reader.VersionNumber >= 748)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_plantId = new BaseId(a_reader);

            if (!UseScenarioCompressSettings)
            {
                m_compressSettings = new OptimizeSettings(a_reader);
                //StartTime for compress used to be the end time
                m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
            }
        }
        else if (a_reader.VersionNumber >= 618)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_specificStartTime);
            m_plantId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write(m_specificStartTime);
        m_plantId.Serialize(a_writer);
        m_compressSettings?.Serialize(a_writer);
        m_limitToResOverrideList.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailCompressT() { }

    public ScenarioDetailCompressT(BaseId a_scenarioId, OptimizeSettings a_compressSettings)
        : base(a_scenarioId)
    {
        m_bools = new BoolVector32();
        m_bools[c_useScenarioCompressSettingsIdx] = a_compressSettings == null;
        m_compressSettings = a_compressSettings;
    }

    private BoolVector32 m_bools;

    private const short c_useScenarioCompressSettingsIdx = 0;
    private const short c_useScenarioCompressSettingsSetIdx = 1;
    private const short c_specificStartTimeSetIdx = 2;
    private const short c_plantIdSet = 3;
    private const short c_limitToResourceOverrideIsSetIdx = 4;

    /// <summary>
    /// If true then the ScenarioDetail's OptimizeSettings should be used and updated.  Otherwise, the User's OptimizeSettings should be used and updated.
    /// </summary>
    public bool UseScenarioCompressSettings => m_bools[c_useScenarioCompressSettingsIdx];

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

    private BaseId m_plantId;

    public BaseId PlantId
    {
        get => m_plantId;
        set
        {
            m_plantId = value;
            m_bools[c_plantIdSet] = true;
        }
    }

    public bool PlantIdSet => m_bools[c_plantIdSet];

    private readonly OptimizeSettings m_compressSettings;

    public OptimizeSettings CompressSettings => m_compressSettings;

    public override string Description => UseScenarioCompressSettings ? "Compressed (Scenario Options)" : "Compressed (User Options)";

    private BaseIdList m_limitToResOverrideList = new ();

    public BaseIdList LimitToResourcesOverride
    {
        get => m_limitToResOverrideList;
        set
        {
            m_limitToResOverrideList = value;
            m_bools[c_limitToResourceOverrideIsSetIdx] = true;
        }
    }

    public bool LimitToResourcesOverrideIsSet => m_bools[c_limitToResourceOverrideIsSetIdx];
}