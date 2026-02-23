using PT.APSCommon;
using PT.Database;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Scheduler.AlternatePaths;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using System.ComponentModel;

using PT.Common.Attributes;

namespace PT.Scheduler;

/// <summary>
/// The class from which all Operations derive.
/// </summary>
public abstract partial class BaseOperation : BaseObject, IPTSerializable
{
    #region IPTSerializable Members
    protected BaseOperation(IReader a_reader, BaseIdGenerator a_idGenerator)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 13009) //Removed jitStartTicks moved to ActivityResourceBufferInfo
        {
            m_needInfo = new OperationNeedInfo(a_reader);

            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_onHold);
            a_reader.Read(out m_isRework);
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_commitStartDate);
            a_reader.Read(out m_commitEndDate);
            m_bools = new BoolVector32(a_reader);

            m_materialRequirementsCollection = new MaterialRequirementsCollection(a_reader, a_idGenerator);
            m_products = new ProductsCollection(a_reader, a_idGenerator);

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(a_reader);
            m_productionInfo = new BaseOperationProductionInfo(a_reader);
            a_reader.Read(out m_standardOperationBufferTicks);

            a_reader.Read(out m_plannedScrapQty);

            m_attributesCollection = new AttributesCollection(a_reader);
            m_transferInfo = new TransferInfoCollection(a_reader);
        }
        else if (a_reader.VersionNumber >= 13007) //Removed jitStartTicksForTransferSpanBeforeStartOfPredecessor
        {
            m_needInfo = new OperationNeedInfo(a_reader); 

            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_onHold);
            a_reader.Read(out m_isRework);
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_commitStartDate);
            a_reader.Read(out m_commitEndDate);
            m_bools = new BoolVector32(a_reader);

            m_materialRequirementsCollection = new MaterialRequirementsCollection(a_reader, a_idGenerator);
            m_products = new ProductsCollection(a_reader, a_idGenerator);

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(a_reader);
            m_productionInfo = new BaseOperationProductionInfo(a_reader);
            a_reader.Read(out m_standardOperationBufferTicks);

            a_reader.Read(out m_plannedScrapQty);

            m_attributesCollection = new AttributesCollection(a_reader);
            m_transferInfo = new TransferInfoCollection(a_reader);
            a_reader.Read(out long jitBufferTicks);
        }
        else if (a_reader.VersionNumber >= 13005) //Removed Jit start date and need date. Now stored in dbrNeedInfo
        {
            m_needInfo = new OperationNeedInfo(a_reader); //13005

            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_onHold);
            a_reader.Read(out m_isRework);
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out long jitStartTicksForTransferSpanBeforeStartOfPredecessor);
            a_reader.Read(out m_commitStartDate);
            a_reader.Read(out m_commitEndDate);
            m_bools = new BoolVector32(a_reader);

            m_materialRequirementsCollection = new MaterialRequirementsCollection(a_reader, a_idGenerator);
            m_products = new ProductsCollection(a_reader, a_idGenerator);

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(a_reader);
            m_productionInfo = new BaseOperationProductionInfo(a_reader);
            a_reader.Read(out m_standardOperationBufferTicks);

            a_reader.Read(out m_plannedScrapQty);

            m_attributesCollection = new AttributesCollection(a_reader);
            m_transferInfo = new TransferInfoCollection(a_reader);
            a_reader.Read(out long jitBufferTicks);
        }
        else if (a_reader.VersionNumber >= 12536)
        {
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_onHold);
            a_reader.Read(out m_isRework);
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out long jitStartDateTicks);
            a_reader.Read(out long jitStartTicksForTransferSpanBeforeStartOfPredecessor);
            a_reader.Read(out long needDateTicks);
            a_reader.Read(out m_commitStartDate);
            a_reader.Read(out m_commitEndDate);
            m_bools = new BoolVector32(a_reader);

            m_materialRequirementsCollection = new MaterialRequirementsCollection(a_reader, a_idGenerator);
            m_products = new ProductsCollection(a_reader, a_idGenerator);

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(a_reader);
            m_productionInfo = new BaseOperationProductionInfo(a_reader);
            a_reader.Read(out m_standardOperationBufferTicks);

            a_reader.Read(out long dbrNeedTicks);
            a_reader.Read(out long dbrJITStartTicks);
            a_reader.Read(out m_plannedScrapQty);

            m_attributesCollection = new AttributesCollection(a_reader);
            m_transferInfo = new TransferInfoCollection(a_reader);
        }
        else if (a_reader.VersionNumber >= 12404)
        {
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_onHold);
            a_reader.Read(out m_isRework);
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out long jitStartDateTicks);
            a_reader.Read(out long jitStartTicksForTransferSpanBeforeStartOfPredecessor);
            a_reader.Read(out long needDateTicks);
            a_reader.Read(out m_commitStartDate);
            a_reader.Read(out m_commitEndDate);
            m_bools = new BoolVector32(a_reader);

            m_materialRequirementsCollection = new MaterialRequirementsCollection(a_reader, a_idGenerator);
            m_products = new ProductsCollection(a_reader, a_idGenerator);

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(a_reader);
            m_productionInfo = new BaseOperationProductionInfo(a_reader);
            a_reader.Read(out m_standardOperationBufferTicks);

            a_reader.Read(out long dbrNeedTicks);
            a_reader.Read(out long dbrJITStartTicks);
            a_reader.Read(out m_plannedScrapQty);

            m_attributesCollection = new AttributesCollection(a_reader);
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            a_reader.Read(out int val); // Use to be ConstraintType, and it's been removed
            a_reader.Read(out m_holdReason);
            a_reader.Read(out val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_onHold);
            a_reader.Read(out m_isRework);
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out long jitStartDateTicks);
            a_reader.Read(out long jitStartTicksForTransferSpanBeforeStartOfPredecessor);
            a_reader.Read(out long needDateTicks);
            a_reader.Read(out m_commitStartDate);
            a_reader.Read(out m_commitEndDate);
            m_bools = new BoolVector32(a_reader);

            m_materialRequirementsCollection = new MaterialRequirementsCollection(a_reader, a_idGenerator);
            m_products = new ProductsCollection(a_reader, a_idGenerator);

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(a_reader);
            m_productionInfo = new BaseOperationProductionInfo(a_reader);
            a_reader.Read(out m_standardOperationBufferTicks);

            a_reader.Read(out long dbrNeedTicks);
            a_reader.Read(out long dbrJITStartTicks);
            a_reader.Read(out m_plannedScrapQty);

            m_attributesCollection = new AttributesCollection(a_reader);
        }
        else if (a_reader.VersionNumber >= 675)
        {
            a_reader.Read(out int val); // Use to be ConstraintType, and it's been removed
            a_reader.Read(out m_holdReason);
            a_reader.Read(out val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_onHold);
            a_reader.Read(out m_isRework);
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out long jitStartDateTicks);
            a_reader.Read(out long jitStartTicksForTransferSpanBeforeStartOfPredecessor);
            a_reader.Read(out long needDateTicks);
            a_reader.Read(out m_commitStartDate);
            a_reader.Read(out m_commitEndDate);
            m_bools = new BoolVector32(a_reader);

            m_materialRequirementsCollection = new MaterialRequirementsCollection(a_reader, a_idGenerator);
            m_products = new ProductsCollection(a_reader, a_idGenerator);

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(a_reader);
            m_productionInfo = new BaseOperationProductionInfo(a_reader);
            a_reader.Read(out m_standardOperationBufferTicks);

            a_reader.Read(out long dbrNeedTicks);
            a_reader.Read(out long dbrJITStartTicks);
            a_reader.Read(out m_plannedScrapQty);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_needInfo.Serialize(a_writer);
        a_writer.Write(m_holdReason);
        a_writer.Write((int)m_omitted);
        a_writer.Write(m_onHold);
        a_writer.Write(m_isRework);
        a_writer.Write(m_requiredFinishQty);
        a_writer.Write(m_uom);
        a_writer.Write(m_outputName);
        a_writer.Write(m_holdUntilDate);
        a_writer.Write(m_commitStartDate);
        a_writer.Write(m_commitEndDate);
        m_bools.Serialize(a_writer);

        m_materialRequirementsCollection.Serialize(a_writer);
        m_products.Serialize(a_writer);
        m_finishedPredecessorMOReleaseInfoManager.Serialize(a_writer);
        m_productionInfo.Serialize(a_writer);
        a_writer.Write(m_standardOperationBufferTicks);

        a_writer.Write(m_plannedScrapQty);
        m_attributesCollection.Serialize(a_writer);
        m_transferInfo.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 302;
    public override int UniqueId => UNIQUE_ID;

    internal virtual void RestoreReferences(BaseOperationManager a_opManager, ManufacturingOrder a_mo, WarehouseManager a_warehouses, ItemManager a_items, SalesOrderManager a_salesOrderManager, TransferOrderManager a_transferOrderManager, AttributeManager a_attributeManager, BaseIdGenerator a_idGen, PlantManager a_plantManager, CapabilityManager a_capabilityManager)
    {
        m_manufacturingOrder = a_mo;
        m_materialRequirementsCollection.RestoreReferences(a_warehouses, a_items, this);
        m_products.RestoreReferences(a_opManager, a_warehouses, a_items, a_salesOrderManager, a_transferOrderManager, a_idGen);
        Attributes.RestoreReferences(a_attributeManager);

        if (m_commitStartDate > PTDateTime.MaxDateTimeTicks)
        {
            m_commitStartDate = PTDateTime.MaxDateTimeTicks;
        }

        if (m_commitEndDate > PTDateTime.MaxDateTimeTicks)
        {
            m_commitEndDate = PTDateTime.MaxDateTimeTicks;
        }

        if (m_holdUntilDate > PTDateTime.MaxDateTimeTicks)
        {
            m_holdUntilDate = PTDateTime.MaxDateTimeTicks;
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.ResourceOperations);
    }
    #endregion

    #region Construction
    internal BaseOperation(BaseId a_id, ManufacturingOrder a_manufacturingOrder, JobT.BaseOperation a_baseOperation, ScenarioDetail a_sd, BaseIdGenerator a_idGen, UserFieldDefinitionManager a_udfManager, bool a_autoDeleteOperationAttributes, IScenarioDataChanges a_dataChanges)
        : base(a_id, a_baseOperation, a_udfManager, UserField.EUDFObjectType.ResourceOperations)
    {
        m_manufacturingOrder = a_manufacturingOrder;
        m_materialRequirementsCollection = new(a_idGen);

        if (a_baseOperation.HoldUntilDateTimeSet)
        {
            HoldUntil = a_baseOperation.HoldUntilDateTime;
        }

        if (a_baseOperation.HoldReasonSet)
        {
            HoldReason = a_baseOperation.HoldReason;
        }

        if (a_baseOperation.OnHoldSet)
        {
            OnHold = a_baseOperation.OnHold;
        }

        if (a_baseOperation.OmittedSet)
        {
            m_omitted = a_baseOperation.Omitted;
        }

        if (a_baseOperation.IsReworkSet)
        {
            IsRework = a_baseOperation.IsRework;
        }

        if (a_baseOperation.RequiredFinishQtySet)
        {
            RequiredFinishQty = a_sd.ScenarioOptions.RoundQty(a_baseOperation.RequiredFinishQty);
        }

        if (a_baseOperation.UOMSet)
        {
            UOM = a_baseOperation.UOM;
        }

        if (a_baseOperation.OutputNameSet)
        {
            m_outputName = a_baseOperation.OutputName;
        }

        if (a_baseOperation.UseExpectedFinishQtySet)
        {
            UseExpectedFinishQty = a_baseOperation.UseExpectedFinishQty;
        }

        if (a_baseOperation.AutoReportProgressSet)
        {
            AutoReportProgress = a_baseOperation.AutoReportProgress;
        }

        if (a_baseOperation.AutoFinishSet)
        {
            AutoFinish = a_baseOperation.AutoFinish;
        }

        if (a_baseOperation.CommitStartDateSet)
        {
            CommitStartDate = a_baseOperation.CommitStartDate;
        }

        if (a_baseOperation.CommitEndDateSet)
        {
            CommitEndDate = a_baseOperation.CommitEndDate;
        }

        if (a_baseOperation.MinOperationBufferSet)
        {
            StandardOperationBufferTicks = a_baseOperation.MinOperationBufferTicks;
        }

        if (a_baseOperation.PlannedScrapQtySet)
        {
            PlannedScrapQty = a_baseOperation.PlannedScrapQty;
        }

        ProductionInfoBaseOperation.OnlyAllowManualupdates.Materials = a_baseOperation.ProductionInfoFlagsBaseOperation.Materials;
        ProductionInfoBaseOperation.OnlyAllowManualupdates.Products = a_baseOperation.ProductionInfoFlagsBaseOperation.Products;

        UpdateAttributes(a_sd.AttributeManager, a_baseOperation.ResourceAttributes, false, a_autoDeleteOperationAttributes, a_dataChanges);
        //AllowMoveToOverrideConnectorTransferTicks = true;
    }

    /// <summary>
    /// Create a BaseOperation from an existing Operation.  Used for BreakOffs.
    /// </summary>
    /// <param name="a_sourceOp"></param>
    internal BaseOperation(BaseId a_newId, BaseOperation a_sourceOp, ManufacturingOrder a_parentMo, BaseIdGenerator a_idGen)
        : base(a_newId, a_sourceOp)
    {
        m_manufacturingOrder = a_parentMo;
        HoldUntil = a_sourceOp.HoldUntil;
        HoldReason = a_sourceOp.HoldReason;
        m_omitted = a_sourceOp.Omitted;
        OnHold = a_sourceOp.OnHold;
        IsRework = a_sourceOp.IsRework;
        RequiredFinishQty = a_sourceOp.RequiredFinishQty;
        UOM = a_sourceOp.UOM;
        m_outputName = a_sourceOp.OutputName;
        UseExpectedFinishQty = a_sourceOp.UseExpectedFinishQty;
        CommitStartDate = a_sourceOp.CommitStartDate;
        CommitEndDate = a_sourceOp.CommitEndDate;
        StandardOperationBufferTicks = a_sourceOp.StandardOperationBufferTicks;
        PlannedScrapQty = a_sourceOp.PlannedScrapQty;

        //Copy Materials
        m_materialRequirementsCollection = new MaterialRequirementsCollection(a_idGen);
        for (int i = 0; i < a_sourceOp.MaterialRequirements.Count; i++)
        {
            MaterialRequirement sourceMr = a_sourceOp.MaterialRequirements[i];
            MaterialRequirement mr = new(sourceMr.Id, sourceMr.ExternalId, sourceMr, a_idGen);
            MaterialRequirements.Add(mr);
        }

        //Copy Products
        Products = new ProductsCollection();
        for (int i = 0; i < a_sourceOp.Products.Count; i++)
        {
            Product sourceProduct = a_sourceOp.Products[i];
            Product product = sourceProduct.DeepCopy();
            Products.Add(product);
        }

        //Copy Attributes
        CopyAttributes(a_sourceOp.Attributes);

    }
    #endregion

    #region BoolVector32
    private BoolVector32 m_bools;
    private const int UseExpectedFinishQtyIdx = 0;
    private const int BottleneckFlagIdx = 1; // Part of a temporary property
    private const int BottleneckTestedFlagIdx = 2; // A temporary property.
    private const int AutoReportProgressFlagIdx = 3;

    private const int AutoFinishIdx = 4;
    //const int AllowMoveToOverrideConnectorTransferTicksIdx = 5;
    #endregion

    #region Shared Properties
    private long m_holdUntilDate = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// If the Operation is on hold then Activities for this Operation are scheduled to start after this date.
    /// </summary>
    internal long HoldUntilTicks => m_holdUntilDate;

    /// <summary>
    /// If the Operation is on hold then Activities for this Operation are scheduled to start after the Hold Until Date.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public DateTime HoldUntil
    {
        get => new(m_holdUntilDate);
        internal set => m_holdUntilDate = value.Ticks;
    }

    public static string HoldReasonFieldName => "HoldReason"; //this must match the field name below!

    private string m_holdReason = "";

    /// <summary>
    /// If the Operation is on hold then this is why.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public string HoldReason
    {
        get => m_holdReason;
        internal set => m_holdReason = value;
    }

    private BaseOperationDefs.omitStatuses m_omitted = BaseOperationDefs.omitStatuses.NotOmitted;

    /// <summary>
    /// Omitted Operations are treated as if they take zero time.
    /// 'OmittedByUser' means the Operation was omitted explicitly.
    /// 'OmittedAutomatically' means the system omitted the Operation due to bad data (See System Options).
    /// 'NotOmitted' means the Operation will be included as normal.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public BaseOperationDefs.omitStatuses Omitted
    {
        get => m_omitted;
        internal set
        {
            m_omitted = value;
            if (Scheduled && IsOmitted)
            {
                Unschedule();
            }
        }
    }

    internal virtual void Unschedule(bool a_clearLocks = true, bool a_removeFromBatch = true) { }

    private bool m_onHold;

    /// <summary>
    /// Whether the Operation should not be worked on for some reason until the Hold Until Date.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool OnHold
    {
        get => m_onHold;
        internal set
        {
            m_onHold = value;
            if (!m_onHold)
            {
                HoldReason = "";
                HoldUntil = PTDateTime.MinDateTime;
            }
        }
    }

    private bool m_isRework;

    /// <summary>
    /// Indicates that this is work being performed to fix a problem encountered in another operation. For display only.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool IsRework
    {
        get => m_isRework;
        private set => m_isRework = value;
    }

    private decimal m_requiredFinishQty;

    /// <summary>
    /// The quanity of good units that should be produced by the Operation.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    [Required(true)]
    public decimal RequiredFinishQty
    {
        get => m_requiredFinishQty;
        internal set => m_requiredFinishQty = value;
    }

    private string m_uom = "";

    /// <summary>
    /// Indicates the meaning of one unit of this product. For display only.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public string UOM
    {
        get => m_uom;
        private set => m_uom = value;
    }

    /// <summary>
    /// Whether the finish quantities of predecessor operations will have an influence on the expected finish quantity of this operation. In the event of
    /// multiple predecessors the most limiting one determines the finish quantity.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool UseExpectedFinishQty
    {
        get => m_bools[UseExpectedFinishQtyIdx];
        private set => m_bools[UseExpectedFinishQtyIdx] = value;
    }

    private string m_outputName = "";

    /// <summary>
    /// The name of the output product produced by this operation. This is useful in cases where several predecessor operations producing the same type of material
    /// supply the same successor operation. The successor will use this field to recognise quantities of identicle materials available. If this field isn't set then the
    /// material will not be grouped with any other predecessor operations. Groups of material are used for things such as calculating the ExpectedFinishQuantity.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public string OutputName
    {
        get => m_outputName;
        private set => m_outputName = value;
    }

    /// <summary>
    /// If true then when the Clock is advanced the Operation has progress reported according to schedule.
    /// Hours are automatically added to the fields for Reported Setup, Run, and Post-Processing values --
    /// depending upon the current Production Status of the Activities.  For example, if the Activity's Production Status
    /// is Running when the Clock is advanced then the Reported Run Hours is incremented by the difference
    /// in hours between the old Clock and new Clock values.
    /// Note that in order for the Activities to have their scheduled lengths adjusted as a result of this
    /// auto reporting of progress, the Time Base Reporting value on the Operation must also be checked.
    /// To preserve the auto-computed values for Reported Setup, Run and Post-Processing, updates from Imports are ignored.
    /// However, manual updates of these fields from the user interface are applied.
    /// </summary>
    public bool AutoReportProgress
    {
        get => m_bools[AutoReportProgressFlagIdx];
        internal set => m_bools[AutoReportProgressFlagIdx] = value;
    }

    /// <summary>
    /// If true then when the Clock is advanced past the end of the Operation it is Finished automatically.
    /// </summary>
    public bool AutoFinish
    {
        get => m_bools[AutoFinishIdx];
        internal set => m_bools[AutoFinishIdx] = value;
    }

    /// <summary>
    /// Whether to allow a moved block's start time to override its predecessor's 
    /// resource connector transfer ticks.
    /// For example given:
    /// Two resources. Resource 1 and resource 2.
    /// A job with two operations; 10 and 20. Operation 10 that can schedule on resoure 1 and
    /// operation 20 that can schedule on resource 2.
    /// A resource connector between resource 1 and 2 with 2 hours of transfer time.
    /// 
    /// An optimize would schedule operation 10 on resource 1 and operation 20 on
    /// resource 2 two hours after operation 10 finished. 
    /// 
    /// If this field is set to true and operation 20 is moved within 2 hours of the 
    /// finish of operation 10, the  move will be at the desired time,overriding 
    /// the transfer time.
    /// 
    /// If this field is set to false and operation 20 is moved within 2 hours of the
    /// finish of operation 10, the transfer time will be enforced and  operation 
    /// 20 will be scheduled at least 2 hours from the finish time of operation 10.
    /// </summary>
    //public bool AllowMoveToOverrideConnectorTransferTicks
    //{
    //    get { return m_bools[AllowMoveToOverrideConnectorTransferTicksIdx]; }
    //    internal set { m_bools[AllowMoveToOverrideConnectorTransferTicksIdx] = value; }
    //}

    /// <summary>
    /// Finish the Operation using default values.
    /// </summary>
    internal abstract void AutoFinishAllActivities(string aCommentsToLog);

    private long m_commitStartDate = PTDateTime.MaxDateTimeTicks;

    /// <summary>
    /// The planned Operation start date to be used to measure schedule conformity.
    /// </summary>
    public DateTime CommitStartDate
    {
        get => new(m_commitStartDate);
        private set => m_commitStartDate = value.Ticks;
    }

    private long m_commitEndDate = PTDateTime.MaxDateTimeTicks;

    /// <summary>
    /// The planned Operation end date to be used to measure schedule conformity.
    /// </summary>
    public DateTime CommitEndDate
    {
        get => new(m_commitEndDate);
        private set => m_commitEndDate = value.Ticks;
    }
    #endregion Shared Properties

    #region Properties
    //This is so that Name can be ReadOnly.
    /// <summary>
    /// Unique, changeable, text identifier.
    /// </summary>
    [ParenthesizePropertyName(true)]
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public override string Name
    {
        get => base.Name;
        internal set => base.Name = value;
    }

    //This is so that Name can be ReadOnly.
    /// <summary>
    /// Text for describing the object.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public override string Description
    {
        get => base.Description;
        internal set => base.Description = value;
    }

    //This is so that Name can be ReadOnly.
    /// <summary>
    /// Comments or special considerations pertaining to this object.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public override string Notes
    {
        get => base.Notes;
        set => base.Notes = value; //allow cusotmizations to set
    }

    private ManufacturingOrder m_manufacturingOrder;

    [Browsable(false)]
    public ManufacturingOrder ManufacturingOrder => m_manufacturingOrder;

    internal ScenarioDetail ScenarioDetail => ManufacturingOrder.ScenarioDetail;

    private AlternatePath.Node m_alternatePathNode;

    [Browsable(false)]
    public AlternatePath.Node AlternatePathNode
    {
        get => m_alternatePathNode;
        internal set => m_alternatePathNode = value;
    }

    /// <summary>
    /// The sum of the Work Content of the Activities
    /// </summary>
    public abstract TimeSpan WorkContent { get; }

    /// <summary>
    /// The expected number of work hours required to complete the Operation.
    /// </summary>
    public abstract decimal SchedulingHours { get; }

    /// <summary>
    /// The standard number of work hours required to complete the Operation.
    /// This value has no effect on scheduling but is used to compare scheduled versus standard hours.
    /// If this value is note set explicitly then it is set based on the scheduled hours when the operation is created.
    /// </summary>
    public abstract decimal StandardHours { get; protected set; }

    internal long NeedDateTicks => m_needInfo.OperationNeedDate;
    internal long DbrNeedDateTicks => m_needInfo.OperationDbrNeedDate;

    /// <summary>
    /// The latest the Operation can start and still finish by the Operation Buffer start.
    /// </summary>
    public DateTime DbrJitStartDate => new (DbrJitStartDateTicks);

    public DateTime JitStartDate => new (JitStartDateTicks);

    /// <summary>
    /// If the Operation has no Successors, then this is the Manufacturiong Order Need Date.
    /// Otherwise, this is the JIT Start Date of the closest Successor minus the Transfer Span.
    /// </summary>
    public DateTime NeedDate => new (NeedDateTicks);

    public DateTime DbrNeedDate => new (DbrNeedDateTicks);


    protected OperationNeedInfo m_needInfo = new ();

    protected class OperationNeedInfo : IPTSerializable
    {
        /// <summary>
        /// Need date is a precalculated field. Calculated whenever certain factors have changed that may result in operations being required sooner or later.
        /// </summary>
        internal long OperationNeedDate;
        internal long OperationDbrNeedDate;
        internal long m_asnTT;
        //internal ActivityResourceBufferInfo EarliestNeedBufferInfo;
        internal ActivityResourceBufferInfo EarliestJitBufferInfo;
        
        internal AlternatePath.Association m_asn;

        public OperationNeedInfo(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 13011)
            {
                a_reader.Read(out OperationNeedDate);
                a_reader.Read(out OperationDbrNeedDate);
                EarliestJitBufferInfo = new ActivityResourceBufferInfo(a_reader);
            }
            else
            {
                a_reader.Read(out OperationDbrNeedDate);
                new ActivityResourceBufferInfo(a_reader);
                EarliestJitBufferInfo = new ActivityResourceBufferInfo(a_reader);
            }

        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(OperationNeedDate);
            a_writer.Write(OperationDbrNeedDate);
            EarliestJitBufferInfo.Serialize(a_writer);
        }

        internal OperationNeedInfo()
        {

        }

        internal OperationNeedInfo(long a_moNeedTicks)
        {
            OperationDbrNeedDate = a_moNeedTicks;
        }

        /// <summary>
        /// Calculate the time this activity must begin transferring material to the successor factoring in things such as successor transfer time,
        /// resource transfer time, and material post processing time, to allow the material to reach the successor by its JIT start date.
        /// </summary>
        /// <param name="a_res">A primary resource this activity can schedule on. It might be used to estimate available capacity.</param>
        /// <param name="a_act"></param>
        /// <returns></returns>
        internal long CalcStartTransferBy(long a_clockDate, Resource a_res, InternalActivity a_act, long a_resTransferTime)
        {
            return CalcStartTransferBy(a_clockDate, a_res, OperationDbrNeedDate, a_act, m_asn, a_resTransferTime);
        }

        /// <summary>
        /// Calculate the time this activity must begin transferring material to the successor factoring in things such as successor transfer time,
        /// resource transfer time, and material post processing time, to allow the material to reach the successor by a specified start date.
        /// </summary>
        /// <param name="a_res">A primary resource this activity can schedule on. It might be used to estimate available capacity.</param>
        /// <param name="a_need"></param>
        /// <param name="a_act"></param>
        /// <returns></returns>
        internal long CalcStartTransferBy(long a_clockDate, Resource a_res, long a_need, InternalActivity a_act, AlternatePath.Association a_asn, long a_resTransferTime)
        {
            long startTransferBy = a_need - a_resTransferTime - a_act.GetResourceProductionInfo(a_res).MaterialPostProcessingSpanTicks; //Account for Resource Connector Transit hours.

            if (a_asn != null)
            {
                if (a_asn.TransferDuringPredeccessorOnlineTime)
                {
                    RequiredCapacity rc = new(RequiredSpanPlusClean.s_notInit, RequiredSpanPlusSetup.s_notInit, new RequiredSpan(a_asn.TransferSpanTicks, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                    startTransferBy = a_act.Operation.JITBackcalculate(a_clockDate, a_res, a_act, rc, a_need);
                }
                else
                {
                    startTransferBy -= a_asn.TransferSpanTicks;
                }
            }

            return startTransferBy;
        }

        public override string ToString()
        {
            string sucName = m_asn == null ? "" : m_asn.Successor.Operation.Name;
            return string.Format("Need={0}; asnTT={3}; suc={5}", new DateTimeOffset(OperationDbrNeedDate, TimeSpan.Zero).ToDisplayTime(), DateTimeHelper.PrintTimeSpan(m_asnTT), sucName);
        }

        public int UniqueId => 34;

        /// <summary>
        /// This Operation does not have a schedulable path
        /// </summary>
        internal void OmitFromScheduling()
        {
            OperationDbrNeedDate = PTDateTime.MinDateTime.Ticks;
        }

        //TODO: Can we improve this process. Always taking the min will lead to issues if one resource has a long offline or otherwise very early DbrNeedDate

        internal void FinalizeJitCalculations(InternalOperation a_operation)
        {
            //EarliestNeedBufferInfo = GetEarliestBufferNeedDate(a_operation);
            EarliestJitBufferInfo = GetEarliestJitStartDate(a_operation);
        }

        private static ActivityResourceBufferInfo GetEarliestBufferNeedDate(InternalOperation a_operation)
        {
            ActivityResourceBufferInfo earliestBufferInfo = new ();
            foreach (InternalActivity act in a_operation.Activities)
            {
                ActivityResourceBufferInfo earliestActJit = act.BufferInfo.EarliestJitBufferInfo;
                if (earliestActJit.DbrJitDateCalculated)
                {
                    if (!earliestBufferInfo.DbrJitDateCalculated || earliestActJit.BufferEndDate < earliestBufferInfo.BufferEndDate)
                    {
                        earliestBufferInfo = earliestActJit;
                    }
                }
            }

            return earliestBufferInfo;
        }

        private static ActivityResourceBufferInfo GetEarliestJitStartDate(InternalOperation a_operation)
        {
            ActivityResourceBufferInfo earliestBufferInfo = new ();
            foreach (InternalActivity act in a_operation.Activities)
            {
                ActivityResourceBufferInfo earliestActJit = act.BufferInfo.EarliestJitBufferInfo;
                if (earliestActJit.DbrJitDateCalculated)
                {
                    if (!earliestBufferInfo.DbrJitDateCalculated || earliestActJit.DbrJitStartDate < earliestBufferInfo.DbrJitStartDate)
                    {
                        earliestBufferInfo = earliestActJit;
                    }
                }
            }

            return earliestBufferInfo;
        }

        internal void Reset(InternalOperation a_op)
        {
            foreach (InternalActivity activity in a_op.Activities)
            {
                activity.BufferInfo.Reset();
            }
        }

        internal void CacheEarliestBufferInfo(InternalOperation a_op)
        {
            //TODO: We need to cache earliest, but should we use first act, or check all??
            foreach (InternalActivity activity in a_op.Activities)
            {
                activity.BufferInfo.Reset();
            }
        }
    }

    /// <summary>
    /// If this is a Safety Stock Job then ignore the lateness if the current inventory is greater than the warning level.
    /// </summary>
    /// <returns></returns>
    internal bool IgnoreLatenessDueToSafetyStockWarningOption()
    {
        if (ManufacturingOrder.Job.Classification != JobDefs.classifications.SafetyStock)
        {
            return false;
        }

        bool ignoreLates = Products.Count > 0;

        //See if any of the Products' inventories are below their SafetyStockWarningLevel
        for (int prodI = 0; prodI < Products.Count; prodI++)
        {
            Inventory inv = Products[prodI].Inventory;
            if (inv != null && inv.OnHandQty <= inv.SafetyStockWarningLevel)
            {
                ignoreLates = false;
                break;
            }
        }

        return ignoreLates;
    }

    /// <summary>
    /// An Operation is a Bottlneck if any of its Activities are Bottlenecks.
    /// Unless this Operation is scheduled sooner the Job or Manufacturing Order will be late.
    /// </summary>
    public abstract bool Bottleneck { get; }

    /// <summary>
    /// The Scheduled Start Date of the earliest of the Operations Activities.
    /// </summary>
    public abstract DateTime StartDateTime { get; }

    /// <summary>
    /// The Scheduled Start Date of the earliest Activity
    /// </summary>
    public DateTime ScheduledStartDate
    {
        get
        {
            GetScheduledStartDateTicks(out long opScheduledStartDate);
            return new DateTime(opScheduledStartDate, DateTimeKind.Utc);
        }
    }

    internal long ScheduledStartTicks
    {
        get
        {
            GetScheduledStartDateTicks(out long startTicks);
            return startTicks;
        }
    }

    internal abstract bool GetScheduledStartDateTicks(out long o_scheduledStartTicks);

    /// <summary>
    /// The Scheduled End Date of the latest of the Operation's Activities.
    /// </summary>
    public DateTime ScheduledEndDate
    {
        get
        {
            GetScheduledFinishDate(out long opScheduledFinishDate, true);
            return new DateTime(opScheduledFinishDate);
        }
    }

    /// <summary>
    /// The Need Date minus the Scheduled End Date.  Indicates how early or late the operation is.
    /// </summary>
    public TimeSpan Slack => NeedDate.Subtract(ScheduledEndDate);

    public abstract decimal ReportedGoodQty { get; }

    public abstract decimal ExpectedFinishQty { get; }

    /// <summary>
    /// A temporary bool used to help with bottleneck calculations. Used by a function of ManufacturingOrder.GetBottleneckOperations(); no other use is valid.
    /// This is set to true when the operation has been determined to be a bottleneck.
    /// </summary>
    internal bool BottleneckFlag
    {
        get => m_bools[BottleneckFlagIdx];
        set => m_bools[BottleneckFlagIdx] = value;
    }

    /// <summary>
    /// A temporary bool used to help with bottleneck calculations. Used by a function of ManufacturingOrder.GetBottleneckOperations(); no other use is valid.
    /// This is set to true when the operation has been tested for bottlenecks.
    /// </summary>
    internal bool BottleneckTestedFlag
    {
        get => m_bools[BottleneckTestedFlagIdx];
        set => m_bools[BottleneckTestedFlagIdx] = value;
    }

    private readonly BaseOperationProductionInfo m_productionInfo = new();

    public BaseOperationProductionInfo ProductionInfoBaseOperation => m_productionInfo;

    private AttributesCollection m_attributesCollection = new();

    public AttributesCollection Attributes
    {
        get => m_attributesCollection;
        private set => m_attributesCollection = value;
    }
    #endregion

    #region Abstract Methods
    /// <summary>
    /// Returns the sum of the Operation Resource and Material costs for the current scheduled Path.
    /// </summary>
    /// <returns></returns>
    public void GetWipCosts(ref decimal resourceCost, ref decimal materialCost)
    {
        resourceCost = GetResourceCost();
        materialCost = GetMaterialCost();
    }

    /// <summary>
    /// The sum of the TotalCost of each Material Requirement.
    /// </summary>
    /// <returns></returns>
    public decimal GetMaterialCost()
    {
        decimal materialCost = 0;
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            materialCost += MaterialRequirements[i].TotalCost;
        }

        return materialCost;
    }

    /// <summary>
    /// The interest rate cost of MaterialRequirements calculated from the beginning of the operation until the end of the Manufacturing Order.
    /// </summary>
    /// <returns></returns>
    public decimal GetMaterialCarryingCost()
    {
        decimal materialCost = 0;
        decimal interest = 0;
        if (MaterialRequirements.Count > 0)
        {
            decimal incurredTimes = (decimal)TimeSpan.FromTicks(ManufacturingOrder.ScheduledEnd - ScheduledStartDate.Ticks).TotalDays;

            for (int i = 0; i < MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = MaterialRequirements[i];

                if (mr.Warehouse != null)
                {
                    interest = mr.Warehouse.DailyInterestRate * incurredTimes;
                }
                else
                {
                    interest = ScenarioDetail.ScenarioOptions.DailyInterestRate * incurredTimes; // use system wide rate if not specified.
                }

                materialCost += mr.TotalCost * interest;
            }
        }

        return materialCost;
    }

    /// <summary>
    /// This is the sum of the Activity durations times there resources' resource costs, if scheduled.
    /// </summary>
    /// <returns></returns>
    public abstract decimal GetResourceCost();

    /// <summary>
    /// Sum of Activities Resource carrying costs.
    /// </summary>
    /// <returns></returns>
    public abstract decimal GetResourceCarryingCost();

    #endregion

    #region Transmission functionality
    internal abstract void Receive(OperationIdBaseT a_t, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_dataChanges);

    internal abstract void AdvancingClock(TimeSpan a_clockAdvancedBy, DateTime a_newClock, bool a_autoFinishAllActivities, bool a_autoReportProgressOnAllActivities);

    /// <summary>
    /// Sets the CommitStartDate and CommitEndDate to the Scheduled Start and End dates.
    /// Dates are not set if the Operation is not scheduled.
    /// </summary>
    internal void SetCommitDates()
    {
        if (Scheduled)
        {
            m_commitStartDate = StartDateTime.Ticks;
            m_commitEndDate = ScheduledEndDate.Ticks;
        }
    }

    /// <summary>
    /// Sets the CommitStartDate and CommitEndDate to MAX_DATE.
    /// </summary>
    internal void ClearCommitDates()
    {
        m_commitStartDate = PTDateTime.MaxDateTimeTicks;
        m_commitEndDate = PTDateTime.MaxDateTimeTicks;
    }
    #endregion

    #region Pred/Suc stuff
    /// <summary>
    /// Returns the Successor that has the earliest Scheduled Start.
    /// </summary>
    /// <returns>The earliest Successor or null if no successors.</returns>
    public BaseOperation GetTightestSuccessor()
    {
        AlternatePath.AssociationCollection successors = AlternatePathNode.Successors;

        DateTime earliestStart = PTDateTime.MaxDateTime;
        BaseOperation earliestOpn = null;
        for (int successorOperationI = 0; successorOperationI < successors.Count; ++successorOperationI)
        {
            BaseOperation baseOperation = successors[successorOperationI].Successor.Operation;
            if (!baseOperation.IsFinishedOrOmitted)
            {
                if (baseOperation.StartDateTime < earliestStart)
                {
                    earliestStart = baseOperation.StartDateTime;
                    earliestOpn = baseOperation;
                }
            }
            else
            {
                BaseOperation successor = baseOperation.GetTightestSuccessor();
                if (successor != null && successor.StartDateTime < earliestStart)
                {
                    earliestStart = successor.StartDateTime;
                    earliestOpn = successor;
                }
            }
        }

        return earliestOpn;
    }

    /// <summary>
    /// Returns the Successor that has the earliest JITStartDate.
    /// </summary>
    /// <returns>The earliest Successor or null if no successors.</returns>
    internal BaseOperation CalcJITLatestSuccessorConstraintDate()
    {
        AlternatePath.AssociationCollection successors = AlternatePathNode.Successors;

        long earliestJitStart = PTDateTime.MaxDateTimeTicks;
        BaseOperation earliestOpn = null;

        for (int successorOperationI = 0; successorOperationI < successors.Count; ++successorOperationI)
        {
            BaseOperation baseOperation = successors[successorOperationI].Successor.Operation;

            if (baseOperation.DbrJitStartDateTicks < earliestJitStart)
            {
                earliestJitStart = baseOperation.DbrJitStartDateTicks;
                earliestOpn = baseOperation;
            }
        }

        return earliestOpn;
    }

    /// <summary>
    /// Returns the earliest Successor Scheduled Start (in Server Time) or PtDateTime.MAX_DATE if there's no successor.
    /// </summary>
    public DateTime GetTightestSuccessorStartDate()
    {
        BaseOperation successor = GetTightestSuccessor();
        return successor?.StartDateTime ?? PTDateTime.MaxDateTime;
    }

    /// <summary>
    /// Whether the operation isn't part of the current Routing.
    /// If it isn't then AlternatePathNodes have been set and any functions that require the AlternatePathNode won't work.
    /// </summary>
    public bool NotPartOfCurrentRouting()
    {
        return AlternatePathNode == null;
    }

    /// <summary>
    /// Whether the operation is part of the current routing. If it is then AlternatePathNode is set to something
    /// </summary>
    /// <returns></returns>
    public bool PartOfCurrentRouting()
    {
        return AlternatePathNode != null;
    }

    /// <summary>
    /// Whether this operation has any predecessor operations.
    /// You might need to verify that this is part of the current routing before attempting to use this function. You can do this with a call to NotPartOfCurrentRouting.
    /// </summary>
    /// <returns></returns>
    public bool HasPredecessors()
    {
        return AlternatePathNode.Predecessors.Count > 0;
    }

    internal void AutoFinishPredecessors(string a_commentsToLog)
    {
        for (int i = 0; i < AlternatePathNode.Predecessors.Count; i++)
        {
            AlternatePath.Association association = AlternatePathNode.Predecessors[i];
            BaseOperation pred = association.Predecessor.Operation;
            if (!pred.Finished)
            {
                pred.AutoFinishAllActivities(a_commentsToLog);
            }

            pred.AutoFinishPredecessors(a_commentsToLog);
        }
    }

    /// <summary>
    /// Whether this operation has any successors.
    /// You might need to verify that this is part of the current routing before attempting to use this function. You can do this with a call to NotPartOfCurrentRouting.
    /// </summary>
    /// <returns></returns>
    protected bool HasSuccessors()
    {
        return AlternatePathNode.Successors.Count > 0;
    }
    [DoNotAuditProperty]
    [Browsable(false)]
    public AlternatePath.AssociationCollection Predecessors => AlternatePathNode.Predecessors;

    [DoNotAuditProperty]
    [Browsable(false)]
    public AlternatePath.AssociationCollection Successors => AlternatePathNode.Successors;

    /// <summary>
    /// Returns the TransferSpan of the resource used that has the longest TransferSpan.
    /// </summary>
    public abstract long GetLongestResourceTransferTicks();
    #endregion

    #region Cost
    public decimal MaterialCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < MaterialRequirements.Count; i++)
            {
                cost += MaterialRequirements[i].TotalCost;
            }

            return cost;
        }
    }

    /// <summary>
    /// Cost of Materials that have Item.Type=RawMaterial.
    /// </summary>
    public decimal RawMaterialCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = MaterialRequirements[i];
                if (mr.Item != null && mr.Item.ItemType == ItemDefs.itemTypes.RawMaterial)
                {
                    cost += Convert.ToDecimal(mr.TotalRequiredQty) * mr.Item.Cost;
                }
            }

            return cost;
        }
    }

    public abstract decimal LaborCost { get; }

    public abstract decimal MachineCost { get; }

    public abstract decimal SubcontractCost { get; }
    #endregion Cost

    #region Materials
    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly MaterialRequirementsCollection m_materialRequirementsCollection;

    [Browsable(false)]
    public MaterialRequirementsCollection MaterialRequirements
    {
        get => m_materialRequirementsCollection;
    }

    /// <summary>
    /// Browsable list of all Material Requirements for the Operation.
    /// </summary>
    public string MaterialList
    {
        get
        {
            System.Text.StringBuilder builder = new();
            for (int i = 0; i < MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = MaterialRequirements[i];
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(string.Format("{0} {1} {2}", mr.MaterialName, mr.TotalRequiredQty, mr.UOM));
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// The number of Materials that are not Planned.
    /// </summary>
    public int MaterialsUnplannedCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = MaterialRequirements[i];
                if (!mr.Planned)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public List<MaterialRequirement.MaterialShortage> GetMaterialShortages(ScenarioDetail a_sd)
    {
        List<MaterialRequirement.MaterialShortage> materialShortages = new();
        //Only worry about it if it's scheduled.  Otherwise the material allocations won't be satisifed.
        if (Scheduled)
        {
            for (int i = 0; i < MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = MaterialRequirements[i];

                if (mr.BuyDirect)
                {
                    //Check if there are no sources (LatestSourceDate is 0 if there are no MRSupply nodes) or if the latest source date is too late
                    if (mr.LatestSourceDate == 0 || mr.LatestSourceDateTime.Ticks > Math.Min(DbrJitStartDateTicks, StartDateTime.Ticks)) //It's a shortage.
                    {
                        MaterialRequirement.MaterialShortage shortage = new(mr, this, mr.LatestSourceDateTime);
                        materialShortages.Add(shortage);
                    }
                }
                else //Stock MR
                {
                    bool allOnHandOrExpected = mr.MRSupply.SourcesFrom(true, false, InventoryDefs.EAdjustmentType.OnHand, InventoryDefs.EAdjustmentType.MaterialRequirement);

                    if (!allOnHandOrExpected) //TODO need to adjust once we can go negative...will want to warn if from backlog too
                    {
                        DateTime earliestDelivery;
                        if (mr.Warehouse != null && mr.Item != null && mr.Warehouse.Inventories.Contains(mr.Item.Id))
                        {
                            Inventory inv = mr.Warehouse.Inventories[mr.Item.Id];
                            earliestDelivery = new DateTime(a_sd.Clock + inv.LeadTimeTicks);
                        }
                        else //find shortest lead time 
                        {
                            bool foundASupplyingWarehouse;
                            Inventory fastestInventory;
                            a_sd.WarehouseManager.GetShortestLeadTime(mr.Item, out foundASupplyingWarehouse, out fastestInventory);
                            earliestDelivery = new DateTime(a_sd.CalcLeadTimeTicks((InternalOperation)this, mr, fastestInventory));
                        }

                        switch (mr.ConstraintType)
                        {
                            case MaterialRequirementDefs.constraintTypes.NonConstraint:
                            case MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate:
                            case MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate:
                                if (earliestDelivery.Ticks > StartDateTime.Ticks) //Used before lead time
                                {
                                    MaterialRequirement.MaterialShortage shortage = new(mr, this, earliestDelivery);
                                    materialShortages.Add(shortage);
                                }

                                break;
                            case MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate:
                                DateTime availableDate = PTDateTime.Min(earliestDelivery, mr.LatestSourceDateTime);
                                if (availableDate.Ticks > DbrJitStartDateTicks) //Needed before lead time
                                {
                                    MaterialRequirement.MaterialShortage shortage = new(mr, this, earliestDelivery);
                                    materialShortages.Add(shortage);
                                }

                                break;
                            case MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate:
                                //Check if there are no sources (LatestSourceDate is 0 if there are no MRSupply nodes) or if the latest source date is too late
                                if (mr.LatestSourceDate == 0 || mr.LatestSourceDate > StartDateTime.Ticks)
                                {
                                    MaterialRequirement.MaterialShortage shortage = new(mr, this, earliestDelivery);
                                    materialShortages.Add(shortage);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        return materialShortages;
    }

    /// <summary>
    /// Returns inventory information indicating the material with the supply date closest the the operation start date.  This is intended to give a clue as to which material is a constraint.
    /// </summary>
    /// <returns></returns>
    public List<MaterialSupply> GetTightestMaterialSupplies(ScenarioDetail a_sd)
    {
        List<MaterialSupply> list = new();
        long maxDateSoFar = DateTime.MinValue.Ticks;
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            List<MaterialSupply> newMaterialSupplies = new();
            MaterialRequirement mr = MaterialRequirements[i];
            if (mr.BuyDirect)
            {
                MaterialSupply matlSuply = new();
                matlSuply.Date = mr.LatestSourceDateTime;
                matlSuply.Qty = mr.TotalRequiredQty;
                matlSuply.Description = "Buy-Direct".Localize();
                newMaterialSupplies.Add(matlSuply);
            }
            else
            {
                bool expectedInventory = mr.MRSupply.SourcesFrom(false, false, InventoryDefs.EAdjustmentType.MaterialRequirement);
                bool inventoryLeadTime = mr.MRSupply.SourcesFrom(false, false, InventoryDefs.EAdjustmentType.LeadTime);
                bool partsLeadTime = mr.MRSupply.SourcesFrom(false, false, InventoryDefs.EAdjustmentType.LeadTime); //supplySource.ItemsLeadTime;
                bool shortage = mr.MRSupply.SourcesFrom(false, false, InventoryDefs.EAdjustmentType.Shortage);
                bool onHandInventory = mr.MRSupply.SourcesFrom(false, false, InventoryDefs.EAdjustmentType.OnHand);

                if (expectedInventory)
                {
                    MaterialSupply matlSuply = new();
                    DateTime lastSupplyDate;
                    decimal lastSupplyQty;
                    string lastSupplyDesc;
                    mr.GetMostRecentSupplyOfItemBeingConsumed(a_sd, out lastSupplyDate, out lastSupplyQty, out lastSupplyDesc);
                    matlSuply.Date = lastSupplyDate;
                    matlSuply.Qty = lastSupplyQty;
                    matlSuply.Description = lastSupplyDesc;
                    newMaterialSupplies.Add(matlSuply);
                }

                if (inventoryLeadTime)
                {
                    MaterialSupply matlSuply = new();
                    matlSuply.Date = mr.LatestSourceDateTime;
                    matlSuply.Qty = mr.TotalRequiredQty;
                    matlSuply.Description = "Using Inventory Lead Time".Localize();
                    newMaterialSupplies.Add(matlSuply);
                }

                if (partsLeadTime)
                {
                    MaterialSupply matlSuply = new();
                    matlSuply.Date = mr.LatestSourceDateTime;
                    matlSuply.Qty = mr.TotalRequiredQty;
                    matlSuply.Description = "Using Item Lead Time".Localize();
                    newMaterialSupplies.Add(matlSuply);
                }

                if (shortage)
                {
                    MaterialSupply matlSuply = new();
                    matlSuply.Date = mr.LatestSourceDateTime;
                    matlSuply.Qty = mr.TotalRequiredQty;
                    matlSuply.Description = "Shortage".Localize();
                    newMaterialSupplies.Add(matlSuply);
                }

                //if (onHandInventory)
                //{
                //    //Nothing to do.  This is on hand so not tight.  Don't bother listing.            
                //}

                foreach (MaterialSupply newMaterialSupply in newMaterialSupplies)
                {
                    newMaterialSupply.ItemDesc = mr.MaterialDescription;
                    newMaterialSupply.ItemName = mr.MaterialName;
                    newMaterialSupply.UOM = mr.UOM;

                    if (newMaterialSupply.Date.Ticks == maxDateSoFar)
                    {
                        list.Add(newMaterialSupply);
                    }
                    else if (newMaterialSupply.Date.Ticks > maxDateSoFar)
                    {
                        list.Clear();
                        list.Add(newMaterialSupply);
                        maxDateSoFar = newMaterialSupply.Date.Ticks;
                    }
                }
            }
        }

        return list;
    }

    public class MaterialSupply
    {
        public DateTime Date { get; internal set; }

        public decimal Qty { get; internal set; }

        public string Description { get; internal set; }

        public string UOM { get; internal set; }

        public string ItemName { get; internal set; }

        public string ItemDesc { get; internal set; }
    }

    public enum MaterialStatuses
    {
        /// <summary>
        /// All materials are satisfied from OnHand inventory or are Issued or are Buy Direct and Available.
        /// </summary>
        MaterialsAvailable,

        /// <summary>
        /// All Materials are satisfied from OnHand inventory or existing Jobs or Purchases that are Firm or Released
        /// </summary>
        MaterialSourcesFirmed,

        /// <summary>
        /// All Materials are satisfied from OnHand inventory or Jobs/POs, at least one of which is Planned or Estimate.
        /// </summary>
        MaterialSourcesPlanned,

        /// <summary>
        /// Materials are not available and there is no source of supply other than Lead Time.
        /// </summary>
        MaterialSourcesUnknown,

        /// <summary>
        /// Ignored constraint material has a shortage.
        /// </summary>
        MaterialsIgnoredConstraintViolation,
    }

    /// <summary>
    /// Returns a list of the Item Names for all Material Requirements that are for stocked Items.
    /// </summary>
    public string StockMaterialsList
    {
        get
        {
            System.Text.StringBuilder list = new();
            MaterialRequirementsCollection materials = MaterialRequirements;
            for (int mrI = 0; mrI < materials.Count; mrI++)
            {
                MaterialRequirement mr = materials[mrI];
                if (!mr.BuyDirect)
                {
                    if (list.Length > 0)
                    {
                        list.Append(", ");
                    }

                    list.Append(string.Format("{0}", mr.Item.Name));
                }
            }

            return list.ToString();
        }
    }

    /// <summary>
    /// Returns a list of the Item Names for all Material Requirements that are for stocked Items
    /// where the material requirement is not satisfied by On Hand Inventory.
    /// </summary>
    public string StockMaterialsListAwaitingAllocation
    {
        get
        {
            System.Text.StringBuilder list = new();
            MaterialRequirementsCollection materials = MaterialRequirements;
            for (int mrI = 0; mrI < materials.Count; mrI++)
            {
                MaterialRequirement mr = materials[mrI];
                bool allOnHand = mr.MRSupply.SourcesFrom(true, true, InventoryDefs.EAdjustmentType.OnHand);

                if (!mr.BuyDirect && !allOnHand)
                {
                    if (list.Length > 0)
                    {
                        list.Append(", ");
                    }

                    list.Append(string.Format("{0}", mr.Item.Name));
                }
            }

            return list.ToString();
        }
    }
    #endregion Materials

    #region Products
    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private ProductsCollection m_products = new();

    [Browsable(false)]
    public ProductsCollection Products
    {
        get => m_products;
        private set => m_products = value;
    }

    public string ProductsList
    {
        get
        {
            System.Text.StringBuilder list = new();

            for (int i = 0; i < m_products.Count; i++)
            {
                Product product = m_products[i];
                if (list.Length > 0)
                {
                    list.Append(", ");
                }

                list.Append(string.Format("{0} {1}", product.Item.Name, Math.Round(product.TotalOutputQty, 2)));
            }

            return list.ToString();
        }
    }
    #endregion

    #region State properties
    /// <summary>
    /// Whether the Operation is currently scheduled or not.
    /// </summary>
    public abstract bool Scheduled { get; }

    /// <summary>
    /// Whether the Operation is finished.
    /// </summary>
    public abstract bool Finished { get; }

    /// <summary>
    /// Whether this operation has no predecessors or has no predecessors which require completion (as in BaseOperation.RequiresCompletion).
    /// </summary>
    internal bool Leaf
    {
        get
        {
            bool leaf = true;
            AlternatePath.AssociationCollection predecessors = AlternatePathNode.Predecessors;

            for (int predecessorsI = 0; predecessorsI < predecessors.Count; ++predecessorsI)
            {
                AlternatePath.Association predecessorAssociation = predecessors[predecessorsI];
                BaseOperation predBaseOp = predecessorAssociation.Predecessor.Operation;

                if (!predBaseOp.IsFinishedOrOmitted)
                {
                    leaf = false;
                    break;
                }
            }

            return leaf;
        }
    }
    internal abstract bool GetScheduledFinishDate(out long o_scheduledFinishDate, bool a_includeCleanout);

    internal enum OperationStatusChanges : ulong { Unchanged = 0, ActivitiesFinished = 1 }
    #endregion

    #region Predecessor MO: both finished and unfinished predecessor MOs
    #region Predecessor MO Requirements
    /// <summary>
    /// The number of predecessor MOs this operation is waiting on.
    /// </summary>
    private int m_predecessorMOCount;

    /// <summary>
    /// During a simulation call this function for each predecessor MO constraint.
    /// </summary>
    internal void NotifyOfPredecessorMOConstraint()
    {
        ++m_predecessorMOCount;
        m_predecessorMosReleasedTime = 0;
    }

    /// <summary>
    /// During a simulation call this function as each predecessor MO constraint is lifted.
    /// </summary>
    /// <param name="a_nextEventTime"></param>
    internal void NotifyOfPredecessorMOConstraintSatisfaction(long a_nextEventTime)
    {
        --m_predecessorMOCount;
        if (m_predecessorMOCount == 0)
        {
            m_predecessorMosReleasedTime = a_nextEventTime;
        }
    }

    private long m_predecessorMosReleasedTime;

    public DateTime PredecessorMosReleasedTime => m_predecessorMosReleasedTime > 0 ? new DateTime(m_predecessorMosReleasedTime) : PTDateTime.InvalidDateTime;
    #endregion

    #region FinishedPredecessorMOReleaseInfo. Information on prececessor MOs that have been completed.
    private readonly FinishedPredecessorMOReleaseInfoManager m_finishedPredecessorMOReleaseInfoManager = new();

    public FinishedPredecessorMOReleaseInfoManager FinishedPredecessorMOReleaseInfoManager => m_finishedPredecessorMOReleaseInfoManager;

    /// <summary>
    /// This function needs to be called on the successor operation when a predecessor MO is finished.
    /// </summary>
    /// <param name="mo">The predecessor MO.</param>
    /// <param name="readyTicks">The time the predecessor releases this successor.</param>
    internal void NotificationOfPredecessorMOFinish(ManufacturingOrder aPredMO, long aReadyTicks)
    {
        m_finishedPredecessorMOReleaseInfoManager.Add(aPredMO, aReadyTicks);
    }
    #endregion
    #endregion

    #region Constraint Violations
    /// <summary>
    /// Find constraint violations of the operation's activities and add them to the list parameter.
    /// </summary>
    /// <param name="violations">All constraint violations are added to this list.</param>
    internal abstract void GetConstraintViolations(ConstraintViolationList violations);
    #endregion

    #region PT Database
    internal abstract void PopulateJobDataSet(JobDataSet.ManufacturingOrderRow a_moRow, JobManager a_jobs, ref JobDataSet a_dataSet);

    public virtual void PtDbPopulate(ref PtDbDataSet dataSet, PtDbDataSet.ManufacturingOrdersRow moRow, PtDbDataSet.JobOperationsRow jobOpRow, bool publishInventory, PTDatabaseHelper a_dbHelper)
    {
        if (publishInventory)
        {
            Products.PtDbPopulate(ref dataSet, this, moRow, publishInventory, a_dbHelper);
        }

        //TODO: Publish OperationAttribute
        for (int i = 0; i < Attributes.Count; i++)
        {
            OperationAttribute attribute = Attributes[i];
            dataSet.JobOperationAttributes.AddJobOperationAttributesRow(
                jobOpRow.PublishDate,
                jobOpRow.InstanceId,
                jobOpRow.JobId,
                jobOpRow.ManufacturingOrderId,
                jobOpRow.OperationId,
                attribute.PTAttribute.ExternalId,
                attribute.Code,
                attribute.Number,
                attribute.Cost,
                attribute.Duration.TotalHours,
                ColorUtils.ConvertColorToHexString(attribute.ColorCode),
                attribute.ShowInGantt
            );
        }
    }
    #endregion

    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal virtual void ResetERPStatusUpdateVariables() { }
    #endregion

    #region Update
    /// <summary>
    /// The new version of Update.
    /// </summary>
    /// <param name="a_tempOperation"></param>
    /// <param name="a_tOp"></param>
    /// <param name="a_scheduled"></param>
    /// <param name="a_clock"></param>
    /// <param name="a_erpUpdate"></param>
    /// <param name="a_am"></param>
    /// <param name="a_warehouseManager"></param>
    /// <param name="a_opManager"></param>
    /// <param name="a_t"></param>
    /// <param name="a_udfManager"></param>
    /// <param name="a_dataChanges"></param>
    /// <returns></returns>
    protected bool Update(BaseOperation a_tempOperation,
                          JobT.BaseOperation a_tOp,
                          bool a_scheduled,
                          long a_clock,
                          bool a_erpUpdate,
                          AttributeManager a_am,
                          WarehouseManager a_warehouseManager,
                          BaseOperationManager a_opManager,
                          PTTransmission a_t,
                          UserFieldDefinitionManager a_udfManager,
                          IScenarioDataChanges a_dataChanges)
    {
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;

        bool updated = flagProductionChanges = UpdateAttributes(a_am, a_tOp.ResourceAttributes, a_erpUpdate, ((JobT)a_t).AutoDeleteOperationAttributes, a_dataChanges);

        updated |= Update(a_tOp, a_t, a_udfManager, UserField.EUDFObjectType.ResourceOperations);

        if (a_tOp.ProductionInfoFlagsBaseOperation.MaterialsSet)
        {
            ProductionInfoBaseOperation.OnlyAllowManualupdates.Materials = a_tempOperation.ProductionInfoBaseOperation.OnlyAllowManualupdates.Materials;
            updated = true;
        }

        if (a_tOp.ProductionInfoFlagsBaseOperation.ProductsSet)
        {
            ProductionInfoBaseOperation.OnlyAllowManualupdates.Products = a_tempOperation.ProductionInfoBaseOperation.OnlyAllowManualupdates.Products;
            updated = true;
        }

        if (a_tOp.MinOperationBufferSet && StandardOperationBufferTicks != a_tempOperation.StandardOperationBufferTicks)
        {
            StandardOperationBufferTicks = a_tempOperation.StandardOperationBufferTicks;
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }

        if (a_tOp.PlannedScrapQtySet)
        {
            PlannedScrapQty = a_tempOperation.PlannedScrapQty;
            updated = true;
        }

        if (a_tOp.UseExpectedFinishQtySet && UseExpectedFinishQty != a_tempOperation.UseExpectedFinishQty)
        {
            UseExpectedFinishQty = a_tempOperation.UseExpectedFinishQty;
            flagProductionChanges = true;
            updated = true;
        }

        if (IsRework != a_tempOperation.IsRework)
        {
            IsRework = a_tempOperation.IsRework;
            updated = true;
        }

        if (a_tOp.OmittedSet && Omitted != a_tempOperation.Omitted)
        {
            if (Omitted == BaseOperationDefs.omitStatuses.NotOmitted)
            {
                flagProductionChanges = true;
            }
            else if (a_tempOperation.Omitted != BaseOperationDefs.omitStatuses.NotOmitted)
            {
                //An operation is now being scheduled again, need to calculate eligibility
                a_dataChanges.FlagEligibilityChanges(Job.Id);
            }
            Omitted = a_tempOperation.Omitted;
            // *LRH*TODO*STATUSUPDTE* I skipped this because you need to unschedule the Job in some circumstances. 
            updated = true;
        }

        if (a_tOp.UOMSet && UOM != a_tempOperation.UOM)
        {
            UOM = a_tempOperation.UOM;
            updated = true;
        }

        if (a_tOp.OutputNameSet && OutputName != a_tempOperation.OutputName)
        {
            OutputName = a_tempOperation.OutputName;
            updated = true;
        }

        if (a_tOp.HoldReasonSet && HoldReason != a_tempOperation.HoldReason)
        {
            HoldReason = a_tempOperation.HoldReason;
            updated = true;
        }

        if (a_tOp.HoldUntilDateTimeSet && HoldUntilTicks != a_tempOperation.HoldUntil.Ticks)
        {
            HoldUntil = a_tempOperation.HoldUntil;
            if (HoldUntil > StartDateTime)
            {
                flagConstraintChanges = true;
                updated = true;
            }
        }

        if (a_tOp.OnHoldSet && OnHold != a_tempOperation.OnHold)
        {
            OnHold = a_tempOperation.OnHold;
            if (OnHold && HoldUntil > StartDateTime)
            {
                flagConstraintChanges = true;
                updated = true;
            }
        }

        if (a_tOp.AutoReportProgressSet && AutoReportProgress != a_tempOperation.AutoReportProgress)
        {
            AutoReportProgress = a_tempOperation.AutoReportProgress;
            updated = true;
        }

        if (a_tOp.AutoFinishSet && AutoFinish != a_tempOperation.AutoFinish)
        {
            AutoFinish = a_tempOperation.AutoFinish;
            updated = true;
        }

        if (a_tOp.CommitStartDateSet && CommitStartDate != a_tempOperation.CommitStartDate)
        {
            CommitStartDate = a_tempOperation.CommitStartDate;
            updated = true;
        }

        if (a_tOp.CommitEndDateSet && CommitEndDate != a_tempOperation.CommitEndDate)
        {
            CommitEndDate = a_tempOperation.CommitEndDate;
            updated = true;
        }

        if (!a_erpUpdate || !m_productionInfo.OnlyAllowManualupdates.Materials)
        {
            //Update Material Requirements
            updated |= MaterialRequirements.Update(a_tempOperation.MaterialRequirements, this, a_clock, a_erpUpdate, a_warehouseManager, a_dataChanges);
        }

        if (!a_erpUpdate || !m_productionInfo.OnlyAllowManualupdates.Products)
        {
            //Update Products
            updated |= Products.Update(a_tempOperation.Products, this, a_erpUpdate, ScenarioDetail.ScenarioOptions, a_opManager, a_dataChanges);
        }

        if (a_scheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(Job.Id);
            }
        }
        

        return updated;
    }

    internal bool Edit(ScenarioDetail a_sd, OperationEdit a_edit, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Edit(a_edit);

        foreach ((string propertyName, (string attributeExternalId, byte[] value)) in a_edit.GetAttributeValues())
        {
            //TODO: Attributes
            if (Attributes == null)
            {
                Attributes = new AttributesCollection();
            }

            PTAttribute ptAttribute = a_sd.AttributeManager.GetByExternalId(attributeExternalId);

            if (Attributes.GetByExternalId(attributeExternalId) is OperationAttribute attribute)
            {
                AuditEntry auditEntry = new AuditEntry(attribute.PTAttribute.Id, Id, attribute);
                attribute.SetDataValueFromSerializedData(propertyName, value);
                updated = true;
                a_dataChanges.AuditEntry(auditEntry);
            }
            else
            {
                OperationAttribute opAttribute = new(ptAttribute);
                opAttribute.SetDataValueFromSerializedData(propertyName, value);
                Attributes.Add(opAttribute);
                updated = true;

                a_dataChanges.AuditEntry(new AuditEntry(opAttribute.PTAttribute.Id, Id, opAttribute), true);
            }
        }

        if (a_edit.OnlyAllowManualUpdatesToProductsSet && ProductionInfoBaseOperation.OnlyAllowManualupdates.Products != a_edit.OnlyAllowManualUpdatesToProducts)
        {
            ProductionInfoBaseOperation.OnlyAllowManualupdates.Products = a_edit.OnlyAllowManualUpdatesToProducts;
            updated = true;
        }

        if (a_edit.OnlyAllowManualUpdatesToMaterialsSet && ProductionInfoBaseOperation.OnlyAllowManualupdates.Materials != a_edit.OnlyAllowManualUpdatesToMaterials)
        {
            ProductionInfoBaseOperation.OnlyAllowManualupdates.Materials = a_edit.OnlyAllowManualUpdatesToMaterials;
            updated = true;
        }

        if (a_edit.PlannedScrapQtySet && PlannedScrapQty != a_edit.PlannedScrapQty)
        {
            PlannedScrapQty = a_edit.PlannedScrapQty;
            updated = true;
        }

        if (a_edit.UseExpectedFinishQtySet && UseExpectedFinishQty != a_edit.UseExpectedFinishQty)
        {
            UseExpectedFinishQty = a_edit.UseExpectedFinishQty;
            a_dataChanges.FlagProductionChanges(Job.Id);
            updated = true;
        }

        if (a_edit.IsReworkSet && IsRework != a_edit.IsRework)
        {
            IsRework = a_edit.IsRework;
            updated = true;
        }

        if (a_edit.OmittedSet && Omitted != a_edit.Omitted)
        {
            BaseOperationDefs.omitStatuses previousOmitted = Omitted;
            Omitted = a_edit.Omitted;
            if (previousOmitted == BaseOperationDefs.omitStatuses.NotOmitted)
            {
                a_dataChanges.FlagProductionChanges(Job.Id);
            }
            else if (Omitted != BaseOperationDefs.omitStatuses.NotOmitted)
            {
                a_dataChanges.FlagEligibilityChanges(Job.Id);
            }
            updated = true;
        }

        if (a_edit.UOMSet && UOM != a_edit.UOM)
        {
            UOM = a_edit.UOM;
            updated = true;
        }

        if (a_edit.OutputNameSet && OutputName != a_edit.OutputName)
        {
            OutputName = a_edit.OutputName;
            updated = true;
        }

        if (a_edit.HoldReasonSet && HoldReason != a_edit.HoldReason)
        {
            HoldReason = a_edit.HoldReason;
            updated = true;
        }

        if (a_edit.HoldUntilSet && HoldUntil != a_edit.HoldUntil)
        {
            HoldUntil = a_edit.HoldUntil;
            if (HoldUntil > StartDateTime)
            {
                // Hold window shifts block placement; treat as constraint change.
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }

            updated = true;
        }

        if (a_edit.OnHoldSet && OnHold != a_edit.OnHold)
        {
            OnHold = a_edit.OnHold;
            if (OnHold && HoldUntil > StartDateTime)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }

            updated = true;
        }

        if (a_edit.AutoReportProgressSet && AutoReportProgress != a_edit.AutoReportProgress)
        {
            AutoReportProgress = a_edit.AutoReportProgress;
            updated = true;
        }

        if (a_edit.AutoFinishSet && AutoFinish != a_edit.AutoFinish)
        {
            AutoFinish = a_edit.AutoFinish;
            updated = true;
        }

        if (a_edit.MinBufferDaysIsSet && StandardOperationBufferDays != a_edit.StandardBufferDays)
        {
            m_standardOperationBufferTicks = TimeSpan.FromDays(Convert.ToDouble(a_edit.StandardBufferDays)).Ticks;
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }

        return updated;
    }

    internal bool UpdateAttributes(AttributeManager a_am, OperationAttributeList a_attributeListT, bool a_erpUpdate, bool a_autoDeleteOperationAttributes, IScenarioDataChanges a_dataChanges)
    {
        AttributesCollection attCollection = new();
        foreach (Transmissions.OperationAttribute opAttributeT in a_attributeListT)
        {
            PTAttribute ptAttribute = a_am.GetByExternalId(opAttributeT.PTAttributeExternalId);
            if (ptAttribute != null)
            {
                OperationAttribute att = new(opAttributeT, ptAttribute);
                attCollection.Add(att);
            }
            else
            {
                //TODO:Attribute add Profile for being able to add all attributes that can be added and collecting pt validation exceptions for incorrect ones.
                throw new PTValidationException("4470", new object[] { Job.ExternalId, ExternalId, opAttributeT.PTAttributeExternalId });
            }
        }

        return UpdateAttributes(a_dataChanges, attCollection, a_erpUpdate, a_autoDeleteOperationAttributes);
    }

    private bool UpdateAttributes(IScenarioDataChanges a_dataChanges, AttributesCollection a_newAttCollection, bool a_erpUpdate, bool a_autoDeleteOperationAttributes)
    {
        bool changed = false;

        HashSet<string> updatedAttributeExternalIds = new();

        for (int i = 0; i < a_newAttCollection.Count; i++)
        {
            OperationAttribute newAttribute = a_newAttCollection[i];
            if (Attributes.TryGetValue(newAttribute.PTAttributeExternalId, out OperationAttribute existingAttribute))
            {
                //Update it if allowed
                AuditEntry auditEntry = new AuditEntry(newAttribute.PTAttribute.Id, Id, newAttribute);
                changed |= existingAttribute.Update(newAttribute, a_erpUpdate);
                a_dataChanges.AuditEntry(auditEntry);
                updatedAttributeExternalIds.Add(existingAttribute.PTAttributeExternalId);
            }
            else //new attribute being added
            {
                Attributes.Add(newAttribute);
                updatedAttributeExternalIds.Add(newAttribute.PTAttributeExternalId);
                a_dataChanges.AuditEntry(new AuditEntry(newAttribute.PTAttribute.Id, Id, newAttribute), true);
                changed = true;
            }
        }

        //Remove any obsolete Attributes if allowed
        for (int i = Attributes.Count - 1; i >= 0; i--)
        {
            OperationAttribute att = Attributes[i];
            if (!updatedAttributeExternalIds.Contains(att.PTAttributeExternalId) && (!(att.CanRemove && a_erpUpdate) || a_autoDeleteOperationAttributes))
            {
                Attributes.Remove(att.PTAttributeExternalId);
                changed = true;
                a_dataChanges.AuditEntry(new AuditEntry(att.PTAttribute.Id, Id, att), false, true);
            }
        }

        return changed;
    }

    private void CopyAttributes(AttributesCollection a_source)
    {
        Attributes.Clear();
        for (int i = 0; i < a_source.Count; i++)
        {
            OperationAttribute sourceAttribute = a_source[i];
            Attributes.Add(sourceAttribute);
        }
    }
    #endregion

    #region Delete Validation
    /// <summary>
    /// Checks to make sure the Warehouse is not in use.
    /// </summary>
    internal void ValidateWarehouseDelete(Warehouse a_warehouse)
    {
        //Make sure the Material Requiremnets are not using the Warehouse
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = MaterialRequirements[i];
            if (mr.Warehouse == a_warehouse)
            {
                throw new PTValidationException("2224", new object[] { a_warehouse.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, mr.ExternalId });
            }
        }

        //Make sure the Products are not using the Warehouse
        for (int i = 0; i < Products.Count; i++)
        {
            Product product = Products[i];
            if (product.Warehouse == a_warehouse)
            {
                throw new PTValidationException("2225", new object[] { a_warehouse.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, product.ExternalId });
            }
        }
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ClearingJobs)
        {
            return;
        }

        //Make sure the Material Requiremnets are not using the StorageArea
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = MaterialRequirements[i];
            if (mr.StorageArea == a_storageArea)
            {
                a_deleteProfile.AddValidationException(a_storageArea, new PTValidationException("3099", new object[] { a_storageArea.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, mr.ExternalId }));
            }
        }

        //Make sure the Products are not using the StorageArea
        for (int i = 0; i < Products.Count; i++)
        {
            Product product = Products[i];
            product.ValidateStorageAreaDelete(this, a_storageArea, a_deleteProfile);
        }
    }

    /// <summary>
    /// Checks to make sure the Inventory is not in use.
    /// </summary>
    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile == null || a_deleteProfile.Empty)
        {
            return;
        }

        //Make sure the Products are not using the Inventory
        for (int i = 0; i < Products.Count; i++)
        {
            Product product = Products[i];
            if (a_deleteProfile.ContainsInventory(product.Inventory.Id))
            {
                Inventory inventory = a_deleteProfile.GetById(product.Inventory.Id);
                a_deleteProfile.AddValidationException(inventory, new PTValidationException("2226", new object[] { inventory.Item.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, product.ExternalId, inventory.Warehouse.ExternalId }));
            }
        }

        //Make sure the Material Requiremnets are not using the Warehouse
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = MaterialRequirements[i];
            if (!mr.BuyDirect && mr.Warehouse != null)
            {
                if (a_deleteProfile.ContainsInventory(mr.Warehouse.Id, mr.Item.Id))
                {
                    //This MR is using the inventory by referenced warehouse and item. Find the inventory for error reporting
                    Inventory inventory = mr.Warehouse.Inventories[mr.Item.Id];
                    a_deleteProfile.AddValidationException(inventory, new PTValidationException("2961", new object[] { inventory.Item.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, mr.ExternalId, inventory.Warehouse.ExternalId }));
                }
            }
        }
    }

    /// <summary>
    /// Checks to make sure the Item is not in use.
    /// </summary>
    internal void ValidateItemDelete(ItemDeleteProfile a_itemDeleteProfile)
    {
        //Make sure the Material Requiremnets are not using the Warehouse
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = MaterialRequirements[i];
            if (!mr.BuyDirect)
            {
                if (a_itemDeleteProfile.ContainsItem(mr.Item.Id))
                {
                    PTValidationException itemDeleteException = new("2227", new object[] { mr.Item.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, mr.ExternalId });
                    a_itemDeleteProfile.AddValidationException(mr.Item, itemDeleteException);
                }
            }
        }

        //Make sure the Products are not using the Warehouse
        for (int i = 0; i < Products.Count; i++)
        {
            Product product = Products[i];
            if (a_itemDeleteProfile.ContainsItem(product.Item.Id))
            {
                PTValidationException itemDeleteException = new("2228", new object[] { product.Item.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, product.ExternalId });
                a_itemDeleteProfile.AddValidationException(product.Item, itemDeleteException);
            }
        }
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        //Make sure the Material Requiremnets are not using the StorageArea
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {

            MaterialRequirement mr = MaterialRequirements[i];
            if (mr.StorageArea == null)
            {
                //bool availableSa = false;
                ////Check that there is at least one valid SA for this MR
                //if (mr.Warehouse != null)
                //{
                //    foreach (StorageArea area in mr.Warehouse.StorageAreas)
                //    {
                //        if (area.CanStoreItem(mr.Item.Id))
                //        {
                //            availableSa = true;
                //            break;
                //        }
                //    }
                //}

                //if (!availableSa)
                //{
                //    //TODO: New exception indicating there are no available SAs for this MR.
                //}
            }
            else if (a_itemStorageDeleteProfile.ContainsItems(mr.StorageArea.Id, mr.Item.Id))
            {
                PTValidationException itemDeleteException = new("4496", new object[] { mr.Item.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, mr.ExternalId, mr.StorageArea.ExternalId });
                a_itemStorageDeleteProfile.AddValidationException(mr.StorageArea.Storage.GetValue(mr.Item.Id), itemDeleteException);
            }
        }

        //Make sure the Products are not using the Warehouse
        for (int i = 0; i < Products.Count; i++)
        {
            Product product = Products[i];
            if (product.StorageArea == null)
            {
                //TODO:?
            }
            else if (a_itemStorageDeleteProfile.ContainsItems(product.StorageArea.Id, product.Item.Id))
            {
                PTValidationException itemDeleteException = new("4497", new object[] { product.Item.ExternalId, ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId, product.ExternalId, product.StorageArea.ExternalId });
                a_itemStorageDeleteProfile.AddValidationException(product.StorageArea.Storage.GetValue(product.Item.Id), itemDeleteException);
            }
        }
    }
    #endregion

    /// <summary>
    /// If all of the Products in total only make one Item then that Item is returned.
    /// Otherwise, null is returned.
    /// </summary>
    /// <returns></returns>
    internal Item GetOnlyProductMade()
    {
        Item item = null;
        for (int i = 0; i < Products.Count; i++)
        {
            Product product = Products[i];
            if (item != null && product.Item != item) //found second product made.
            {
                return null;
            }

            item = product.Item;
        }

        return item;
    }

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long clockAdvanceTicks)
    {
        if (OnHold)
        {
            HoldUntil += new TimeSpan(clockAdvanceTicks);
        }

        MaterialRequirements.AdjustDemoDataForClockAdvance(clockAdvanceTicks);
    }
    #endregion

    public Job Job => ManufacturingOrder.Job;

    /// <summary>
    /// The date/time when this Operation must start in order to avoid violating any of the Predecessor Max Delay limits.
    /// If there are no Predecessors then this is Max Date.
    /// </summary>
    public DateTime MaxDelayRequiredStartBy
    {
        get
        {
            if (NotPartOfCurrentRouting())
            {
                return PTDateTime.MaxDateTime;
            }

            if (HasPredecessors())
            {
                AlternatePath.AssociationCollection predecessorCollection = AlternatePathNode.Predecessors;
                long earliestMaxDelayDateSoFar = PTDateTime.MaxDateTimeTicks;
                for (int predecessorI = 0; predecessorI < predecessorCollection.Count; predecessorI++)
                {
                    BaseOperation predecessorOperation = predecessorCollection[predecessorI].Predecessor.Operation;
                    if (predecessorOperation.Scheduled) //ignore if not scheduled since this date might not be valid.
                    {
                        predecessorOperation.GetScheduledFinishDate(out long predSchedEndTicks, false); //Don't include cleanout

                        long overflowMaxAdd = long.MaxValue - predSchedEndTicks;

                        if (predecessorCollection[predecessorI].MaxDelayTicks < overflowMaxAdd)
                        {
                            long maxDelayDate = Math.Min(long.MaxValue, predSchedEndTicks + predecessorCollection[predecessorI].MaxDelayTicks);
                            if (maxDelayDate < earliestMaxDelayDateSoFar)
                            {
                                earliestMaxDelayDateSoFar = maxDelayDate;
                            }
                        }
                    }
                }

                return new DateTime(earliestMaxDelayDateSoFar);
            }

            return PTDateTime.MaxDateTime;
        }
    }

    /// <summary>
    /// The date/time when this Operation must start in order to avoid violating any of the Predecessor Max Delay limits.
    /// If there are no Predecessors then this is Max Date.
    /// </summary>
    public DateTime MaxDelayRequiredEndAfter
    {
        get
        {
            if (NotPartOfCurrentRouting())
            {
                return PTDateTime.MinDateTime;
            }

            if (HasSuccessors())
            {
                AlternatePath.AssociationCollection successorsCollection = AlternatePathNode.Successors;
                long latestMaxDelayDateSoFar = PTDateTime.MinDateTimeTicks;
                for (int successorI = 0; successorI < successorsCollection.Count; successorI++)
                {
                    InternalOperation successorOperation = (InternalOperation)successorsCollection[successorI].Successor.Operation;
                    if (successorOperation.Scheduled) //ignore if not scheduled since this date might not be valid.
                    {
                        long successorStartTicks = successorOperation.GetEarliestScheduledActivityStartDate(out _);

                        long overflowMaxSubtract = long.MinValue + successorStartTicks;

                        if (successorsCollection[successorI].MaxDelayTicks > overflowMaxSubtract)
                        {
                            long maxDelayDate = Math.Max(long.MinValue, successorStartTicks - successorsCollection[successorI].MaxDelayTicks);
                            if (maxDelayDate > latestMaxDelayDateSoFar)
                            {
                                latestMaxDelayDateSoFar = maxDelayDate;
                            }
                        }
                    }
                }

                return new DateTime(latestMaxDelayDateSoFar);
            }

            return PTDateTime.MinDateTime;
        }
    }

    #region Demand
    internal void DeletingDemand(BaseIdObject a_demand, PTTransmissionBase a_t, BaseIdList a_distributionsToDelete = null)
    {
        for (int pI = 0; pI < Products.Count; ++pI)
        {
            Product p = Products[pI];
            if (p.Demands != null)
            {
                p.Demands.DeletingDemand(a_demand, a_t, a_distributionsToDelete);
            }
        }
    }
    #endregion

    private decimal m_plannedScrapQty;

    public decimal PlannedScrapQty
    {
        get => m_plannedScrapQty;
        private set => m_plannedScrapQty = value;
    }

    /// <summary>
    /// Apply planned scrap quantity to all material requirements
    /// </summary>
    public virtual void ApplyPlannedScrapQty()
    {
        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = MaterialRequirements[i];
            mr.ApplyPlannedScrapQty();
        }

        RequiredFinishQty += PlannedScrapQty;
    }

    /// <summary>
    /// Verifies that OverlapTransferQty is > 0
    /// <param name="a_automaticallyResolveErrors">Whether to alter transfer quantity as needed to avoid validation errors</param>
    /// </summary>
    internal abstract void ValidateOverlapTransferQuantity(bool a_automaticallyResolveErrors);

    internal abstract void Deleting(PlantManager a_plantManager, IScenarioDataChanges a_dataChanges);

    internal void ClearAllJobReferences()
    {
        for (int pI = 0; pI < Products.Count; ++pI)
        {
            Product p = Products[pI];
            p.Demands?.ClearAllDemands();
            p.Demands = null;
        }
    }
}
