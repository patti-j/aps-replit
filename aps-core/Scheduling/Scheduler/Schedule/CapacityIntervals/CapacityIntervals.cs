using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using System.Drawing;
using System.Globalization;

using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Defines the capacity for CalendarResources by specifying Regular Time, Overtime, Offline time and PotentialOvertime.
/// </summary>
public class CapacityInterval : BaseObject, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 339;

    #region IPTSerializable Members
    public CapacityInterval(IReader a_reader)
        : base(a_reader)
    {
        int intervalTypeValue = 0;
        if (a_reader.VersionNumber >= 12405)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out intervalTypeValue);
            IntervalType = GetIntervalTypeFromIntervalTypeValue(intervalTypeValue);

            a_reader.Read(out m_endDateTime);
            a_reader.Read(out m_startDateTime);

            a_reader.Read(out m_color);

            a_reader.Read(out m_capacityCode);
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out intervalTypeValue);
            IntervalType = GetIntervalTypeFromIntervalTypeValue(intervalTypeValue);

            a_reader.Read(out m_endDateTime);
            a_reader.Read(out m_startDateTime);

            a_reader.Read(out m_color);

            PreventOperationsFromSpanning = CleanOutSetups = intervalTypeValue == 6;
        }
        else
        {
            if (a_reader.VersionNumber >= 232)
            {
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out intervalTypeValue);
                IntervalType = GetIntervalTypeFromIntervalTypeValue(intervalTypeValue);

                a_reader.Read(out m_endDateTime);
                a_reader.Read(out m_startDateTime);

                m_bools = new BoolVector32(a_reader);
            }

            InitDefaultColors(intervalTypeValue);

            PreventOperationsFromSpanning = CleanOutSetups = intervalTypeValue == 6; 
            // 6 use to be CapacityIntervalDefs.capacityIntervalTypes.Cleanout,
            // which is equivalent to the CapacityInterval preset, Maintenance now. 
            // Cleanout type, is not exactly the same as a capacity interval's UsedForClean being set to true.
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
            CapacityCode = string.Empty;
        }

        if (a_reader.VersionNumber <= 12404)
        {
            //Check start and end dates
            if (EndDateTime < StartDateTime)
            {
                (StartDateTime, EndDateTime) = (EndDateTime, StartDateTime);
            }
            else if (StartDateTime == EndDateTime)
            {
                //Just use some value so there isn't an error
                EndDateTime = StartDateTime.AddHours(1);
            }
        }
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
            return (CapacityIntervalDefs.capacityIntervalTypes) a_intervalTypeValue;
        }

        // The ones below should no longer exist 
        if (a_intervalTypeValue == 1 //Overtime
            || a_intervalTypeValue == 2 //PotentialOvertime
            ) 
        {
            return CapacityIntervalDefs.capacityIntervalTypes.Online;
        }

        if (a_intervalTypeValue == 7  //Holiday
            || a_intervalTypeValue == 6)  //Cleanout, which is equivalent to Interval preset, Maintenance, not the same as UsedForClean == true
        {
            return CapacityIntervalDefs.capacityIntervalTypes.Offline;
        }

        // Just some random default value I decided to return
        return CapacityIntervalDefs.capacityIntervalTypes.Online;
    }

    /// <summary>
    /// This function is for backwards compatibility for both Color and IntervalType.
    /// Color use to not be serialized so we're setting color based on IntervalType for
    /// older scenarios. A few values were removed from the IntervalType enum, and this
    /// would cause some issues with casting the int value into those deleted enum values.
    /// This is why everything is done using the int type instead of CapacityIntervalDefs.capacityIntervalTypes 
    /// </summary>
    /// <param name="a_typeVal"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void InitDefaultColors(int a_typeVal)
    {
        switch (a_typeVal)
        {
            case (int)CapacityIntervalDefs.capacityIntervalTypes.Online: 
                m_color = Color.FromArgb(144, 220, 144);
                break;
            case 1: // Use to be Overtime
                m_color = ColorUtils.ChangeColorBrightness(Color.PaleVioletRed, .42f);
                break;
            case 2: // Use to be PotentialOvertime
                m_color = Color.PaleVioletRed;
                break;
            case (int)CapacityIntervalDefs.capacityIntervalTypes.Offline:
                m_color = Color.FromArgb(210, 200, 200, 200);
                break;
            case 4: // CapacityIntervalDefs.capacityIntervalTypes.Occupied
                m_color = Color.FromArgb(210, 200, 200, 200);
                break;
            case 5: // CapacityIntervalDefs.capacityIntervalTypes.ReservedOnline
                m_color = Color.FromArgb(144, 220, 144);
                break;
            case 6: // CapacityIntervalDefs.capacityIntervalTypes.Cleanout
                m_color = ColorUtils.ChangeColorBrightness(Color.Blue, .35f);
                break;
            case 7: //CapacityIntervalDefs.capacityIntervalTypes.Holiday
                m_color = Color.Yellow;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);

        a_writer.Write(NbrOfPeople);
        a_writer.Write((int)IntervalType);

        a_writer.Write(EndDateTime);
        a_writer.Write(StartDateTime);

        a_writer.Write(m_color);

        a_writer.Write(m_capacityCode);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CapacityInterval(BaseId a_id, long a_clock)
        : base(a_id)
    {
        InitTimes(a_clock);
        InitProfile(true);
        Color = ColorUtils.ColorCodes.CapacityIntervalOnlineColor;
        CanDelete = true;
        CanDragAndResize = true;
    }

    internal CapacityInterval(
        BaseId a_id, 
        CapacityIntervalDefs.capacityIntervalTypes a_intervalType, 
        DateTime a_startDateTime, 
        DateTime a_endDateTime, 
        IntervalProfile a_intervalProfile, 
        Color a_color,
        bool a_canDragAndResize,
        bool a_canDelete)
        : base(a_id)
    {
        IntervalType = a_intervalType;
        StartDateTime = a_startDateTime;
        EndDateTime = a_endDateTime;
        PreventOperationsFromSpanning = a_intervalProfile.PreventOperationsFromSpanning;
        CleanOutSetups = a_intervalProfile.CleanOutSetups;
        CanStartActivity = a_intervalProfile.CanStartActivity;
        UsedForSetup = a_intervalProfile.RunSetup;
        UsedForRun = a_intervalProfile.RunProcessing;
        UsedForPostProcessing = a_intervalProfile.RunPostProcessing;
        UsedForClean = a_intervalProfile.RunCleanout;
        UsedForStorage = a_intervalProfile.RunStoragePostProcessing;
        Overtime = a_intervalProfile.Overtime;
        UseOnlyWhenLate = a_intervalProfile.UseOnlyWhenLate;
        CapacityCode = a_intervalProfile.CapacityCode;
        Color = a_color;
        CanDelete = a_canDelete;
        CanDragAndResize = a_canDragAndResize;
    }

    internal CapacityInterval(UserFieldDefinitionManager a_udfManager, PT.Transmissions.CapacityInterval a_originalCi, BaseId a_id, DateTime a_clockDate, DateTime a_planningHorizonEnd, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
        : base(a_id, a_originalCi)
    {
        InitProfile(a_t is not ERPMaintenanceTransmission<CapacityIntervalT.CapacityIntervalDef> and not ERPMaintenanceTransmission<RecurringCapacityIntervalDef>);
        Update(a_udfManager, a_originalCi, out bool _, a_t, a_clockDate, a_planningHorizonEnd, a_dataChanges);
        InitTimes(a_clockDate.Ticks);
        CanDelete = true;
        CanDragAndResize = true;
    }

    public CapacityInterval(BaseId a_id, CapacityInterval a_original)
        : base(a_id, a_original)
    {
        EndDateTime = a_original.EndDateTime;
        IntervalType = a_original.IntervalType;
        NbrOfPeople = a_original.NbrOfPeople;
        StartDateTime = a_original.StartDateTime;
        m_bools = new BoolVector32(a_original.m_bools);
        Color = a_original.Color;
    }

    private void InitTimes(long a_clock)
    {
        InitTime(ref m_startDateTime, a_clock, 8);
        InitTime(ref m_endDateTime, a_clock, 16);
    }

    private static void InitTime(ref DateTime r_dt, long a_clock, long a_hours)
    {
        if (r_dt.Ticks == 0)
        {
            DateTime clockDateTime = new (a_clock);
            r_dt = clockDateTime.AddHours(a_hours);
        }
    }

    private void InitProfile(bool a_enabled)
    {
        CanStartActivity = a_enabled;
        UsedForSetup = a_enabled;
        UsedForRun = a_enabled;
        UsedForPostProcessing = a_enabled;
        UsedForClean = a_enabled;
        UsedForStorage = a_enabled;
    }

    internal bool Update(UserFieldDefinitionManager a_udfManager, Transmissions.CapacityInterval a_copyFrom, out bool a_needToReExpand, PTTransmission a_t, DateTime a_clockDate, DateTime a_planningHorizonEnd, IScenarioDataChanges a_dataChanges)
    {
        a_needToReExpand = false;
        ValidateUpdate(a_copyFrom);
        bool updated = base.Update(a_copyFrom, a_t, a_udfManager, UserField.EUDFObjectType.CapacityIntervals);

        if (a_copyFrom.NbrOfPeopleSet && a_copyFrom.NbrOfPeople != NbrOfPeople)
        {
            NbrOfPeople = a_copyFrom.NbrOfPeople;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.IntervalTypeSet && a_copyFrom.IntervalType != IntervalType)
        {
            IntervalType = a_copyFrom.IntervalType;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        TimeSpan originalDuration = EndDateTime - StartDateTime;
         
        //Validate the end time is after the start time.
        if (a_copyFrom.StartDateTimSet && a_copyFrom.EndDateTimeSet)
        {
            if (a_copyFrom.StartDateTime >= a_copyFrom.EndDateTime)
            {
                throw new PTValidationException("3060", new object[] { ExternalId });
            }
        }
        else if (a_copyFrom.StartDateTimSet)
        {
            if (a_copyFrom.StartDateTime >= EndDateTime)
            {
                throw new PTValidationException("3060", new object[] { ExternalId });
            }
        }
        else if (a_copyFrom.EndDateTimeSet)
        {
            if (a_copyFrom.EndDateTime <= StartDateTime)
            {
                throw new PTValidationException("3060", new object[] { ExternalId });
            }
        }

        if (a_copyFrom.EndDateTimeSet && a_copyFrom.EndDateTime != EndDateTime)
        {
            EndDateTime = a_copyFrom.EndDateTime;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.StartDateTimSet && a_copyFrom.StartDateTime != StartDateTime)
        {
            StartDateTime = a_copyFrom.StartDateTime;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
            a_needToReExpand = true;
        }

        if (a_copyFrom.ColorSet && a_copyFrom.Color != Color)
        {
            Color = a_copyFrom.Color;
            updated = true;
        }

        if (a_copyFrom.CleanOutSetupsSet && a_copyFrom.CleanOutSetups != CleanOutSetups)
        {
            CleanOutSetups = a_copyFrom.CleanOutSetups;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.PreventOperationsFromSpanningSet && a_copyFrom.PreventOperationsFromSpanning != PreventOperationsFromSpanning)
        {
            PreventOperationsFromSpanning = a_copyFrom.PreventOperationsFromSpanning;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (originalDuration != EndDateTime - StartDateTime)
        {
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
            a_needToReExpand = true;
        }

        if (a_copyFrom.CanStartActivityIsSet && a_copyFrom.CanStartActivity != CanStartActivity)
        {
            CanStartActivity = a_copyFrom.CanStartActivity;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.UsedForSetupIsSet && a_copyFrom.UsedForSetup != UsedForSetup)
        {
            UsedForSetup = a_copyFrom.UsedForSetup;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.UsedForRunIsSet && a_copyFrom.UsedForRun != UsedForRun)
        {
            UsedForRun = a_copyFrom.UsedForRun;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.UsedForPostProcessingIsSet && a_copyFrom.UsedForPostProcessing != UsedForPostProcessing)
        {
            UsedForPostProcessing = a_copyFrom.UsedForPostProcessing;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.UsedForCleanIsSet && a_copyFrom.UsedForClean != UsedForClean)
        {
            UsedForClean = a_copyFrom.UsedForClean;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.UsedForStorageIsSet && a_copyFrom.UsedForStorage != UsedForStorage)
        {
            UsedForStorage = a_copyFrom.UsedForStorage;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.OvertimeIsSet && a_copyFrom.OvertimeIsSet != Overtime)
        {
            Overtime = a_copyFrom.Overtime;
            updated = true;
        }

        if (a_copyFrom.UseOnlyWhenLateIsSet && a_copyFrom.UseOnlyWhenLate != UseOnlyWhenLate)
        {
            UseOnlyWhenLate = a_copyFrom.UseOnlyWhenLate;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        string currentCapacityCode = CapacityCode ?? string.Empty;
        if (a_copyFrom.CapacityCodeIsSet && a_copyFrom.CapacityCode != currentCapacityCode)
        {
            CapacityCode = a_copyFrom.CapacityCode;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.CanDeleteIsSet && a_copyFrom.CanDelete != CanDelete)
        {
            CanDelete = a_copyFrom.CanDelete;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_copyFrom.CanDragAndResizeIsSet && a_copyFrom.CanDragAndResize != CanDragAndResize)
        {
            CanDragAndResize = a_copyFrom.CanDragAndResize;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        //Only regenerate here if it wasn't a RecurringCapacityInterval update. Recurring Intervals need to expand first.
        if (updated)
        {
            RegenerateCapacityProfiles(a_planningHorizonEnd.Ticks, true);
            a_dataChanges.FlagConstraintChanges(Id);
        }

        return updated;
    }

    internal void ValidateUpdate(Transmissions.CapacityInterval a_copyFrom)
    {
        if (a_copyFrom.StartDateTimSet && a_copyFrom.EndDateTimeSet)
        {
            if (a_copyFrom.EndDateTime <= a_copyFrom.StartDateTime || a_copyFrom.StartDateTime < PTDateTime.MinDateTime || a_copyFrom.EndDateTime > PTDateTime.MaxDateTime)
            {
                throw new CapacityIntervalException("2929", new object[] { a_copyFrom.ExternalId, a_copyFrom.StartDateTime, a_copyFrom.EndDateTime });
            }
        }
        else if (a_copyFrom.StartDateTimSet)
        {
            if (EndDateTime <= a_copyFrom.StartDateTime || a_copyFrom.StartDateTime < PTDateTime.MinDateTime)
            {
                throw new CapacityIntervalException("2929", new object[] { a_copyFrom.ExternalId, a_copyFrom.StartDateTime, EndDateTime });
            }
        }
        else if (a_copyFrom.EndDateTimeSet)
        {
            if (a_copyFrom.EndDateTime <= StartDateTime || a_copyFrom.EndDateTime > PTDateTime.MaxDateTime)
            {
                throw new CapacityIntervalException("2929", new object[] { a_copyFrom.ExternalId, StartDateTime, a_copyFrom.EndDateTime });
            }
        }
        else if ((EndDateTime - StartDateTime).TotalHours > 24)
        {
            // Capacity Durations are not allowed to span over 24h's.
            throw new CapacityIntervalException("3067", new object[] { StartDateTime, EndDateTime });
        }

        if (a_copyFrom.NbrOfPeopleSet && a_copyFrom.NbrOfPeople <= 0)
        {
            throw new CapacityIntervalException("2930", new object[] { a_copyFrom.ExternalId });
        }

        if (a_copyFrom.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Offline)
        {
            if (a_copyFrom.Overtime)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "Overtime" });
            }
            if (a_copyFrom.CanStartActivity)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "CanStartActivity" });
            }
            if (a_copyFrom.UseOnlyWhenLate)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "UseOnlyWhenLate" });
            }
            if (a_copyFrom.UsedForSetup)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "UsedForSetup" });
            }
            if (a_copyFrom.UsedForRun)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "UsedForRun" });
            }
            if (a_copyFrom.UsedForPostProcessing)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "UsedForPostProcessing" });
            }
            if (a_copyFrom.UsedForStorage)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "UsedForStoragePostProcessing" });
            }
            if (a_copyFrom.UsedForClean)
            {
                throw new CapacityIntervalException("3069", new object[] { a_copyFrom.ExternalId, "UsedForClean" });
            }
        }
    }

    
    #region Declarations
    public class CapacityIntervalException : PTHandleableException
    {
        public CapacityIntervalException(string a_message)
            : base(a_message) { }

        public CapacityIntervalException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }

        public CapacityIntervalException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region SharedProperties
    //Strings for Property Categories
    protected const string _RECURRENCE = "Recurrence";
    protected const string _TIME = "Time";
    protected const string _DAYSOFWEEK = "DaysOfWeek";
    protected const string _CAPACITY = "Capacity";

    //Override ExternalId so it can be made editable so that the user can set it to match their 
    //   system ids even if they create the object in PT manually.
    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.AllowEdit)]
    public override string ExternalId
    {
        get => base.ExternalId;
        internal set => base.ExternalId = value;
    }

    private decimal m_nbrOfPeople = 1;

    /// <summary>
    /// Multiplied by the duration of the event to calculate the total amount of capacity added by the event.  For example, if the event duration is 8 hours and the NbrOfPeople is 2 then the even specifies
    /// an addition of 16 hours of capacity.  Not used if Type=Offline.
    /// </summary>
    public decimal NbrOfPeople
    {
        get => m_nbrOfPeople;
        internal set
        {
            m_nbrOfPeople = value;
        }
    }

    private DateTime m_endDateTime;

    /// <summary>
    /// Specifies when the event ends.
    /// </summary>
    public DateTime EndDateTime
    {
        get => m_endDateTime;
        internal set
        {
            m_endDateTime = value;
        }
    }

    private CapacityIntervalDefs.capacityIntervalTypes m_intervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;

    /// <summary>
    /// Determines how the event will affect capacity.
    /// </summary>
    public CapacityIntervalDefs.capacityIntervalTypes IntervalType
    {
        get => m_intervalType;
        private set
        {
            m_intervalType = value;
        }
    }

    private DateTime m_startDateTime;

    /// <summary>
    /// Specifies when the event starts.
    /// </summary>
    public DateTime StartDateTime
    {
        get => m_startDateTime;
        internal set
        {
            m_startDateTime = value;
        }
    }

    private Color m_color;

    public Color Color
    {
        get => m_color;
        internal set => m_color = value;
    }

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
        }
    }

    #region bools
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
    public bool CleanOutSetups
    {
        get => m_bools[c_cleanOutSetupsIdx];

        set
        {
            m_bools[c_cleanOutSetupsIdx] = value;
        }
    }

    public bool PreventOperationsFromSpanning
    {
        get => m_bools[c_preventOperationsFromSpanningIdx];

        set
        {
            m_bools[c_preventOperationsFromSpanningIdx] = value;
        }
    }

    public bool CanStartActivity
    {
        get => m_bools[c_canStartActivityIdx];

        set
        {
            m_bools[c_canStartActivityIdx] = value;
        }
    }

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
        }
    }

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
        }
    }

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
        }
    }

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
        }
    }

    /// <summary>
    /// Whether this interval will be used for scheduling Storage Post Processing
    /// If not, the activity may span this interval but capacity won't be used
    /// </summary>
    public bool UsedForStorage
    {
        get => m_bools[c_usedForStorageIdx];

        set
        {
            m_bools[c_usedForStorageIdx] = value;
        }
    }

    /// <summary>
    /// Whether this interval will incur overtime costs
    /// </summary>
    public bool Overtime
    {
        get => m_bools[c_overtimeIdx];

        set
        {
            m_bools[c_overtimeIdx] = value;
        }
    }

    /// <summary>
    /// Whether this interval will only be used if the activity is late and scheduling on this interval
    /// </summary>
    public bool UseOnlyWhenLate
    {
        get => m_bools[c_useOnlyWhenLateIdx];

        set
        {
            m_bools[c_useOnlyWhenLateIdx] = value;
        }
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
    #endregion
    #endregion

    #region Display-only Properties
    /// <summary>
    /// The amount of calendar time between the start and end.
    /// </summary>
    public TimeSpan Duration => EndDateTime.Subtract(StartDateTime);

    /// <summary>
    /// This static variable is used to create columns in both CapacityInterval and RecurringCapacityInterval Grid.
    /// If CapacityInterval.Duration has its property name changed for some reason, this string should change too. 
    /// </summary>
    public static string DurationPropertyName = "Duration"; 

    /// <summary>
    /// The number of Resources to which this Capacity Interval has been assigned.
    /// </summary>
    public int ResourcesUsing => CalendarResources.Count;
    #endregion Display-only Properties

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public override string DefaultNamePrefix => "CapacityInterval";
    #endregion

    #region Transmissions
    public void Receive(UserFieldDefinitionManager a_udfManager, CapacityIntervalIdBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is CapacityIntervalMoveInTimeT)
        {
            CapacityIntervalMoveInTimeT moveInTimeT = (CapacityIntervalMoveInTimeT)a_t;
            StartDateTime = moveInTimeT.NewStartTime;
            EndDateTime = moveInTimeT.NewEndTime;
            RegenerateCapacityProfiles(a_sd.GetPlanningHorizonEndTicks(), true);
            a_dataChanges.CapacityIntervalChanges.UpdatedObject(Id);
        }

        if (a_t is CapacityIntervalMoveT)
        {
            CapacityIntervalMoveT moveT = (CapacityIntervalMoveT)a_t;
            EndDateTime = moveT.newStartTime.Add(Duration);
            StartDateTime = moveT.newStartTime;
            RegenerateCapacityProfiles(a_sd.GetPlanningHorizonEndTicks(), true);
            a_dataChanges.CapacityIntervalChanges.UpdatedObject(Id);
        }
        else if (a_t is CapacityIntervalUpdateT)
        {
            bool updated = Update(a_udfManager, ((CapacityIntervalUpdateT)a_t).capacityInterval, out bool _, a_t, a_sd.ClockDate, a_sd.GetPlanningHorizonEnd(), a_dataChanges);

            if (updated)
            {
                RegenerateCapacityProfiles(a_sd.GetPlanningHorizonEndTicks(), true);
                a_dataChanges.CapacityIntervalChanges.UpdatedObject(Id);
            }
        }
        else if (a_t is CapacityIntervalDeleteT) { }
    }

    internal virtual void DeleteFromResources(long a_planningHorizonEndTicks)
    {
        //Remove references form all of its resources
        for (int i = CalendarResources.Count - 1; i >= 0; i--)
        {
            InternalResource r = CalendarResources[i];
            r.RemoveCapacityInterval(this);
            r.RegenerateCapacityProfile(a_planningHorizonEndTicks, true);
        }
    }

    /// <summary>
    /// Remove the reference from Resources but don't regenerate the Capacity Profile since changes are only being made in the past.
    /// </summary>
    internal virtual void PurgeFromResources()
    {
        //Remove references form all of its resources
        for (int i = CalendarResources.Count - 1; i >= 0; i--)
        {
            InternalResource r = CalendarResources[i];
            r.RemoveCapacityInterval(this);
        }
    }

    internal void RegenerateCapacityProfiles(long a_planningHorizonEndTicks, bool a_ciUpdated)
    {
        for (int i = 0; i < CalendarResources.Count; i++)
        {
            InternalResource r = CalendarResources[i];
            r.RegenerateCapacityProfile(a_planningHorizonEndTicks, a_ciUpdated);
        }
    }
    #endregion

    #region Cloning
    public CapacityInterval Clone()
    {
        return (CapacityInterval)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region Internal Resources
    /// <summary>
    /// Adds a reference to the InternalResource from the CapacityInterval.
    /// </summary>
    /// <param name="a_resource"></param>
    internal void Add(InternalResource a_resource)
    {
        CalendarResources.Add(a_resource);
    }

    //Note that this list is deserialized by InternalResource
    private readonly CalendarResourcesCollection m_capacityIntervals = new ();

    /// <summary>
    /// Stores a list of the CalendarResources thatthis CapacityInterval is assigned to.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public CalendarResourcesCollection CalendarResources => m_capacityIntervals;
    #endregion Calendar Resources

    #region Miscellaneous
    /// <summary>
    /// Whether the Interval type is NormalOnline or Offline.
    /// </summary>
    /// <returns></returns>
    public bool NormalOnlineOrOffline()
    {
        return IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Online || IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Offline;
    }

    /// <summary>
    /// Means the interval type is the type where work can be performed, such as NormalOnline, Overtime, and PotentialOvertime.
    /// </summary>
    public bool Active => m_intervalType is CapacityIntervalDefs.capacityIntervalTypes.Online;
    #endregion

    #region Transmission Creation
    public static PT.Transmissions.CapacityInterval CreateCapacityIntervalForTransmission(CapacityInterval a_ci)
    {
        PT.Transmissions.CapacityInterval ci4t = new ();
        InitCapacityIntervalForTransmission(ci4t, a_ci);
        return ci4t;
    }

    public static void InitCapacityIntervalForTransmission(PT.Transmissions.CapacityInterval a_ci4t, CapacityInterval a_ci)
    {
        a_ci4t.ExternalId = a_ci.ExternalId;
        a_ci4t.Name = a_ci.Name;
        a_ci4t.Description = a_ci.Description;
        a_ci4t.IntervalType = a_ci.IntervalType;
        a_ci4t.StartDateTime = a_ci.StartDateTime;
        a_ci4t.EndDateTime = a_ci.EndDateTime;
        a_ci4t.NbrOfPeople = a_ci.NbrOfPeople;
    }
    #endregion

    internal IntervalProfile GetIntervalProfile()
    {
        return new IntervalProfile(
            CleanOutSetups,
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
}