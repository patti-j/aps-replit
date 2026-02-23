using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// For creating a CapacityInterval via ERP transmission.
/// </summary>
public class CapacityInterval : PTObjectBase, IPTSerializable
{
    public new const int UNIQUE_ID = 35;

    #region IPTSerializable Members
    public CapacityInterval(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12406)
        {
            m_bools = new BoolVector32(a_reader);
            m_setFlags = new BoolVector32(a_reader);

            a_reader.Read(out nbrOfPeople);

            a_reader.Read(out int val);
            intervalType = (CapacityIntervalDefs.capacityIntervalTypes)val;

            a_reader.Read(out m_endDate);
            a_reader.Read(out m_startDate);
            a_reader.Read(out m_color);
            a_reader.Read(out m_capacityCode);
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out nbrOfPeople);

            a_reader.Read(out int val);
            intervalType = (CapacityIntervalDefs.capacityIntervalTypes)val;

            a_reader.Read(out m_endDate);
            a_reader.Read(out m_startDate);
            a_reader.Read(out m_color);
            m_setFlags = new BoolVector32(a_reader);
        }

        #region Version 96
        else if (a_reader.VersionNumber >= 96)
        {
            a_reader.Read(out nbrOfPeople);
            int val;
            a_reader.Read(out val);
            intervalType = (CapacityIntervalDefs.capacityIntervalTypes)val;

            a_reader.Read(out m_endDate);
            a_reader.Read(out m_startDate);
            m_setFlags = new BoolVector32(a_reader);
        }
        #endregion

        if (a_reader.VersionNumber <= 12402)
        {
            //Set old capacity interval profile values
            CanStartActivity = true;
            UsedForSetup = true;
            UsedForRun = true;
            UsedForPostProcessing = true;
            UsedForClean = true;
            UsedForStorage = true;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        m_setFlags.Serialize(a_writer);

        a_writer.Write(nbrOfPeople);
        a_writer.Write((int)intervalType);

        a_writer.Write(m_endDate);
        a_writer.Write(m_startDate);
        a_writer.Write(m_color);
        a_writer.Write(m_capacityCode);
    }

    public override int UniqueId => UNIQUE_ID;

    private BoolVector32 m_setFlags;

    private const int c_nbrOfPeopleSetIdx = 0;
    private const int c_startDateTimeSetIdx = 1;
    private const int c_endDateTimeSetIdx = 2;
    private const int c_intervalTypeSetIdx = 3;
    private const int c_colorSetIdx = 4;
    private const int c_cleanOutSetupsSetIdx = 5;
    private const int c_preventOperationsFromSpanningSetIdx = 6;
    private const short c_canStartActivityIsSetIdx = 7;
    private const short c_usedForCleanIsSetIdx = 8;
    private const short c_usedForSetupIsSetIdx = 9;
    private const short c_usedForRunIsSetIdx = 10;
    private const short c_usedForPostProcessingIsSetIdx = 11;
    private const short c_usedForStoragePostProcessingIsSetIdx = 12;
    private const short c_overtimeIsSetIdx = 13;
    private const short c_useOnlyWhenLateIsSetIdx = 14;
    private const short c_capacityCodeIsSetIdx = 15;

    private BoolVector32 m_bools;
    private const int c_cleanOutSetupIdx = 0;
    private const int c_preventOperationsFromSpanningIdx = 1;
    private const short c_canStartActivityIdx = 2;
    private const short c_usedForCleanIdx = 3;
    private const short c_usedForSetupIdx = 4;
    private const short c_usedForRunIdx = 5;
    private const short c_usedForPostProcessingIdx = 6;
    private const short c_usedForStoragePostProcessingIdx = 7;
    private const short c_overtimeIdx = 8;
    private const short c_useOnlyWhenLateIdx = 9;
    private const short c_canDragAndResizeIdx = 10;
    private const short c_canDeleteIdx = 11;

    #endregion

    public CapacityInterval(string externalId, string name)
        : base(externalId, name) { }

    public CapacityInterval()
        : base("", "") { }

    #region SharedProperties
    //Strings for Property Categories
    protected const string _TIME = "Time";
    protected const string _DAYSOFWEEK = "DaysOfWeek";
    protected const string _CAPACITY = "Capacity";

    private decimal nbrOfPeople = 1;

    /// <summary>
    /// Multiplied by the duration of the event to calculate the total amount of capacity added by the event.  For example, if the event duration is 8 hours and the NbrOfPeople is 2 then the even specifies
    /// an addition of 16 hours of capacity.  Not used if Type=Offline.
    /// </summary>
    public decimal NbrOfPeople
    {
        get => nbrOfPeople;
        set
        {
            nbrOfPeople = value;
            m_setFlags[c_nbrOfPeopleSetIdx] = true;
        }
    }

    public bool NbrOfPeopleSet => m_setFlags[c_nbrOfPeopleSetIdx];

    private long m_endDate = PTDateTime.UtcNow.RemoveSeconds().AddHours(4).Ticks;

    /// <summary>
    /// Specifies when the event ends.
    /// </summary>
    [Required(true)]
    public DateTime EndDateTime
    {
        get => new (m_endDate);
        set
        {
            m_endDate = value.Ticks;
            m_setFlags[c_endDateTimeSetIdx] = true;
        }
    }

    public bool EndDateTimeSet => m_setFlags[c_endDateTimeSetIdx];

    private CapacityIntervalDefs.capacityIntervalTypes intervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;

    /// <summary>
    /// Determines how the event will affect capacity.
    /// </summary>
    [Required(true)]
    public CapacityIntervalDefs.capacityIntervalTypes IntervalType
    {
        get => intervalType;
        set
        {
            intervalType = value;
            m_setFlags[c_intervalTypeSetIdx] = true;
        }
    }

    public bool IntervalTypeSet => m_setFlags[c_intervalTypeSetIdx];

    private long m_startDate = PTDateTime.UtcNow.RemoveSeconds().Ticks;

    /// <summary>
    /// Specifies when the event starts.
    /// </summary>
    [Required(true)]
    public DateTime StartDateTime
    {
        get => new (m_startDate);
        set
        {
            m_startDate = value.Ticks;
            m_setFlags[c_startDateTimeSetIdx] = true;
        }
    }

    public bool StartDateTimSet => m_setFlags[c_startDateTimeSetIdx];

    private System.Drawing.Color m_color;

    public System.Drawing.Color Color
    {
        get => m_color;
        set
        {
            m_color = value;
            m_setFlags[c_colorSetIdx] = true;
        }
    }

    public bool ColorSet => m_setFlags[c_colorSetIdx];

    public bool CleanOutSetups
    {
        get => m_bools[c_cleanOutSetupIdx];
        set
        {
            m_bools[c_cleanOutSetupIdx] = value;
            m_setFlags[c_cleanOutSetupsSetIdx] = true;
        }
    }

    public bool CleanOutSetupsSet => m_setFlags[c_cleanOutSetupsSetIdx];

    public bool PreventOperationsFromSpanning
    {
        get => m_bools[c_preventOperationsFromSpanningIdx];
        set
        {
            m_bools[c_preventOperationsFromSpanningIdx] = value;
            m_setFlags[c_preventOperationsFromSpanningSetIdx] = true;
        }
    }

    public bool PreventOperationsFromSpanningSet => m_setFlags[c_preventOperationsFromSpanningSetIdx];

    /// <summary>
    /// Whether this interval can start a new activity
    /// </summary>
    public bool CanStartActivity
    {
        get => m_bools[c_canStartActivityIdx];

        set
        {
            m_bools[c_canStartActivityIdx] = value;
            m_setFlags[c_canStartActivityIsSetIdx] = true;
        }
    }

    public bool CanStartActivityIsSet => m_setFlags[c_canStartActivityIsSetIdx];

    /// <summary>
    /// Whether this interval will be used for scheduling Cleanouts
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForClean
    {
        get => m_bools[c_usedForCleanIdx];

        set
        {
            m_bools[c_usedForCleanIdx] = value;
            m_setFlags[c_usedForCleanIsSetIdx] = true;
        }
    }

    public bool UsedForCleanIsSet => m_setFlags[c_usedForCleanIsSetIdx];

    /// <summary>
    /// Whether this interval will be used for scheduling Setups
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForSetup
    {
        get => m_bools[c_usedForSetupIdx];

        set
        {
            m_bools[c_usedForSetupIdx] = value;
            m_setFlags[c_usedForSetupIsSetIdx] = true;
        }
    }

    public bool UsedForSetupIsSet => m_setFlags[c_usedForSetupIsSetIdx];

    /// <summary>
    /// Whether this interval will be used for scheduling Run
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForRun
    {
        get => m_bools[c_usedForRunIdx];

        set
        {
            m_bools[c_usedForRunIdx] = value;
            m_setFlags[c_usedForRunIsSetIdx] = true;
        }
    }

    public bool UsedForRunIsSet => m_setFlags[c_usedForRunIsSetIdx];

    /// <summary>
    /// Whether this interval will be used for scheduling Post Processing
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForPostProcessing
    {
        get => m_bools[c_usedForPostProcessingIdx];

        set
        {
            m_bools[c_usedForPostProcessingIdx] = value;
            m_setFlags[c_usedForPostProcessingIsSetIdx] = true;
        }
    }

    public bool UsedForPostProcessingIsSet => m_setFlags[c_usedForPostProcessingIsSetIdx];

    /// <summary>
    /// Whether this interval will be used for scheduling Storage Post Processing
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForStorage
    {
        get => m_bools[c_usedForStoragePostProcessingIdx];

        set
        {
            m_bools[c_usedForStoragePostProcessingIdx] = value;
            m_setFlags[c_usedForStoragePostProcessingIsSetIdx] = true;
        }
    }

    public bool UsedForStorageIsSet => m_setFlags[c_usedForStoragePostProcessingIsSetIdx];

    /// <summary>
    /// Whether this interval will incur overtime costs
    /// </summary>
    public bool Overtime
    {
        get => m_bools[c_overtimeIdx];

        set
        {
            m_bools[c_overtimeIdx] = value;
            m_setFlags[c_overtimeIsSetIdx] = true;

        }
    }

    public bool OvertimeIsSet => m_setFlags[c_overtimeIsSetIdx];

    /// <summary>
    /// Whether this interval will only be used if the activity is late and scheduling on this interval
    /// </summary>
    public bool UseOnlyWhenLate
    {
        get => m_bools[c_useOnlyWhenLateIdx];

        set
        {
            m_bools[c_useOnlyWhenLateIdx] = value;
            m_setFlags[c_useOnlyWhenLateIsSetIdx] = true;
        }
    }

    public bool UseOnlyWhenLateIsSet => m_setFlags[c_useOnlyWhenLateIsSetIdx];

    public bool CanDragAndResize
    {
        get => m_bools[c_canDragAndResizeIdx];

        set
        {
            m_bools[c_canDragAndResizeIdx] = value;
        }
    }

    public bool CanDragAndResizeIsSet => m_setFlags[c_canDragAndResizeIdx];

    public bool CanDelete
    {
        get => m_bools[c_canDeleteIdx];

        set
        {
            m_bools[c_canDeleteIdx] = value;
        }
    }

    public bool CanDeleteIsSet => m_setFlags[c_canDeleteIdx];

    private string m_capacityCode;
    /// <summary>
    /// The capacity code will prevent using capacity for operations with a different (non-blank) capacity code.
    /// </summary>
    public string CapacityCode
    {
        get => m_capacityCode;
        set
        {
            m_capacityCode = value;
            m_setFlags[c_capacityCodeIsSetIdx] = true;
        }
    }

    public bool CapacityCodeIsSet => m_setFlags[c_capacityCodeIsSetIdx];
    #endregion
}