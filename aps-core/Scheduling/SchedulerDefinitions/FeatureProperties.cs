namespace PT.SchedulerDefinitions;

public enum EFeatures
{
    None = 0,
    AllowedHelpers = 1,
    ResourceConnectors = 2,
    Cells = 3,
    DBR = 4,
    ActivityBatching = 5,
    ResourceMultipliers = 6,
    ResourceCapacity = 7,
    Tanks = 8,
    Forecasts = 9,
    Financials = 10,
    UDF = 11,
    Attributes = 12,
    QuantityConstraints = 13,
    ResourceRules = 14,
    SetupAdvanced = 15,
    AttributesAdvanced = 16,
    Setup = 17, //Basic
    CompatibilityGroup = 18,
    ResourceStaging = 19,
    Splitting = 20,
    Lots = 21,
    MaxDelay = 22,
    MRP = 23,
    MrpBatching = 24,
    MrpAdvanced = 25,
    ScheduleAdherance = 26,
    MES = 27,
    Priority = 28,
    Routings = 29,
    SuccessorMOs = 30,
    MultiPlant = 31,
    Overlap = 32,
    Scrap = 33,
    Customizations = 34,
    ProductRules = 35,
    MOBatching = 36,
    Hold = 37, //Basic
    Capabilities = 38, //Basic
    AlternatePaths = 39
}

public class FeatureProperties : IPTSerializable
{
    private const int UNIQUE_ID = 902;

    public FeatureProperties(IReader a_reader)
    {
        int intValue;
        a_reader.Read(out intValue);
        if (Enum.IsDefined(typeof(EFeatures), (EFeatures)intValue))
        {
            m_featureKey = (EFeatures)intValue;
            m_valid = true;
        }

        m_bools = new BoolVector32(a_reader);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((int)m_featureKey);
        m_bools.Serialize(a_writer);
    }

    public int UniqueId => UNIQUE_ID;

    private BoolVector32 m_bools;
    private readonly EFeatures m_featureKey;
    private readonly bool m_valid;

    public bool IsValid => m_valid;

    public FeatureProperties(EFeatures a_features)
    {
        m_featureKey = a_features;
        m_valid = true;
    }

    private const short c_enabledIdx = 0;

    public bool Enabled
    {
        set => m_bools[c_enabledIdx] = value;
        get => m_bools[c_enabledIdx];
    }

    public EFeatures Key => m_featureKey;
}