using PT.APSCommon;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Set Commitments and Priorities for select Jobs.
/// </summary>
public class ScenarioDetailSetJobPropertiesT : ScenarioIdBaseT, IPTSerializable
{
    public static readonly int UNIQUE_ID = 712;

    #region IPTSerializable Members
    public ScenarioDetailSetJobPropertiesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12001)
        {
            m_bools = new BoolVector32(reader);
            m_boolsSetValues = new BoolVector32(reader);
            m_jobs = new BaseIdList(reader);
            reader.Read(out int val);
            m_commitment = (JobDefs.commitmentTypes)val;
            reader.Read(out val);
            m_classification = (JobDefs.classifications)val;
            reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            reader.Read(out m_priority);
            reader.Read(out m_importance);
            reader.Read(out m_revenue);
            reader.Read(out m_needDate);
            reader.Read(out m_moQty);
            reader.Read(out m_notes);
            reader.Read(out m_jobColor);
            reader.Read(out m_jobDescription);
            reader.Read(out m_name);
            reader.Read(out m_moQtyRatio);
            reader.Read(out m_anchorDate);
        }
        #region 710
        else if (reader.VersionNumber >= 710)
        {
            m_jobs = new BaseIdList(reader);
            int val;
            reader.Read(out val);
            m_commitment = (JobDefs.commitmentTypes)val;
            reader.Read(out val);
            m_classification = (JobDefs.classifications)val;
            reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            reader.Read(out m_priority);
            reader.Read(out m_importance);
            reader.Read(out m_revenue);
            reader.Read(out m_needDate);
            reader.Read(out m_moQty);
            reader.Read(out m_notes);
            reader.Read(out m_jobColor);
            reader.Read(out m_moQtyRatio);
            reader.Read(out m_anchorDate);

            m_bools = new BoolVector32(reader);
            m_boolsSetValues = new BoolVector32(reader);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);
        m_boolsSetValues.Serialize(writer);
        m_jobs.Serialize(writer);
        writer.Write((int)m_commitment);
        writer.Write((int)m_classification);
        writer.Write((int)m_shipped);
        writer.Write(m_priority);
        writer.Write(m_importance);
        writer.Write(m_revenue);
        writer.Write(m_needDate);
        writer.Write(m_moQty);
        writer.Write(m_notes);
        writer.Write(m_jobColor);
        writer.Write(m_jobDescription);
        writer.Write(m_name);
        writer.Write(m_moQtyRatio);
        writer.Write(m_anchorDate);

        m_bools.Serialize(writer);
        m_boolsSetValues.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;
    private BoolVector32 m_boolsSetValues;

    //Set Values
    private const int c_commitmentSetIdx = 0;
    private const int c_prioritySetIdx = 1;
    private const int c_schedulabilitySetIdx = 2;
    private const int c_classificationSetIdx = 3;
    private const int c_printedSetIdx = 4;
    private const int c_revenueSetIdx = 5;
    private const int c_moQtySetIdx = 6;
    private const int c_needDateSetIdx = 7;
    private const int c_reviewedSetIdx = 8;
    private const int c_shippedSetIdx = 9;
    private const int c_cancelledSetIdx = 10;
    private const int c_invoicedSetIdx = 11;
    private const int c_hotSetIdx = 12;
    private const int c_importanceSetIdx = 13;
    private const int c_autoFinishSetIdx = 14;
    private const int c_descriptionSetIdx = 15;
    private const int c_notesSetIdx = 16;
    private const int c_doNotDeleteSetIdx = 17;
    private const int c_jobColoreSetIdx = 18;
    private const int c_anchorDateSetIdx = 19;
    private const int c_moQtyRatioSetIdx = 20;
    private const int c_canSpanPlantsSetIdx = 21;

    private const int c_customerSetIdx = 22;
    private const int c_nameSetIdx = 23;
    private const int c_useResourceSetupIsSetIdx = 24;
    private const int c_useOperationSetupIsSetIdx = 25;
    private const int c_useSequencedSetupIsSetIdx = 26;

    //Bools
    private const int c_setToDonotscheduleIfInvalid = 0;
    private const int c_printedValueIdx = 1;
    private const int c_schedulabilityValueIdx = 2;
    private const int c_reviewedValueIdx = 3;
    private const int c_cancelledValueIdx = 4;
    private const int c_invoicedValueIdx = 5;
    private const int c_hotValueIdx = 6;
    private const int c_autoFinishValueIdx = 7;
    private const int c_doNotDeleteValueIdx = 8;
    private const int c_canSpanPlantsIdx = 9;
    private const int c_useResourceSetupIdx = 10;
    private const int c_useOperationSetupIdx = 11;
    private const int c_useSequencedSetupIdx = 12;

    public ScenarioDetailSetJobPropertiesT() { }

    /// <summary>
    /// Set Commitments and Priorities for select Jobs.
    /// </summary>
    public ScenarioDetailSetJobPropertiesT(BaseId a_scenarioId, BaseIdList a_jobs)
        : base(a_scenarioId)
    {
        m_jobs = a_jobs;
    }

    private readonly BaseIdList m_jobs;
    public BaseIdList Jobs => m_jobs;

    public bool CommitmentsSet => m_boolsSetValues[c_commitmentSetIdx];

    #region ENUMs
    private JobDefs.commitmentTypes m_commitment;

    public JobDefs.commitmentTypes Commitment
    {
        get => m_commitment;
        set
        {
            m_commitment = value;
            m_boolsSetValues[c_commitmentSetIdx] = true;
        }
    }

    public bool ClassificationSet => m_boolsSetValues[c_classificationSetIdx];

    private JobDefs.classifications m_classification;

    public JobDefs.classifications Classification
    {
        get => m_classification;
        set
        {
            m_classification = value;
            m_boolsSetValues[c_classificationSetIdx] = true;
        }
    }

    public bool ShippedSet => m_boolsSetValues[c_shippedSetIdx];

    private JobDefs.ShippedStatuses m_shipped;

    public JobDefs.ShippedStatuses Shipped
    {
        get => m_shipped;
        set
        {
            m_shipped = value;
            m_boolsSetValues[c_shippedSetIdx] = true;
        }
    }
    #endregion

    #region NUMERIC
    public bool PrioritiesSet => m_boolsSetValues[c_prioritySetIdx];

    private int m_priority;

    public int Priority
    {
        get => m_priority;
        set
        {
            m_priority = value;
            m_boolsSetValues[c_prioritySetIdx] = true;
        }
    }

    public bool ImportanceSet => m_boolsSetValues[c_importanceSetIdx];

    private int m_importance;

    public int Importance
    {
        get => m_importance;
        set
        {
            m_importance = value;
            m_boolsSetValues[c_importanceSetIdx] = true;
        }
    }

    public bool RevenueSet => m_boolsSetValues[c_revenueSetIdx];

    private decimal m_revenue;

    public decimal Revenue
    {
        get => m_revenue;
        set
        {
            m_revenue = value;
            m_boolsSetValues[c_revenueSetIdx] = true;
        }
    }

    public bool MoQtySet => m_boolsSetValues[c_moQtySetIdx];

    private decimal m_moQty;

    public decimal MoQty
    {
        get => m_moQty;
        set
        {
            m_moQty = value;
            m_boolsSetValues[c_moQtySetIdx] = true;
        }
    }

    public bool NotesSet => m_boolsSetValues[c_notesSetIdx];

    private string m_notes;

    public string Notes
    {
        get => m_notes;
        set
        {
            m_notes = value;
            m_boolsSetValues[c_notesSetIdx] = true;
        }
    }

    public bool MOQtyRatioSet => m_boolsSetValues[c_moQtyRatioSetIdx];

    private decimal m_moQtyRatio;

    public decimal MOQtyRatio
    {
        get => m_moQtyRatio;
        set
        {
            m_moQtyRatio = value;
            m_boolsSetValues[c_moQtyRatioSetIdx] = true;
        }
    }

    public bool JobDescriptionSet => m_boolsSetValues[c_descriptionSetIdx];

    private string m_jobDescription;

    public string JobDescription
    {
        get => m_jobDescription;
        set
        {
            m_jobDescription = value;
            m_boolsSetValues[c_descriptionSetIdx] = true;
        }
    }

    public bool NameSet => m_boolsSetValues[c_nameSetIdx];

    private string m_name;

    public string Name
    {
        get => m_name;
        set
        {
            m_name = value;
            m_boolsSetValues[c_nameSetIdx] = true;
        }
    }
    #endregion

    #region BOOLs
    public bool DoNotDelete
    {
        get => m_bools[c_doNotDeleteValueIdx];
        set
        {
            m_bools[c_doNotDeleteValueIdx] = value;
            m_boolsSetValues[c_doNotDeleteSetIdx] = true;
        }
    }

    public bool DoNotDeleteSet => m_boolsSetValues[c_doNotDeleteSetIdx];

    public bool SchedulabilitySet => m_boolsSetValues[c_schedulabilitySetIdx];

    public bool DoNotSchedule
    {
        get => m_bools[c_schedulabilityValueIdx];
        set
        {
            m_bools[c_schedulabilityValueIdx] = value;
            m_boolsSetValues[c_schedulabilitySetIdx] = true;
        }
    }

    public bool PrintedSet => m_boolsSetValues[c_printedSetIdx];

    public bool Printed
    {
        get => m_bools[c_printedValueIdx];
        set
        {
            m_bools[c_printedValueIdx] = value;
            m_boolsSetValues[c_printedSetIdx] = true;
        }
    }

    public bool ReviewedSet => m_boolsSetValues[c_reviewedSetIdx];

    public bool Reviewed
    {
        get => m_bools[c_reviewedValueIdx];
        set
        {
            m_bools[c_reviewedValueIdx] = value;
            m_boolsSetValues[c_reviewedSetIdx] = true;
        }
    }

    public bool InvoicedSet => m_boolsSetValues[c_invoicedSetIdx];

    public bool Invoiced
    {
        get => m_bools[c_invoicedValueIdx];
        set
        {
            m_bools[c_invoicedValueIdx] = value;
            m_boolsSetValues[c_invoicedSetIdx] = true;
        }
    }

    public bool CancelledSet => m_boolsSetValues[c_cancelledSetIdx];

    public bool Cancelled
    {
        get => m_bools[c_cancelledValueIdx];
        set
        {
            m_bools[c_cancelledValueIdx] = value;
            m_boolsSetValues[c_cancelledSetIdx] = true;
        }
    }

    public bool HotSet => m_boolsSetValues[c_hotSetIdx];

    public bool Hot
    {
        get => m_bools[c_hotValueIdx];
        set
        {
            m_bools[c_hotValueIdx] = value;
            m_boolsSetValues[c_hotSetIdx] = true;
        }
    }

    public bool AutoFinishSet => m_boolsSetValues[c_autoFinishSetIdx];

    public bool AutoFinish
    {
        get => m_bools[c_autoFinishValueIdx];
        set
        {
            m_bools[c_autoFinishValueIdx] = value;
            m_boolsSetValues[c_autoFinishSetIdx] = true;
        }
    }

    public bool CanSpanPlantsSet => m_boolsSetValues[c_canSpanPlantsSetIdx];

    public bool CanSpanPlants
    {
        get => m_bools[c_canSpanPlantsIdx];
        set
        {
            m_bools[c_canSpanPlantsIdx] = value;
            m_boolsSetValues[c_canSpanPlantsSetIdx] = true;
        }
    }

    public bool SetInvalidJobsToDoNotSchedule
    {
        get => m_bools[c_setToDonotscheduleIfInvalid];
        set => m_bools[c_setToDonotscheduleIfInvalid] = value;
    }

    public bool UseResourceSetupIsSet => m_boolsSetValues[c_useResourceSetupIsSetIdx];

    public bool UseResourceSetup
    {
        get => m_bools[c_useResourceSetupIdx];
        set
        {
            m_bools[c_useResourceSetupIdx] = value;
            m_boolsSetValues[c_useResourceSetupIsSetIdx] = true;
        }
    }

    public bool UseOperationSetupIsSet => m_boolsSetValues[c_useOperationSetupIsSetIdx];

    public bool UseOperationSetup
    {
        get => m_bools[c_useOperationSetupIdx];
        set
        {
            m_bools[c_useOperationSetupIdx] = value;
            m_boolsSetValues[c_useOperationSetupIsSetIdx] = true;
        }
    }

    public bool UseSequencedSetupIsSet => m_boolsSetValues[c_useSequencedSetupIsSetIdx];

    public bool UseSequencedSetup
    {
        get => m_bools[c_useSequencedSetupIdx];
        set
        {
            m_bools[c_useSequencedSetupIdx] = value;
            m_boolsSetValues[c_useSequencedSetupIsSetIdx] = true;
        }
    }
    #endregion

    #region DATETIME
    public bool NeedDateSet => m_boolsSetValues[c_needDateSetIdx];

    private DateTime m_needDate;

    public DateTime NeedDate
    {
        get => m_needDate;
        set
        {
            m_needDate = value;
            m_boolsSetValues[c_needDateSetIdx] = true;
        }
    }

    public bool AnchorDateSet => m_boolsSetValues[c_anchorDateSetIdx];

    private DateTime m_anchorDate;

    public DateTime AnchorDate
    {
        get => m_anchorDate;
        set
        {
            m_anchorDate = value;
            m_boolsSetValues[c_anchorDateSetIdx] = true;
        }
    }
    #endregion

    private System.Drawing.Color m_jobColor;

    public System.Drawing.Color JobColor
    {
        get => m_jobColor;
        set
        {
            m_jobColor = value;
            m_boolsSetValues[c_jobColoreSetIdx] = true;
        }
    }

    public bool JobColorSet => m_boolsSetValues[c_jobColoreSetIdx];

    public override string Description => "Job Values set";
}