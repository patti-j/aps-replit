using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.Common.PTMath;
using PT.Common.Range;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Defines a capacity change for a specific time interval for a specific resource.
/// Each Calendar Resource contains two of these to define its start and end conditions plus any number of these can
/// be added by CapacityIntervals and expanded RecurringCapacityIntervals.
/// </summary>
public partial class ResourceCapacityInterval : ISearchableRange, ICloneable, IPTSerializable
{
    public const int UNIQUE_ID = 323;
    public int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public ResourceCapacityInterval(IReader a_reader)
    {
        int intervalTypeValue = 0;
        if (a_reader.VersionNumber >= 12405)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_endDate);
            a_reader.Read(out intervalTypeValue);
            IntervalType = GetIntervalTypeFromIntervalTypeValue(intervalTypeValue);
            a_reader.Read(out m_startDate);

            m_id = new BaseId(a_reader);

            a_reader.Read(out m_capacityCode);
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_endDate);
            a_reader.Read(out intervalTypeValue);
            IntervalType = GetIntervalTypeFromIntervalTypeValue(intervalTypeValue);
            a_reader.Read(out m_startDate);

            m_id = new BaseId(a_reader);

            PreventOperationsFromSpanning = ClearChangeovers = intervalTypeValue == 6;
        }
        else
        {
            if (a_reader.VersionNumber >= 12000)
            {
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_endDate);
                a_reader.Read(out intervalTypeValue);
                IntervalType = GetIntervalTypeFromIntervalTypeValue(intervalTypeValue);
                a_reader.Read(out m_startDate);

                m_id = new BaseId(a_reader);
            }
            else if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_endDate);
                a_reader.Read(out intervalTypeValue);
                IntervalType = GetIntervalTypeFromIntervalTypeValue(intervalTypeValue);
                a_reader.Read(out m_startDate);
            }

            //For backwards compatibility before these were serialized
            PreventOperationsFromSpanning = ClearChangeovers = intervalTypeValue == 6;
        }

        if (a_reader.VersionNumber <= 12402)
        {
            //Set old capacity interval profile values
            CanStartActivity = true;
            UsedForSetup = true;
            UsedForRun = true;
            UsedForPostProcessing = true;
            UsedForClean = true;
            UsedForStorage = true;
            Overtime = intervalTypeValue == 1 || intervalTypeValue == 2; //These use to be Overtime and PotentialOvertime
            CapacityCode = string.Empty;
        }
    }

    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif

        m_bools.Serialize(a_writer);
        a_writer.Write(m_nbrOfPeople);
        a_writer.Write(m_endDate);
        a_writer.Write((int)IntervalType);
        a_writer.Write(m_startDate);
        m_id.Serialize(a_writer);
        a_writer.Write(m_capacityCode);
    }

    /// <summary>
    /// This function is here for backwards compatibility. Some IntervalTypes were removed from the enum, thus
    /// we need to look at the underlying integer value and determine what IntervalType should be returned.
    /// </summary>
    /// <param name="a_intervalTypeValue">An integer value that should correspond to the IntervalType of the CapacityInterval</param>
    /// <returns></returns>
    private static CapacityIntervalDefs.capacityIntervalTypes GetIntervalTypeFromIntervalTypeValue(int a_intervalTypeValue)
    {
        if (a_intervalTypeValue == (int)CapacityIntervalDefs.capacityIntervalTypes.Online
            || a_intervalTypeValue == (int)CapacityIntervalDefs.capacityIntervalTypes.Offline
            || a_intervalTypeValue == (int)CapacityIntervalDefs.capacityIntervalTypes.Occupied
            || a_intervalTypeValue == (int)CapacityIntervalDefs.capacityIntervalTypes.ReservedOnline)
        {
            // These still exist and will be converted 
            return (CapacityIntervalDefs.capacityIntervalTypes)a_intervalTypeValue;
        }

        // The ones below should no longer exist 
        if (a_intervalTypeValue == 1 //Overtime
            || a_intervalTypeValue == 2 //PotentialOvertime
            )
        {
            return CapacityIntervalDefs.capacityIntervalTypes.Online;
        }

        if (a_intervalTypeValue == 7 //Holiday
            || a_intervalTypeValue == 6 ) // Cleanout, which is not the same as UsedForClean
        {
            return CapacityIntervalDefs.capacityIntervalTypes.Offline;
        }

        // Just some random default value I decided to return
        return CapacityIntervalDefs.capacityIntervalTypes.Online;
    }
    #endregion

    #region Construction
    internal ResourceCapacityInterval(BaseId a_intervalId, CapacityIntervalDefs.capacityIntervalTypes a_intervalType, DateTime a_startDateTime, DateTime a_endDateTime, decimal a_nbrOfPeople, IntervalProfile a_intervalProfile)
    {
        Init(a_intervalId, a_intervalType, a_startDateTime.Ticks, a_endDateTime.Ticks, a_nbrOfPeople, a_intervalProfile);
    }

    public ResourceCapacityInterval(ResourceCapacityInterval a_rci)
    {
        Init(a_rci.Id, a_rci.IntervalType, a_rci.StartDateTime.Ticks, a_rci.EndDateTime.Ticks, a_rci.NbrOfPeople, a_rci.GetIntervalProfile());
    }

    internal ResourceCapacityInterval(BaseId a_intervalId, CapacityIntervalDefs.capacityIntervalTypes a_intervalType, long a_startDate, long a_endDate, IntervalProfile a_intervalProfile)
    {
        Init(a_intervalId, a_intervalType, a_startDate, a_endDate, a_intervalProfile);
    }

    internal ResourceCapacityInterval(BaseId a_intervalId, CapacityIntervalDefs.capacityIntervalTypes a_intervalType, long a_startDate, long a_endDate, decimal a_nbrOfPeople, IntervalProfile a_intervalProfile)
    {
        Init(a_intervalId, a_intervalType, a_startDate, a_endDate, a_nbrOfPeople, a_intervalProfile);
    }

    internal void Init(BaseId a_intervalId, CapacityIntervalDefs.capacityIntervalTypes a_intervalType, long a_startDate, long a_endDate, decimal a_nbrOfPeople, IntervalProfile a_intervalProfile)
    {
        if (a_nbrOfPeople <= 0)
        {
            throw new PTValidationException("2159");
        }

        Init(a_intervalId, a_intervalType, a_startDate, a_endDate, a_intervalProfile);
        NbrOfPeople = a_nbrOfPeople;
    }

    internal void Init(BaseId a_intervalId, CapacityIntervalDefs.capacityIntervalTypes a_intervalType, long a_startDate, long a_endDate, IntervalProfile a_intervalProfile)
    {
        if (a_startDate >= a_endDate)
        {
            throw new PTException("2160", new object[] { new DateTime(a_startDate), new DateTime(a_endDate) });
        }

        Id = a_intervalId;
        IntervalType = a_intervalType;
        StartDate = a_startDate;
        EndDate = a_endDate;
        PreventOperationsFromSpanning = a_intervalProfile.PreventOperationsFromSpanning;
        ClearChangeovers = a_intervalProfile.CleanOutSetups;
        CanStartActivity = a_intervalProfile.CanStartActivity;
        UsedForSetup = a_intervalProfile.RunSetup;
        UsedForRun = a_intervalProfile.RunProcessing;
        UsedForPostProcessing = a_intervalProfile.RunPostProcessing;
        UsedForClean = a_intervalProfile.RunCleanout;
        UsedForStorage = a_intervalProfile.RunStoragePostProcessing;
        UseOnlyWhenLate = a_intervalProfile.UseOnlyWhenLate;
        CapacityCode = a_intervalProfile.CapacityCode;
    }
    #endregion

    #region Declarations
    public class ResourceCapacityIntervalException : PTException
    {
        public ResourceCapacityIntervalException(string message)
            : base(message) { }
    }
    #endregion

    #region SharedProperties
    //Strings for Property Categories
    protected const string _RECURRENCE = "Recurrence";
    protected const string _TIME = "Time";
    protected const string _DAYSOFWEEK = "DaysOfWeek";
    protected const string _CAPACITY = "Capacity";

    private decimal m_nbrOfPeople = 1;

    /// <summary>
    /// Multiplied by the duration of the event to calculate the total amount of capacity added by the event.  For example, if the event duration is 8 hours and the NbrOfPeople is 2 then the even specifies
    /// an addition of 16 hours of capacity.  Not used if Type=Offline.
    /// </summary>
    public decimal NbrOfPeople
    {
        get => m_nbrOfPeople;
        set => m_nbrOfPeople = value;
    }

    private long m_endDate;

    /// <summary>
    /// Specifies when the event ends.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    public DateTime EndDateTime
    {
        get => new (EndDate);
        set => EndDate = value.Ticks;
    }

    private CapacityIntervalDefs.capacityIntervalTypes m_intervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;

    /// <summary>
    /// Determines how the event will affect capacity.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    public CapacityIntervalDefs.capacityIntervalTypes IntervalType
    {
        get => m_intervalType;

        set
        {
            m_intervalType = value;

            if (m_intervalType == CapacityIntervalDefs.capacityIntervalTypes.Offline ||
                m_intervalType == CapacityIntervalDefs.capacityIntervalTypes.Occupied)
            {
                m_online = false;
            }
            else
            {
                m_online = true;
            }
        }
    }

    private long m_startDate;

    /// <summary>
    /// Specifies when the event starts.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    public DateTime StartDateTime
    {
        get => new (m_startDate);
        set => m_startDate = value.Ticks;
    }

    private BaseId m_id;

    public BaseId Id
    {
        get => m_id;
        set => m_id = value;
    }

    private string m_capacityCode;

    /// <summary>
    /// The capacity code will prevent using capacity for operations with a different (non-blank) capacity code.
    /// </summary>
    public string CapacityCode
    {
        get => m_capacityCode;
        set => m_capacityCode = value;
    }
    #endregion

    #region Properties
    private bool m_online;

    //TODO: Rename Active to Online
    // Active use to check whether or not the ResourceCapacityInterval was of a certain IntervalType,
    // but we've removed many of those types now due to the introduction of IntervalProfiles
    public bool Active => m_online;

    internal bool IsPastPlanningHorizon => EndDate == PTDateTime.MaxDateTicks;
    #endregion

    private BoolVector32 m_bools;
    private const short c_cleanOutSetupsIdx = 0;
    private const short c_preventOperationsFromSpanningIdx = 1;
    private const short c_canStartActivityIdx = 2;
    private const short c_usedForCleanIdx = 3;
    private const short c_usedForSetupIdx = 4;
    private const short c_usedForRunIdx = 5;
    private const short c_usedForPostProcessingIdx = 6;
    private const short c_usedForStorageIdx = 7;
    private const short c_overtimeIdx = 8;
    private const short c_useOnlyWhenLateIdx = 9;
    private const short c_canDragAndResizeIdx = 10;
    private const short c_canDeleteIdx = 11;

    /// <summary>
    /// Whether to clean out setups on the resource during this interval
    /// </summary>
    public bool ClearChangeovers
    {
        get => m_bools[c_cleanOutSetupsIdx];

        set => m_bools[c_cleanOutSetupsIdx] = value;
    }

    public bool PreventOperationsFromSpanning
    {
        get => m_bools[c_preventOperationsFromSpanningIdx];

        set => m_bools[c_preventOperationsFromSpanningIdx] = value;
    }

    /// <summary>
    /// Whether this interval can start a new activity
    /// </summary>
    public bool CanStartActivity
    {
        get => m_bools[c_canStartActivityIdx];

        set => m_bools[c_canStartActivityIdx] = value;
    }

    /// <summary>
    /// Whether this interval will be used for scheduling Cleanouts
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForClean
    {
        get => m_bools[c_usedForCleanIdx];

        set => m_bools[c_usedForCleanIdx] = value;
    }

    /// <summary>
    /// Whether this interval will be used for scheduling Setups
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForSetup
    {
        get => m_bools[c_usedForSetupIdx];

        set => m_bools[c_usedForSetupIdx] = value;
    }

    /// <summary>
    /// Whether this interval will be used for scheduling Run
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForRun
    {
        get => m_bools[c_usedForRunIdx];

        set => m_bools[c_usedForRunIdx] = value;
    }

    /// <summary>
    /// Whether this interval will be used for scheduling Post Processing
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForPostProcessing
    {
        get => m_bools[c_usedForPostProcessingIdx];

        set => m_bools[c_usedForPostProcessingIdx] = value;
    }

    /// <summary>
    /// Whether this interval will be used for scheduling Storage Post Processing
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForStorage
    {
        get => m_bools[c_usedForStorageIdx];

        set => m_bools[c_usedForStorageIdx] = value;
    }

    /// <summary>
    /// Whether this interval incurs overtime costs
    /// </summary>
    public bool Overtime
    {
        get => m_bools[c_overtimeIdx];

        set => m_bools[c_overtimeIdx] = value;
    }
    
    /// <summary>
    /// Whether this interval will only be used if the activity is late and scheduling on this interval
    /// </summary>
    public bool UseOnlyWhenLate
    {
        get => m_bools[c_useOnlyWhenLateIdx];

        set => m_bools[c_useOnlyWhenLateIdx] = value;
    }

    public bool CanDragAndResize
    {
        get => m_bools[c_canDragAndResizeIdx];

        set
        {
            m_bools[c_canDragAndResizeIdx] = value;
        }
    }

    public bool CanDelete
    {
        get => m_bools[c_canDeleteIdx];

        set
        {
            m_bools[c_canDeleteIdx] = value;
        }
    }

    #region Cloning
    public ResourceCapacityInterval Clone()
    {
        return new ResourceCapacityInterval(this);
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    public long StartDate
    {
        get => m_startDate;

        internal set => m_startDate = value;
    }

    public long EndDate
    {
        get => m_endDate;

        internal set
        {
            if (value <= StartDate)
            {
                throw new PTValidationException("2161");
            }

            m_endDate = value;
        }
    }

    /// <summary>
    /// Whether this interval intersects contains another interval.
    /// </summary>
    /// <param name="a_intervalStartDate">The start date of the comparison interval.</param>
    /// <param name="a_intervalEndDate">The end date of the comparison interval.</param>
    /// <returns>Whether the intervals intersect.</returns>
    public bool ContainsInterval(long a_intervalStartDate, long a_intervalEndDate)
    {
        return m_startDate >= a_intervalStartDate && m_endDate < a_intervalEndDate;
    }

    public bool Intersects(long a_intervalStartDate, long a_intervalEndDate)
    {
        return Common.PTMath.Interval.Intersection(m_startDate, m_endDate, a_intervalStartDate, a_intervalEndDate);
    }

    internal bool FindIntersection(long a_startTicks, long a_finishTicks, ref long r_intersectionStartTicks, ref long r_intersectionFinishTicks)
    {
        return Common.PTMath.Interval.Intersection(a_startTicks, a_finishTicks, StartDate, EndDate, out r_intersectionStartTicks, out r_intersectionFinishTicks);
    }

    /// <summary>
    /// Whether a point in time is contained within this interval.
    /// </summary>
    /// <param name="a_dt"></param>
    /// <returns></returns>
    internal bool Contains(long a_dt)
    {
        return a_dt >= m_startDate && a_dt < m_endDate;
    }

    internal enum ContainmentType { Contains, LessThan, GreaterThan }

    internal ContainmentType ContainsStartPoint(long a_startPoint)
    {
        if (a_startPoint >= m_startDate && a_startPoint < m_endDate)
        {
            return ContainmentType.Contains;
        }

        if (a_startPoint < m_startDate)
        {
            return ContainmentType.LessThan;
        }

        return ContainmentType.GreaterThan;
    }

    internal ContainmentType ContainsEndPoint(long a_endPoint)
    {
        if (a_endPoint > m_startDate && a_endPoint <= m_endDate)
        {
            return ContainmentType.Contains;
        }

        if (a_endPoint <= m_startDate)
        {
            return ContainmentType.LessThan;
        }

        return ContainmentType.GreaterThan;
    }

    /// <summary>
    /// End Date minus Start Date.
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetDuration()
    {
        return new TimeSpan(EndDate - StartDate);
    }

    #region Debugging
    [DebugLogging(EDebugLoggingType.None)]
    public override string ToString()
    {
        return string.Format("{0} to {1}; online={2}; People={3}; Type={4};", StartDateTime, EndDateTime, Active.ToString(), NbrOfPeople.ToString(), m_intervalType.ToString());
    }
    #endregion

    public DateTime Start => StartDateTime;
    public DateTime End => EndDateTime;

    public bool ContainsPoint(DateTime a_dt)
    {
        return a_dt.Ticks >= Start.Ticks && a_dt.Ticks <= End.Ticks;
    }

    internal IntervalProfile GetIntervalProfile()
    {
        return new IntervalProfile(
            ClearChangeovers,
            PreventOperationsFromSpanning,
            CanStartActivity,
            UsedForSetup,
            UsedForRun,
            UsedForPostProcessing,
            UsedForClean,
            UsedForStorage,
            Overtime,
            UseOnlyWhenLate,
            CapacityCode
        );
    }

    internal decimal CalculateCapacityMultiple(decimal a_actNbrOfPeople)
    {
        decimal nbrOfMultiples = NbrOfPeople / a_actNbrOfPeople;
        nbrOfMultiples = Math.Truncate(nbrOfMultiples);
        decimal nbrOfPeopleMultiple = nbrOfMultiples * a_actNbrOfPeople;
        return nbrOfPeopleMultiple;
    }
}