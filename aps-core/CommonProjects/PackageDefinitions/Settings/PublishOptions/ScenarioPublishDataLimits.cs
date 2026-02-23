namespace PT.PackageDefinitions.Settings.PublishOptions
{
    public class ScenarioPublishDataLimits :ISettingData, ICloneable, IPTSerializable, IEquatable<ScenarioPublishDataLimits>
    {
        #region BoolVector32
        private BoolVector32 m_boolVector; 
        private const int c_publishInventoryIdx = 1;
        private const int c_publishBlocksIdx = 2;
        private const int c_publishBlockIntervalsIdx = 3;
        private const int c_keepHistoryInventoryIdx = 4;
        private const int c_keepHistoryBlocksIdx = 5;
        private const int c_keepHistoryBlockIntervalsIdx = 6;
        private const int c_publishCapacityIntervalsIdx = 7;
        private const int c_keepCapacityIntervalHistoryIdx = 8;
        private const int c_publishProductRulesIdx = 9;
        private const int c_keepHistoryProductRulesIdx = 10;
        private const int c_publishJobsIdx = 11;
        private const int c_keepHistoryJobsIdx = 12;
        private const int c_publishTemplatesIdx = 13;
        private const int c_keepHistoryTemplatesIdx = 14;
        private const int c_publishManufacturingOrdersIdx = 15;
        private const int c_keepHistoryManufacturingOrdersIdx = 16;
        private const int c_publishOperationsIdx = 17;
        private const int c_keepHistoryOperationsIdx = 18;
        private const int c_publishActivitiesIdx = 19;
        private const int c_keepHistoryActivitiesIdx = 20;
        #endregion

        #region Fields
        /// <summary>
        /// Whether to export Inventory related objects including Warehouses, Items, Inventories, Material Requirements, and Products.
        /// </summary>
        public bool PublishInventory
        {
            get => m_boolVector[c_publishInventoryIdx];
            set
            {
                m_boolVector[c_publishInventoryIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep any published Inventory related objects.
        /// </summary>
        public bool KeepHistoryInventory
        {
            get => m_boolVector[c_keepHistoryInventoryIdx];
            set
            {
                m_boolVector[c_keepHistoryInventoryIdx] = value;
            }
        }

        public bool PublishCapacityIntervals
        {
            get => m_boolVector[c_publishCapacityIntervalsIdx];
            set
            {
                m_boolVector[c_publishCapacityIntervalsIdx] = value;
            }
        }

        public bool KeepCapacityIntervals
        {
            get => m_boolVector[c_keepCapacityIntervalHistoryIdx];
            set
            {
                m_boolVector[c_keepCapacityIntervalHistoryIdx] = value;
            }
        }
        /// <summary>
        /// Whether to publish Product Rules
        /// </summary>
        public bool PublishProductRules
        {
            get => m_boolVector[c_publishProductRulesIdx];
            set
            {
                m_boolVector[c_publishProductRulesIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep history for Product Rules
        /// </summary>
        public bool KeepHistoryProductRules
        {
            get => m_boolVector[c_keepHistoryProductRulesIdx];
            set
            {
                m_boolVector[c_keepHistoryProductRulesIdx] = value;
            }
        }

        //HERE

        /// <summary>
        /// Whether to publish Jobs
        /// </summary>
        public bool PublishJobs
        {
            get => m_boolVector[c_publishJobsIdx];
            set
            {
                m_boolVector[c_publishJobsIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep history for Jobs
        /// </summary>
        public bool KeepHistoryJobs
        {
            get => m_boolVector[c_keepHistoryJobsIdx];
            set
            {
                if (value && !PublishJobs)
                {
                    throw new APSCommon.PTValidationException("Cannot keep history for Jobs if they're not being published.");
                }

                m_boolVector[c_keepHistoryJobsIdx] = value;
            }
        }

        /// <summary>
        /// Whether to publish Template Jobs
        /// </summary>
        public bool PublishTemplates
        {
            get => m_boolVector[c_publishTemplatesIdx];
            set
            {
                if (value && !PublishJobs)
                {
                    throw new APSCommon.PTValidationException("Publish Jobs must be turned on to publish Templates.");
                }

                m_boolVector[c_publishTemplatesIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep history for Template Jobs
        /// </summary>
        public bool KeepHistoryTemplates
        {
            get => m_boolVector[c_keepHistoryTemplatesIdx];
            set
            {
                if (value && !PublishTemplates)
                {
                    throw new APSCommon.PTValidationException("Cannot keep history for Templates if they're not being published.");
                }

                m_boolVector[c_keepHistoryTemplatesIdx] = value;
            }
        }

        /// <summary>
        /// Whether to publish Manufacturing Order
        /// </summary>
        public bool PublishManufacturingOrders
        {
            get => m_boolVector[c_publishManufacturingOrdersIdx];
            set
            {
                if (value && !PublishJobs)
                {
                    throw new APSCommon.PTValidationException("Publish Jobs must be turned on to publish Manufacturing Orders.");
                }

                m_boolVector[c_publishManufacturingOrdersIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep history for Manufacturing Order
        /// </summary>
        public bool KeepHistoryManufacturingOrders
        {
            get => m_boolVector[c_keepHistoryManufacturingOrdersIdx];
            set
            {
                m_boolVector[c_keepHistoryManufacturingOrdersIdx] = value;
            }
        }

        /// <summary>
        /// Whether to publish Operations
        /// </summary>
        public bool PublishOperations
        {
            get => m_boolVector[c_publishOperationsIdx];
            set
            {
                if (value && !PublishManufacturingOrders)
                {
                    throw new APSCommon.PTValidationException("Publish ManufacturingOrders must be turned on to publish Operations.");
                }

                m_boolVector[c_publishOperationsIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep history for Operations
        /// </summary>
        public bool KeepHistoryOperations
        {
            get => m_boolVector[c_keepHistoryOperationsIdx];
            set
            {
                if (value && !PublishOperations)
                {
                    throw new APSCommon.PTValidationException("Cannot keep history for Operations if they're not being published.");
                }

                m_boolVector[c_keepHistoryOperationsIdx] = value;
            }
        }

        /// <summary>
        /// Whether to publish Activities
        /// </summary>
        public bool PublishActivities
        {
            get => m_boolVector[c_publishActivitiesIdx];
            set
            {
                if (value && !PublishOperations)
                {
                    throw new APSCommon.PTValidationException("Publish Operations must be turned on to publish Activities");
                }

                m_boolVector[c_publishActivitiesIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep history for Activities
        /// </summary>
        public bool KeepHistoryActivities
        {
            get => m_boolVector[c_keepHistoryActivitiesIdx];
            set
            {
                if (value && !PublishActivities)
                {
                    throw new APSCommon.PTValidationException("Cannot keep history for Activities if they're not being published.");
                }

                m_boolVector[c_keepHistoryActivitiesIdx] = value;
            }
        }

        /// <summary>
        /// Whether to export Blocks.
        /// </summary>
        public bool PublishBlocks
        {
            get => m_boolVector[c_publishBlocksIdx];

            set
            {
                if (value && !PublishActivities)
                {
                    throw new APSCommon.PTValidationException("Publish Activities must be turned on to publish Blocks");
                }

                m_boolVector[c_publishBlocksIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep any published Blocks.
        /// </summary>
        public bool KeepHistoryBlocks
        {
            get => m_boolVector[c_keepHistoryBlocksIdx];

            set
            {
                if (value && !PublishBlocks)
                {
                    throw new APSCommon.PTValidationException("Cannot keep history for Blocks if they're not being published.");
                }

                m_boolVector[c_keepHistoryBlocksIdx] = value;
            }
        }

        /// <summary>
        /// Whether to export Block Intervals.
        /// Irrelevant if PublishBlocks is false.
        /// </summary>
        public bool PublishBlockIntervals
        {
            get => m_boolVector[c_publishBlockIntervalsIdx];

            set
            {
                if (value && !PublishBlocks)
                {
                    throw new APSCommon.PTValidationException("Publish Blocks must be turned on to publish Block Intervals.");
                }

                m_boolVector[c_publishBlockIntervalsIdx] = value;
            }
        }

        /// <summary>
        /// Whether to keep any published Block Intervals.
        /// Irrelevant if KeepHistoryBlocks is false.
        /// </summary>
        public bool KeepHistoryBlockIntervals
        {
            get => m_boolVector[c_keepHistoryBlockIntervalsIdx];

            set
            {
                if (value && !PublishBlockIntervals)
                {
                    throw new APSCommon.PTValidationException("Cannot keep history for Block Intervals if they're not being published.");
                }

                m_boolVector[c_keepHistoryBlockIntervalsIdx] = value;
            }
        }
        #endregion

        public ScenarioPublishDataLimits(bool a_publishInventory, bool a_publishBlocks, bool a_publishBlockIntervals, bool a_keepHistoryInventory,
                                         bool a_keepHistoryBlocks, bool a_keepHistoryBlockIntervals, bool a_publishCapacityIntervals, bool a_keepCapacityInterval,
                                         bool a_publishProductRules, bool a_keepHistoryProductRules, bool a_publishJobs, bool a_keepHistoryJobs, bool a_publishTemplates,
                                         bool a_keepHistoryTemplates, bool a_publishManufacturingOrders, bool a_keepHistoryManufacturingOrders, bool a_publishOperations,
                                         bool a_keepHistoryOperations, bool a_publishActivities, bool a_keepHistoryActivities)
        {
            PublishInventory = a_publishInventory;
            KeepHistoryInventory = a_keepHistoryInventory;

            PublishCapacityIntervals = a_publishCapacityIntervals;
            KeepCapacityIntervals = a_keepCapacityInterval;

            PublishProductRules = a_publishProductRules;
            KeepHistoryProductRules = a_keepHistoryProductRules;

            PublishJobs = a_publishJobs;
            KeepHistoryJobs = a_keepHistoryJobs;

            PublishTemplates = a_publishTemplates;
            KeepHistoryTemplates = a_keepHistoryTemplates;

            PublishManufacturingOrders = a_publishManufacturingOrders;
            KeepHistoryManufacturingOrders = a_keepHistoryManufacturingOrders;

            PublishOperations = a_publishOperations;
            KeepHistoryOperations = a_keepHistoryOperations;

            PublishActivities = a_publishActivities;
            KeepHistoryActivities = a_keepHistoryActivities;

            PublishBlocks = a_publishBlocks;
            KeepHistoryBlocks = a_keepHistoryBlocks;

            PublishBlockIntervals = a_publishBlockIntervals;
            KeepHistoryBlockIntervals = a_keepHistoryBlockIntervals;
        }
        public ScenarioPublishDataLimits(bool a_publishJobs)
        {
            PublishInventory = false;
            KeepHistoryInventory = false;
            PublishCapacityIntervals = false;
            KeepCapacityIntervals = false;
            PublishProductRules = false;
            KeepHistoryProductRules = false;

            PublishJobs = a_publishJobs; // the order in which we update below is important due to validation
            KeepHistoryJobs = false;
            PublishTemplates = false;
            KeepHistoryTemplates = false;
            PublishManufacturingOrders = false;
            KeepHistoryManufacturingOrders = false;
            PublishOperations = false;
            KeepHistoryOperations = false;
            PublishActivities = false;
            KeepHistoryActivities = false;
            PublishBlocks = false;
            KeepHistoryBlocks = false;
            PublishBlockIntervals = false;
            KeepHistoryBlockIntervals = false;
        }
        public ScenarioPublishDataLimits()
        {
            PublishInventory = false;
            KeepHistoryInventory = false;
            PublishCapacityIntervals = false;
            KeepCapacityIntervals = false;
            PublishProductRules = false;
            KeepHistoryProductRules = false;

            PublishJobs = false;
            KeepHistoryJobs = false;
            PublishTemplates = false;
            KeepHistoryTemplates = false;
            PublishManufacturingOrders = false;
            KeepHistoryManufacturingOrders = false;
            PublishOperations = false;
            KeepHistoryOperations = false;
            PublishActivities = false;
            KeepHistoryActivities = false;
            PublishBlocks = false;
            KeepHistoryBlocks = false;
            PublishBlockIntervals = false;
            KeepHistoryBlockIntervals = false;
        }
        #region IPTSerializable Members
        public ScenarioPublishDataLimits(IReader a_reader)
        {
            m_boolVector = new BoolVector32(a_reader);
        }
        public void Serialize(IWriter a_writer)
        {
            m_boolVector.Serialize(a_writer);
        }
        #endregion

        public int UniqueId => 9802;
        /// <summary>
        /// Returns if settings are identical
        /// <param name="a_dataLimits"></param>
        /// <returns></returns>
        /// </summary>
        public bool Equals(ScenarioPublishDataLimits a_dataLimits)
        {
            if (a_dataLimits == null)
            {
                return false;

            }
            return m_boolVector.Equals(a_dataLimits.m_boolVector);
        }
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public ScenarioPublishDataLimits Clone()
        {
            return (ScenarioPublishDataLimits)MemberwiseClone();
        }
        #endregion
        public string SettingKey => Key;
        public string SettingCaption => "Publish Data Limits";
        public string Description => "Publish Data Limits";
        public string SettingsGroup => SettingGroupConstants.SystemSettingsGroup;
        public string SettingsGroupCategory => SettingGroupConstants.Publish;
        public static string Key => "scenarioSetting_ScenarioPublishDataLimits";
    }
}
