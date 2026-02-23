using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Updates the values of the BalancedCompositeDispatcherDefinition.
/// </summary>
public class BalancedCompositeDispatcherDefinitionUpdateT : BalancedCompositeDispatcherDefinitionIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 511;

    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinitionUpdateT(IReader a_reader)
        : base(a_reader)
    {
        m_updatedMappings = new List<(string, OptimizeRuleElementSettings)>();
        m_categoryScalingMultipliers = new Dictionary<string, decimal>();

        if (a_reader.VersionNumber >= 12433)
        {
            a_reader.Read(out m_name);
            a_reader.Read(out m_notes);
            a_reader.Read(out m_globalMinScore);

            //Sequence Factor Element Settings
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string elementKey);
                OptimizeRuleElementSettings settings = new(a_reader);
                AddUpdatedMapping(elementKey, settings);
            }

            //Category Multiplier Values
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string categoryKey);
                a_reader.Read(out decimal multiplier);
                m_categoryScalingMultipliers.Add(categoryKey, multiplier);
            }
        }
        else if (a_reader.VersionNumber >= 12021)
        {
            a_reader.Read(out m_name);
            a_reader.Read(out m_notes);

            //Sequence Factor Element Settings
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string elementKey);
                OptimizeRuleElementSettings settings = new (a_reader);
                AddUpdatedMapping(elementKey, settings);
            }

            //Category Multiplier Values
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string categoryKey);
                a_reader.Read(out decimal multiplier);
                m_categoryScalingMultipliers.Add(categoryKey, multiplier);
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out m_name);
            a_reader.Read(out m_notes);

            //Sequence Factor Element Settings
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string elementKey);
                OptimizeRuleElementSettings settings = new (a_reader);
                AddUpdatedMapping(elementKey, settings);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_name);
        a_writer.Write(m_notes);
        a_writer.Write(m_globalMinScore);
        a_writer.Write(m_updatedMappings.Count);
        foreach ((string, OptimizeRuleElementSettings) mapping in m_updatedMappings)
        {
            a_writer.Write(mapping.Item1);
            mapping.Item2.Serialize(a_writer);
        }

        a_writer.Write(m_categoryScalingMultipliers.Count);
        foreach (KeyValuePair<string, decimal> pair in m_categoryScalingMultipliers)
        {
            a_writer.Write(pair.Key);
            a_writer.Write(pair.Value);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BalancedCompositeDispatcherDefinitionUpdateT() { }

    public BalancedCompositeDispatcherDefinitionUpdateT(BaseId a_scenarioId, BaseId a_balancedCompositeDispatcherDefinitionId)
        : base(a_scenarioId, a_balancedCompositeDispatcherDefinitionId)
    {
        m_updatedMappings = new List<(string, OptimizeRuleElementSettings)>();
        m_categoryScalingMultipliers = new Dictionary<string, decimal>();
    }

    public override string Description => string.Format("Optimize rule '{0}' updated".Localize(), m_name);

    private string m_name;
    private readonly string m_notes;
    private decimal m_globalMinScore = decimal.MinValue;
    private readonly List<(string, OptimizeRuleElementSettings)> m_updatedMappings;
    private readonly Dictionary<string, decimal> m_categoryScalingMultipliers;

    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    public string Notes => m_notes;

    public decimal GlobalMinScore
    {
        get => m_globalMinScore;
        set { m_globalMinScore = value; }
    }

    public List<(string, OptimizeRuleElementSettings)> UpdatedMappings => m_updatedMappings;
    public Dictionary<string, decimal> CategoryScalingMultipliers => m_categoryScalingMultipliers;

    public void AddUpdatedMapping(string a_elementKey, OptimizeRuleElementSettings a_elementSettings)
    {
        m_updatedMappings.Add((a_elementKey, a_elementSettings));
    }

    public void AddUpdatedCategoryMultiplier(string a_categoryKey, decimal a_value)
    {
        m_categoryScalingMultipliers[a_categoryKey] = a_value;
    }

    public class OptimizeRuleElementSettings : IPTSerializable
    {
        private BoolVector32 m_bools;
        private decimal m_points;

        public decimal Points
        {
            get => m_points;
            set
            {
                m_bools[c_pointsIsSetIdx] = true;
                m_points = value;
            }
        }

        private decimal m_resourceMultiplier;

        public decimal ResourceMultiplier
        {
            get => m_resourceMultiplier;
            set
            {
                m_bools[c_resMultiplierIsSetIdx] = true;
                m_resourceMultiplier = value;
            }
        }

        private decimal m_minimumScore;

        public decimal MinimumScore
        {
            get { return m_minimumScore; }
            set
            {
                m_bools[c_minimumScoreIsSetIdx] = true;
                m_minimumScore = value;
            }
        }

        public bool UseMinimumScore
        {
            get { return m_bools[c_useMinimumScoreIdx]; }
            set
            {
                m_bools[c_useMinimumScoreIsSetIdx] = true;
                m_bools[c_useMinimumScoreIdx] = value;

            }
        }

        private SettingData m_settings;

        public SettingData Settings
        {
            get => m_settings;
            set
            {
                m_bools[c_settingDataIsSetIdx] = true;
                m_settings = value;
            }
        }

        private decimal m_maxPenalty;
        public decimal MaxPenalty
        {
            get => m_maxPenalty;
            set
            {
                m_maxPenalty = value;
                m_bools[c_maxPenaltyIsSetIdx] = true;
            }
        }

        private decimal m_minPenalty;
        public decimal MinPenalty
        {
            get => m_minPenalty;
            set
            {
                m_minPenalty = value;
                m_bools[c_minPenaltyIsSetIdx] = true;
            }
        }

        private PackageEnums.ESequencingFactorEarlyWindowPenaltyScale m_earlyWindowPenaltyScale;
        public PackageEnums.ESequencingFactorEarlyWindowPenaltyScale EarlyWindowPenaltyScale
        {
            get => m_earlyWindowPenaltyScale;
            set
            {
                m_earlyWindowPenaltyScale = value;
                m_bools[c_earlyWindowPenaltyScaleIsSetIdx] = true;
            }
        }

        public OptimizeRuleElementSettings(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 13004)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
                a_reader.Read(out bool hasSettingData);
                if (hasSettingData)
                {
                    m_settings = new SettingData(a_reader);
                }
                a_reader.Read(out m_minimumScore);
                a_reader.Read(out m_maxPenalty);
                a_reader.Read(out m_minPenalty);
                a_reader.Read(out short earlyWindowScaleValue);
                m_earlyWindowPenaltyScale = (PackageEnums.ESequencingFactorEarlyWindowPenaltyScale)earlyWindowScaleValue;
            }
            else if (a_reader.VersionNumber >= 12432)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
                a_reader.Read(out bool hasSettingData);
                if (hasSettingData)
                {
                    m_settings = new SettingData(a_reader);
                }
                a_reader.Read(out m_minimumScore);
            }
            else if (a_reader.VersionNumber >= 12070)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
                a_reader.Read(out bool hasSettingData);
                if (hasSettingData)
                {
                    m_settings = new SettingData(a_reader);
                }
            }
            // Should I bother with handling the 12100-12102 VersionNumbers? 
            else if (a_reader.VersionNumber >= 12055)
            {
                /*
                 * This block is here for backwards compatibility.
                 * History/Details:
                 * 12.0 and 12.1 Serialization split originally at VersionNumber == 12053,
                 * but then I merged the two serializations and incremented the VersionNumber
                 * to 12054.
                 * Instead of continuing to increment 12.0 and 12.1 by one each
                 * time and dealing with a mess due to the version number conflict.
                 * We decided to increment 12.1 to 12100, and 12.0 was incremented to 12070.
                 * 12.1 was on VersionNumber == 12059 (or 12060) and 12.0 was on 12054 when
                 * their respective VersionNumbers were incremented to the aforementioned values.
                 * This block is here to make sure 12055 through 12059 (or 12060) scenarios
                 * are handled correctly.
                 */
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
            }
            else if (a_reader.VersionNumber >= 12054)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
                a_reader.Read(out bool hasSettingData);
                if (hasSettingData)
                {
                    m_settings = new SettingData(a_reader);
                }
            }
            else
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
            }
        }

        public OptimizeRuleElementSettings()
        {
            m_settings = null;
        }

        public void Serialize(IWriter a_writer)
        {
            m_bools.Serialize(a_writer);
            a_writer.Write(m_points);
            a_writer.Write(m_resourceMultiplier);
            a_writer.Write(m_settings != null);
            m_settings?.Serialize(a_writer);
            a_writer.Write(m_minimumScore);
            a_writer.Write(m_maxPenalty);
            a_writer.Write(m_minPenalty);
            a_writer.Write((short)m_earlyWindowPenaltyScale);
        }

        private const short c_pointsIsSetIdx = 0;
        private const short c_resMultiplierIsSetIdx = 1;
        private const short c_settingDataIsSetIdx = 2;
        private const short c_hasSettingDataIdx = 3;
        private const short c_preventScalingIdx = 4;
        private const short c_preventScalingIsSetIdx = 5;
        private const short c_minimumScoreIsSetIdx = 6;
        private const short c_useMinimumScoreIdx = 7;
        private const short c_useMinimumScoreIsSetIdx = 8;
        private const short c_maxPenaltyIsSetIdx = 9;
        private const short c_minPenaltyIsSetIdx = 10;
        private const short c_earlyWindowPenaltyScaleIsSetIdx = 11;

        public bool PointsIsSet => m_bools[c_pointsIsSetIdx];
        public bool ResourceMultiplierIsSet => m_bools[c_resMultiplierIsSetIdx];
        public bool SettingDataIsSet => m_bools[c_settingDataIsSetIdx];
        public bool PreventScalingIsSet => m_bools[c_preventScalingIsSetIdx];
        public bool MinimumScoreIsSet => m_bools[c_minimumScoreIsSetIdx];
        public bool UseMinimumScoreIsSet => m_bools[c_useMinimumScoreIsSetIdx];
        public bool MaxPenaltyIsSet => m_bools[c_maxPenaltyIsSetIdx];
        public bool MinPenaltyIsSet => m_bools[c_minPenaltyIsSetIdx];
        public bool EarlyWindowPenaltyScaleIsSet => m_bools[c_earlyWindowPenaltyScaleIsSetIdx];
        public bool HasSettingData => m_bools[c_hasSettingDataIdx];

        public bool PreventScaling
        {
            get => m_bools[c_preventScalingIdx];
            set
            {
                m_bools[c_preventScalingIdx] = value;
                m_bools[c_preventScalingIsSetIdx] = true;
            }
        }

        public int UniqueId => 1017;
    }

    //For backwards compatibility only. Used to read old dispatcher definitions
    public class AttributeRuleInfo : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 652;

        public AttributeRuleInfo(IReader reader)
        {
            if (reader.VersionNumber >= 308)
            {
                reader.Read(out AttributeName);
                reader.Read(out Weight);

                reader.Read(out int tempVal);
                OptimizeType = (PTAttributeDefs.OptimizeType)tempVal;

                reader.Read(out m_minAttributeValue);
                reader.Read(out m_maxAttributeValue);
            }
        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(AttributeName);
            a_writer.Write(Weight);
            a_writer.Write((int)OptimizeType);

            a_writer.Write(m_minAttributeValue);
            a_writer.Write(m_maxAttributeValue);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public AttributeRuleInfo(string attributeName, decimal weight, PTAttributeDefs.OptimizeType optimizeType, decimal aMinValue, decimal aMaxValue)
        {
            AttributeName = attributeName;
            Weight = weight;
            OptimizeType = optimizeType;
            m_minAttributeValue = aMinValue;
            m_maxAttributeValue = aMaxValue;
        }

        public string AttributeName;
        public decimal Weight;
        public PTAttributeDefs.OptimizeType OptimizeType;

        // Values used to map variables to comparable ranges.
        public decimal m_minAttributeValue; //=11;
        public decimal m_maxAttributeValue; //=344;
    }
}