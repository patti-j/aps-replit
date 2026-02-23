using PT.SchedulerDefinitions;

namespace PT.ERPTransmissions;

public class ProductRulesT : ERPMaintenanceTransmission<ProductRulesT.ProductRule>, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 621;

    public ProductRulesT(IReader reader)
        : base(reader)
    {
        reader.Read(out generateCapabilitiesBaseOnProductRules);

        int count;
        reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            ProductRules.Add(new ProductRule(reader));
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(generateCapabilitiesBaseOnProductRules);

        writer.Write(ProductRules.Count);
        for (int i = 0; i < ProductRules.Count; i++)
        {
            ProductRules[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ProductRulesT() { }

    public List<ProductRule> ProductRules = new ();

    public override int Count => ProductRules.Count;

    private bool generateCapabilitiesBaseOnProductRules;

    /// <summary>
    /// If true then a Capability is created for each ProductRule.
    /// The Capability is the Item Name and Operation Name.
    /// The Capabilities are linked to the Resources as well.
    /// </summary>
    public bool GenerateCapabilitiesBaseOnProductRules
    {
        get => generateCapabilitiesBaseOnProductRules;
        set => generateCapabilitiesBaseOnProductRules = value;
    }

    #region Database Loading
    public void Fill(System.Data.IDbCommand productRulesTableCmd)
    {
        ProductRulesTDataSet ds = new ();
        FillTable(ds.ProductRules, productRulesTableCmd);
        Fill(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void Fill(ProductRulesTDataSet ds)
    {
        Dictionary<string, ProductRuleKey> rulesAdded = new ();

        for (int i = 0; i < ds.ProductRules.Count; i++)
        {
            ProductRule newRule = new (ds.ProductRules[i]);
            ProductRuleKey newRuleKey = new (newRule);
            if (!rulesAdded.ContainsKey(newRuleKey.ToString()))
            {
                ProductRules.Add(newRule);
                rulesAdded.Add(newRuleKey.ToString(), newRuleKey);
            }
            else
            {
                throw new APSCommon.PTValidationException("2109", new object[] { newRule.ProductItemExternalId, newRule.PlantExternalId, newRule.DepartmentExternalId, newRule.ResourceExternalId, newRule.ProductCode });
            }
        }
    }

    private class ProductRuleKey
    {
        public ProductRuleKey(ProductRule pRule)
        {
            ItemExternalId = pRule.ProductItemExternalId;
            PlantExternalId = pRule.PlantExternalId;
            DepartmentExternalId = pRule.DepartmentExternalId;
            ResourceExternalId = pRule.ResourceExternalId;
            if (pRule.ProductCode == null)
            {
                ProductCode = "";
            }
            else
            {
                ProductCode = pRule.ProductCode;
            }
        }

        internal readonly string ItemExternalId;
        internal readonly string PlantExternalId;
        internal readonly string DepartmentExternalId;
        internal readonly string ResourceExternalId;
        internal readonly string ProductCode;

        public override string ToString()
        {
            return string.Format("Item={0}, Plant={1}, Dept={2}, Res={3}, Op={4}", ItemExternalId, PlantExternalId, DepartmentExternalId, ResourceExternalId, ProductCode);
        }
    }
    #endregion Database Loading

    #region ProductRule
    public class ProductRule : IPTSerializable
    {
        #region IPTSerializable Members
        public ProductRule(IReader reader)
        {
            if (reader.VersionNumber >= 12528)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
                reader.Read(out m_transferQty);
                reader.Read(out m_priority);
                reader.Read(out m_productionSetupCost);
                reader.Read(out m_cleanoutCost);
                reader.Read(out m_storageSpan);
            }
            else if (reader.VersionNumber >= 12526)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
                reader.Read(out m_transferQty);
                reader.Read(out m_priority);
                reader.Read(out m_productionSetupCost);
                reader.Read(out m_cleanoutCost);
            }
            else  if (reader.VersionNumber >= 12504)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
                reader.Read(out m_transferQty);
                reader.Read(out m_priority);
            }
            else if (reader.VersionNumber >= 12500)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
                reader.Read(out m_transferQty);
            }
            else if (reader.VersionNumber >= 12420)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
                reader.Read(out m_transferQty);
                reader.Read(out m_priority);
            }
            else if (reader.VersionNumber >= 12419)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
                reader.Read(out m_transferQty);
            }
            else if (reader.VersionNumber >= 12402)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
            }
            else if (reader.VersionNumber >= 12401)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
            }
            else if (reader.VersionNumber >= 12326)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
                reader.Read(out m_minQty);
                reader.Read(out m_maxQty);
            }
            else if (reader.VersionNumber >= 12303)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
                reader.Read(out m_cleanoutUnitsRatio);
            }
            else if (reader.VersionNumber >= 12302)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
            }
            else if (reader.VersionNumber >= 12211)
            {
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_minVolume);
                reader.Read(out m_maxVolume);
            }
            else if (reader.VersionNumber >= 713)
            {
                reader.Read(out plantExternalId);
                reader.Read(out deptExternalId);
                reader.Read(out resourceExternalId);
                reader.Read(out productItemExternalId);
                reader.Read(out m_productCode);
                reader.Read(out cycleSpan);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out qtyPerCycle);
                reader.Read(out headStartSpan);
                reader.Read(out materialPostProcessingSpan);
                m_bools = new BoolVector32(reader);
                m_userFields = new UserFieldList(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            m_bools.Serialize(writer);
            m_userFields.Serialize(writer);
            writer.Write(plantExternalId);
            writer.Write(deptExternalId);
            writer.Write(resourceExternalId);
            writer.Write(productItemExternalId);
            writer.Write(m_productCode);
            writer.Write(cycleSpan);
            writer.Write(setupSpan);
            writer.Write(postProcessingSpan);
            writer.Write(cleanSpan);
            writer.Write(planningScrapPercent);
            writer.Write(qtyPerCycle);
            writer.Write(headStartSpan);
            writer.Write(materialPostProcessingSpan);
            writer.Write(m_minVolume);
            writer.Write(m_maxVolume);
            writer.Write(m_cleanoutUnitsRatio);
            writer.Write(m_minQty);
            writer.Write(m_maxQty);
            writer.Write(m_transferQty);
            writer.Write(m_priority);
            writer.Write(m_productionSetupCost);
            writer.Write(m_cleanoutCost);
            writer.Write(m_storageSpan);

        }

        public const int UNIQUE_ID = 607;

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public ProductRule() { } // reqd. for xml serialization

        public ProductRule(string aProductItemExternalId, string aPlantExternalId, string aDeptExternalId, string aResourceExternalId)
        {
            productItemExternalId = aProductItemExternalId;
            plantExternalId = aPlantExternalId;
            deptExternalId = aDeptExternalId;
            resourceExternalId = aResourceExternalId;
        }

        public ProductRule(ProductRulesTDataSet.ProductRulesRow row)
        {
            PlantExternalId = row.PlantExternalId;
            DepartmentExternalId = row.DepartmentExternalId;
            ResourceExternalId = row.ResourceExternalId;
            ProductItemExternalId = row.ProductItemExternalId;

            if (!row.IsCycleHrsNull())
            {
                CycleSpan = PTDateTime.GetSafeTimeSpan(row.CycleHrs);
            }

            if (!row.IsProductCodeNull())
            {
                ProductCode = row.ProductCode;
            }

            if (!row.IsPlanningScrapPercentNull())
            {
                PlanningScrapPercent = (decimal)row.PlanningScrapPercent;
            }

            if (!row.IsPostProcessingHrsNull())
            {
                PostProcessingSpan = PTDateTime.GetSafeTimeSpan(row.PostProcessingHrs);
            }

            if (!row.IsQtyPerCycleNull())
            {
                QtyPerCycle = (decimal)row.QtyPerCycle;
            }

            if (!row.IsSetupHrsNull())
            {
                SetupSpan = PTDateTime.GetSafeTimeSpan(row.SetupHrs);
            }

            if (!row.IsUseSetupHrsNull())
            {
                UseSetupSpan = row.UseSetupHrs;
            }

            if (!row.IsProductionSetupCostNull())
            {
                ProductionSetupCost = row.ProductionSetupCost;
            }

            if (!row.IsUseProductionSetupCostNull())
            {
                UseProductionSetupCost = row.UseProductionSetupCost;
            }

            if (!row.IsHeadStartHrsNull())
            {
                HeadStartSpan = PTDateTime.GetSafeTimeSpan(row.HeadStartHrs);
            }

            if (!row.IsUseCycleHrsNull())
            {
                UseCycleSpan = row.UseCycleHrs;
            }

            if (!row.IsUsePlanningScrapPercentNull())
            {
                UsePlanningScrapPercent = row.UsePlanningScrapPercent;
            }

            if (!row.IsUsePostProcessingHrsNull())
            {
                UsePostProcessingSpan = row.UsePostProcessingHrs;
            }

            if (!row.IsUseQtyPerCycleNull())
            {
                UseQtyPerCycle = row.UseQtyPerCycle;
            }

            if (!row.IsUseHeadStartSpanNull())
            {
                UseHeadStartSpan = row.UseHeadStartSpan;
            }

            if (!row.IsUseMaterialPostProcessingSpanNull())
            {
                UseMaterialPostProcessingSpan = row.UseMaterialPostProcessingSpan;
            }

            if (!row.IsMaterialPostProcessingHrsNull())
            {
                MaterialPostProcessingSpan = PTDateTime.GetSafeTimeSpan(row.MaterialPostProcessingHrs);
            }

            if (!row.IsCleanoutUnitsRatioNull())
            {
                CleanoutUnitsRatio = row.CleanoutUnitsRatio;
            }

            if (!row.IsUseCleanoutUnitsNull())
            {
                UseCleanoutUnits = row.UseCleanoutUnits;
            }

            if (!row.IsMinVolumeNull())
            {
                MinVolume = row.MinVolume;
            }

            if (!row.IsUseMinVolumeNull())
            {
                UseMinVolume = row.UseMinVolume;
            }

            if (!row.IsMaxVolumeNull())
            {
                MaxVolume = row.MaxVolume;
            }

            if (!row.IsUseMaxVolumeNull())
            {
                UseMaxVolume = row.UseMaxVolume;
            }

            if (!row.IsMinQtyNull())
            {
                MinQty = row.MinQty;
            }

            if (!row.IsUseMinQtyNull())
            {
                UseMinQty = row.UseMinQty;
            }

            if (!row.IsMaxQtyNull())
            {
                MaxQty = row.MaxQty;
            }

            if (!row.IsUseMaxQtyNull())
            {
                UseMaxQty = row.UseMaxQty;
            }

            if (!row.IsTransferQtyNull())
            {
                TransferQty = row.TransferQty;
            }

            if (!row.IsUseTransferQtyNull())
            {
                UseTransferQty = row.UseTransferQty;
            }

            if (!row.IsPriorityNull())
            {
                Priority = row.Priority;
            }

            if (!row.IsUsePriorityNull())
            {
                UsePriority = row.UsePriority;
            }

            if (!row.IsCleanHrsNull())
            {
                CleanSpan = TimeSpan.FromHours(row.CleanHrs);
            }

            if (!row.IsUseCleanHrsNull())
            {
                UseCleanSpan = row.UseCleanHrs;
            }

            if (!row.IsCleanoutCostNull())
            {
                CleanoutCost = row.CleanoutCost;
            }

            if (!row.IsUseCleanoutCostNull())
            {
                UseCleanoutCost = row.UseCleanoutCost;
            }

            if (!row.IsUseStorageHrsNull())
            {
                UseStorageSpan = row.UseStorageHrs;
            }

            if (!row.IsStorageHrsNull())
            {
                StorageSpan = PTDateTime.GetSafeTimeSpan(row.StorageHrs);
            }
        }

        #region Shared Properties
        private BoolVector32 m_bools;
        private const short useCycleSpanIdx = 0;
        private const short useSetupSpanIdx = 1;
        private const short usePostProcessingSpanIdx = 2;
        private const short usePlanningScrapPercentIdx = 3;
        private const short useQtyPerCycleIdx = 4;
        private const short useHeadStartSpanIdx = 5;
        private const short useMaterialPostProcessingSpanIdx = 6;
        private const short c_useMinVolume = 7;
        private const short c_useMaxVolume = 8;
        private const short useCleanSpanIdx = 9;
        private const short c_useCleanoutUnitsIdx = 10;
        private const short c_useMinQtyIdx = 11;
        private const short c_useMaxQtyIdx = 12;
        private const short c_useTransferQtyIdx = 13;
        private const short c_usePriorityIdx = 14;
        private const short c_useProductionSetupCostIdx = 15;
        private const short c_useCleanoutCostIdx = 16;
        private const short c_useStorageSpanIdx = 17;

        private string m_productCode = ""; //Can't be null.

        /// <summary>
        /// If specified, then these rules only apply to Operations with this Operation Name.  Otherwise, these rules apply to all
        /// Operations for any Manufacturing Order making this Product.
        /// </summary>
        public string ProductCode
        {
            get => m_productCode;
            set => m_productCode = value ?? ""; //don't allow nulls -- scheduler code can't handle it.
        }

        private TimeSpan cycleSpan;

        /// <summary>
        /// Overrides Operation CycleSpan if UseCycleSpan is true.
        /// </summary>
        public TimeSpan CycleSpan
        {
            get => cycleSpan;
            set => cycleSpan = value;
        }

        /// <summary>
        /// If true then overrides the Operation CycleSpan when the Operation is scheduled.
        /// Otherwise the Operation CycleSpan is used as is.
        /// </summary>
        public bool UseCycleSpan
        {
            get => m_bools[useCycleSpanIdx];
            set => m_bools[useCycleSpanIdx] = value;
        }
        
        private TimeSpan m_storageSpan;

        /// <summary>
        /// Overrides Operation StorageSpan if UseStorageSpan is true.
        /// </summary>
        public TimeSpan StorageSpan
        {
            get => m_storageSpan;
            set => m_storageSpan = value;
        }

        /// <summary>
        /// If true then overrides the Operation StorageSpan when the Operation is scheduled.
        /// Otherwise the Operation StorageSpan is used as is.
        /// </summary>
        public bool UseStorageSpan
        {
            get => m_bools[c_useStorageSpanIdx];
            set => m_bools[c_useStorageSpanIdx] = value;
        }

        private TimeSpan setupSpan;

        /// <summary>
        /// Overrides Operation SetupSpan if UseSetupSpan is true.
        /// </summary>
        public TimeSpan SetupSpan
        {
            get => setupSpan;
            set => setupSpan = value;
        }

        /// <summary>
        /// If true then overrides the Operation SetupSpan when the Operation is scheduled.
        /// Otherwise the Operation SetupSpan is used as is.
        /// </summary>
        public bool UseSetupSpan
        {
            get => m_bools[useSetupSpanIdx];
            set => m_bools[useSetupSpanIdx] = value;
        }

        private decimal m_productionSetupCost;
        public decimal ProductionSetupCost
        {
            get => m_productionSetupCost;
            set => m_productionSetupCost = value;
        }

        public bool UseProductionSetupCost
        {
            get => m_bools[c_useProductionSetupCostIdx];
            set => m_bools[c_useProductionSetupCostIdx] = value;
        }

        private TimeSpan postProcessingSpan;

        /// <summary>
        /// Overrides Operation PostProcessingSpan if UsePostProcessingSpan is true.
        /// </summary>
        public TimeSpan PostProcessingSpan
        {
            get => postProcessingSpan;
            set => postProcessingSpan = value;
        }

        /// <summary>
        /// If true then overrides the Operation PostProcessingSpan when the Operation is scheduled.
        /// Otherwise the Operation PostProcessingSpan is used as is.
        /// </summary>
        public bool UsePostProcessingSpan
        {
            get => m_bools[usePostProcessingSpanIdx];
            set => m_bools[usePostProcessingSpanIdx] = value;
        }

        private TimeSpan cleanSpan;

        /// <summary>
        /// Overrides Operation Clean Span if UseCleanSpan is true.
        /// </summary>
        public TimeSpan CleanSpan
        {
            get => cleanSpan;
            set => cleanSpan = value;
        }

        /// <summary>
        /// If true then overrides the Operation Clean Span when the Operation is scheduled.
        /// Otherwise the Operation Clean Span is used as is.
        /// </summary>
        public bool UseCleanSpan
        {
            get => m_bools[useCleanSpanIdx];
            set => m_bools[useCleanSpanIdx] = value;
        }

        private decimal m_cleanoutCost;

        public decimal CleanoutCost
        {
            get => m_cleanoutCost;
            set => m_cleanoutCost = value;
        }

        public bool UseCleanoutCost
        {
            get => m_bools[c_useCleanoutCostIdx];
            set => m_bools[c_useCleanoutCostIdx] = value;
        }

        private decimal planningScrapPercent;

        /// <summary>
        /// Overrides Operation PlanningScrapPercent if UseScrapPercent is true.
        /// </summary>
        public decimal PlanningScrapPercent
        {
            get => planningScrapPercent;
            set
            {
                if (value >= 0 && value < 1)
                {
                    planningScrapPercent = value;
                }
                else
                {
                    throw new APSCommon.PTValidationException("2239", new object[] { value });
                }
            }
        }

        /// <summary>
        /// If true then overrides the Operation PlanningScrapPercent when the Operation is scheduled.
        /// Otherwise the Operation ScrapPercent is used as is.
        /// </summary>
        public bool UsePlanningScrapPercent
        {
            get => m_bools[usePlanningScrapPercentIdx];
            set => m_bools[usePlanningScrapPercentIdx] = value;
        }

        private decimal qtyPerCycle = DefaultQtyPerCycle;
        public const decimal DefaultQtyPerCycle = 1;

        /// <summary>
        /// Overrides Operation QtyPerCycle if UseQtyPerCycle is true.
        /// </summary>
        public decimal QtyPerCycle
        {
            get => qtyPerCycle;
            set
            {
                if (value > 0)
                {
                    qtyPerCycle = value;
                }
                else
                {
                    qtyPerCycle = DefaultQtyPerCycle;
                }
            }
        }

        /// <summary>
        /// If true then overrides the Operation QtyPerCycle when the Operation is scheduled.
        /// Otherwise the Operation QtyPerCycle is used as is.
        /// </summary>
        public bool UseQtyPerCycle
        {
            get => m_bools[useQtyPerCycleIdx];
            set => m_bools[useQtyPerCycleIdx] = value;
        }

        private TimeSpan headStartSpan;

        /// <summary>
        /// The Resource is only eligible to be assigned an Activity when the amount of slack time (JIT DateTime minus current time)
        /// is less than or equal to this amount.  Smaller values mean the Resource won't be used unless time is running out.
        /// </summary>
        public TimeSpan HeadStartSpan
        {
            get => headStartSpan;
            set => headStartSpan = value;
        }

        public bool UseHeadStartSpan
        {
            get => m_bools[useHeadStartSpanIdx];
            set => m_bools[useHeadStartSpanIdx] = value;
        }

        private UserFieldList m_userFields = new ();

        public UserFieldList UserFields
        {
            get => m_userFields;
            set => m_userFields = value;
        }

        public void SetUserFields(string userFieldStr)
        {
            m_userFields = new UserFieldList(userFieldStr);
        }

        private TimeSpan materialPostProcessingSpan;

        public TimeSpan MaterialPostProcessingSpan
        {
            get => materialPostProcessingSpan;
            set => materialPostProcessingSpan = value;
        }

        public bool UseMaterialPostProcessingSpan
        {
            get => m_bools[useMaterialPostProcessingSpanIdx];
            set => m_bools[useMaterialPostProcessingSpanIdx] = value;
        }

        private decimal m_minVolume;

        /// <summary>
        /// Overrides Resource min volume constraint.
        /// </summary>
        public decimal MinVolume
        {
            get => m_minVolume;
            set
            {
                if (value > 0)
                {
                    m_minVolume = value;
                }
            }
        }

        /// <summary>
        /// If true then overrides the Resource min volume constraint.
        /// Otherwise the Resource min volume constraint as is.
        /// </summary>
        public bool UseMinVolume
        {
            get => m_bools[c_useMinVolume];
            set => m_bools[c_useMinVolume] = value;
        }

        private decimal m_maxVolume;

        /// <summary>
        /// Overrides Resource max volume constraint.
        /// </summary>
        public decimal MaxVolume
        {
            get => m_maxVolume;
            set
            {
                if (value > 0)
                {
                    m_maxVolume = value;
                }
            }
        }

        /// <summary>
        /// If true then overrides the Resource max volume constraint.
        /// Otherwise the Resource min volume constraint as is.
        /// </summary>
        public bool UseMaxVolume
        {
            get => m_bools[c_useMaxVolume];
            set => m_bools[c_useMaxVolume] = value;
        }

        private decimal m_cleanoutUnitsRatio;

        /// <summary>
        /// Overrides Units Produced value if UseCleanoutUnits is true.
        /// </summary>
        public decimal CleanoutUnitsRatio
        {
            get => m_cleanoutUnitsRatio;
            set => m_cleanoutUnitsRatio = value;
        }

        /// <summary>
        /// If true then overrides Units Produced when calculating Resource Cleanout.
        /// Otherwise the Resource min volume constraint as is.
        /// </summary>
        public bool UseCleanoutUnits
        {
            get => m_bools[c_useCleanoutUnitsIdx];
            set => m_bools[c_useCleanoutUnitsIdx] = value;
        }

        private decimal m_minQty;

        /// <summary>
        /// Overrides Resource min qty constraint.
        /// </summary>
        public decimal MinQty
        {
            get => m_minQty;
            set
            {
                if (value > 0)
                {
                    m_minQty = value;
                }
            }
        }

        /// <summary>
        /// If true then overrides the Resource min qty constraint.
        /// Otherwise the Resource min qty constraint as is.
        /// </summary>
        public bool UseMinQty
        {
            get => m_bools[c_useMinQtyIdx];
            set => m_bools[c_useMinQtyIdx] = value;
        }

        private decimal m_maxQty;

        /// <summary>
        /// Overrides Resource max Qty constraint.
        /// </summary>
        public decimal MaxQty
        {
            get => m_maxQty;
            set
            {
                if (value > 0)
                {
                    m_maxQty = value;
                }
            }
        }

        /// <summary>
        /// If true then overrides the Resource max Qty constraint.
        /// Otherwise the Resource min Qty constraint as is.
        /// </summary>
        public bool UseMaxQty
        {
            get => m_bools[c_useMaxQtyIdx];
            set => m_bools[c_useMaxQtyIdx] = value;
        }
        
        private decimal m_transferQty;
        /// <summary>
        /// Overrides Resource max Qty constraint.
        /// </summary>
        public decimal TransferQty
        {
            get => m_transferQty;
            set
            {
                if (value > 0)
                {
                    m_transferQty = value;
                }
            }
        }

        /// <summary>
        /// If true then overrides the Resource max Qty constraint.
        /// Otherwise the Resource min Qty constraint as is.
        /// </summary>
        public bool UseTransferQty
        {
            get => m_bools[c_useTransferQtyIdx];
            set => m_bools[c_useTransferQtyIdx] = value;
        }

        private int m_priority;
        /// <summary>
        /// Used to determine a priority for scheduling on this Resource
        /// </summary>
        public int Priority
        {
            get => m_priority;
            set => m_priority = value;
        }

        public bool UsePriority
        {
            get => m_bools[c_usePriorityIdx];
            set => m_bools[c_usePriorityIdx] = value;
        }
        #endregion

        #region Key Properties
        public string GetKey()
        {
            return GetKey(ProductItemExternalId, PlantExternalId, DepartmentExternalId, resourceExternalId, ProductCode);
        }

        public static string GetKey(string aProductItemExternalId, string aPlantExternalId, string aDepartmentExternalId, string aResourceExternalId, string aOperationName)
        {
            return string.Format("{0}$%{1}*&{2}^*{3}^&*{4}", aProductItemExternalId, aPlantExternalId, aDepartmentExternalId, aResourceExternalId, aOperationName);
        }

        private string productItemExternalId;

        public string ProductItemExternalId
        {
            get => productItemExternalId;
            set => productItemExternalId = value;
        }

        private string plantExternalId;

        public string PlantExternalId
        {
            get => plantExternalId;
            set => plantExternalId = value;
        }

        private string deptExternalId;

        public string DepartmentExternalId
        {
            get => deptExternalId;
            set => deptExternalId = value;
        }

        private string resourceExternalId;

        public string ResourceExternalId
        {
            get => resourceExternalId;
            set => resourceExternalId = value;
        }
        #endregion Key Properties

        #region Validation
        public void Validate()
        {
            if (CycleSpan.Ticks <= 0 && UseCycleSpan)
            {
                throw new APSCommon.PTValidationException("2110");
            }

            if (PlanningScrapPercent >= 100 || PlanningScrapPercent < 0)
            {
                throw new APSCommon.PTValidationException("2111");
            }

            if (SetupSpan.Ticks < 0)
            {
                throw new APSCommon.PTValidationException("2112");
            }

            if (PostProcessingSpan.Ticks < 0)
            {
                throw new APSCommon.PTValidationException("2113");
            }

            if (QtyPerCycle <= 0)
            {
                throw new APSCommon.PTValidationException("2114");
            }

            if (MaterialPostProcessingSpan.Ticks < 0)
            {
                throw new APSCommon.PTValidationException("2980");
            }

            if (CleanSpan.Ticks < 0)
            {
                throw new APSCommon.PTValidationException("3048");
            }

            if (CleanoutUnitsRatio < 0)
            {
                throw new APSCommon.PTValidationException("3049");
            }
        }
        #endregion Validation
    }
    #endregion
}