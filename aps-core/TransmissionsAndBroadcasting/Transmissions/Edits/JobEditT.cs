using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using System.Drawing;

namespace PT.Transmissions;

public class JobEditT : ScenarioIdBaseT, IPTSerializable
{
    #region PT Serialization
    public static int UNIQUE_ID => 1048;

    public JobEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                JobEdit node = new (a_reader);
                m_jobEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ManufacturingOrderEdit node = new (a_reader);
                m_moEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                OperationEdit node = new (a_reader);
                m_opEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ActivityEdit node = new (a_reader);
                m_actEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_jobEdits);
        a_writer.Write(m_moEdits);
        a_writer.Write(m_opEdits);
        a_writer.Write(m_actEdits);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public JobEditT() { }
    public JobEditT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public void Validate()
    {
        foreach (JobEdit jobEdit in m_jobEdits)
        {
            jobEdit.Validate();
        }

        foreach (ManufacturingOrderEdit moEdit in m_moEdits)
        {
            moEdit.Validate();
        }

        foreach (OperationEdit opEdit in m_opEdits)
        {
            opEdit.Validate();
        }

        foreach (ActivityEdit actEdit in m_actEdits)
        {
            actEdit.Validate();
        }
    }

    public override string Description
    {
        get
        {
            //This isn't fully comprehensive, but is a basic message to show that updates have occurred.
            if (m_jobEdits.Count > 0)
            {
                return string.Format("Job edits made ({0})".Localize(), m_jobEdits.Count);
            }

            if (m_moEdits.Count > 0)
            {
                return string.Format("MO edits made ({0})".Localize(), m_moEdits.Count);
            }

            if (m_opEdits.Count > 0)
            {
                return string.Format("Operation edits made ({0})".Localize(), m_opEdits.Count);
            }

            if (m_actEdits.Count > 0)
            {
                return string.Format("Activity edits made ({0})".Localize(), m_actEdits.Count);
            }

            return "No edits made";
        }
    }

    private List<JobEdit> m_jobEdits = new ();

    public List<JobEdit> JobEdits
    {
        get => m_jobEdits;
        set => m_jobEdits = value;
    }

    private List<ManufacturingOrderEdit> m_moEdits = new ();

    public List<ManufacturingOrderEdit> MOEdits
    {
        get => m_moEdits;
        set => m_moEdits = value;
    }

    private List<OperationEdit> m_opEdits = new ();

    public List<OperationEdit> OpEdits
    {
        get => m_opEdits;
        set => m_opEdits = value;
    }

    private List<ActivityEdit> m_actEdits = new ();

    public List<ActivityEdit> ActivityEdits
    {
        get => m_actEdits;
        set => m_actEdits = value;
    }

    public List<BaseId> JobIds
    {
        get
        {
            List<BaseId> ids = new ();
            ids.AddRange(m_jobEdits.Select(x => x.JobId));
            ids.AddRange(m_moEdits.Select(x => x.JobId));
            ids.AddRange(m_opEdits.Select(x => x.JobId));
            ids.AddRange(m_actEdits.Select(x => x.JobId));
            return ids.Distinct().ToList();
        }
    }

    public List<BaseId> MOIds
    {
        get
        {
            List<BaseId> ids = new ();
            ids.AddRange(m_moEdits.Select(x => x.MOId));
            ids.AddRange(m_opEdits.Select(x => x.MOId));
            ids.AddRange(m_actEdits.Select(x => x.MOId));
            return ids.Distinct().ToList();
        }
    }

    public List<BaseId> OpIds
    {
        get
        {
            List<BaseId> ids = new ();
            ids.AddRange(m_opEdits.Select(x => x.OpId));
            ids.AddRange(m_actEdits.Select(x => x.OpId));
            return ids.Distinct().ToList();
        }
    }

    public List<BaseId> ActivityIds
    {
        get { return m_actEdits.Select(x => x.ActivityId).Distinct().ToList(); }
    }
}

public class JobEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId JobId;

    #region PT Serialization
    public JobEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12054
        if (a_reader.VersionNumber >= 12054)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);
            JobId = new BaseId(a_reader);

            a_reader.Read(out int val);
            m_classification = (JobDefs.classifications)val;
            a_reader.Read(out val);
            m_commitment = (JobDefs.commitmentTypes)val;
            a_reader.Read(out m_hotReason);
            a_reader.Read(out m_importance);
            a_reader.Read(out m_latePenaltyCost);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_revenue);
            a_reader.Read(out m_type);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_hold);
            a_reader.Read(out m_holdUntilDateTicks);

            a_reader.Read(out m_entryDateTicks);
            a_reader.Read(out m_maxEarlyDeliverySpanTicks);
            a_reader.Read(out m_almostLateSpanTicks);
            a_reader.Read(out m_needDateTicks);
            a_reader.Read(out m_colorCode);
            a_reader.Read(out m_orderNumber);
            a_reader.Read(out m_customerEmail);
            a_reader.Read(out m_agentEmail);


            a_reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            a_reader.Read(out m_destination);
            a_reader.Read(out m_shippingCost);
            a_reader.Read(out m_lowLevelCode);
        }
        #endregion

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);
            JobId = new BaseId(a_reader);

            a_reader.Read(out int val);
            m_classification = (JobDefs.classifications)val;
            a_reader.Read(out val);
            m_commitment = (JobDefs.commitmentTypes)val;
            a_reader.Read(out m_hotReason);
            a_reader.Read(out m_importance);
            a_reader.Read(out m_latePenaltyCost);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_revenue);
            a_reader.Read(out m_type);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_hold);
            a_reader.Read(out m_holdUntilDateTicks);

            a_reader.Read(out m_entryDateTicks);
            a_reader.Read(out m_maxEarlyDeliverySpanTicks);
            a_reader.Read(out m_almostLateSpanTicks);
            a_reader.Read(out m_needDateTicks);
            a_reader.Read(out m_colorCode);
            a_reader.Read(out m_orderNumber);
            a_reader.Read(out m_customerEmail);
            a_reader.Read(out m_agentEmail);


            a_reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            a_reader.Read(out m_destination);
            // _travelerReport is obsolete now, just discard
            a_reader.Read(out string _travelerReport);
            a_reader.Read(out m_shippingCost);
            a_reader.Read(out m_lowLevelCode);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);
        m_setBools2.Serialize(a_writer);

        JobId.Serialize(a_writer);

        a_writer.Write((int)m_classification);
        a_writer.Write((int)m_commitment);
        a_writer.Write(m_hotReason);
        a_writer.Write(m_importance);
        a_writer.Write(m_latePenaltyCost);
        a_writer.Write(m_priority);
        a_writer.Write(m_revenue);
        a_writer.Write(m_type);
        a_writer.Write(m_holdReason);
        a_writer.Write(m_hold);
        a_writer.Write(m_holdUntilDateTicks);

        a_writer.Write(m_entryDateTicks);
        a_writer.Write(m_maxEarlyDeliverySpanTicks);
        a_writer.Write(m_almostLateSpanTicks);
        a_writer.Write(m_needDateTicks);
        a_writer.Write(m_colorCode);
        a_writer.Write(m_orderNumber);
        a_writer.Write(m_customerEmail);
        a_writer.Write(m_agentEmail);

        a_writer.Write((int)m_shipped);
        a_writer.Write(m_destination);
        a_writer.Write(m_shippingCost);
        a_writer.Write(m_lowLevelCode);
    }

    public new int UniqueId => 1048;
    #endregion

    public JobEdit(BaseId a_jobId)
    {
        JobId = a_jobId;
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet || m_setBools2.AnyFlagsSet;

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_canSpanPlantsIdx = 0;
    private const short c_cancelledIdx = 1;
    private const short c_hotIdx = 2;
    private const short c_reviewedIdx = 3;
    private const short c_invoicedIdx = 4;
    private const short c_printedIdx = 5;
    private const short c_doNotDeleteIdx = 6;
    private const short c_doNotScheduleIdx = 7;
    private const short c_templateIdx = 8;

    //Set bools
    private BoolVector32 m_setBools;
    private const short c_canSpanPlantsSetIdx = 0;
    private const short c_classificationSetIdx = 1;
    private const short c_commitmentSetIdx = 2;
    private const short c_hotReasonSetIdx = 3;
    private const short c_importanceSetIdx = 4;
    private const short c_cancelledSetIdx = 5;
    private const short c_hotSetIdx = 6;
    private const short c_colorSetIdx = 7;
    private const short c_latePenaltyCostSetIdx = 8;
    private const short c_orderNumberSetIdx = 9;
    private const short c_prioritySetIdx = 10;
    private const short c_revenueSetIdx = 11;
    private const short c_typeSetIdx = 12;
    private const short c_reviewedSetIdx = 13;
    private const short c_invoicedSetIdx = 14;
    private const short c_holdReasonSetIdx = 15;
    private const short c_holdUntilDateTicksSetIdx = 16;
    private const short c_printedSetIdx = 17;
    private const short c_destinationSetIdx = 18;
    private const short c_shippedSetIdx = 19;
    private const short c_holdSetIdx = 21;
    private const short c_doNotDeleteSetIdx = 22;
    private const short c_doNotScheduleSetIdx = 23;
    private const short c_templateSetIdx = 24;
    private const short c_entryDateSetIdx = 25;
    private const short c_maxEarlyDeliverySpanTicksSetIdx = 26;
    private const short c_almostLateSpanTicksSetIdx = 27;
    private const short c_needDateTicksSetIdx = 28;
    private const short c_customerEmailSetIdx = 29;
    private const short c_agentEmailSetIdx = 30;
    private const short c_shippingCostSetIdx = 31;

    //Set bools 2
    private BoolVector32 m_setBools2;
    private const short c_lowLevelCodeSetIdx = 0;

    public bool CanSpanPlants
    {
        get => m_bools[c_canSpanPlantsIdx];
        set
        {
            m_bools[c_canSpanPlantsIdx] = value;
            m_setBools[c_canSpanPlantsIdx] = true;
        }
    }

    public bool CanSpanPlantsSet => m_setBools[c_canSpanPlantsSetIdx];

    private JobDefs.classifications m_classification = JobDefs.classifications.ProductionOrder;

    public JobDefs.classifications Classification
    {
        get => m_classification;
        set
        {
            m_classification = value;
            m_setBools[c_classificationSetIdx] = true;
        }
    }

    public bool ClassificationSet => m_setBools[c_classificationSetIdx];

    private JobDefs.commitmentTypes m_commitment = JobDefs.commitmentTypes.Firm;

    public JobDefs.commitmentTypes Commitment
    {
        get => m_commitment;
        set
        {
            m_commitment = value;
            m_setBools[c_commitmentSetIdx] = true;
        }
    }

    public bool CommitmentSet => m_setBools[c_commitmentSetIdx];

    private string m_hotReason = "";

    public string HotReason
    {
        get => m_hotReason;
        set
        {
            m_hotReason = value;
            m_setBools[c_hotReasonSetIdx] = true;
        }
    }

    public bool HotReasonSet => m_setBools[c_hotReasonSetIdx];

    private int m_importance;

    public int Importance
    {
        get => m_importance;
        set
        {
            m_importance = value;
            m_setBools[c_importanceSetIdx] = true;
        }
    }

    public bool ImportanceSet => m_setBools[c_importanceSetIdx];

    public bool Cancelled
    {
        get => m_bools[c_cancelledSetIdx];

        set
        {
            m_bools[c_cancelledSetIdx] = value;
            m_setBools[c_cancelledSetIdx] = true;
        }
    }

    public bool CancelledSet => m_setBools[c_cancelledSetIdx];

    public bool Hot
    {
        get => m_bools[c_hotIdx];
        set
        {
            m_bools[c_hotIdx] = value;
            m_setBools[c_hotSetIdx] = true;
        }
    }

    public bool HotSet => m_setBools[c_hotSetIdx];

    private Color m_colorCode = Color.Empty;

    public Color ColorCode
    {
        get => m_colorCode;
        set
        {
            m_colorCode = value;
            m_setBools[c_colorSetIdx] = true;
        }
    }

    public bool ColorCodeSet => m_setBools[c_colorSetIdx];

    private decimal m_latePenaltyCost;

    public decimal LatePenaltyCost
    {
        get => m_latePenaltyCost;
        set
        {
            m_latePenaltyCost = Math.Max(0, value);
            m_setBools[c_latePenaltyCostSetIdx] = true;
        }
    }

    public bool LatePenaltyCostSet => m_setBools[c_latePenaltyCostSetIdx];

    private string m_orderNumber = "";

    public string OrderNumber
    {
        get => m_orderNumber;
        set
        {
            m_orderNumber = value;
            m_setBools[c_orderNumberSetIdx] = true;
        }
    }

    public bool OrderNumberSet => m_setBools[c_orderNumberSetIdx];

    private int m_priority;

    public int Priority
    {
        get => m_priority;
        set
        {
            m_priority = value;
            m_setBools[c_prioritySetIdx] = true;
        }
    }

    public bool PrioritySet => m_setBools[c_prioritySetIdx];

    private decimal m_revenue;

    public decimal Revenue
    {
        get => m_revenue;
        set
        {
            m_revenue = value;
            m_setBools[c_revenueSetIdx] = true;
        }
    }

    public bool RevenueSet => m_setBools[c_revenueSetIdx];

    private string m_type = "";

    public string Type
    {
        get => m_type;
        set
        {
            m_type = value;
            m_setBools[c_typeSetIdx] = true;
        }
    }

    public bool TypeSet => m_setBools[c_typeSetIdx];

    private bool m_hold;

    public bool Hold
    {
        get => m_hold;
        set
        {
            m_hold = value;
            m_setBools[c_holdSetIdx] = true;
        }
    }

    public bool HoldSet => m_setBools[c_holdSetIdx];

    private string m_holdReason = "";

    public string HoldReason
    {
        get => m_holdReason;
        set
        {
            m_holdReason = value;
            m_setBools[c_holdReasonSetIdx] = true;
        }
    }

    public bool HoldReasonSet => m_setBools[c_holdReasonSetIdx];

    private long m_holdUntilDateTicks = PTDateTime.MinDateTime.Ticks;

    public DateTime HoldUntil
    {
        get => new (m_holdUntilDateTicks);
        set
        {
            m_holdUntilDateTicks = value.Ticks;
            m_setBools[c_holdUntilDateTicksSetIdx] = true;
        }
    }

    public bool HoldUntilSet => m_setBools[c_holdUntilDateTicksSetIdx];

    public bool Reviewed
    {
        get => m_bools[c_reviewedIdx];
        set
        {
            m_bools[c_reviewedIdx] = value;
            m_setBools[c_reviewedSetIdx] = true;
        }
    }

    public bool ReviewedSet => m_setBools[c_reviewedSetIdx];

    public bool Invoiced
    {
        get => m_bools[c_invoicedIdx];
        set
        {
            m_bools[c_invoicedIdx] = value;
            m_setBools[c_invoicedSetIdx] = true;
        }
    }

    public bool InvoicedSet => m_setBools[c_invoicedSetIdx];

    public bool Printed
    {
        get => m_bools[c_printedIdx];
        set
        {
            m_bools[c_printedIdx] = value;
            m_setBools[c_printedSetIdx] = true;
        }
    }

    public bool PrintedSet => m_setBools[c_printedSetIdx];

    private string m_destination = "";

    public string Destination
    {
        get => m_destination;
        set
        {
            m_destination = value;
            m_setBools[c_destinationSetIdx] = true;
        }
    }

    public bool DestinationSet => m_setBools[c_destinationSetIdx];

    private JobDefs.ShippedStatuses m_shipped = JobDefs.ShippedStatuses.NotShipped;

    public JobDefs.ShippedStatuses Shipped
    {
        get => m_shipped;
        set
        {
            m_shipped = value;
            m_setBools[c_shippedSetIdx] = true;
        }
    }

    public bool ShippedSet => m_setBools[c_shippedSetIdx];

    public bool DoNotDelete
    {
        get => m_bools[c_doNotDeleteIdx];
        set
        {
            m_bools[c_doNotDeleteIdx] = value;
            m_setBools[c_doNotDeleteSetIdx] = true;
        }
    }

    public bool DoNotDeleteSet => m_setBools[c_doNotDeleteSetIdx];

    public bool DoNotSchedule
    {
        get => m_bools[c_doNotScheduleIdx];
        set
        {
            m_bools[c_doNotScheduleIdx] = value;
            m_setBools[c_doNotScheduleSetIdx] = true;
        }
    }

    public bool DoNotScheduleSet => m_setBools[c_doNotScheduleSetIdx];

    public bool Template
    {
        get => m_bools[c_templateIdx];
        set
        {
            m_bools[c_templateIdx] = value;
            m_setBools[c_templateSetIdx] = true;
        }
    }

    public bool TemplateSet => m_setBools[c_templateSetIdx];

    private long m_entryDateTicks;

    public DateTimeOffset EntryDate
    {
        get => new (m_entryDateTicks, TimeSpan.Zero);
        set
        {
            PTDateTime.ValidateUtc(value);
            m_entryDateTicks = value.Ticks;
            m_setBools[c_entryDateSetIdx] = true;
        }
    }

    public bool EntryDateSet => m_setBools[c_entryDateSetIdx];

    private long m_maxEarlyDeliverySpanTicks;

    public TimeSpan MaxEarlyDeliverySpan
    {
        get => new (m_maxEarlyDeliverySpanTicks);

        set
        {
            m_maxEarlyDeliverySpanTicks = value.Ticks;
            m_setBools[c_maxEarlyDeliverySpanTicksSetIdx] = true;
        }
    }

    public bool MaxEarlyDeliverySpanSet => m_setBools[c_maxEarlyDeliverySpanTicksSetIdx];

    private long m_almostLateSpanTicks;

    public TimeSpan AlmostLateSpan
    {
        get => new (m_almostLateSpanTicks);
        set
        {
            m_almostLateSpanTicks = value.Ticks;
            m_setBools[c_almostLateSpanTicksSetIdx] = true;
        }
    }

    public bool AlmostLateSpanSet => m_setBools[c_almostLateSpanTicksSetIdx];

    private long m_needDateTicks;

    public DateTime NeedDateTime
    {
        get => new (m_needDateTicks);
        set
        {
            m_needDateTicks = value.Ticks;
            m_setBools[c_needDateTicksSetIdx] = true;
        }
    }

    public bool NeedDateTimeSet => m_setBools[c_needDateTicksSetIdx];

    private string m_customerEmail = "";

    public string CustomerEmail
    {
        get => m_customerEmail;
        set
        {
            m_customerEmail = value;
            m_setBools[c_customerEmailSetIdx] = true;
        }
    }

    public bool CustomerEmailSet => m_setBools[c_customerEmailSetIdx];

    private string m_agentEmail = "";

    public string AgentEmail
    {
        get => m_agentEmail;
        set
        {
            m_agentEmail = value;
            m_setBools[c_agentEmailSetIdx] = true;
        }
    }

    public bool AgentEmailSet => m_setBools[c_agentEmailSetIdx];

    private decimal m_shippingCost;

    public decimal ShippingCost
    {
        get => m_shippingCost;
        set
        {
            m_shippingCost = value;
            m_setBools[c_shippingCostSetIdx] = true;
        }
    }

    public bool ShippingCostSet => m_setBools[c_shippingCostSetIdx];

    private int m_lowLevelCode;

    public int LowLevelCode
    {
        get => m_lowLevelCode;
        set
        {
            m_lowLevelCode = value;
            m_setBools2[c_lowLevelCodeSetIdx] = true;
        }
    }

    public bool LowLevelCodeSet => m_setBools[c_shippingCostSetIdx];
    #endregion

    public void Validate()
    {
        if ((decimal)m_maxEarlyDeliverySpanTicks / TimeSpan.TicksPerDay > 1000)
        {
            throw new PTValidationException("3035", new object[] { m_maxEarlyDeliverySpanTicks });
        }
    }
}

public class ManufacturingOrderEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId JobId;
    public BaseId MOId;

    #region PT Serialization
    public ManufacturingOrderEdit(IReader a_reader)
        : base(a_reader)
    {
        
        if (a_reader.VersionNumber >= 13006)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);

            a_reader.Read(out m_expectedFinishQty);
            a_reader.Read(out m_family);
            a_reader.Read(out m_productDescription);
            a_reader.Read(out m_productName);

            a_reader.Read(out m_requiredQty);
            a_reader.Read(out m_requestedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_holdUntilDateTicks);
            a_reader.Read(out m_moNeedDate); //new in 51
            a_reader.Read(out m_needDateTicks); //new in 51
            a_reader.Read(out m_shippingBufferOverride);
            a_reader.Read(out m_productColor);

            m_breakoffMoId = new BaseId(a_reader);
            a_reader.Read(out m_defaultPathExternalId);

            m_lockedPlantId = new BaseId(a_reader);

            a_reader.Read(out int splitUpdateModeTemp);
            m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;
            a_reader.Read(out m_autoJoinGroup);
        }
        else if (a_reader.VersionNumber >= 13005)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);

            a_reader.Read(out m_expectedFinishQty);
            a_reader.Read(out m_family);
            a_reader.Read(out  string batchGroupName); //Obsolete
            a_reader.Read(out  string batchDefinitionName); //Obsolete
            a_reader.Read(out m_productDescription);
            a_reader.Read(out m_productName);

            a_reader.Read(out m_requiredQty);
            a_reader.Read(out m_requestedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_holdUntilDateTicks);
            a_reader.Read(out m_moNeedDate); //new in 51
            a_reader.Read(out m_needDateTicks); //new in 51
            a_reader.Read(out m_shippingBufferOverride);
            a_reader.Read(out m_productColor);

            m_breakoffMoId = new BaseId(a_reader);
            a_reader.Read(out m_defaultPathExternalId);

            m_lockedPlantId = new BaseId(a_reader);

            a_reader.Read(out int splitUpdateModeTemp);
            m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;
            a_reader.Read(out m_autoJoinGroup);
        }
        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);

            a_reader.Read(out m_expectedFinishQty);
            a_reader.Read(out m_family);
            a_reader.Read(out string batchGroupName);
            a_reader.Read(out string batchDefinitionName);
            a_reader.Read(out m_productDescription);
            a_reader.Read(out m_productName);
            a_reader.Read(out long m_releaseDateTicks);

            a_reader.Read(out m_requiredQty);
            a_reader.Read(out m_requestedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_holdReason);
            a_reader.Read(out m_holdUntilDateTicks);
            a_reader.Read(out m_moNeedDate); //new in 51
            a_reader.Read(out m_needDateTicks); //new in 51
            a_reader.Read(out m_shippingBufferOverride);
            a_reader.Read(out m_productColor);

            m_breakoffMoId = new BaseId(a_reader);
            a_reader.Read(out m_defaultPathExternalId);

            m_lockedPlantId = new BaseId(a_reader);

            a_reader.Read(out int splitUpdateModeTemp);
            m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;
            a_reader.Read(out m_autoJoinGroup);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        JobId.Serialize(a_writer);
        MOId.Serialize(a_writer);

        a_writer.Write(m_expectedFinishQty);
        a_writer.Write(m_family);
        a_writer.Write(m_productDescription);
        a_writer.Write(m_productName);

        a_writer.Write(m_requiredQty);
        a_writer.Write(m_requestedQty);
        a_writer.Write(m_uom);
        a_writer.Write(m_holdReason);
        a_writer.Write(m_holdUntilDateTicks);
        a_writer.Write(m_moNeedDate);
        a_writer.Write(m_needDateTicks);
        a_writer.Write(m_shippingBufferOverride);
        a_writer.Write(m_productColor);

        m_breakoffMoId.Serialize(a_writer);

        a_writer.Write(m_defaultPathExternalId);

        m_lockedPlantId.Serialize(a_writer);

        a_writer.Write((int)m_splitUpdateMode);
        a_writer.Write(m_autoJoinGroup);
    }

    public new int UniqueId => 1048;
    #endregion

    public ManufacturingOrderEdit(BaseId a_jobId, BaseId a_moId)
    {
        JobId = a_jobId;
        MOId = a_moId;
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_canSpanPlantsIdx = 0;
    private const short c_obsoleteIdx = 1; //Released
    private const short c_copyRoutingFromTemplateIdx = 2;
    private const short c_holdIdx = 3;
    private const short c_moNeedDateIdx = 4;
    private const short c_preserveRequiredQtyIdx = 5;
    private const short c_lockedToCurrentAlternatePathIdx = 6;
    private const short c_resizeForStorageIdx = 7;

    //Set bools
    private BoolVector32 m_setBools;
    private const short c_canSpanPlantsSetIdx = 0;
    private const short c_expectedFinishQtySetIdx = 1;
    private const short c_familySetIdx = 2;
    private const short c_batchDefinitionsNameSetIdx = 3; //Obsolete
    private const short c_batchGroupNameSetIdx = 4; //Obsolete
    private const short c_needDateTicksSet = 5;
    private const short c_obsolete2Idx = 6; //ReleasedSet
    private const short c_productDescriptionSetIdx = 7;
    private const short c_productNameSetIdx = 8;
    private const short c_productColorSetIdx = 9;
    private const short c_obsolete3SetIdx = 10; //releaseDate
    private const short c_copyRoutingFromTemplateSetIdx = 11;
    private const short c_requiredQtySetIdx = 12;
    private const short c_uomSetIdx = 13;
    private const short c_autoJoinGroupSetIdx = 14;
    private const short c_requestedQtySetIdx = 15;
    private const short c_holdReasonSetIdx = 16;
    private const short c_holdUntilDateTicksSetIdx = 17;
    private const short c_holdSetIdx = 18;
    private const short c_moNeedDateSetIdx = 19;
    private const short c_shippingBufferOverrideSetIdx = 20;
    private const short c_preserveRequiredQtySetIdx = 21;
    private const short c_defaultPathExternalIdSetIdx = 22;
    private const short c_breakoffMoIdSetIdx = 23;
    private const short c_lockedPlantIdSetIdx = 24;
    private const short c_splitUpdateModeSetIdx = 25;
    private const short c_lockedToCurrentAlternatePathSetIdx = 26;
    private const short c_resizeForStorageIsSetIdx = 27;

    public bool CanSpanPlants
    {
        get => m_bools[c_canSpanPlantsIdx];
        set
        {
            m_bools[c_canSpanPlantsIdx] = value;
            m_setBools[c_canSpanPlantsIdx] = true;
        }
    }

    public bool CanSpanPlantsSet => m_setBools[c_canSpanPlantsSetIdx];

    private decimal m_expectedFinishQty;

    public decimal ExpectedFinishQty
    {
        get => m_expectedFinishQty;
        set
        {
            m_expectedFinishQty = value;
            m_setBools[c_expectedFinishQtySetIdx] = true;
        }
    }

    public bool ExpectedFinishQtySet => m_setBools[c_expectedFinishQtySetIdx];

    private string m_family = "";

    public string Family
    {
        get => m_family;
        set
        {
            m_family = value;
            m_setBools[c_familySetIdx] = true;
        }
    }

    public bool FamilySet => m_setBools[c_familySetIdx];

    private long m_needDateTicks;

    public DateTime NeedDate
    {
        get => new (m_needDateTicks);
        set
        {
            m_needDateTicks = value.Ticks;
            m_setBools[c_needDateTicksSet] = true;
        }
    }

    public bool NeedDateSet => m_setBools[c_needDateTicksSet];

    private string m_productDescription = "";

    public string ProductDescription
    {
        get => m_productDescription;
        set
        {
            m_productDescription = value;
            m_setBools[c_productDescriptionSetIdx] = true;
        }
    }

    public bool ProductDescriptionSet => m_setBools[c_needDateTicksSet];

    private string m_productName;

    public string ProductName
    {
        get => m_productName;
        set
        {
            m_productName = value;
            m_setBools[c_productNameSetIdx] = true;
        }
    }

    public bool ProductNameSet => m_setBools[c_productNameSetIdx];

    private Color m_productColor;

    public Color ProductColor
    {
        get => m_productColor;
        set
        {
            m_productColor = value;
            m_setBools[c_productColorSetIdx] = true;
        }
    }

    public bool ProductColorSet => m_setBools[c_productColorSetIdx];

    public bool CopyRoutingFromTemplate
    {
        get => m_bools[c_copyRoutingFromTemplateIdx];
        set
        {
            m_bools[c_copyRoutingFromTemplateIdx] = value;
            m_setBools[c_copyRoutingFromTemplateSetIdx] = true;
        }
    }

    public bool CopyRoutingFromTemplateSet => m_setBools[c_copyRoutingFromTemplateSetIdx];

    private decimal m_requiredQty;

    public decimal RequiredQty
    {
        get => m_requiredQty;

        set
        {
            m_requiredQty = value;
            m_setBools[c_requiredQtySetIdx] = true;
        }
    }

    public bool RequiredQtySet => m_setBools[c_requiredQtySetIdx];

    private string m_uom = "";

    public string UOM
    {
        get => m_uom;
        set
        {
            m_uom = value;
            m_setBools[c_uomSetIdx] = true;
        }
    }

    public bool UOMSet => m_setBools[c_uomSetIdx];

    private string m_autoJoinGroup;

    public string AutoJoinGroup
    {
        get => m_autoJoinGroup;
        set
        {
            m_autoJoinGroup = value;
            m_setBools[c_autoJoinGroupSetIdx] = true;
        }
    }

    public bool AutoJoinGroupSet => m_setBools[c_autoJoinGroupSetIdx];

    /// <summary>
    /// Whether the MO should adjust its quantity up or down to fill storage
    /// </summary>
    public bool ResizeForStorage
    {
        get => m_bools[c_resizeForStorageIdx];
        set
        {
            m_bools[c_resizeForStorageIdx] = value;
            m_setBools[c_resizeForStorageIsSetIdx] = true;
        }
    }

    public bool ResizeForStorageIsSet => m_setBools[c_resizeForStorageIsSetIdx];

    private decimal m_requestedQty;

    public decimal RequestedQty
    {
        get => m_requestedQty;
        set
        {
            m_requestedQty = value;
            m_setBools[c_requestedQtySetIdx] = true;
        }
    }

    public bool RequestedQtySet => m_setBools[c_requestedQtySetIdx];

    private string m_holdReason;

    public string HoldReason
    {
        get => m_holdReason;
        set
        {
            m_holdReason = value;
            m_setBools[c_holdReasonSetIdx] = true;
        }
    }

    public bool HoldReasonSet => m_setBools[c_holdReasonSetIdx];

    private long m_holdUntilDateTicks;

    public DateTime HoldUntil
    {
        get => new (m_holdUntilDateTicks);
        set
        {
            m_holdUntilDateTicks = SQLServerConversions.GetValidDateTime(value.Ticks);
            m_setBools[c_holdUntilDateTicksSetIdx] = true;
        }
    }

    public bool HoldUntilSet => m_setBools[c_holdUntilDateTicksSetIdx];

    public bool Hold
    {
        get => m_bools[c_holdIdx];
        set
        {
            m_bools[c_holdIdx] = value;
            m_setBools[c_holdSetIdx] = true;
        }
    }

    public bool HoldSet => m_setBools[c_holdSetIdx];

    private readonly bool m_moNeedDate;

    public bool MoNeedDate
    {
        get => m_bools[c_moNeedDateIdx];
        set
        {
            m_bools[c_moNeedDateIdx] = value;
            m_setBools[c_moNeedDateSetIdx] = true;
        }
    }

    public bool MoNeedDateSet => m_setBools[c_moNeedDateSetIdx];

    private long m_shippingBufferOverride;

    public long ShippingBufferOverrideTicks
    {
        get => m_shippingBufferOverride;
        set
        {
            m_shippingBufferOverride = value;
            m_setBools[c_shippingBufferOverrideSetIdx] = true;
        }
    }

    public bool ShippingBufferOverrideTicksSet => m_setBools[c_shippingBufferOverrideSetIdx];

    public bool PreserveRequiredQty
    {
        get => m_bools[c_preserveRequiredQtyIdx];
        set
        {
            m_bools[c_preserveRequiredQtyIdx] = value;
            m_setBools[c_preserveRequiredQtySetIdx] = true;
        }
    }

    public bool PreserveRequiredQtySet => m_setBools[c_preserveRequiredQtySetIdx];

    private string m_defaultPathExternalId;

    public string DefaultPathExternalId
    {
        get => m_defaultPathExternalId;
        set
        {
            m_defaultPathExternalId = value;
            m_setBools[c_defaultPathExternalIdSetIdx] = true;
        }
    }

    public bool DefaultPathExternalIdSet => m_setBools[c_defaultPathExternalIdSetIdx];

    private BaseId m_breakoffMoId;

    public BaseId BreakoffMoId
    {
        get => m_breakoffMoId;
        set
        {
            m_breakoffMoId = value;
            m_setBools[c_breakoffMoIdSetIdx] = true;
        }
    }

    public bool BreakoffMoIdSet => m_setBools[c_breakoffMoIdSetIdx];

    private BaseId m_lockedPlantId;

    public BaseId LockedPlantId
    {
        get => m_lockedPlantId;
        set
        {
            m_lockedPlantId = value;
            m_setBools[c_lockedPlantIdSetIdx] = true;
        }
    }

    public bool LockedPlantIdSet => m_setBools[c_lockedPlantIdSetIdx];

    private ManufacturingOrderDefs.SplitUpdateModes m_splitUpdateMode;

    public ManufacturingOrderDefs.SplitUpdateModes SplitUpdateMode
    {
        get => m_splitUpdateMode;
        set
        {
            m_splitUpdateMode = value;
            m_setBools[c_splitUpdateModeSetIdx] = true;
        }
    }

    public bool SplitUpdateModeSet => m_setBools[c_splitUpdateModeSetIdx];

    public bool LockedToCurrentAlternatePath
    {
        get => m_bools[c_lockedToCurrentAlternatePathIdx];
        set
        {
            m_bools[c_lockedToCurrentAlternatePathIdx] = value;
            m_setBools[c_lockedToCurrentAlternatePathSetIdx] = true;
        }
    }

    public bool LockedToCurrentAlternatePathSet => m_setBools[c_lockedToCurrentAlternatePathSetIdx];
    #endregion

    public void Validate()
    {

    }
}

public class OperationEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId JobId;
    public BaseId MOId;
    public BaseId OpId;

    #region PT Serialization
    public OperationEdit(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 13013)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);
            m_isSetBools3 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_standardStorageSpan);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int setupSplitTypeVal);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitTypeVal;
            a_reader.Read(out m_campaignCode);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_productionCleanoutCost);
            a_reader.Read(out m_standardBufferDays);
            a_reader.Read(out m_sequenceHeadStartDays);
        }
        else if (a_reader.VersionNumber >= 13008)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_standardStorageSpan);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int setupSplitTypeVal);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitTypeVal;
            a_reader.Read(out m_campaignCode);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_productionCleanoutCost);
            a_reader.Read(out m_standardBufferDays);
            a_reader.Read(out m_sequenceHeadStartDays);
        }
        else if (a_reader.VersionNumber >= 13005)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_standardStorageSpan);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int setupSplitTypeVal);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitTypeVal;
            a_reader.Read(out m_campaignCode);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_productionCleanoutCost);
            a_reader.Read(out m_standardBufferDays);
        }
        else if (a_reader.VersionNumber >= 12526)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_standardStorageSpan);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int setupSplitTypeVal);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitTypeVal;
            a_reader.Read(out m_campaignCode);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_productionCleanoutCost);
        }
        else if (a_reader.VersionNumber >= 12508)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int setupSplitTypeVal);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitTypeVal;
            a_reader.Read(out m_campaignCode);
        }
        else if (a_reader.VersionNumber >= 12502)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int setupSplitTypeVal);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitTypeVal;
        }
        else if (a_reader.VersionNumber >= 12316)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
            a_reader.Read(out int setupSplitTypeVal);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitTypeVal;
        }
        else if (a_reader.VersionNumber >= 12306)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out short autoSplitTypeVal);
            m_autoAutoSplitType = (OperationDefs.EAutoSplitType)autoSplitTypeVal;
            a_reader.Read(out m_maxAutoSplitAmount);
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_cleanoutGrade);
        }

        #region 12302
        else if (a_reader.VersionNumber >= 12302)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
            a_reader.Read(out m_productCode);
        }
        #endregion

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);

            //Base properties
            a_reader.Read(out m_holdReason);
            a_reader.Read(out int val);
            m_omitted = (BaseOperationDefs.omitStatuses)val;
            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_outputName);
            a_reader.Read(out m_holdUntilDate);
            a_reader.Read(out m_plannedScrapQty);

            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_minAutoSplitAmount);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            a_reader.Read(out m_keepSuccessorTimeLimit);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);
        m_setBools2.Serialize(a_writer);
        m_isSetBools3.Serialize(a_writer);

        JobId.Serialize(a_writer);
        MOId.Serialize(a_writer);
        OpId.Serialize(a_writer);

        a_writer.Write(m_holdReason);
        a_writer.Write((int)m_omitted);
        a_writer.Write(m_requiredFinishQty);
        a_writer.Write(m_uom);
        a_writer.Write(m_outputName);
        a_writer.Write(m_holdUntilDate);
        a_writer.Write(m_plannedScrapQty);

        a_writer.Write(m_carryingCost);
        a_writer.Write(m_compatibilityCode);
        a_writer.Write(m_overlapTransferQty);
        a_writer.Write(m_minAutoSplitAmount);
        a_writer.Write(m_setupNumber);

        a_writer.Write((int)m_successorProcessing);

        a_writer.Write(m_keepSuccessorTimeLimit);

        a_writer.Write(m_planningScrapPercent);
        a_writer.Write(m_setupSpanTicks);
        a_writer.Write(m_cycleSpanTicks);
        a_writer.Write(m_postProcessingSpanTicks);
        a_writer.Write(m_qtyPerCycle);
        a_writer.Write(m_materialPostProcessingSpanTicks);

        a_writer.Write(m_setupColor);

        a_writer.Write(m_standardRunSpan);
        a_writer.Write(m_standardSetupSpan);

        a_writer.Write(m_batchCode);
        a_writer.Write(m_productCode);
        a_writer.Write(m_standardCleanSpan);
        a_writer.Write(m_cleanoutGrade);
        a_writer.Write(m_standardStorageSpan);
        a_writer.Write((short)m_autoAutoSplitType);
        a_writer.Write(m_maxAutoSplitAmount);
        a_writer.Write((int)m_setupSplitType);
        a_writer.Write(m_campaignCode);
        a_writer.Write(m_productionSetupCost);
        a_writer.Write(m_productionCleanoutCost);
        a_writer.Write(m_standardBufferDays);
        a_writer.Write(m_sequenceHeadStartDays);
    }

    public new int UniqueId => 1048;
    #endregion

    public OperationEdit(BaseId a_jobId, BaseId a_moId, BaseId a_opId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OpId = a_opId;
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet || m_setBools2.AnyFlagsSet;

    #region Shared Properties
    //Bools
    private BoolVector32 m_bools;
    private const short c_onlyAllowManualUpdatesToPlanningScrapPercentIdx = 0;
    private const short c_onlyAllowManualUpdatesToSetupSpanIdx = 1;
    private const short c_onlyAllowManualUpdatesToCycleSpanIdx = 2;
    private const short c_onlyAllowManualUpdatesToPostProcessingSpanIdx = 3;
    private const short c_onlyAllowManualUpdatesToQtyPerCycleIdx = 4;
    private const short c_onlyAllowManualUpdatesToResReqtsIdx = 5;
    private const short c_onlyAllowManualUpdatesToMaterialPostProcessingSpanIdx = 6;
    private const short c_onlyAllowManualUpdatesToSplitOperationIdx = 7;
    private const short c_useCompatibilityCodeIdx = 8;
    private const short c_setupColorSetIdx = 9;
    private const short c_autoSplitIdx = 10;
    private const short c_canPauseIdx = 11;
    private const short c_canSubcontractIdx = 12;
    private const short c_wholeNumberSplitsIdx = 13;
    private const short c_deductScrapFromRequiredIdx = 14;
    private const short c_timeBasedReportingIdx = 15;
    private const short c_canResizeIdx = 16;
    private const short c_onHoldIdx = 17;
    private const short c_isReworkIdx = 18;
    private const short c_useExpectedFinishQtyIdx = 19;
    private const short c_autoReportProgressFlagIdx = 20;
    private const short c_autoFinishIdx = 21;
    private const short c_onlyAllowManualUpdatesToMaterialsIdx = 22;
    private const short c_onlyAllowManualUpdatesToProductsIdx = 23;
    private const short c_onlyAllowManualUpdatesToCleanIdx = 24;
    private const short c_preventSplitsFromIncurringSetupIdx = 25;
    private const short c_preventSplitsFromIncurringCleanIdx = 26;
    private const short c_allowExpiredSupplyIdx = 27;
    private const short c_allowSameLotInNonEmptyStorageAreaIdx = 28;
    private const short c_keepSplitsOnSameResourceIdx = 29;

    private decimal m_plannedScrapQty;

    public decimal PlannedScrapQty
    {
        get => m_plannedScrapQty;
        set
        {
            m_plannedScrapQty = value;
            m_setBools2[c_plannedScrapQtySetIdx] = true;
        }
    }

    private long m_holdUntilDate = PTDateTime.MinDateTime.Ticks;

    public DateTime HoldUntil
    {
        get => new (m_holdUntilDate);
        set
        {
            m_holdUntilDate = value.Ticks;
            m_setBools2[c_holdUntilSetIdx] = true;
        }
    }

    private string m_holdReason;

    public string HoldReason
    {
        get => m_holdReason;
        set
        {
            m_holdReason = value;
            m_setBools2[c_holdReasonSetIdx] = true;
        }
    }

    private BaseOperationDefs.omitStatuses m_omitted = BaseOperationDefs.omitStatuses.NotOmitted;

    public BaseOperationDefs.omitStatuses Omitted
    {
        get => m_omitted;
        set
        {
            m_omitted = value;
            m_setBools2[c_omittedSetIdx] = true;
        }
    }

    public bool OnHold
    {
        get => m_bools[c_onHoldIdx];
        set
        {
            m_bools[c_onHoldIdx] = value;
            m_setBools2[c_onHoldSetIdx] = true;
        }
    }

    public bool IsRework
    {
        get => m_bools[c_isReworkIdx];
        set
        {
            m_bools[c_isReworkIdx] = value;
            m_setBools2[c_isReworkSetIdx] = true;
        }
    }

    private decimal m_requiredFinishQty;

    public decimal RequiredFinishQty
    {
        get => m_requiredFinishQty;
        set
        {
            m_requiredFinishQty = value;
            m_setBools2[c_requiredFinishQtySetIdx] = true;
        }
    }

    private string m_uom;

    public string UOM
    {
        get => m_uom;
        set
        {
            m_uom = value;
            m_setBools2[c_uomSetIdx] = true;
        }
    }

    public bool UseExpectedFinishQty
    {
        get => m_bools[c_useExpectedFinishQtyIdx];
        set
        {
            m_bools[c_useExpectedFinishQtyIdx] = value;
            m_setBools2[c_useExpectedFinishQtySetIdx] = true;
        }
    }

    private string m_outputName;

    public string OutputName
    {
        get => m_outputName;
        set
        {
            m_outputName = value;
            m_setBools2[c_outputNameSetIdx] = true;
        }
    }

    public bool AutoReportProgress
    {
        get => m_bools[c_autoReportProgressFlagIdx];
        set
        {
            m_bools[c_autoReportProgressFlagIdx] = value;
            m_setBools2[c_autoReportProgressFlagSetIdx] = true;
        }
    }

    public bool AutoFinish
    {
        get => m_bools[c_autoFinishIdx];
        set
        {
            m_bools[c_autoFinishIdx] = value;
            m_setBools2[c_autoFinishSetIdx] = true;
        }
    }

    public bool AutoSplit
    {
        get => m_bools[c_autoSplitIdx];
        set
        {
            m_bools[c_autoSplitIdx] = value;
            m_setBools[c_autoSplitSetIdx] = true;
        }
    }

    public bool CanPause
    {
        get => m_bools[c_canPauseIdx];
        set
        {
            m_bools[c_canPauseIdx] = value;
            m_setBools[c_canPauseSetIdx] = true;
        }
    }

    public bool TimeBasedReporting
    {
        get => m_bools[c_timeBasedReportingIdx];
        set
        {
            m_bools[c_timeBasedReportingIdx] = value;
            m_setBools[c_timeBasedReportingSetIdx] = true;
        }
    }

    public bool CanSubcontract
    {
        get => m_bools[c_canSubcontractIdx];
        set
        {
            m_bools[c_canSubcontractIdx] = value;
            m_setBools[c_canSubcontractSetIdx] = true;
        }
    }

    public bool WholeNumberSplits
    {
        get => m_bools[c_wholeNumberSplitsIdx];
        set
        {
            m_bools[c_wholeNumberSplitsIdx] = value;
            m_setBools[c_wholeNumberSplitsSetIdx] = true;
        }
    }

    public bool DeductScrapFromRequired
    {
        get => m_bools[c_deductScrapFromRequiredIdx];
        set
        {
            m_bools[c_deductScrapFromRequiredIdx] = value;
            m_setBools[c_deductScrapFromRequiredSetIdx] = true;
        }
    }

    public bool CanResize
    {
        get => m_bools[c_canResizeIdx];
        set
        {
            m_bools[c_canResizeIdx] = value;
            m_setBools2[c_canResizeSetIdx] = true;
        }
    }

    private Color m_setupColor;

    public Color SetupColor
    {
        get => m_setupColor;
        set
        {
            m_setupColor = value;
            m_setBools2[c_setupColorSetIdx] = true;
        }
    }

    private decimal m_carryingCost;

    public decimal CarryingCost
    {
        get => m_carryingCost;
        set
        {
            m_carryingCost = value;
            m_setBools[c_carryingCostSetIdx] = true;
        }
    }

    private string m_compatibilityCode;

    /// <summary>
    /// Used to restrict Resources to only perform compatible work at simulataneous times. If specified, then any scheduled Operation's CompatibilityCode must match the CompatibilityCode of other Operations
    /// scheduled on Resources with the same CompatibilityGroup.
    /// </summary>
    public string CompatibilityCode
    {
        get => m_compatibilityCode;
        set
        {
            m_compatibilityCode = value;
            m_setBools[c_compatibilityCodeSetIdx] = true;
        }
    }

    public bool UseCompatibilityCode
    {
        get => m_bools[c_useCompatibilityCodeIdx];
        set
        {
            m_bools[c_useCompatibilityCodeIdx] = value;
            m_setBools[c_useCompatibilityCodeSetIdx] = true;
        }
    }

    private long m_cycleSpanTicks;

    public TimeSpan CycleSpan
    {
        get => new (m_cycleSpanTicks);
        set
        {
            m_cycleSpanTicks = value.Ticks;
            m_setBools[c_cycleSpanTicksSetIdx] = true;
        }
    }

    private decimal m_overlapTransferQty;

    public decimal OverlapTransferQty
    {
        get => m_overlapTransferQty;
        set
        {
            m_overlapTransferQty = value;
            m_setBools[c_overlapTransferQtySetIdx] = true;
        }
    }

    private long m_postProcessingSpanTicks;

    public TimeSpan PostProcessingSpan
    {
        get => new (m_postProcessingSpanTicks);
        set
        {
            m_postProcessingSpanTicks = value.Ticks;
            m_setBools[c_postProcessingSpanTicksSetIdx] = true;
        }
    }

    private string m_batchCode;

    public string BatchCode
    {
        get => m_batchCode;
        set
        {
            m_batchCode = value;
            m_setBools[c_batchCodeSetIdx] = true;
        }
    }


    private decimal m_setupNumber;

    public decimal SetupNumber
    {
        get => m_setupNumber;

        set
        {
            m_setupNumber = value;
            m_setBools[c_setupNumberSetIdx] = true;
        }
    }

    private OperationDefs.successorProcessingEnumeration m_successorProcessing = OperationDefs.successorProcessingEnumeration.NoPreference;

    public OperationDefs.successorProcessingEnumeration SuccessorProcessing
    {
        get => m_successorProcessing;
        set
        {
            m_successorProcessing = value;
            m_setBools[c_successorProcessingSetIdx] = true;
        }
    }

    private long m_keepSuccessorTimeLimit;

    public TimeSpan KeepSuccessorsTimeLimit
    {
        get => new (m_keepSuccessorTimeLimit);

        set
        {
            m_keepSuccessorTimeLimit = value.Ticks;
            m_setBools[c_keepSuccessorTimeLimitSetIdx] = true;
        }
    }

    private decimal m_planningScrapPercent;

    public decimal PlanningScrapPercent
    {
        get => m_planningScrapPercent;
        set
        {
            m_planningScrapPercent = value;
            m_setBools[c_planningScrapPercentSetIdx] = true;
        }
    }

    private long m_setupSpanTicks;

    public long SetupSpanTicks
    {
        get => m_setupSpanTicks;
        set
        {
            m_setupSpanTicks = value;
            m_setBools[c_setupSpanTicksSetIdx] = true;
        }
    }

    private decimal m_qtyPerCycle;

    public decimal QtyPerCycle
    {
        get => m_qtyPerCycle;
        set
        {
            m_qtyPerCycle = value;
            m_setBools[c_qtyPerCycleSetIdx] = true;
        }
    }

    private double m_standardBufferDays;

    public double StandardBufferDays
    {
        get => m_standardBufferDays;
        set
        {
            m_standardBufferDays = value;
            m_setBools2[c_minBufferDaysSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToPlanningScrapPercent
    {
        get => m_bools[c_onlyAllowManualUpdatesToPlanningScrapPercentIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToPlanningScrapPercentIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToPlanningScrapPercentSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToSetupSpan
    {
        get => m_bools[c_onlyAllowManualUpdatesToSetupSpanIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToSetupSpanIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToSetupSpanSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToCycleSpan
    {
        get => m_bools[c_onlyAllowManualUpdatesToCycleSpanIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToCycleSpanIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToCycleSpanSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToPostProcessingSpan
    {
        get => m_bools[c_onlyAllowManualUpdatesToPostProcessingSpanIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToPostProcessingSpanIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToPostProcessingSpanSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToCleanSpan
    {
        get => m_bools[c_onlyAllowManualUpdatesToCleanIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToCleanIdx] = value;
            m_setBools2[c_onlyAllowManualUpdatesToCleanSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToMaterialPostProcessingSpan
    {
        get => m_bools[c_onlyAllowManualUpdatesToMaterialPostProcessingSpanIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToMaterialPostProcessingSpanIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToMaterialPostProcessingSpanSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToQtyPerCycle
    {
        get => m_bools[c_onlyAllowManualUpdatesToQtyPerCycleIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToQtyPerCycleIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToQtyPerCycleSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToResourceRequirements
    {
        get => m_bools[c_onlyAllowManualUpdatesToResReqtsIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToResReqtsIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToResReqtsSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToSplitOperation
    {
        get => m_bools[c_onlyAllowManualUpdatesToSplitOperationIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToSplitOperationIdx] = value;
            m_setBools[c_onlyAllowManualUpdatesToSplitOperationSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToProducts
    {
        get => m_bools[c_onlyAllowManualUpdatesToProductsIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToProductsIdx] = value;
            m_setBools2[c_onlyAllowManualUpdatesToProductsSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToMaterials
    {
        get => m_bools[c_onlyAllowManualUpdatesToMaterialsIdx];
        set
        {
            m_bools[c_onlyAllowManualUpdatesToMaterialsIdx] = value;
            m_setBools2[c_onlyAllowManualUpdatesToMaterialsSetIdx] = true;
        }
    }

    private long m_materialPostProcessingSpanTicks;

    public long MaterialPostProcessingSpanTicks
    {
        get => m_materialPostProcessingSpanTicks;
        set
        {
            m_materialPostProcessingSpanTicks = value;
            m_setBools[c_materialPostProcessingSpanTicksSetIdx] = true;
        }
    }

    public long UnfinishedActivityMaterialPostProcessingTicks { get; set; }

    private TimeSpan m_standardRunSpan;

    public TimeSpan StandardRunSpan
    {
        get => m_standardRunSpan;
        set
        {
            m_standardRunSpan = value;
            m_setBools[c_standardRunSpanSetIdx] = true;
        }
    }

    private TimeSpan m_standardSetupSpan;

    public TimeSpan StandardSetupSpan
    {
        get => m_standardSetupSpan;
        set
        {
            m_standardSetupSpan = value;
            m_setBools[c_standardSetupSpanIsSetIdx] = true;
        }
    }

    private TimeSpan m_standardCleanSpan;

    public TimeSpan StandardCleanSpan
    {
        get => m_standardCleanSpan;
        set
        {
            m_standardCleanSpan = value;
            m_setBools2[c_standardCleanSpanIsSetIdx] = true;
        }
    }

    private TimeSpan m_standardStorageSpan;

    public TimeSpan StandardStorageSpan
    {
        get => m_standardStorageSpan;
        set
        {
            m_standardStorageSpan = value;
            m_setBools[c_standardStorageSpanIsSetIdx] = true;
        }
    }

    private string m_productCode = "";

    public string ProductCode
    {
        get => m_productCode;
        set
        {
            m_productCode = value;
            m_setBools2[c_productCodeSetIdx] = true;
        }
    }

    private int m_cleanoutGrade;

    public int CleanoutGrade
    {
        get => m_cleanoutGrade;
        set
        {
            m_cleanoutGrade = value;
            m_setBools2[c_cleanoutGradeIsSetIdx] = true;
        }
    }

    private OperationDefs.EAutoSplitType m_autoAutoSplitType;

    public OperationDefs.EAutoSplitType AutoSplitType
    {
        get => m_autoAutoSplitType;
        set
        {
            m_autoAutoSplitType = value;
            m_setBools2[c_autoSplitTypeIsSetIdx] = true;
        }
    }

    private OperationDefs.ESetupSplitType m_setupSplitType;

    public OperationDefs.ESetupSplitType SetupSplitType
    {
        get => m_setupSplitType;
        set
        {
            m_setupSplitType = value;
            m_setBools2[c_setupSplitTypeIsSetIdx] = true;
        }
    }

    private decimal m_minAutoSplitAmount;

    public decimal MinAutoSplitAmount
    {
        get => m_minAutoSplitAmount;
        set
        {
            m_minAutoSplitAmount = value;
            m_setBools[c_minAutoSplitQtyIsSetIdx] = true;
        }
    }

    private decimal m_maxAutoSplitAmount;

    public decimal MaxAutoSplitAmount
    {
        get => m_maxAutoSplitAmount;
        set
        {
            m_maxAutoSplitAmount = value;
            m_setBools2[c_maxAutoSplitAmountIsSetIdx] = true;
        }
    }
    public bool PreventSplitsFromIncurringSetup
    {
        get => m_bools[c_preventSplitsFromIncurringSetupIdx];
        set
        {
            m_bools[c_preventSplitsFromIncurringSetupIdx] = value;
            m_setBools2[c_preventSplitsFromIncurringSetupIsSetIdx] = true;
        }
    }
    public bool PreventSplitsFromIncurringClean
    {
        get => m_bools[c_preventSplitsFromIncurringCleanIdx];
        set
        {
            m_bools[c_preventSplitsFromIncurringCleanIdx] = value;
            m_setBools2[c_preventSplitsFromIncurringCleanIsSetIdx] = true;
        }
    }

    /// <summary>
    /// Represents the campaign identifier for an operation. 
    /// A campaign is defined as a sequence of operations on a resource that produce the same product or belong to a specific campaign group.
    /// 
    /// This property is used to group operations as part of a campaign and is critical for optimization factors that prioritize or enforce campaign scheduling.
    /// </summary>
    private string m_campaignCode = "";
    public string CampaignCode
    {
        get => m_campaignCode;
        set
        {
            m_campaignCode = value;
            m_setBools2[c_campaignCodeIdx] = true;
        }
    }

    private decimal m_productionSetupCost;

    public decimal ProductionSetupCost
    {
        get => m_productionSetupCost;
        set
        {
            m_productionSetupCost = value;
            m_setBools2[c_productionSetupCostIsSetIdx] = true;
        }
    }

    private decimal m_productionCleanoutCost;

    public decimal ProductionCleanoutCost
    {
        get => m_productionCleanoutCost;
        set
        {
            m_productionCleanoutCost = value;
            m_setBools2[c_productionCleanoutCostIsSetIdx] = true;
        }
    }

    public bool AllowExpiredSupply
    {
        get => m_bools[c_allowExpiredSupplyIdx];
        set
        {
            m_bools[c_allowExpiredSupplyIdx] = value;
            m_setBools2[c_allowExpiredSupplySetIdx] = true;
        }
    }

    private double m_sequenceHeadStartDays;
    public double SequenceHeadStartDays
    {
        get => m_sequenceHeadStartDays;
        set
        {
            m_sequenceHeadStartDays = value;
            m_setBools2[c_sequenceHeadStartDaysIsSetIdx] = true;
        }
    }

    public bool AllowSameLotInNonEmptyStorageArea
    {
        get => m_bools[c_allowSameLotInNonEmptyStorageAreaIdx];
        set
        {
            m_bools[c_allowSameLotInNonEmptyStorageAreaIdx] = value;
            m_setBools2[c_allowSameLotInNonEmptyStorageAreaIsSetIdx] = true;
        }
    }

    public bool KeepSplitsOnSameResource
    {
        get => m_bools[c_keepSplitsOnSameResourceIdx];
        set
        {
            m_bools[c_keepSplitsOnSameResourceIdx] = value;
            m_isSetBools3[c_keepSplitsOnSameResourceIsSetIdx] = true;
        }
    }
    #endregion

    #region IsSet
    //Set bools
    private BoolVector32 m_setBools;
    private const short c_carryingCostSetIdx = 0;
    private const short c_compatibilityCodeSetIdx = 1;
    private const short c_useCompatibilityCodeSetIdx = 2;
    private const short c_cycleSpanTicksSetIdx = 3;
    private const short c_overlapTransferQtySetIdx = 4;
    private const short c_minAutoSplitQtyIsSetIdx = 5;
    private const short c_postProcessingSpanTicksSetIdx = 6;
    private const short c_batchCodeSetIdx = 7;
    //private const short c_setupCodeSetIdx = 8;
    private const short c_setupNumberSetIdx = 9;
    private const short c_successorProcessingSetIdx = 10;
    private const short c_keepSuccessorTimeLimitSetIdx = 11;
    private const short c_planningScrapPercentSetIdx = 12;
    private const short c_setupSpanTicksSetIdx = 13;
    private const short c_qtyPerCycleSetIdx = 14;
    private const short c_onlyAllowManualUpdatesToPlanningScrapPercentSetIdx = 15;
    private const short c_onlyAllowManualUpdatesToSetupSpanSetIdx = 16;
    private const short c_onlyAllowManualUpdatesToCycleSpanSetIdx = 17;
    private const short c_onlyAllowManualUpdatesToPostProcessingSpanSetIdx = 18;
    private const short c_onlyAllowManualUpdatesToQtyPerCycleSetIdx = 19;
    private const short c_onlyAllowManualUpdatesToResReqtsSetIdx = 20;
    private const short c_onlyAllowManualUpdatesToMaterialPostProcessingSpanSetIdx = 21;
    private const short c_onlyAllowManualUpdatesToSplitOperationSetIdx = 22;
    private const short c_materialPostProcessingSpanTicksSetIdx = 23;
    private const short c_standardRunSpanSetIdx = 24;
    private const short c_standardSetupSpanIsSetIdx = 25;
    private const short c_autoSplitSetIdx = 26;
    private const short c_canPauseSetIdx = 27;
    private const short c_canSubcontractSetIdx = 28;
    private const short c_wholeNumberSplitsSetIdx = 29;
    private const short c_deductScrapFromRequiredSetIdx = 30;
    private const short c_timeBasedReportingSetIdx = 31;

    private BoolVector32 m_setBools2;
    private const short c_canResizeSetIdx = 0;
    private const short c_holdReasonSetIdx = 1;
    private const short c_omittedSetIdx = 2;
    private const short c_onHoldSetIdx = 3;
    private const short c_isReworkSetIdx = 4;
    private const short c_requiredFinishQtySetIdx = 5;
    private const short c_uomSetIdx = 6;
    private const short c_useExpectedFinishQtySetIdx = 7;
    private const short c_outputNameSetIdx = 8;
    private const short c_autoReportProgressFlagSetIdx = 9;
    private const short c_autoFinishSetIdx = 10;
    private const short c_holdUntilSetIdx = 11;
    private const short c_plannedScrapQtySetIdx = 12;
    private const short c_onlyAllowManualUpdatesToProductsSetIdx = 13;
    private const short c_onlyAllowManualUpdatesToMaterialsSetIdx = 14;
    private const short c_productCodeSetIdx = 15;
    private const short c_onlyAllowManualUpdatesToCleanSetIdx = 16;
    private const short c_standardCleanSpanIsSetIdx = 17;
    private const short c_cleanoutGradeIsSetIdx = 18;
    private const short c_autoSplitTypeIsSetIdx = 19;
    private const short c_maxAutoSplitAmountIsSetIdx = 20;
    private const short c_setupSplitTypeIsSetIdx = 21;
    private const short c_preventSplitsFromIncurringSetupIsSetIdx = 22;
    private const short c_preventSplitsFromIncurringCleanIsSetIdx = 23;
    private const short c_campaignCodeIdx = 24;
    private const short c_productionSetupCostIsSetIdx = 25;
    private const short c_productionCleanoutCostIsSetIdx = 26;
    private const short c_standardStorageSpanIsSetIdx = 27;
    private const short c_allowExpiredSupplySetIdx = 28;
    private const short c_minBufferDaysSetIdx = 29;
    private const short c_sequenceHeadStartDaysIsSetIdx = 30;
    private const short c_allowSameLotInNonEmptyStorageAreaIsSetIdx = 31;
    
    private BoolVector32 m_isSetBools3;
    private const short c_keepSplitsOnSameResourceIsSetIdx = 0;

    public bool PlannedScrapQtySet => m_setBools2[c_plannedScrapQtySetIdx];
    public bool HoldUntilSet => m_setBools2[c_holdUntilSetIdx];
    public bool HoldReasonSet => m_setBools2[c_holdReasonSetIdx];
    public bool OmittedSet => m_setBools2[c_omittedSetIdx];

    public bool OnHoldSet => m_setBools2[c_onHoldSetIdx];
    public bool IsReworkSet => m_setBools2[c_isReworkSetIdx];
    public bool RequiredFinishQtySet => m_setBools2[c_requiredFinishQtySetIdx];
    public bool UOMSet => m_setBools2[c_uomSetIdx];
    public bool UseExpectedFinishQtySet => m_setBools2[c_useExpectedFinishQtySetIdx];
    public bool OutputNameSet => m_setBools2[c_outputNameSetIdx];
    public bool AutoReportProgressSet => m_setBools2[c_autoReportProgressFlagSetIdx];
    public bool AutoFinishSet => m_setBools2[c_autoFinishSetIdx];
    public bool AutoSplitSet => m_setBools[c_autoSplitSetIdx];
    public bool CanPauseSet => m_setBools[c_canPauseSetIdx];
    public bool TimeBasedReportingSet => m_setBools[c_timeBasedReportingSetIdx];
    public bool CanSubcontractSet => m_setBools[c_canSubcontractSetIdx];
    public bool WholeNumberSplitsSet => m_setBools[c_wholeNumberSplitsSetIdx];
    public bool DeductScrapFromRequiredSet => m_setBools[c_deductScrapFromRequiredSetIdx];
    public bool CanResizeSet => m_setBools2[c_canResizeSetIdx];
    public bool SetupColorSet => m_setBools[c_setupColorSetIdx];
    public bool CarryingCostSet => m_setBools[c_carryingCostSetIdx];
    public bool CompatibilityCodeSet => m_setBools[c_compatibilityCodeSetIdx];
    public bool UseCompatibilityCodeSet => m_setBools[c_useCompatibilityCodeSetIdx];
    public bool CycleSpanSet => m_setBools[c_cycleSpanTicksSetIdx];
    public bool OverlapTransferQtySet => m_setBools[c_overlapTransferQtySetIdx];
    public bool PostProcessingSpanSet => m_setBools[c_postProcessingSpanTicksSetIdx];
    public bool BatchCodeSet => m_setBools[c_batchCodeSetIdx];
    public bool SetupNumberSet => m_setBools[c_setupNumberSetIdx];
    public bool SuccessorProcessingSet => m_setBools[c_successorProcessingSetIdx];
    public bool KeepSuccessorsTimeLimitSet => m_setBools[c_keepSuccessorTimeLimitSetIdx];
    public bool PlanningScrapPercentSet => m_setBools[c_planningScrapPercentSetIdx];
    public bool SetupSpanTicksSet => m_setBools[c_setupSpanTicksSetIdx];
    public bool QtyPerCycleSet => m_setBools[c_qtyPerCycleSetIdx];
    public bool ProductCodeSet => m_setBools2[c_productCodeSetIdx];
    public bool OnlyAllowManualUpdatesToPlanningScrapPercentSet => m_setBools[c_onlyAllowManualUpdatesToPlanningScrapPercentSetIdx];
    public bool OnlyAllowManualUpdatesToSetupSpanSet => m_setBools[c_onlyAllowManualUpdatesToSetupSpanSetIdx];
    public bool OnlyAllowManualUpdatesToCycleSpanSet => m_setBools[c_onlyAllowManualUpdatesToCycleSpanSetIdx];
    public bool OnlyAllowManualUpdatesToPostProcessingSpanSet => m_setBools[c_onlyAllowManualUpdatesToPostProcessingSpanSetIdx];
    public bool OnlyAllowManualUpdatesToCleanSpanSet => m_setBools2[c_onlyAllowManualUpdatesToCleanSetIdx];
    public bool OnlyAllowManualUpdatesToMaterialPostProcessingSpanSet => m_setBools[c_onlyAllowManualUpdatesToMaterialPostProcessingSpanSetIdx];
    public bool OnlyAllowManualUpdatesToQtyPerCycleSet => m_setBools[c_onlyAllowManualUpdatesToQtyPerCycleSetIdx];
    public bool OnlyAllowManualUpdatesToResourceRequirementsSet => m_setBools[c_onlyAllowManualUpdatesToResReqtsSetIdx];
    public bool OnlyAllowManualUpdatesToSplitOperationSet => m_setBools[c_onlyAllowManualUpdatesToSplitOperationSetIdx];
    public bool OnlyAllowManualUpdatesToProductsSet => m_setBools2[c_onlyAllowManualUpdatesToProductsSetIdx];
    public bool OnlyAllowManualUpdatesToMaterialsSet => m_setBools2[c_onlyAllowManualUpdatesToMaterialsSetIdx];
    public bool MaterialPostProcessingSpanTicksSet => m_setBools[c_materialPostProcessingSpanTicksSetIdx];
    public bool StandardRunSpanSet => m_setBools[c_standardRunSpanSetIdx];
    public bool StandardSetupSpanSet => m_setBools[c_standardSetupSpanIsSetIdx];
    public bool StandardCleanSpanIsSet => m_setBools2[c_standardCleanSpanIsSetIdx];
    public bool CleanoutGradeIsSet => m_setBools2[c_cleanoutGradeIsSetIdx];
    public bool StandardStorageSpanIsSet => m_setBools2[c_standardStorageSpanIsSetIdx];
    public bool AutoSplitTypeIsSet => m_setBools2[c_autoSplitTypeIsSetIdx];
    public bool MinAutoSplitAmountIsSet => m_setBools[c_minAutoSplitQtyIsSetIdx];
    public bool MaxAutoSplitAmountIsSet => m_setBools2[c_maxAutoSplitAmountIsSetIdx];
    public bool SetupSplitTypeIsSet => m_setBools2[c_setupSplitTypeIsSetIdx];
    public bool PreventSplitsFromIncurringCleanIsSet => m_setBools2[c_preventSplitsFromIncurringCleanIsSetIdx];
    public bool PreventSplitsFromIncurringSetupIsSet => m_setBools2[c_preventSplitsFromIncurringSetupIsSetIdx];
    public bool CampaignCodeSet => m_setBools2[c_campaignCodeIdx];
    public bool ProductionSetupCostIsSet => m_setBools2[c_productionSetupCostIsSetIdx];
    public bool ProductionCleanoutCostIsSet => m_setBools2[c_productionCleanoutCostIsSetIdx];
    public bool MinBufferDaysIsSet => m_setBools2[c_minBufferDaysSetIdx];
    public bool SequenceHeadStartDaysIsSet => m_setBools2[c_sequenceHeadStartDaysIsSetIdx];
    public bool AllowSameLotInNonEmptyStorageAreaIsSet => m_setBools2[c_allowSameLotInNonEmptyStorageAreaIsSetIdx];
    public bool KeepSplitsOnSameResourceIsSet => m_isSetBools3[c_keepSplitsOnSameResourceIsSetIdx];
    #endregion

    public void Validate() { }
}

public class ActivityEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId JobId;
    public BaseId MOId;
    public BaseId OpId;
    public BaseId ActivityId;

    #region PT Serialization
    public ActivityEdit(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12536)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);

            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            a_reader.Read(out m_batchAmount);

            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRunSpan);
            a_reader.Read(out m_reportedSetupSpan);
            a_reader.Read(out m_reportedPostProcessingSpan);
            a_reader.Read(out m_reportedStorage); // new in 12522

            a_reader.Read(out m_reportedEndOfPostProcessingTicks); // new in 12523
            a_reader.Read(out m_reportedEndOfStorageTicks); // new in 12523

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_comments2);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_cycleSpan);
            a_reader.Read(out m_cleanSpan);

            a_reader.Read(out m_reportedCleanoutGrade);
            a_reader.Read(out m_reportedCleanSpan);

            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_storageSpan);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_productionCleanoutCost);
        }
        #region 12523
        else if (a_reader.VersionNumber >= 12528)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);

            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            a_reader.Read(out m_batchAmount);

            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRunSpan);
            a_reader.Read(out m_reportedSetupSpan);
            a_reader.Read(out m_reportedPostProcessingSpan);
            a_reader.Read(out m_reportedStorage); // new in 12522

            a_reader.Read(out m_reportedEndOfPostProcessingTicks); // new in 12523
            a_reader.Read(out m_reportedEndOfStorageTicks); // new in 12523

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_comments2);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_cycleSpan);
            a_reader.Read(out m_cleanSpan);

            a_reader.Read(out m_reportedCleanoutGrade);
            a_reader.Read(out m_reportedCleanSpan);

            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_storageSpan);
        }
        #endregion
        #region 12523
        else if (a_reader.VersionNumber >= 12523)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);

            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            a_reader.Read(out m_batchAmount);

            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRunSpan);
            a_reader.Read(out m_reportedSetupSpan);
            a_reader.Read(out m_reportedPostProcessingSpan);
            a_reader.Read(out m_reportedStorage); // new in 12522

            a_reader.Read(out m_reportedEndOfPostProcessingTicks); // new in 12523
            a_reader.Read(out m_reportedEndOfStorageTicks); // new in 12523

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_comments2);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_cycleSpan);
            a_reader.Read(out m_cleanSpan);

            a_reader.Read(out m_reportedCleanoutGrade);
            a_reader.Read(out m_reportedCleanSpan);

            a_reader.Read(out m_cleanoutGrade);
        }
        #endregion
        #region 12522
        else if (a_reader.VersionNumber >= 12522)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);

            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            a_reader.Read(out m_batchAmount);

            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRunSpan);
            a_reader.Read(out m_reportedSetupSpan);
            a_reader.Read(out m_reportedPostProcessingSpan);
            a_reader.Read(out m_reportedStorage); // new in 12522

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_comments2);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_cycleSpan);
            a_reader.Read(out m_cleanSpan);

            a_reader.Read(out m_reportedCleanoutGrade);
            a_reader.Read(out m_reportedCleanSpan);

            a_reader.Read(out m_cleanoutGrade);
        }
        #endregion
        else if (a_reader.VersionNumber >= 12504)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);

            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);
            a_reader.Read(out m_reportedStorage);

            a_reader.Read(out m_batchAmount);

            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRunSpan);
            a_reader.Read(out m_reportedSetupSpan);
            a_reader.Read(out m_reportedPostProcessingSpan);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_comments2);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_cycleSpan);
            a_reader.Read(out m_cleanSpan);

            a_reader.Read(out m_reportedCleanoutGrade);
            a_reader.Read(out m_reportedCleanSpan);

            a_reader.Read(out m_cleanoutGrade);
        }
        else if (a_reader.VersionNumber >= 12301)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);

            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            a_reader.Read(out m_batchAmount);

            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRunSpan);
            a_reader.Read(out m_reportedSetupSpan);
            a_reader.Read(out m_reportedPostProcessingSpan);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_comments2);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_cycleSpan);
            a_reader.Read(out m_cleanSpan);
        }

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OpId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);

            a_reader.Read(out m_requiredFinishQty);
            a_reader.Read(out m_reportedFinishDateTicks);
            a_reader.Read(out m_reportedStartDateTicks);
            a_reader.Read(out m_reportedProcessingStartTicks);

            a_reader.Read(out m_batchAmount);

            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRunSpan);
            a_reader.Read(out m_reportedSetupSpan);
            a_reader.Read(out m_reportedPostProcessingSpan);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_comments2);
            a_reader.Read(out m_anchorDateTicks);
            a_reader.Read(out m_cycleSpan);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);
        m_setBools2.Serialize(a_writer);

        JobId.Serialize(a_writer);
        MOId.Serialize(a_writer);
        OpId.Serialize(a_writer);
        ActivityId.Serialize(a_writer);

        a_writer.Write(m_requiredFinishQty);
        a_writer.Write(m_reportedFinishDateTicks);
        a_writer.Write(m_reportedStartDateTicks);
        a_writer.Write(m_reportedProcessingStartTicks);

        a_writer.Write(m_batchAmount);

        a_writer.Write((int)m_productionStatus);
        a_writer.Write(m_reportedGoodQty);
        a_writer.Write(m_reportedScrapQty);
        a_writer.Write((int)m_peopleUsage);
        a_writer.Write(m_nbrOfPeople);
        a_writer.Write(m_comments);

        a_writer.Write(m_reportedRunSpan);
        a_writer.Write(m_reportedSetupSpan);
        a_writer.Write(m_reportedPostProcessingSpan);
        a_writer.Write(m_reportedStorage); // new in 12522

        a_writer.Write(m_reportedEndOfPostProcessingTicks); // new in 12523
        a_writer.Write(m_reportedEndOfStorageTicks); // new in 12523

        a_writer.Write(m_reportedEndOfRunTicks);

        a_writer.Write(m_scheduledStartTimePostProcessingNoResources);
        a_writer.Write(m_scheduledFinishTimePostProcessingNoResources);

        a_writer.Write(m_planningScrapPercent);
        a_writer.Write(m_setupSpanTicks);
        a_writer.Write(m_postProcessingSpanTicks);
        a_writer.Write(m_qtyPerCycle);
        a_writer.Write(m_materialPostProcessingSpanTicks);
        a_writer.Write(m_comments2);
        a_writer.Write(m_anchorDateTicks);
        a_writer.Write(m_cycleSpan);
        a_writer.Write(m_cleanSpan);

        a_writer.Write(m_reportedCleanoutGrade);
        a_writer.Write(m_reportedCleanSpan);

        a_writer.Write(m_cleanoutGrade);

        a_writer.Write(m_storageSpan);
        a_writer.Write(m_productionSetupCost);
        a_writer.Write(m_productionCleanoutCost);

    }

    public new int UniqueId => 1048;
    #endregion

    public ActivityEdit(BaseId a_jobId, BaseId a_moId, BaseId a_opId, BaseId a_actId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OpId = a_opId;
        ActivityId = a_actId;
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet || m_setBools2.AnyFlagsSet;

    #region Shared Properties
    //bools
    private BoolVector32 m_bools;
    private const short c_FeasibleIdx = 0;
    private const short c_JumpableIdx = 1;
    private const short c_AnchorIdx = 2;
    private const short c_ScheduledIdx = 3;
    private const short c_ConnectionViolatedIdx = 4;
    private const short c_pausedIdx = 5;
    private const short c_activityManualUpdateOnlyIdx = 6;

    private const short c_useSetupSpanIdx = 7;
    private const short c_usePostProcessingSpanIdx = 8;
    private const short c_useCleanSpanIdx = 9;

    private const short c_cycleSpanOverrideIdx = 10;
    private const short c_useStorageSpanIdx = 11;
    private const short c_setupSpanOverrideIdx = 12;
    private const short c_materialPostProcessingSpanOverrideIdx = 13;
    private const short c_qtyPerCycleOverrideIdx = 14;

    private const short c_cycleSpanManualUpdateFlagIdx = 15;
    private const short c_cleanSpanManualUpdateFlagIdx = 16;
    private const short c_setupSpanManualUpdateFlagIdx = 17;
    private const short c_postProcessingSpanManualUpdateFlagIdx = 18;
    private const short c_qtyPerCycleManualUpdateFlagIdx = 19;

    private const short c_allocateMaterialFromOnHand = 20;
    private const short c_releaseProductToWarehouse = 21;

    private const short c_finishPredecessors = 22;
    private const short c_useProductionInfoOverride = 23;

    private const short c_planningScrapPercentManualUpdateFlagIdx = 24;
    private const short c_planningScrapPercentOverrideIdx = 25;

    private const short c_materialPostProcessingSpanManualUpdateFlagIdx = 26;

    //Set bools
    private BoolVector32 m_setBools;
    private const short c_requiredFinishQtySetIdx = 0;
    private const short c_AnchorSetIdx = 1;
    private const short c_reportedFinishDateSetIdx = 2;
    private const short c_reportedStartDateTicksSetIdx = 3;
    private const short c_reportedProcessingStartTicksSetIdx = 4;
    private const short c_batchAmountSetIdx = 5;
    private const short c_productionStatusSetIdx = 6;
    private const short c_reportedGoodQtySetIdx = 7;
    private const short c_reportedScrapQtySetIdx = 8;
    private const short c_peopleUsageSetIdx = 9;
    private const short c_nbrOfPeopleSetIdx = 10;
    private const short c_commentsSetIdx = 11;
    private const short c_comments2SetIdx = 12;
    private const short c_reportedRunSetIdx = 13;
    private const short c_reportedSetupSetIdx = 14;
    private const short c_reportedPostProcessingSetIdx = 15;
    private const short c_reportedEndOfRunTicksSetIdx = 16;
    private const short c_scheduledStartTimePostProcessingNoResourcesSetIdx = 17;
    private const short c_scheduledFinishTimePostProcessingNoResourcesSetIdx = 18;
    private const short c_pausedSetIdx = 19;
    private const short c_activityManualUpdateOnlySetIdx = 20;
    private const short c_planningScrapPercentSetIdx = 21;
    private const short c_setupSpanTicksIsSetIdx = 22;
    private const short c_postProcessingSpanTicksIsSetIdx = 23;
    private const short c_qtyPerCycleSetIdx = 24;
    private const short c_materialPostProcessingSpanTicksSetIdx = 25;
    private const short c_anchorDateSetIdx = 26;
    private const short c_cycleSpanSetIdx = 27;
    private const short c_useSetupSpanIsSetIdx = 28;
    private const short c_usePostProcessingSpanIsSetIdx = 29;
    private const short c_cleanSpanIsSetIdx = 30;
    private const short c_useCleanSpanIsSetIdx = 31;

    private BoolVector32 m_setBools2;
    private const short c_reportedCleanoutGradeSetIdx = 0;
    private const short c_reportedCleanSpanSetIdx = 1;
    private const short c_cycleSpanOverrideSetIdx = 2;
    private const short c_cleanSpanOverrideSetIdx = 3;
    private const short c_setupSpanOverrideSetIdx = 12;
    private const short c_materialPostProcessingSpanOverrideSetIdx = 13;
    private const short c_qtyPerCycleOverrideSetIdx = 14;

    private const short c_cycleSpanManualUpdateFlagSetIdx = 15;
    private const short c_cleanSpanManualUpdateFlagSetIdx = 16;
    private const short c_setupSpanManualUpdateFlagSetIdx = 17;
    private const short c_postProcessingSpanManualUpdateFlagSetIdx = 18;
    private const short c_qtyPerCycleManualUpdateFlagSetIdx = 19;

    private const short c_storageSpanSet = 20;
    private const short c_useStorageSpanSet = 21;

    private const short c_finishPredecessorsSet = 22;
    private const short c_useProductionInfoOverrideSet = 23;

    private const short c_planningScrapPercentManualUpdateFlagSetIdx = 24;
    private const short c_planningScrapPercentOverrideSetIdx = 25;

    private const short c_materialPostProcessingSpanManualUpdateFlagSetIdx = 26;
    private const short c_cleanoutGradeSetIdx = 26;

    private const short c_reportedStorageSetIdx = 27;
    private const short c_reportedEndOfPostProcessingSetIdx = 28;
    private const short c_reportedEndOfStorageSetIdx = 29;
    private const short c_productionSetupCostIsSetIdx = 30;
    private const short c_productionCleanoutCostIsSetIdx = 31;

    private TimeSpan m_cycleSpan;

    public TimeSpan CycleSpan
    {
        get => m_cycleSpan;
        set
        {
            m_cycleSpan = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools[c_cycleSpanSetIdx] = true;
        }
    }

    public bool CycleSpanSet => m_setBools[c_cycleSpanSetIdx];
    
    private TimeSpan m_storageSpan;

    public TimeSpan StorageSpan
    {
        get => m_storageSpan;
        set
        {
            m_storageSpan = value;
            m_setBools2[c_storageSpanSet] = true;
        }
    }

    public bool StorageSpanSet => m_setBools2[c_storageSpanSet];

    public bool UseStorageSpan
    {
        get => m_bools[c_useStorageSpanIdx];
        set => m_bools[c_useStorageSpanIdx] = value;
    }
    public bool UseStorageSpanSet => m_setBools2[c_useStorageSpanSet];

    private long m_reportedStorage;

    public long ReportedStorage
    {
        get => m_reportedStorage;
        set
        {
            m_reportedStorage = value;
            m_setBools[c_reportedStorageSetIdx] = true;
        }
    }
    private long m_reportedEndOfPostProcessingTicks;

    public long ReportedEndOfPostProcessingTicks
    {
        get => m_reportedEndOfPostProcessingTicks;
        set
        {
            m_reportedEndOfPostProcessingTicks = value;
            m_setBools[c_reportedEndOfPostProcessingSetIdx] = true;
        }
    }
    private long m_reportedEndOfStorageTicks;

    public long ReportedEndOfStorageTicks
    {
        get => m_reportedEndOfStorageTicks;
        set
        {
            m_reportedEndOfStorageTicks = value;
            m_setBools[c_reportedEndOfStorageSetIdx] = true;
        }
    }
    private long m_materialPostProcessingSpanTicks;

    public long MaterialPostProcessingSpanTicks
    {
        get => m_materialPostProcessingSpanTicks;
        set
        {
            m_materialPostProcessingSpanTicks = value;
            m_setBools[c_materialPostProcessingSpanTicksSetIdx] = true;
            m_bools[c_useProductionInfoOverride] = true;
        }
    }

    public bool MaterialPostProcessingSpanTicksSet => m_setBools[c_materialPostProcessingSpanTicksSetIdx];

    private decimal m_qtyPerCycle;

    public decimal QtyPerCycle
    {
        get => m_qtyPerCycle;
        set
        {
            m_qtyPerCycle = value;
            m_setBools[c_qtyPerCycleSetIdx] = true;
            m_bools[c_useProductionInfoOverride] = true;
        }
    }

    public bool QtyPerCycleSet => m_setBools[c_qtyPerCycleSetIdx];

    private decimal m_planningScrapPercent;

    public decimal PlanningScrapPercent
    {
        get => m_planningScrapPercent;
        set
        {
            m_planningScrapPercent = value;
            m_setBools[c_planningScrapPercentSetIdx] = true;
            m_bools[c_useProductionInfoOverride] = true;
        }
    }

    public bool PlanningScrapPercentSet => m_setBools[c_planningScrapPercentSetIdx];

    public bool Paused
    {
        get => m_bools[c_pausedIdx];
        set
        {
            m_bools[c_pausedIdx] = value;
            m_setBools[c_pausedSetIdx] = true;
        }
    }

    public bool PausedSet => m_setBools[c_pausedSetIdx];

    public bool ActivityManualUpdateOnly
    {
        get => m_bools[c_activityManualUpdateOnlyIdx];
        set
        {
            m_bools[c_activityManualUpdateOnlyIdx] = value;
            m_setBools[c_activityManualUpdateOnlySetIdx] = true;
        }
    }

    public bool ActivityManualUpdateOnlySet => m_setBools[c_activityManualUpdateOnlySetIdx];

    private long m_scheduledFinishTimePostProcessingNoResources;

    public long ScheduledFinishTimePostProcessingNoResources
    {
        get => m_scheduledFinishTimePostProcessingNoResources;

        set
        {
            m_scheduledFinishTimePostProcessingNoResources = value;
            m_setBools[c_scheduledFinishTimePostProcessingNoResourcesSetIdx] = true;
        }
    }

    public bool ScheduledFinishTimePostProcessingNoResourcesSet => m_setBools[c_scheduledFinishTimePostProcessingNoResourcesSetIdx];

    private long m_scheduledStartTimePostProcessingNoResources;

    public long ScheduledStartTimePostProcessingNoResources
    {
        get => m_scheduledStartTimePostProcessingNoResources;

        set
        {
            m_scheduledStartTimePostProcessingNoResources = value;
            m_setBools[c_scheduledStartTimePostProcessingNoResourcesSetIdx] = true;
        }
    }

    public bool ScheduledStartTimePostProcessingNoResourcesSet => m_setBools[c_scheduledStartTimePostProcessingNoResourcesSetIdx];

    private long m_reportedEndOfRunTicks = PTDateTime.MinDateTime.Ticks;

    public long ReportedEndOfRunTicks
    {
        get => m_reportedEndOfRunTicks;

        set
        {
            m_reportedEndOfRunTicks = value;
            m_setBools[c_reportedEndOfRunTicksSetIdx] = true;
        }
    }

    public bool ReportedEndOfRunTicksSet => m_setBools[c_reportedEndOfRunTicksSetIdx];

    private long m_reportedPostProcessingSpan;

    public long ReportedPostProcessingSpan
    {
        get => m_reportedPostProcessingSpan;

        set
        {
            m_reportedPostProcessingSpan = value;
            m_setBools[c_reportedPostProcessingSetIdx] = true;
        }
    }

    public bool ReportedPostProcessingSpanSet => m_setBools[c_reportedPostProcessingSetIdx];

    private long m_reportedSetupSpan;

    public long ReportedSetupSpan
    {
        get => m_reportedSetupSpan;
        set
        {
            m_reportedSetupSpan = value;
            m_setBools[c_reportedSetupSetIdx] = true;
        }
    }

    public bool ReportedSetupSpanSet => m_setBools[c_reportedSetupSetIdx];

    private long m_reportedRunSpan;

    public long ReportedRunSpan
    {
        get => m_reportedRunSpan;
        set
        {
            m_reportedRunSpan = value;
            m_setBools[c_reportedRunSetIdx] = true;
        }
    }

    public bool ReportedRunSpanSet => m_setBools[c_reportedRunSetIdx]; 
    
    private string m_comments = "";

    public string Comments
    {
        get => m_comments;
        set
        {
            m_comments = value;
            m_setBools[c_commentsSetIdx] = true;
        }
    }

    public bool CommentsSet => m_setBools[c_commentsSetIdx];

    private string m_comments2 = "";

    public string Comments2
    {
        get => m_comments2;
        set
        {
            m_comments2 = value;
            m_setBools[c_comments2SetIdx] = true;
        }
    }

    public bool Comments2Set => m_setBools[c_comments2SetIdx];

    private decimal m_nbrOfPeople = 1;

    public decimal NbrOfPeople
    {
        get => m_nbrOfPeople;
        set
        {
            m_nbrOfPeople = value;
            m_setBools[c_nbrOfPeopleSetIdx] = true;
        }
    }

    public bool NbrOfPeopleSet => m_setBools[c_nbrOfPeopleSetIdx];

    private InternalActivityDefs.peopleUsages m_peopleUsage = InternalActivityDefs.peopleUsages.UseAllAvailable;

    public InternalActivityDefs.peopleUsages PeopleUsage
    {
        get => m_peopleUsage;
        set
        {
            m_peopleUsage = value;
            m_setBools[c_peopleUsageSetIdx] = true;
        }
    }

    public bool PeopleUsageSet => m_setBools[c_peopleUsageSetIdx];

    private decimal m_reportedScrapQty;

    public decimal ReportedScrapQty
    {
        get => m_reportedScrapQty;
        set
        {
            m_reportedScrapQty = value;
            m_setBools[c_reportedScrapQtySetIdx] = true;
        }
    }

    public bool ReportedScrapQtySet => m_setBools[c_reportedScrapQtySetIdx];

    private decimal m_requiredFinishQty;

    public decimal RequiredFinishQty
    {
        get => m_requiredFinishQty;
        set
        {
            m_requiredFinishQty = value;
            m_setBools[c_requiredFinishQtySetIdx] = true;
        }
    }

    public bool RequiredFinishQtySet => m_setBools[c_requiredFinishQtySetIdx];

    private long m_anchorDateTicks;

    public DateTime AnchorDate
    {
        get => new (m_anchorDateTicks);
        set
        {
            m_anchorDateTicks = value.Ticks;
            m_setBools[c_anchorDateSetIdx] = true;
        }
    }

    public bool AnchorStartDateSet => m_setBools[c_anchorDateSetIdx];

    public bool Anchored
    {
        get => m_bools[c_AnchorIdx];
        set
        {
            m_bools[c_AnchorIdx] = value;
            m_setBools[c_AnchorSetIdx] = true;
        }
    }

    public bool AnchoredSet => m_setBools[c_AnchorSetIdx];

    public long m_reportedFinishDateTicks = PTDateTime.MinDateTime.Ticks;

    public long ReportedFinishDateTicks
    {
        get => m_reportedFinishDateTicks;
        set
        {
            m_reportedFinishDateTicks = value;
            m_setBools[c_reportedFinishDateSetIdx] = true;
        }
    }

    public bool ReportedFinishDateTicksSet => m_setBools[c_reportedFinishDateSetIdx];

    private long m_reportedStartDateTicks = PTDateTime.MinDateTime.Ticks;

    public long ReportedStartDateTicks
    {
        get => m_reportedStartDateTicks;
        set
        {
            m_reportedStartDateTicks = value;
            m_setBools[c_reportedStartDateTicksSetIdx] = true;
        }
    }

    public bool ReportedStarDateTicksSet => m_setBools[c_reportedStartDateTicksSetIdx];

    private long m_reportedProcessingStartTicks = PTDateTime.MinDateTime.Ticks;

    public long ReportedProcessingStartTicks
    {
        get => m_reportedProcessingStartTicks;
        set
        {
            m_reportedProcessingStartTicks = value;
            m_setBools[c_reportedProcessingStartTicksSetIdx] = true;
        }
    }

    public bool ReportedProcessingStartTicksSet => m_setBools[c_reportedProcessingStartTicksSetIdx];

    private decimal m_batchAmount;

    public decimal BatchAmount
    {
        get => m_batchAmount;
        set
        {
            m_batchAmount = value;
            m_setBools[c_batchAmountSetIdx] = true;
        }
    }

    public bool BatchAmountSet => m_setBools[c_batchAmountSetIdx];

    private InternalActivityDefs.productionStatuses m_productionStatus;

    public InternalActivityDefs.productionStatuses ProductionStatus
    {
        get => m_productionStatus;
        set
        {
            m_productionStatus = value;
            m_setBools[c_productionStatusSetIdx] = true;
        }
    }

    public bool ProductionStatusSet => m_setBools[c_productionStatusSetIdx];

    private decimal m_reportedGoodQty;

    public decimal ReportedGoodQty
    {
        get => m_reportedGoodQty;

        set
        {
            m_reportedGoodQty = value;
            m_setBools[c_reportedGoodQtySetIdx] = true;
        }
    }

    public bool ReportedGoodQtySet => m_setBools[c_reportedGoodQtySetIdx];
    
    private TimeSpan m_reportedCleanSpan;

    public TimeSpan ReportedCleanSpan
    {
        get => m_reportedCleanSpan;

        set
        {
            m_reportedCleanSpan = value;
            m_setBools2[c_reportedCleanSpanSetIdx] = true;
        }
    }

    public bool ReportedCleanSpanSet => m_setBools2[c_reportedCleanSpanSetIdx];
    
    private int m_reportedCleanoutGrade;

    public int ReportedCleanoutGrade
    {
        get => m_reportedCleanoutGrade;

        set
        {
            m_reportedCleanoutGrade = value;
            m_setBools2[c_reportedCleanoutGradeSetIdx] = true;
        }
    }

    public bool ReportedCleanoutGradeSet => m_setBools2[c_reportedCleanoutGradeSetIdx];

    private long m_setupSpanTicks;

    public long SetupSpanTicks
    {
        get => m_setupSpanTicks;
        set
        {
            m_setupSpanTicks = value;
            m_setBools[c_setupSpanTicksIsSetIdx] = true;
            m_bools[c_useProductionInfoOverride] = true;
        }
    }

    public bool SetupSpanTicksIsSet => m_setBools[c_setupSpanTicksIsSetIdx];

    public bool UseSetupSpan
    {
        get => m_bools[c_useSetupSpanIdx];
        set
        {
            m_bools[c_useSetupSpanIdx] = value;
            m_setBools[c_useSetupSpanIsSetIdx] = true;
        }
    }

    public bool UseSetupSpanIsSet => m_setBools[c_useSetupSpanIsSetIdx];

    private decimal m_productionSetupCost = 0;
    public decimal ProductionSetupCost
    {
        get => m_productionSetupCost;
        set
        {
            m_productionSetupCost = value;
            m_setBools2[c_productionSetupCostIsSetIdx] = true;
        }
    }

    public bool ProductionSetupCostIsSet => m_setBools2[c_productionSetupCostIsSetIdx];

    private long m_postProcessingSpanTicks;

    public TimeSpan PostProcessingSpan
    {
        get => new (m_postProcessingSpanTicks);
        set
        {
            m_postProcessingSpanTicks = value.Ticks;
            m_setBools[c_postProcessingSpanTicksIsSetIdx] = true;
            m_bools[c_useProductionInfoOverride] = true;
        }
    }

    public bool PostProcessingSpanIsSet => m_setBools[c_postProcessingSpanTicksIsSetIdx];

    public bool UsePostProcessingSpan
    {
        get => m_bools[c_usePostProcessingSpanIdx];
        set
        {
            m_bools[c_usePostProcessingSpanIdx] = value;
            m_setBools[c_usePostProcessingSpanIsSetIdx] = true;
        }
    }

    public bool UsePostProcessingSpanIsSet => m_setBools[c_usePostProcessingSpanIsSetIdx];

    private TimeSpan m_cleanSpan;

    /// <summary>
    /// CIP Span to override the scheduler's calculated values.
    /// </summary>
    public TimeSpan CleanSpan
    {
        get => m_cleanSpan;
        set
        {
            m_cleanSpan = value;
            m_setBools[c_cleanSpanIsSetIdx] = true;
        }
    }

    public bool CleanSpanIsSet => m_setBools[c_cleanSpanIsSetIdx];

    public bool UseCleanSpan
    {
        get => m_bools[c_useCleanSpanIdx];
        set
        {
            m_bools[c_useCleanSpanIdx] = value;
            m_setBools[c_useCleanSpanIsSetIdx] = true;
        }
    }

    public bool UseCleanSpanIsSet => m_setBools[c_useCleanSpanIsSetIdx];
    /// <summary>
    /// If true then finish predecessor Operations at expected values.
    /// </summary>
    public bool FinishPredecessors
    {
        get => m_bools[c_finishPredecessors];
        set => m_bools[c_finishPredecessors] = value;
    }

    public bool AllocateMaterialFromOnHand
    {
        get => m_bools[c_allocateMaterialFromOnHand];
        set => m_bools[c_allocateMaterialFromOnHand] = value;
    }

    public bool ReleaseProductToWarehouse
    {
        get => m_bools[c_releaseProductToWarehouse];
        set => m_bools[c_releaseProductToWarehouse] = value;
    }
    private int m_cleanoutGrade;

    public int CleanoutGrade
    {
        get => m_cleanoutGrade;
        set
        {
            m_cleanoutGrade = value;
            m_setBools2[c_cleanoutGradeSetIdx] = true;
        }
    }

    public bool CleanoutGradeIsSet => m_setBools2[c_cleanoutGradeSetIdx];

    private decimal m_productionCleanoutCost = 0;
    public decimal ProductionCleanoutCost
    {
        get => m_productionCleanoutCost;
        set
        {
            m_productionCleanoutCost = value;
            m_setBools2[c_productionCleanoutCostIsSetIdx] = true;
        }
    }

    public bool ProductionCleanoutCostIsSet => m_setBools2[c_productionCleanoutCostIsSetIdx];
    
    public bool CycleSpanOverride
    {
        get => m_bools[c_cycleSpanOverrideIdx];
        set
        {
            m_bools[c_cycleSpanOverrideIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_cycleSpanOverrideSetIdx] = true;
        }
    }

    public bool CycleSpanOverrideSet => m_setBools2[c_cycleSpanOverrideSetIdx];
    public bool MaterialPostProcessingSpanOverride
    {
        get => m_bools[c_materialPostProcessingSpanOverrideIdx];
        set
        {
            m_bools[c_materialPostProcessingSpanOverrideIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_materialPostProcessingSpanOverrideSetIdx] = true;
        }
    }
    public bool MaterialPostProcessingSpanOverrideSet => m_setBools2[c_materialPostProcessingSpanOverrideSetIdx];
    
    public bool QtyPerCycleOverride
    {
        get => m_bools[c_qtyPerCycleOverrideIdx];
        set
        {
            m_bools[c_qtyPerCycleOverrideIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_qtyPerCycleOverrideSetIdx] = true;
        }
    }

    public bool QtyPerCycleOverrideSet => m_setBools2[c_qtyPerCycleOverrideSetIdx];
    public bool OnlyAllowManualUpdatesToSetupSpan
    {
        get => m_bools[c_setupSpanManualUpdateFlagIdx];
        set
        {
            m_bools[c_setupSpanManualUpdateFlagIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_setupSpanManualUpdateFlagSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToSetupSpanSet => m_setBools2[c_setupSpanManualUpdateFlagSetIdx];
    public bool OnlyAllowManualUpdatesToCycleSpan
    {
        get => m_bools[c_cycleSpanManualUpdateFlagIdx];
        set
        {
            m_bools[c_cycleSpanManualUpdateFlagIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_cycleSpanManualUpdateFlagSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToCycleSpanSet => m_setBools2[c_cycleSpanManualUpdateFlagSetIdx];
    public bool OnlyAllowManualUpdatesToPostProcessingSpan
    {
        get => m_bools[c_postProcessingSpanManualUpdateFlagIdx];
        set
        {
            m_bools[c_postProcessingSpanManualUpdateFlagIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_postProcessingSpanManualUpdateFlagSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToPostProcessingSpanSet => m_setBools2[c_postProcessingSpanManualUpdateFlagSetIdx];
    public bool OnlyAllowManualUpdatesToMaterialPostProcessingSpan
    {
        get => m_bools[c_materialPostProcessingSpanManualUpdateFlagIdx];
        set
        {
            m_bools[c_materialPostProcessingSpanManualUpdateFlagIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_materialPostProcessingSpanManualUpdateFlagSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToMaterialPostProcessingSpanSet => m_setBools2[c_materialPostProcessingSpanManualUpdateFlagSetIdx];
    public bool OnlyAllowManualUpdatesToCleanSpan
    {
        get => m_bools[c_cleanSpanManualUpdateFlagIdx];
        set
        {
            m_bools[c_cleanSpanManualUpdateFlagIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_cleanSpanManualUpdateFlagSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToCleanSpanSet => m_setBools2[c_cleanSpanManualUpdateFlagSetIdx];
    public bool OnlyAllowManualUpdatesToQtyPerCycle
    {
        get => m_bools[c_qtyPerCycleManualUpdateFlagIdx];
        set
        {
            m_bools[c_qtyPerCycleManualUpdateFlagIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_qtyPerCycleManualUpdateFlagSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToQtyPerCycleSet => m_setBools2[c_qtyPerCycleManualUpdateFlagSetIdx];
    public bool OnlyAllowManualUpdatesToPlanningScrapPercent
    {
        get => m_bools[c_planningScrapPercentManualUpdateFlagIdx];
        set
        {
            m_bools[c_planningScrapPercentManualUpdateFlagIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_planningScrapPercentManualUpdateFlagSetIdx] = true;
        }
    }

    public bool OnlyAllowManualUpdatesToPlanningScrapPercentSet => m_setBools2[c_planningScrapPercentManualUpdateFlagSetIdx];
    public bool PlanningScrapPercentOverride
    {
        get => m_bools[c_planningScrapPercentOverrideIdx];
        set
        {
            m_bools[c_planningScrapPercentOverrideIdx] = value;
            m_bools[c_useProductionInfoOverride] = true;
            m_setBools2[c_planningScrapPercentOverrideSetIdx] = true;
        }
    }

    public bool PlanningScrapPercentOverrideSet => m_setBools2[c_planningScrapPercentOverrideSetIdx];
    #endregion

    public void Validate() { }
}