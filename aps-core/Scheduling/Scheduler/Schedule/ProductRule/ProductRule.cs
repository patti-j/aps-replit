using System.Data;
using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Database;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Defines the special values to be applied to Activities scheduled on a particular Resource for a particular Item Operation.
/// </summary>
public class ProductRule : IPTSerializable, IComparable<ProductRule>, IKey<ProductRuleKey>
{
    #region IPTSerializable Members
    public ProductRule(IReader a_reader)
    {
        m_restoreInfo = new RestoreInfo();

        decimal qtyPerCycle;

        if (a_reader.VersionNumber >= 12528)
        {
            m_resourceId = new BaseId(a_reader);
            m_itemId = new BaseId(a_reader);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_cleanoutCost);
            a_reader.Read(out m_storageSpanTicks);
        }
        else if (a_reader.VersionNumber >= 12526)
        {
            m_resourceId = new BaseId(a_reader);
            m_itemId = new BaseId(a_reader);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_cleanoutCost);
        }
        else if (a_reader.VersionNumber >= 12504)
        {
            m_resourceId = new BaseId(a_reader);
            m_itemId = new BaseId(a_reader);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_priority);
        }
        else if (a_reader.VersionNumber >= 12500)
        {
            m_resourceId = new BaseId(a_reader);
            m_itemId = new BaseId(a_reader);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_transferQty);
        }
        else if (a_reader.VersionNumber >= 12420)
        {
            m_resourceId = new BaseId(a_reader);
            m_itemId = new BaseId(a_reader);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_priority);
        }
        else if (a_reader.VersionNumber >= 12419)
        {
            m_resourceId = new BaseId(a_reader);
            m_itemId = new BaseId(a_reader);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_transferQty);
        }
        else if (a_reader.VersionNumber >= 12408)
        {
            m_resourceId = new BaseId(a_reader);
            m_itemId = new BaseId(a_reader);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
        }        
        else if (a_reader.VersionNumber >= 12402)
        {
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_restoreInfo._productExternalId);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
        }
        else if (a_reader.VersionNumber >= 12401)
        {
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_restoreInfo._productExternalId);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);
        }
        else if (a_reader.VersionNumber >= 12326)
        {
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_restoreInfo._productExternalId);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);

            a_reader.Read(out m_minQty);
            a_reader.Read(out m_maxQty);
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_restoreInfo._productExternalId);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);

            a_reader.Read(out m_cleanoutUnitsRatio);
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_restoreInfo._productExternalId);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
        }
        else if (a_reader.VersionNumber >= 12106)
        {
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_restoreInfo._productExternalId);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);

            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
        }
        else if (a_reader.VersionNumber >= 713)
        {
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_planningScrapPercent);

            a_reader.Read(out qtyPerCycle);
            QtyPerCycle = qtyPerCycle;

            a_reader.Read(out m_headStartSpanTicks);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_restoreInfo._productExternalId);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            m_productRulesUserFields = new UserFieldList(a_reader);
        }
    }

    private class RestoreInfo
    {
        internal string _productExternalId;
        internal BaseId _productBaseId;
    }

    private RestoreInfo m_restoreInfo;

    /// <summary>
    /// For Backwards Compatibility
    /// </summary>
    /// <param name="a_aItems"></param>
    internal void RestoreReferences(ItemManager a_aItems)
    {
        Item itemRef;
        if (m_restoreInfo._productExternalId != null)
        {
            itemRef = a_aItems.GetByExternalId(m_restoreInfo._productExternalId);
        }
        else
        {
            itemRef = a_aItems.GetById(m_restoreInfo._productBaseId);
        }

        m_itemId = itemRef.Id;

        m_restoreInfo = null;
    }

    public void Serialize(IWriter a_writer)
    {
        m_resourceId.Serialize(a_writer);
        m_itemId.Serialize(a_writer);
        a_writer.Write(m_productCode);
        a_writer.Write(m_cycleSpanTicks);
        a_writer.Write(m_SetupSpanTicks);
        a_writer.Write(m_postProcessingSpanTicks);
        a_writer.Write(m_cleanSpanTicks);
        a_writer.Write(m_planningScrapPercent);
        a_writer.Write(QtyPerCycle);
        a_writer.Write(m_headStartSpanTicks);
        m_bools.Serialize(a_writer);

        a_writer.Write(m_materialPostProcessingSpanTicks);
        m_productRulesUserFields.Serialize(a_writer);

        a_writer.Write(m_minVolume);
        a_writer.Write(m_maxVolume);

        a_writer.Write(m_cleanoutUnitsRatio);

        a_writer.Write(m_minQty);
        a_writer.Write(m_maxQty);

        a_writer.Write(m_transferQty);
        a_writer.Write(m_priority);
        a_writer.Write(m_productionSetupCost);
        a_writer.Write(m_cleanoutCost);
        a_writer.Write(m_storageSpanTicks);
    }

    public const int UNIQUE_ID = 606;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ProductRule(ScenarioDetail a_sd, PT.ERPTransmissions.ProductRulesT.ProductRule a_prTRule, InternalResource a_res, Item a_item)
    {
        m_resourceId = a_res.Id;
        m_itemId = a_item.Id;
        Update(a_sd, a_prTRule);
    }

    internal void Update(ScenarioDetail a_sd, PT.ERPTransmissions.ProductRulesT.ProductRule a_prTRule)
    {
        CycleSpan = a_prTRule.CycleSpan;
        UseCycleSpan = a_prTRule.UseCycleSpan;
        SetupSpan = a_prTRule.SetupSpan;
        UseSetupSpan = a_prTRule.UseSetupSpan;
        PostProcessingSpan = a_prTRule.PostProcessingSpan;
        CleanSpan = a_prTRule.CleanSpan;
        UsePostProcessingSpan = a_prTRule.UsePostProcessingSpan;
        UseCleanSpan = a_prTRule.UseCleanSpan;
        PlanningScrapPercent = a_prTRule.PlanningScrapPercent;
        QtyPerCycle = a_sd.ScenarioOptions.RoundQty(a_prTRule.QtyPerCycle);
        MinVolume = a_prTRule.MinVolume;
        UseMinVolume = a_prTRule.UseMinVolume;
        MaxVolume = a_prTRule.MaxVolume;
        UseMaxVolume = a_prTRule.UseMaxVolume;
        UsePlanningScrapPercent = a_prTRule.UsePlanningScrapPercent;
        HeadStartSpan = a_prTRule.HeadStartSpan;
        UseQtyPerCycle = a_prTRule.UseQtyPerCycle;
        UseHeadStartSpan = a_prTRule.UseHeadStartSpan;
        MaterialPostProcessingSpan = a_prTRule.MaterialPostProcessingSpan;
        UseMaterialPostProcessingSpan = a_prTRule.UseMaterialPostProcessingSpan;
        UserFields = a_prTRule.UserFields.GetUserFieldImportString();
        ProductCode = string.IsNullOrWhiteSpace(a_prTRule.ProductCode) ? string.Empty : a_prTRule.ProductCode;

        UseCleanoutUnits = a_prTRule.UseCleanoutUnits;
        CleanoutUnitsRatio = a_prTRule.CleanoutUnitsRatio;
        UseMinQty = a_prTRule.UseMinQty;
        MinQty = a_prTRule.MinQty;
        UseMaxQty = a_prTRule.UseMaxQty;
        MaxQty = a_prTRule.MaxQty;
        TransferQty = a_prTRule.TransferQty;
        UseTransferQty = a_prTRule.UseTransferQty;

        Priority = a_prTRule.Priority;
        UsePriority = a_prTRule.UsePriority;

        ProductionSetupCost = a_prTRule.ProductionSetupCost;
        UseProductionSetupCost = a_prTRule.UseProductionSetupCost;

        CleanoutCost = a_prTRule.CleanoutCost;
        UseCleanoutCost = a_prTRule.UseCleanoutCost;

        StorageSpan = a_prTRule.StorageSpan;
        UseStorageSpan = a_prTRule.UseStorageSpan;
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

    private string m_productCode;

    private BaseId m_resourceId;
    private BaseId m_itemId;

    /// <summary>
    /// If specified, then these rules only apply to Operations with this Product Code.  Otherwise, these rules apply to all
    /// Operations for any Manufacturing Order making this Product.
    /// </summary>
    public string ProductCode
    {
        get => m_productCode;
        private set
        {
            if (value == null)
            {
                m_productCode = "";
            }
            else
            {
                m_productCode = value;
            }
        }
    }

    private long m_cycleSpanTicks;

    /// <summary>
    /// Overrides Operation CycleSpan if UseCycleSpan is true.
    /// </summary>
    public TimeSpan CycleSpan
    {
        get => new (m_cycleSpanTicks);
        private set => m_cycleSpanTicks = value.Ticks;
    }

    internal long CycleSpanTicks
    {
        get => m_cycleSpanTicks;
        private set => m_cycleSpanTicks = value;
    }

    /// <summary>
    /// If true then overrides the Operation CycleSpan when the Operation is scheduled.
    /// Otherwise the Operation CycleSpan is used as is.
    /// </summary>
    public bool UseCycleSpan
    {
        get => m_bools[useCycleSpanIdx];
        private set => m_bools[useCycleSpanIdx] = value;
    }
    
    private long m_storageSpanTicks;

    /// <summary>
    /// Overrides Operation StorageSpan if UseStorageSpan is true.
    /// </summary>
    public TimeSpan StorageSpan
    {
        get => new (m_storageSpanTicks);
        private set => m_storageSpanTicks = value.Ticks;
    }

    internal long StorageSpanTicks
    {
        get => m_storageSpanTicks;
        private set => m_storageSpanTicks = value;
    }

    /// <summary>
    /// If true then overrides the Operation StorageSpan when the Operation is scheduled.
    /// Otherwise the Operation StorageSpan is used as is.
    /// </summary>
    public bool UseStorageSpan
    {
        get => m_bools[c_useStorageSpanIdx];
        private set => m_bools[c_useStorageSpanIdx] = value;
    }

    private long m_setupSpanTicks;

    /// <summary>
    /// Overrides Operation SetupSpan if UseSetupSpan is true.
    /// </summary>
    public TimeSpan SetupSpan
    {
        get => new (m_setupSpanTicks);
        private set => m_setupSpanTicks = value.Ticks;
    }

    internal long m_SetupSpanTicks
    {
        get => m_setupSpanTicks;
        set => m_setupSpanTicks = value;
    }

    /// <summary>
    /// If true then overrides the Operation SetupSpan when the Operation is scheduled.
    /// Otherwise the Operation SetupSpan is used as is.
    /// </summary>
    public bool UseSetupSpan
    {
        get => m_bools[useSetupSpanIdx];
        private set => m_bools[useSetupSpanIdx] = value;
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
        private set => m_bools[c_useProductionSetupCostIdx] = value;
    }

    private long m_postProcessingSpanTicks;

    /// <summary>
    /// Overrides Operation PostProcessingSpan if UsePostProcessingSpan is true.
    /// </summary>
    public TimeSpan PostProcessingSpan
    {
        get => new (m_postProcessingSpanTicks);
        private set => m_postProcessingSpanTicks = value.Ticks;
    }

    internal long PostProcessingSpanTicks
    {
        get => m_postProcessingSpanTicks;
        set => m_postProcessingSpanTicks = value;
    }

    /// <summary>
    /// If true then overrides the Operation PostProcessingSpan when the Operation is scheduled.
    /// Otherwise the Operation PostProcessingSpan is used as is.
    /// </summary>
    public bool UsePostProcessingSpan
    {
        get => m_bools[usePostProcessingSpanIdx];
        private set => m_bools[usePostProcessingSpanIdx] = value;
    }

    private long m_cleanSpanTicks;

    /// <summary>
    /// Overrides Operation Clean SPan if UseCleanSpan is true.
    /// </summary>
    public TimeSpan CleanSpan
    {
        get => new (m_cleanSpanTicks);
        private set => m_cleanSpanTicks = value.Ticks;
    }

    internal long CleanSpanTicks
    {
        get => m_cleanSpanTicks;
        set => m_cleanSpanTicks = value;
    }

    /// <summary>
    /// If true then overrides the Operation Clean Span when the Operation is scheduled.
    /// Otherwise the Operation Clean Span is used as is.
    /// </summary>
    public bool UseCleanSpan
    {
        get => m_bools[useCleanSpanIdx];
        private set => m_bools[useCleanSpanIdx] = value;
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
        private set => m_bools[c_useCleanoutCostIdx] = value;
    }

    private decimal m_planningScrapPercent;

    /// <summary>
    /// Overrides Operation PlanningScrapPercent if UseScrapPercent is true.
    /// </summary>
    public decimal PlanningScrapPercent
    {
        get => m_planningScrapPercent;
        private set => m_planningScrapPercent = value;
    }

    /// <summary>
    /// If true then overrides the Operation PlanningScrapPercent when the Operation is scheduled.
    /// Otherwise the Operation ScrapPercent is used as is.
    /// </summary>
    public bool UsePlanningScrapPercent
    {
        get => m_bools[usePlanningScrapPercentIdx];
        private set => m_bools[usePlanningScrapPercentIdx] = value;
    }

    private const decimal c_defaultQtyPerCycle = ERPTransmissions.ProductRulesT.ProductRule.DefaultQtyPerCycle;
    private decimal m_qtyPerCycle = c_defaultQtyPerCycle;

    /// <summary>
    /// Overrides Operation QtyPerCycle if UseQtyPerCycle is true.
    /// </summary>
    public decimal QtyPerCycle
    {
        get => m_qtyPerCycle;
        private set
        {
            if (value > 0)
            {
                m_qtyPerCycle = value;
            }
            else
            {
                m_qtyPerCycle = ERPTransmissions.ProductRulesT.ProductRule.DefaultQtyPerCycle;
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
        private set => m_bools[useQtyPerCycleIdx] = value;
    }

    private long m_headStartSpanTicks;

    /// <summary>
    /// The Resource is only eligible to be assigned an Activity when the amount of slack time (JIT DateTime minus current time)
    /// is less than or equal to this amount.  Smaller values mean the Resource won't be used unless time is running out.
    /// </summary>
    public TimeSpan HeadStartSpan
    {
        get => new (m_headStartSpanTicks);
        private set => m_headStartSpanTicks = value.Ticks;
    }

    internal long HeadStartSpanTicks => m_headStartSpanTicks;

    public bool UseHeadStartSpan
    {
        get => m_bools[useHeadStartSpanIdx];
        private set => m_bools[useHeadStartSpanIdx] = value;
    }

    private UserFieldList m_productRulesUserFields = new ();

    public UserFieldList ProductRulesUserFields
    {
        get => m_productRulesUserFields;
    }

    public string UserFields
    {
        get => m_productRulesUserFields.GetUserFieldImportString();
        set => m_productRulesUserFields = new UserFieldList(value);
    }

    private long m_materialPostProcessingSpanTicks;

    /// <summary>
    /// The Resource is only eligible to be assigned an Activity when the amount of slack time (JIT DateTime minus current time)
    /// is less than or equal to this amount.  Smaller values mean the Resource won't be used unless time is running out.
    /// </summary>
    public TimeSpan MaterialPostProcessingSpan
    {
        get => new (m_materialPostProcessingSpanTicks);
        private set => m_materialPostProcessingSpanTicks = value.Ticks;
    }

    internal long MaterialPostProcessingSpanTicks => m_materialPostProcessingSpanTicks;

    public bool UseMaterialPostProcessingSpan
    {
        get => m_bools[useMaterialPostProcessingSpanIdx];
        private set => m_bools[useMaterialPostProcessingSpanIdx] = value;
    }

    private decimal m_minVolume;

    /// <summary>
    /// Overrides Resource min volume constraint.
    /// </summary>
    public decimal MinVolume
    {
        get => m_minVolume;
        private set
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
        private set => m_bools[c_useMinVolume] = value;
    }

    private decimal m_maxVolume;

    /// <summary>
    /// Overrides Resource max volume constraint.
    /// </summary>
    public decimal MaxVolume
    {
        get => m_maxVolume;
        private set
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
        private set => m_bools[c_useMaxVolume] = value;
    }

    private decimal m_minQty;

    /// <summary>
    /// Overrides Resource min qty constraint.
    /// </summary>
    public decimal MinQty
    {
        get => m_minQty;
        private set
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
        private set => m_bools[c_useMinQtyIdx] = value;
    }

    private decimal m_maxQty;

    /// <summary>
    /// Overrides Resource max Qty constraint.
    /// </summary>
    public decimal MaxQty
    {
        get => m_maxQty;
        private set
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
        private set => m_bools[c_useMaxQtyIdx] = value;
    }

    private decimal m_cleanoutUnitsRatio;

    /// <summary>
    /// Overrides the number of Units Produced from a product for use in Cleanout Calculations,
    /// if UseCleanoutUnits is true.
    /// </summary>
    public decimal CleanoutUnitsRatio
    {
        get => m_cleanoutUnitsRatio;
        private set => m_cleanoutUnitsRatio = value;
    }

    /// <summary>
    /// If true then overrides Units Produced when calculating Resource Cleanout.
    /// </summary>
    public bool UseCleanoutUnits
    {
        get => m_bools[c_useCleanoutUnitsIdx];
        private set => m_bools[c_useCleanoutUnitsIdx] = value;
    }

    private decimal m_transferQty;
    /// <summary>
    /// Overrides Resource max Qty constraint.
    /// </summary>
    public decimal TransferQty
    {
        get => m_transferQty;
        private set
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
        private set => m_bools[c_useTransferQtyIdx] = value;
    }

    private int m_priority;
    /// <summary>
    /// Used to determine a priority for scheduling on this Resource
    /// </summary>
    public int Priority
    {
        get => m_priority;
        internal set => m_priority = value;
    }

    public bool UsePriority
    {
        get => m_bools[c_usePriorityIdx];
        private set => m_bools[c_usePriorityIdx] = value;
    }
    #endregion

    #region Properties

    public BaseId ResourceId => m_resourceId;
    public BaseId ItemId => m_itemId;
    #endregion

    
    public int CompareTo(ProductRule a_other)
    {
        int resIdCompare = a_other.m_resourceId.CompareTo(a_other.m_resourceId);
        if (resIdCompare != 0)
        {
            return resIdCompare;
        }

        int itemIdCompare = a_other.m_itemId.CompareTo(a_other.m_itemId);
        if (itemIdCompare != 0)
        {
            return resIdCompare;
        }

        return String.Compare(a_other.m_productCode, a_other.m_productCode, StringComparison.Ordinal);
    }

    public bool Equals(ProductRuleKey a_other)
    {
        return a_other.Equals(GetKey());
    }

    public ProductRuleKey GetKey()
    {
        return new ProductRuleKey(this);
    }

    public override string ToString()
    {
        StringBuilder sb = new ();
        string productCode;
        if (m_productCode.Length > 0)
        {
            productCode = m_productCode;
        }
        else
        {
            productCode = "{empty string}";
        }

        sb.Append("ProductCode=");
        sb.Append(productCode);
        sb.Append(";");

        if (UseCycleSpan)
        {
            sb.Append("UseCycleSpan;");
        }

        if (UseHeadStartSpan)
        {
            sb.Append("UseHeadStartSpan;");
        }

        if (UsePlanningScrapPercent)
        {
            sb.Append("UsePlanningScrapPercent;");
        }

        if (UsePostProcessingSpan)
        {
            sb.Append("UsePostProcessingSpan;");
        }

        if (UseCleanSpan)
        {
            sb.Append("UseCleanSpan;");
        }

        if (UseQtyPerCycle)
        {
            sb.Append("UseQtyPerCycle;");
        }

        if (UseSetupSpan)
        {
            sb.Append("UseSetupSpan;");
        }

        if (UseMaterialPostProcessingSpan)
        {
            sb.Append("UseMaterialPostProcessingSpan;");
        }

        if (UseMinVolume)
        {
            sb.Append("UseMinVolume;");
        }

        if (UseMaxVolume)
        {
            sb.Append("UseMaxVolume;");
        }

        if (UsePriority)
        {
            sb.Append("UsePriority;");
        }

        if (UseProductionSetupCost)
        {
            sb.Append("UseProductionSetupCost");
        }

        if (UseCleanoutCost)
        {
            sb.Append("UseCleanoutCost");
        }

        return sb.ToString();
    }

    public object GetUserFieldValue(UserField userField, ScenarioDetail a_sd)
    {
        object v = null;

        if (userField.DataType == typeof(DateTime))
        {
            v = SQLServerConversions.GetValidDateTime((DateTime)userField.DataValue);
        }
        else if (userField.DataType == typeof(double))
        {
            v = a_sd.ScenarioOptions.RoundQty((double)userField.DataValue);
        }
        else if (userField.DataType == typeof(decimal))
        {
            v = a_sd.ScenarioOptions.RoundQty((decimal)userField.DataValue);
        }
        else if (userField.DataType == typeof(TimeSpan))
        {
            v = ((TimeSpan)userField.DataValue).Ticks;
        }
        else
        {
            v = userField.DataValue;
        }

        return v;
    }

    internal void SetBackwardsCompatibilityResourceId(BaseId a_id)
    {
        m_resourceId = a_id;
    }
}