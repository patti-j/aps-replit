using PT.PackageDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Simulation.Dispatcher.Dispatchers.BalancedCompositeDispatcher;

public class OptimizeRuleWeightSettings : IPTDeserializable
{
    private readonly Dictionary<string, OptimizeRuleElementSettings> m_rulePoints;
    private readonly Dictionary<string, decimal> m_categoryMultiplier;

    public OptimizeRuleWeightSettings()
    {
        m_rulePoints = new Dictionary<string, OptimizeRuleElementSettings>();
        m_categoryMultiplier = new Dictionary<string, decimal>();
    }

    #region IPTSerializable
    public OptimizeRuleWeightSettings(IReader a_reader)
    {
        m_rulePoints = new Dictionary<string, OptimizeRuleElementSettings>();
        m_categoryMultiplier = new Dictionary<string, decimal>();
        if (a_reader.VersionNumber >= 12021)
        {
            //Element settings
            a_reader.Read(out int ruleCount);
            while (ruleCount > 0)
            {
                a_reader.Read(out string key);
                OptimizeRuleElementSettings settings = new (a_reader);

                m_rulePoints.Add(key, settings);

                ruleCount--;
            }

            //Category settings
            a_reader.Read(out ruleCount);
            while (ruleCount > 0)
            {
                a_reader.Read(out string key);
                a_reader.Read(out decimal scaling);

                m_categoryMultiplier.Add(key, scaling);

                ruleCount--;
            }
        }
        else
        {
            a_reader.Read(out int ruleCount);
            while (ruleCount > 0)
            {
                a_reader.Read(out string key);
                OptimizeRuleElementSettings settings = new (a_reader);

                m_rulePoints.Add(key, settings);

                ruleCount--;
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        //Element settings
        a_writer.Write(m_rulePoints.Count);
        foreach (KeyValuePair<string, OptimizeRuleElementSettings> keyValuePair in m_rulePoints)
        {
            a_writer.Write(keyValuePair.Key);
            keyValuePair.Value.Serialize(a_writer);
        }

        //Category scaling
        a_writer.Write(m_categoryMultiplier.Count);
        foreach (KeyValuePair<string, decimal> keyValuePair in m_categoryMultiplier)
        {
            a_writer.Write(keyValuePair.Key);
            a_writer.Write(keyValuePair.Value);
        }
    }

    public int UniqueId => 1012;
    #endregion

    public List<string> GetAllRules()
    {
        return m_rulePoints.Keys.ToList();
    }

    internal List<string> GetActiveRules()
    {
        List<string> activeRuleKey = new ();

        foreach (KeyValuePair<string, OptimizeRuleElementSettings> keyValuePair in m_rulePoints)
        {
            if (keyValuePair.Value.Points != 0m)
            {
                activeRuleKey.Add(keyValuePair.Key);
            }
        }

        return activeRuleKey;
    }

    public decimal GetRulePoints(string a_ruleKey)
    {
        if (m_rulePoints.TryGetValue(a_ruleKey, out OptimizeRuleElementSettings points))
        {
            return points.Points;
        }

        return 0;
    }

    public decimal GetRuleResourceMultiplier(string a_ruleKey)
    {
        if (m_rulePoints.TryGetValue(a_ruleKey, out OptimizeRuleElementSettings weights))
        {
            return weights.ResourceMultiplier;
        }

        return 0;
    }

    public (decimal, bool) GetMinimumScore(string a_optimizeFactorKey)
    {
        if (m_rulePoints.TryGetValue(a_optimizeFactorKey, out OptimizeRuleElementSettings weights))
        {
            return (weights.MinimumScore, weights.UseMinimumScore);
        }

        return (0, false);
    }


    public SettingData GetRuleSettings(string a_ruleKey)
    {
        if (m_rulePoints.TryGetValue(a_ruleKey, out OptimizeRuleElementSettings settings))
        {
            return settings.Settings;
        }

        return null;
    }

    public decimal GetCategoryScaling(string a_category)
    {
        if (m_categoryMultiplier.TryGetValue(a_category, out decimal scaling))
        {
            return scaling;
        }

        return 0m;
    }

    public void SetCategoryScaling(string a_category, decimal a_newMultiplier)
    {
        m_categoryMultiplier[a_category] = a_newMultiplier;
    }

    public bool PreventsScaling(string a_ruleKey)
    {
        if (m_rulePoints.TryGetValue(a_ruleKey, out OptimizeRuleElementSettings settings))
        {
            return settings.PreventScaling;
        }

        //Default for not set since 0m is a valid scaling value
        return false;
    }

    public decimal GetMaxPenalty(string a_ruleKey)
    {
        if (m_rulePoints.TryGetValue(a_ruleKey, out OptimizeRuleElementSettings settings))
        {
            return settings.MaxPenalty;
        }

        return 0;
    }

    public decimal GetMinPenalty(string a_ruleKey)
    {
        if (m_rulePoints.TryGetValue(a_ruleKey, out OptimizeRuleElementSettings settings))
        {
            return settings.MinPenalty;
        }

        return 0;
    }

    public PackageEnums.ESequencingFactorEarlyWindowPenaltyScale GetEarlyWindowPenaltyScale(string a_ruleKey)
    {
        if (m_rulePoints.TryGetValue(a_ruleKey, out OptimizeRuleElementSettings settings))
        {
            return settings.EarlyWindowPenaltyScale;
        }

        return PackageEnums.ESequencingFactorEarlyWindowPenaltyScale.Linear;
    }

    /// <summary>
    /// Updates Settings for a particular optimize rule element
    /// </summary>
    /// <param name="a_elementName"></param>
    /// <param name="a_settingsToUpdate"></param>
    internal void AddOrUpdateMapping(string a_elementName, OptimizeRuleElementSettings a_settingsToUpdate)
    {
        if (m_rulePoints.TryGetValue(a_elementName, out OptimizeRuleElementSettings existingSettings))
        {
            if (a_settingsToUpdate.PointsIsSet)
            {
                existingSettings.Points = a_settingsToUpdate.Points;
            }

            if (a_settingsToUpdate.ResourceMultiplierIsSet)
            {
                existingSettings.ResourceMultiplier = a_settingsToUpdate.ResourceMultiplier;
            }

            if (a_settingsToUpdate.SettingDataIsSet)
            {
                existingSettings.Settings = a_settingsToUpdate.Settings;
            }

            if (a_settingsToUpdate.PreventScalingIsSet)
            {
                existingSettings.PreventScaling = a_settingsToUpdate.PreventScaling;
            }

            if (a_settingsToUpdate.MinimumScoreIsSet)
            {
                existingSettings.MinimumScore = a_settingsToUpdate.MinimumScore;
            }

            if (a_settingsToUpdate.UseMinimumScoreIsSet)
            {
                existingSettings.UseMinimumScore = a_settingsToUpdate.UseMinimumScore;
            }

            if (a_settingsToUpdate.MaxPenaltyIsSet)
            {
                existingSettings.MaxPenalty = a_settingsToUpdate.MaxPenalty;
            }

            if (a_settingsToUpdate.MinPenaltyIsSet)
            {
                existingSettings.MinPenalty = a_settingsToUpdate.MinPenalty;
            }

            if (a_settingsToUpdate.EarlyWindowPenaltyScaleIsSet)
            {
                existingSettings.EarlyWindowPenaltyScale = a_settingsToUpdate.EarlyWindowPenaltyScale;
            }
        }
        else
        {
            m_rulePoints[a_elementName] = a_settingsToUpdate;
        }
    }

    public class OptimizeRuleElementSettings : IPTSerializable
    {
        internal OptimizeRuleElementSettings() { }

        internal OptimizeRuleElementSettings(BalancedCompositeDispatcherDefinitionUpdateT.OptimizeRuleElementSettings a_updatedSettings)
        {
            if (a_updatedSettings.PointsIsSet)
            {
                Points = a_updatedSettings.Points;
            }

            if (a_updatedSettings.ResourceMultiplierIsSet)
            {
                ResourceMultiplier = a_updatedSettings.ResourceMultiplier;
            }

            if (a_updatedSettings.SettingDataIsSet)
            {
                Settings = a_updatedSettings.Settings;
            }

            if (a_updatedSettings.PreventScalingIsSet)
            {
                PreventScaling = a_updatedSettings.PreventScaling;
            }

            if (a_updatedSettings.MinimumScoreIsSet)
            {
                MinimumScore = a_updatedSettings.MinimumScore;
            }

            if (a_updatedSettings.UseMinimumScoreIsSet)
            {
                UseMinimumScore = a_updatedSettings.UseMinimumScore;
            }

            if (a_updatedSettings.MaxPenaltyIsSet)
            {
                MaxPenalty = a_updatedSettings.MaxPenalty;
            }

            if (a_updatedSettings.MinPenaltyIsSet)
            {
                MinPenalty = a_updatedSettings.MinPenalty;
            }

            if (a_updatedSettings.EarlyWindowPenaltyScaleIsSet)
            {
                EarlyWindowPenaltyScale = a_updatedSettings.EarlyWindowPenaltyScale;
            }
        }

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

        private decimal m_resourceMultiplier = 1;

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
                m_bools[c_minPenaltyIsSetIdx] = true;
                m_minPenalty = value;
            }
        }

        private PackageEnums.ESequencingFactorEarlyWindowPenaltyScale m_earlyWindowPenaltyScale;
        public PackageEnums.ESequencingFactorEarlyWindowPenaltyScale EarlyWindowPenaltyScale
        {
            get => m_earlyWindowPenaltyScale;
            set
            {
                m_bools[c_earlyWindowPenaltyScaleIsSetIdx] = true;
                m_earlyWindowPenaltyScale = value;
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

        public OptimizeRuleElementSettings(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 13004)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
                if (SerializedSettingData)
                {
                    m_settings = new SettingData(a_reader);
                }
                a_reader.Read(out m_minimumScore);
                a_reader.Read(out m_maxPenalty);
                a_reader.Read(out m_minPenalty);
                a_reader.Read(out short earlyWindowScaleValue);
                m_earlyWindowPenaltyScale = (PackageEnums.ESequencingFactorEarlyWindowPenaltyScale)earlyWindowScaleValue;
            }
            else if(a_reader.VersionNumber >= 12432)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
                if (SerializedSettingData)
                {
                    m_settings = new SettingData(a_reader);
                }
                a_reader.Read(out m_minimumScore);
            }
            else
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_points);
                a_reader.Read(out m_resourceMultiplier);
                if (SerializedSettingData)
                {
                    m_settings = new SettingData(a_reader);
                }
            }
        }

        public void Serialize(IWriter a_writer)
        {
            SerializedSettingData = m_settings != null;
            m_bools.Serialize(a_writer);
            a_writer.Write(m_points);
            a_writer.Write(m_resourceMultiplier);
            if (SerializedSettingData)
            {
                m_settings.Serialize(a_writer);
            }
            a_writer.Write(m_minimumScore);
            a_writer.Write(m_maxPenalty);
            a_writer.Write(m_minPenalty);
            a_writer.Write((short)m_earlyWindowPenaltyScale);
        }

        private const short c_pointsIsSetIdx = 0;
        private const short c_resMultiplierIsSetIdx = 1;
        private const short c_settingDataIsSetIdx = 2;
        private const short c_preventScalingIdx = 4;
        private const short c_preventScalingIsSetIdx = 5;
        private const short c_serializedSettingDataIdx = 6;
        private const short c_minimumScoreIsSetIdx = 7;
        private const short c_useMinimumScoreIdx = 8;
        private const short c_useMinimumScoreIsSetIdx = 9;
        private const short c_maxPenaltyIsSetIdx = 9;
        private const short c_minPenaltyIsSetIdx = 10;
        private const short c_earlyWindowPenaltyScaleIsSetIdx = 11;

        public bool PointsIsSet => m_bools[c_pointsIsSetIdx];
        public bool ResourceMultiplierIsSet => m_bools[c_resMultiplierIsSetIdx];
        public bool SettingDataIsSet => m_bools[c_settingDataIsSetIdx];
        public bool MinimumScoreIsSet => m_bools[c_minimumScoreIsSetIdx];
        public bool UseMinimumScoreIsSet => m_bools[c_useMinimumScoreIsSetIdx];
        public bool MaxPenaltyIsSet => m_bools[c_maxPenaltyIsSetIdx];
        public bool MinPenaltyIsSet => m_bools[c_minPenaltyIsSetIdx];
        public bool EarlyWindowPenaltyScaleIsSet => m_bools[c_earlyWindowPenaltyScaleIsSetIdx];

        public bool PreventScaling
        {
            get => m_bools[c_preventScalingIdx];
            set
            {
                m_bools[c_preventScalingIdx] = value;
                m_bools[c_preventScalingIsSetIdx] = true;
            }
        }

        public bool PreventScalingIsSet => m_bools[c_preventScalingIsSetIdx];

        private bool SerializedSettingData
        {
            get => m_bools[c_serializedSettingDataIdx];
            set => m_bools[c_serializedSettingDataIdx] = value;
        }

        public int UniqueId => 1013;
    }
}