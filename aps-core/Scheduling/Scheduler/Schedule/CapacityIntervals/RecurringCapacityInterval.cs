using System.ComponentModel;
using System.Globalization;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Range;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Defines a CapacityInterval that has multiple instances to represent a pattern of intervals such as a repeating shift pattern or holiday
/// </summary>
public class RecurringCapacityInterval : CapacityInterval, IEnumerable<RecurringCapacityInterval.RCIExpansion>, ISearchableRangeCollection
{
    public new const int UNIQUE_ID = 337;

    #region IPTSerializable Members
    public RecurringCapacityInterval(IReader reader)
        : base(reader)
    {
        DateTime recurrenceStart = DateTime.MinValue;

        if (reader.VersionNumber >= 12010)
        {
            reader.Read(out int val);
            m_dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out m_maxNbrRecurrences);
            reader.Read(out m_monthlyDayNumber);
            reader.Read(out val);
            m_monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            m_occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            m_recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            m_recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out m_skipFrequency);
            reader.Read(out val);
            m_yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out m_monday);
            reader.Read(out m_tuesday);
            reader.Read(out m_wednesday);
            reader.Read(out m_thursday);
            reader.Read(out m_friday);
            reader.Read(out m_saturday);
            reader.Read(out m_sunday);

            reader.Read(out m_recurrenceEndDateTime);
            reader.Read(out m_nbrOfPeopleOverride); //new in 45
            reader.Read(out m_nbrIntervalsToOverride); //new in 45

            reader.Read(out int expansionsCount);
            for (int i = 0; i < expansionsCount; i++)
            {
                RCIExpansion e = new (reader);
                m_expansions.Add(e);
            }
        }
        else if (reader.VersionNumber >= 12009)
        {
            //This is the same as version 686. Must do this for backwards compatibility
            int val;
            reader.Read(out val);
            m_dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out m_maxNbrRecurrences);
            reader.Read(out m_monthlyDayNumber);
            reader.Read(out val);
            m_monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            m_occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            m_recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            m_recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out m_skipFrequency);
            reader.Read(out val);
            m_yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out m_monday);
            reader.Read(out m_tuesday);
            reader.Read(out m_wednesday);
            reader.Read(out m_thursday);
            reader.Read(out m_friday);
            reader.Read(out m_saturday);
            reader.Read(out m_sunday);

            reader.Read(out m_recurrenceEndDateTime);
            reader.Read(out recurrenceStart);
            reader.Read(out m_nbrOfPeopleOverride); //new in 45
            reader.Read(out m_nbrIntervalsToOverride); //new in 45

            reader.Read(out int expansionsCount);
            for (int i = 0; i < expansionsCount; i++)
            {
                RCIExpansion e = new (reader);
                m_expansions.Add(e);
            }
        }
        else if (reader.VersionNumber >= 746)
        {
            reader.Read(out int val);
            m_dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out m_maxNbrRecurrences);
            reader.Read(out m_monthlyDayNumber);
            reader.Read(out val);
            m_monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            m_occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            m_recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            m_recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out m_skipFrequency);
            reader.Read(out val);
            m_yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out m_monday);
            reader.Read(out m_tuesday);
            reader.Read(out m_wednesday);
            reader.Read(out m_thursday);
            reader.Read(out m_friday);
            reader.Read(out m_saturday);
            reader.Read(out m_sunday);

            reader.Read(out m_recurrenceEndDateTime);
            reader.Read(out m_nbrOfPeopleOverride); //new in 45
            reader.Read(out m_nbrIntervalsToOverride); //new in 45

            reader.Read(out int expansionsCount);
            for (int i = 0; i < expansionsCount; i++)
            {
                RCIExpansion e = new (reader);
                m_expansions.Add(e);
            }
        }
        else
        {
            if (reader.VersionNumber >= 686)
            {
                int val;
                reader.Read(out val);
                m_dayType = (CapacityIntervalDefs.dayTypes)val;
                reader.Read(out m_maxNbrRecurrences);
                reader.Read(out m_monthlyDayNumber);
                reader.Read(out val);
                m_monthlyOccurrence = (CapacityIntervalDefs.months)val;
                reader.Read(out val);
                m_occurrence = (CapacityIntervalDefs.occurrences)val;
                reader.Read(out val);
                m_recurrence = (CapacityIntervalDefs.recurrences)val;
                reader.Read(out val);
                m_recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
                reader.Read(out m_skipFrequency);
                reader.Read(out val);
                m_yearlyMonth = (CapacityIntervalDefs.months)val;
                reader.Read(out m_monday);
                reader.Read(out m_tuesday);
                reader.Read(out m_wednesday);
                reader.Read(out m_thursday);
                reader.Read(out m_friday);
                reader.Read(out m_saturday);
                reader.Read(out m_sunday);

                reader.Read(out m_recurrenceEndDateTime);
                reader.Read(out recurrenceStart);
                reader.Read(out m_nbrOfPeopleOverride); //new in 45
                reader.Read(out m_nbrIntervalsToOverride); //new in 45

                reader.Read(out int expansionsCount);
                for (int i = 0; i < expansionsCount; i++)
                {
                    RCIExpansion e = new (reader);
                    m_expansions.Add(e);
                }
            }

            if (recurrenceStart != StartDateTime)
            {
                m_regenerate = true;
            }
        }

        if (reader.VersionNumber <= 12404)
        {
            foreach (RCIExpansion expansion in m_expansions)
            {
                //Check start and end dates
                if (expansion.End < expansion.Start)
                {
                    (expansion.Start, expansion.End) = (expansion.End, expansion.Start);
                }
                else if (expansion.Start == expansion.End)
                {
                    //Just use some value so there isn't an error
                    expansion.End = expansion.Start.AddHours(1);
                }
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)DayType);
        writer.Write(m_maxNbrRecurrences);
        writer.Write(m_monthlyDayNumber);
        writer.Write((int)m_monthlyOccurrence);
        writer.Write((int)m_occurrence);
        writer.Write((int)m_recurrence);
        writer.Write((int)m_recurrenceEndType);
        writer.Write(m_skipFrequency);
        writer.Write((int)m_yearlyMonth);
        writer.Write(m_monday);
        writer.Write(m_tuesday);
        writer.Write(m_wednesday);
        writer.Write(m_thursday);
        writer.Write(m_friday);
        writer.Write(m_saturday);
        writer.Write(m_sunday);

        writer.Write(m_recurrenceEndDateTime);
        writer.Write(m_nbrOfPeopleOverride);
        writer.Write(m_nbrIntervalsToOverride);

        writer.Write(m_expansions.Count);
        for (int i = 0; i < m_expansions.Count; i++)
        {
            m_expansions[i].Serialize(writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    private Expander m_expander;
    private readonly List<RCIExpansion> m_expansions = new (); //List of all RCI Expansions

    public int ExpansionsCount => m_expansions.Count;

    public int Count => ExpansionsCount;

    public ISearchableRange GetByIdx(int a_idx)
    {
        return m_expansions[a_idx];
    }

    public int IndexOf(ISearchableRange a_searchableRange)
    {
        return m_expansions.IndexOf(a_searchableRange as RCIExpansion);
    }

    internal RCIExpansion GetExpansionAtIdx(int a_idx)
    {
        return m_expansions[a_idx];
    }

    public IEnumerator<RCIExpansion> GetEnumerator()
    {
        return m_expansions.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #region Constructors
    internal RecurringCapacityInterval(BaseId a_id, long a_clock)
        : base(a_id, a_clock)
    {
        InitDates(a_clock);
    }

    internal RecurringCapacityInterval(UserFieldDefinitionManager a_udfManager, Transmissions.RecurringCapacityInterval a_rciDef, BaseId a_id, DateTime a_clockDate, DateTime a_planningHorizonEnd, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
        : base(a_udfManager, a_rciDef, a_id, a_clockDate, a_planningHorizonEnd, a_t, a_dataChanges)
    {
        //		InitDates(clock);
        RCIUpdate(a_udfManager, a_rciDef, a_t, a_clockDate, a_planningHorizonEnd, a_dataChanges);
    }

    internal RecurringCapacityInterval(BaseId a_id, RecurringCapacityInterval a_original)
        : base(a_id, a_original)
    {
        //       InitDates(clock);

        DayType = a_original.DayType;
        m_monday = a_original.Monday;
        m_tuesday = a_original.Tuesday;
        m_wednesday = a_original.Wednesday;
        m_thursday = a_original.Thursday;
        m_friday = a_original.Friday;
        m_saturday = a_original.Saturday;
        m_sunday = a_original.Sunday;
        m_maxNbrRecurrences = a_original.MaxNbrRecurrences;
        m_monthlyDayNumber = a_original.MonthlyDayNumber;
        m_monthlyOccurrence = a_original.MonthlyOccurrence;
        m_nbrIntervalsToOverride = a_original.NbrIntervalsToOverride;
        m_nbrOfPeopleOverride = a_original.m_nbrOfPeopleOverride;
        m_occurrence = a_original.Occurrence;
        m_recurrence = a_original.Recurrence;
        m_recurrenceEndDateTime = a_original.RecurrenceEndDateTime;
        m_recurrenceEndType = a_original.RecurrenceEndType;
        m_skipFrequency = a_original.SkipFrequency;
        m_yearlyMonth = a_original.YearlyMonth;

        //Validate weekly rci have at least one day checked, otherwise enable all days
        if (m_recurrence == CapacityIntervalDefs.recurrences.Weekly)
        {
            if (!(m_monday || m_tuesday || m_wednesday || m_thursday || m_friday || m_saturday || m_sunday))
            {
                //None of the days of the week are being set.
                throw new PTValidationException("3033", new object[] { ExternalId });
            }
        }
    }

    private void InitDates(long scenarioDetailClock)
    {
        m_recurrenceEndDateTime = new DateTime(scenarioDetailClock);
        m_recurrenceEndDateTime = m_recurrenceEndDateTime.AddYears(1);
        m_recurrenceEndDateTime = m_recurrenceEndDateTime.Date;
    }

    internal void RestoreReferences(ScenarioDetail a_sd)
    {
        if (m_regenerate)
        {
            for (int i = 0; i < CalendarResources.Count; i++)
            {
                InternalResource calendar = CalendarResources[i];
                long planningHorizonEndTicks = a_sd.GetPlanningHorizonEndTicks();
                calendar.RegenerateCapacityProfile(planningHorizonEndTicks, true);

                Expand(new DateTime(planningHorizonEndTicks), a_sd.Clock);
            }
        }
    }

    internal bool RCIUpdate(UserFieldDefinitionManager a_udfManager, Transmissions.RecurringCapacityInterval a_copyFrom, PTTransmission t, DateTime a_clockDate, DateTime a_planningHorizonEnd, IScenarioDataChanges a_dataChanges)
    {
        ValidateUpdate(a_copyFrom);

        // TODO: do we need to let the base updater know this is coming from recurring, since it used/passes through CapacityIntervals? APS-7268
        bool updated = base.Update(a_udfManager, a_copyFrom, out bool needToReExpand, t, a_clockDate, a_planningHorizonEnd, a_dataChanges); //Copy base values.

        bool initialMonday = m_monday;
        bool initialTuesday = m_tuesday;
        bool initialWednesday = m_wednesday;
        bool initialThursday = m_thursday;
        bool initialFriday = m_friday;
        bool initialSaturday = m_saturday;
        bool initialSunday = m_sunday;

        if (a_copyFrom.DayTypeSet && a_copyFrom.DayType != DayType)
        {
            DayType = a_copyFrom.DayType;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.FridaySet && a_copyFrom.Friday != Friday)
        {
            Friday = a_copyFrom.Friday;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.MaxNbrRecurrencesSet && a_copyFrom.MaxNbrRecurrences != MaxNbrRecurrences)
        {
            MaxNbrRecurrences = a_copyFrom.MaxNbrRecurrences;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.MondaySet && a_copyFrom.Monday != Monday)
        {
            Monday = a_copyFrom.Monday;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.MonthlyDayNumberSet && a_copyFrom.MonthlyDayNumber != MonthlyDayNumber)
        {
            MonthlyDayNumber = a_copyFrom.MonthlyDayNumber;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.MonthlyOccurrenceSet && a_copyFrom.MonthlyOccurrence != MonthlyOccurrence)
        {
            MonthlyOccurrence = a_copyFrom.MonthlyOccurrence;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.OccurrenceSet && a_copyFrom.Occurrence != Occurrence)
        {
            Occurrence = a_copyFrom.Occurrence;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.RecurrenceSet && a_copyFrom.Recurrence != Recurrence)
        {
            Recurrence = a_copyFrom.Recurrence;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.RecurrenceEndDateTimeSet && a_copyFrom.RecurrenceEndDateTime != RecurrenceEndDateTime)
        {
            RecurrenceEndDateTime = a_copyFrom.RecurrenceEndDateTime;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.RecurrenceEndTypeSet && a_copyFrom.RecurrenceEndType != RecurrenceEndType)
        {
            RecurrenceEndType = a_copyFrom.RecurrenceEndType;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.NbrOfPeopleOverrideSet && a_copyFrom.NbrOfPeopleOverride != NbrOfPeopleOverride)
        {
            NbrOfPeopleOverride = a_copyFrom.NbrOfPeopleOverride;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.NbrIntervalsToOverrideSet && a_copyFrom.NbrIntervalsToOverride != NbrIntervalsToOverride)
        {
            NbrIntervalsToOverride = a_copyFrom.NbrIntervalsToOverride;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.SaturdaySet && a_copyFrom.Saturday != Saturday)
        {
            Saturday = a_copyFrom.Saturday;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.SkipFrequencySet && a_copyFrom.SkipFrequency != SkipFrequency)
        {
            SkipFrequency = a_copyFrom.SkipFrequency;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.SundaySet && a_copyFrom.Sunday != Sunday)
        {
            Sunday = a_copyFrom.Sunday;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.ThursdaySet && a_copyFrom.Thursday != Thursday)
        {
            Thursday = a_copyFrom.Thursday;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.TuesdaySet && a_copyFrom.Tuesday != Tuesday)
        {
            Tuesday = a_copyFrom.Tuesday;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.WednesdaySet && a_copyFrom.Wednesday != Wednesday)
        {
            Wednesday = a_copyFrom.Wednesday;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_copyFrom.YearlyMonthSet && a_copyFrom.YearlyMonth != YearlyMonth)
        {
            YearlyMonth = a_copyFrom.YearlyMonth;
            needToReExpand = true;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        //Validate weekly rci have at least one day checked, otherwise enable all days
        if (m_recurrence == CapacityIntervalDefs.recurrences.Weekly)
        {
            if (!(m_monday || m_tuesday || m_wednesday || m_thursday || m_friday || m_saturday || m_sunday))
            {
                //None of the days of the week are being set. Reset
                m_monday = initialMonday;
                m_tuesday = initialTuesday;
                m_wednesday = initialWednesday;
                m_thursday = initialThursday;
                m_friday = initialFriday;
                m_saturday = initialSaturday;
                m_sunday = initialSunday;
                a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(Id); //Track changes to refresh the UI.
                throw new PTValidationException("3033", new object[] { ExternalId });
            }
        }

        if ((EndDateTime - StartDateTime).TotalHours > 24)
        {
            // Intervals are not allowed to be more than 24h long.
            throw new CapacityIntervalException("3067", new object[] { StartDateTime, EndDateTime });
        }

        if (needToReExpand)
        {
            Expand(a_planningHorizonEnd, a_clockDate.Ticks);
        }

        if (updated)
        {
            RegenerateCapacityProfiles(a_planningHorizonEnd.Ticks, true);
            a_dataChanges.FlagConstraintChanges(Id);
            a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(Id);

        }

        return updated;
    }
    #endregion Constructors

    #region Shared Properties
    private CapacityIntervalDefs.dayTypes m_dayType = CapacityIntervalDefs.dayTypes.Day;

    /// <summary>
    /// Specifies which occurence of the day to use for MonthlyByDayType or YearlyByDayType Recurrence.   Such as 'Last Thursday of every month'.
    /// </summary>
    [Browsable(false)]
    public CapacityIntervalDefs.dayTypes DayType
    {
        get => m_dayType;
        private set => m_dayType = value;
    }

    private int m_maxNbrRecurrences = 1;

    /// <summary>
    /// No events are scheduled to start after this number of events recur.  Only used if RecurrenceEndType specifies MaxNbrRecurrences.
    /// </summary>
    [Browsable(false)]
    public int MaxNbrRecurrences
    {
        get => m_maxNbrRecurrences;
        private set => m_maxNbrRecurrences = value;
    }

    private int m_monthlyDayNumber = 1;

    /// <summary>
    /// The day of each month that the event occurs.  Applies to Monthly and Yearly Recurrences only.
    /// </summary>
    [Browsable(false)]
    public int MonthlyDayNumber
    {
        get => m_monthlyDayNumber;
        private set => m_monthlyDayNumber = value;
    }

    private CapacityIntervalDefs.months m_monthlyOccurrence = CapacityIntervalDefs.months.January;

    /// <summary>
    /// Specifies the month of a monthly occurrence.
    /// </summary>
    [Browsable(false)]
    public CapacityIntervalDefs.months MonthlyOccurrence
    {
        get => m_monthlyOccurrence;
        private set => m_monthlyOccurrence = value;
    }

    private CapacityIntervalDefs.occurrences m_occurrence = CapacityIntervalDefs.occurrences.First;

    /// <summary>
    /// Specifies which occurence of the day to use for MonthlyByDayType or YearlyByDayType Recurrence.   Such as 'Last Thursday of every month'.
    /// </summary>
    [Browsable(false)]
    public CapacityIntervalDefs.occurrences Occurrence
    {
        get => m_occurrence;
        private set => m_occurrence = value;
    }

    private CapacityIntervalDefs.recurrences m_recurrence = CapacityIntervalDefs.recurrences.Daily;

    /// <summary>
    /// Specifies if and how the event repeats.
    /// </summary>
    [Browsable(false)]
    public CapacityIntervalDefs.recurrences Recurrence
    {
        get => m_recurrence;
        internal set => m_recurrence = value;
    }

    private DateTime m_recurrenceEndDateTime;

    /// <summary>
    /// No events are scheduled to start after this DateTime.  Only used if RecurrenceEndType specifies RecurrenceEndDateTime.
    /// </summary>
    [Browsable(false)]
    public DateTime RecurrenceEndDateTime
    {
        get => m_recurrenceEndDateTime;
        internal set => m_recurrenceEndDateTime = value;
    }

    private CapacityIntervalDefs.recurrenceEndTypes m_recurrenceEndType = CapacityIntervalDefs.recurrenceEndTypes.NoEndDate;

    /// <summary>
    /// Specifies the indicator of when the recurrence stops.
    /// </summary>
    [Browsable(false)]
    public CapacityIntervalDefs.recurrenceEndTypes RecurrenceEndType
    {
        get => m_recurrenceEndType;
        internal set => m_recurrenceEndType = value;
    }

    private int m_skipFrequency;

    /// <summary>
    /// Specifies how many intervals to skip between events.  For example, if Recurrence is Daily and SkipFrequency is 2 then an event is scheduled then two days are skipped and then another event is
    /// scheduled.
    /// </summary>
    public int SkipFrequency
    {
        get => m_skipFrequency;
        private set => m_skipFrequency = value;
    }

    private CapacityIntervalDefs.months m_yearlyMonth = CapacityIntervalDefs.months.January;

    /// <summary>
    /// Specifies the month in which the yearly event occurrs.
    /// </summary>
    [Browsable(false)]
    public CapacityIntervalDefs.months YearlyMonth
    {
        get => m_yearlyMonth;
        private set => m_yearlyMonth = value;
    }

    private decimal m_nbrOfPeopleOverride = 1;

    /// <summary>
    /// Can be used to override the Nbr Of People for a specified number of intervals at the beginning of the Recurring Capacity Interval.
    /// </summary>
    public decimal NbrOfPeopleOverride
    {
        get => m_nbrOfPeopleOverride;
        private set
        {
            if (value <= 0)
            {
                throw new PTValidationException("2153");
            }

            m_nbrOfPeopleOverride = value;
        }
    }

    private int m_nbrIntervalsToOverride;

    /// <summary>
    /// The number of Intervals in the Recurring Capacity Interval to override with the Nbr Of People Override.
    /// </summary>
    public int NbrIntervalsToOverride
    {
        get => m_nbrIntervalsToOverride;
        private set => m_nbrIntervalsToOverride = value;
    }

    /// <summary>
    /// Field used for backwards compatibility to determine whether to regenerate rci if old recurrence start time doesn't
    /// match rci start date
    /// </summary>
    private readonly bool m_regenerate;

    #region Days
    private bool m_monday = true;

    /// <summary>
    /// Specifies whether to include Mondays in Weekly recurring events.
    /// </summary>
    [Browsable(false)]
    public bool Monday
    {
        get => m_monday;
        internal set => m_monday = value;
    }

    private bool m_tuesday = true;

    /// <summary>
    /// Specifies whether to include Tuesdays in Weekly recurring events.
    /// </summary>
    [Browsable(false)]
    public bool Tuesday
    {
        get => m_tuesday;
        internal set => m_tuesday = value;
    }

    private bool m_wednesday = true;

    /// <summary>
    /// Specifies whether to include Wednesdays in Weekly recurring events.
    /// </summary>
    [Browsable(false)]
    public bool Wednesday
    {
        get => m_wednesday;
        internal set => m_wednesday = value;
    }

    private bool m_thursday = true;

    /// <summary>
    /// Specifies whether to include Thursdays in Weekly recurring events.
    /// </summary>
    [Browsable(false)]
    public bool Thursday
    {
        get => m_thursday;
        internal set => m_thursday = value;
    }

    private bool m_friday = true;

    /// <summary>
    /// Specifies whether to include Fridays in Weekly recurring events.
    /// </summary>
    [Browsable(false)]
    public bool Friday
    {
        get => m_friday;
        internal set => m_friday = value;
    }

    private bool m_saturday = true;

    /// <summary>
    /// Specifies whether to include Saturdays in Weekly recurring events.
    /// </summary>
    [Browsable(false)]
    public bool Saturday
    {
        get => m_saturday;
        internal set => m_saturday = value;
    }

    private bool m_sunday = true;

    /// <summary>
    /// Specifies whether to include Sundays in Weekly recurring events.
    /// </summary>
    [Browsable(false)]
    public bool Sunday
    {
        get => m_sunday;
        internal set => m_sunday = value;
    }
    #endregion Days
    #endregion Shared Properties

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "RecurringCapacityInterval";
    #endregion

    #region Transmissions
    public void Receive(UserFieldDefinitionManager a_udfManager, RecurringCapacityIntervalIdBaseT t, DateTime a_planningHorizonEnd, DateTime a_clockDate, IScenarioDataChanges a_dataChanges)
    {
        if (t is RecurringCapacityIntervalUpdateT rciUpdatedT)
        {
            var updated = RCIUpdate(a_udfManager, rciUpdatedT.recurringCapacityInterval, t, a_clockDate, a_planningHorizonEnd, a_dataChanges);
        }

        if (t is RecurringCapacityIntervalMoveInTimeT)
        {
            RecurringCapacityIntervalMoveInTimeT moveInTimeT = (RecurringCapacityIntervalMoveInTimeT)t;
            TimeSpan startChange = moveInTimeT.oldStartTime.Subtract(moveInTimeT.newStartTime);
            TimeSpan endChange = moveInTimeT.oldEndTime.Subtract(moveInTimeT.newEndTime);
            StartDateTime = StartDateTime.Subtract(startChange);
            EndDateTime = EndDateTime.Subtract(endChange);
            Expand(a_planningHorizonEnd, a_clockDate.Ticks);
            RegenerateCapacityProfiles(a_planningHorizonEnd.Ticks, true);
            a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(Id);
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (t is RecurringCapacityIntervalMoveT)
        {
            RecurringCapacityIntervalMoveT moveT = (RecurringCapacityIntervalMoveT)t;
            EndDateTime = moveT.newStartTime.Add(Duration);
            StartDateTime = moveT.newStartTime;
            Expand(a_planningHorizonEnd, a_clockDate.Ticks);
            RegenerateCapacityProfiles(a_planningHorizonEnd.Ticks, true);
            a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(Id);
            a_dataChanges.FlagConstraintChanges(Id);
        }
        else if (t is RecurringCapacityIntervalDeleteT) { }
    }

    internal override void DeleteFromResources(long a_planningHorizonEndTicks)
    {
        //Remove references form all of its resources
        for (int i = CalendarResources.Count - 1; i >= 0; i--)
        {
            InternalResource r = CalendarResources[i];
            r.RemoveRecurringCapacityInterval(this);
            r.RegenerateCapacityProfile(a_planningHorizonEndTicks, true);
        }
    }

    /// <summary>
    /// Remove the reference from Resources but don't regenerate the Capacity Profile since changes are only being made in the past.
    /// </summary>
    internal override void PurgeFromResources()
    {
        //Remove references form all of its resources
        for (int i = CalendarResources.Count - 1; i >= 0; i--)
        {
            InternalResource r = CalendarResources[i];
            r.RemoveRecurringCapacityInterval(this);
        }
    }
    #endregion

    #region RCI Expansion
    /// <summary>
    /// Creates an Expander based on the Intervals current Recurrence Type
    /// </summary>
    private void SetExpander(DateTime planningHorizonEnd)
    {
        if (Recurrence == CapacityIntervalDefs.recurrences.Daily)
        {
            m_expander = new DailyExpander(this, planningHorizonEnd);
        }
        else if (Recurrence == CapacityIntervalDefs.recurrences.Weekly)
        {
            m_expander = new WeeklyExpander(this, planningHorizonEnd);
        }
        else if (Recurrence == CapacityIntervalDefs.recurrences.MonthlyByDayNumber)
        {
            m_expander = new MonthlyByDayExpander(this, planningHorizonEnd);
        }
        else if (Recurrence == CapacityIntervalDefs.recurrences.YearlyByMonthDay)
        {
            m_expander = new YearlyByDayExpander(this, planningHorizonEnd);
        }
        else
        {
            m_expander = null;
        }
    }

    /// <summary>
    /// Regenerates the RCI Expansions.
    /// </summary>
    internal void Expand(DateTime planningHorizonEnd, long clock)
    {
        if (RecurrenceEndType == CapacityIntervalDefs.recurrenceEndTypes.AfterRecurrenceEndDateTime && RecurrenceEndDateTime.Ticks < clock)
        {
            return;
        }

        m_expansions.Clear();
        SetExpander(planningHorizonEnd); //Make sure the expander is of the right type based on the interval's current settings
        if (m_expander != null)
        {
            m_expander.Expand(clock, StartDateTime);
        }

        //
        //			this.expansions.Add(new RCIExpansion(this.StartDateTime,this.EndDateTime));
        //			this.expansions.Add(new RCIExpansion(this.StartDateTime.AddDays(1),this.EndDateTime.AddDays(1)));
        //			this.expansions.Add(new RCIExpansion(this.StartDateTime.AddDays(2),this.EndDateTime.AddDays(2)));
    }

    /// <summary>
    /// If the RecurringCapacityInterval is using an expander that is date dependent then need
    /// to re-expand using the new Clock.
    /// </summary>
    /// <param name="a_planningHorizonEnd"></param>
    internal void ResetPlanningHorizon(DateTime a_planningHorizonEnd, long a_clock)
    {
        SetExpander(a_planningHorizonEnd);
        Expand(a_planningHorizonEnd, a_clock);
    }

    #region Find by time
    /// <summary>
    /// Returns the Expansion that starts at or after the point or null if no such Expansion exists.
    /// Check first to make sure the point is in the expansions range before searching.
    /// </summary>
    /// <param name="pt"></param>
    /// <returns>The Expansion if found else null.</returns>
    internal RCIExpansion FindExpansionAtOrAfterPoint(DateTime pt)
    {
        for (int i = 0; i < m_expansions.Count; i++)
        {
            RCIExpansion exp = GetExpansionAtIdx(i);
            if (exp.Start.Ticks >= pt.Ticks)
            {
                return exp;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the Expansion that start before and ends after the point.
    /// Check first to make sure the point is in the expansions range before searching.
    /// </summary>
    /// <param name="pt"></param>
    /// <returns>The Expansion if found else null.</returns>
    internal RCIExpansion FindExpansionCoveringPoint(DateTime pt)
    {
        for (int i = 0; i < m_expansions.Count; i++)
        {
            RCIExpansion exp = GetExpansionAtIdx(i);
            if (exp.Start.Ticks <= pt.Ticks && exp.End.Ticks > pt.Ticks) //if two expansions are up against each other expansion n's end =n+1's start.  A block in n+1 starts at that instant and we want to consider it to start in n+1.
            {
                return exp;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the end intervals contain the point.  If not
    /// </summary>
    /// <param name="idxStart"></param>
    /// <param name="idxEnd"></param>
    /// <param name="pt"></param>
    /// <returns></returns>
    private RCIExpansion CheckPointInRange(ref int idxStart, ref int idxEnd, DateTime pt)
    {
        RCIExpansion exFirst = GetExpansionAtIdx(idxStart);
        if (exFirst.ContainsPoint(pt))
        {
            idxStart = 0;
            idxEnd = 0;
            return exFirst;
        }

        RCIExpansion exLast = GetExpansionAtIdx(idxEnd);
        if (exLast.ContainsPoint(pt))
        {
            idxStart = 0;
            idxEnd = 0;
            return exLast;
        }

        idxStart = idxStart + 1;
        idxEnd = idxEnd - 1;
        //find out whether the point is in the first or second half of the list remaining and reset the search ends accordingly
        int midIdx = idxStart + (idxEnd - idxStart) / 2;
        RCIExpansion midEx = GetExpansionAtIdx(midIdx);
        if (pt.Ticks < midEx.End.Ticks)
        {
            idxEnd = midIdx;
        }
        else
        {
            idxStart = midIdx;
        }

        return null;
    }
    #endregion Find by time

    public class RCIExpansion : IPTSerializable, ISearchableRange
    {
        public const int UNIQUE_ID = 338;

        public RCIExpansion(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out start);
                reader.Read(out end);
            }
        }

        public void Serialize(IWriter writer)
        {
            #if DEBUG
            writer.DuplicateErrorCheck(this);
            #endif

            writer.Write(Start);
            writer.Write(End);
        }

        public int UniqueId => UNIQUE_ID;

        private DateTime start;

        public DateTime Start
        {
            get => start;
            set => start = value;
        }

        private DateTime end;

        public DateTime End
        {
            get => end;
            set => end = value;
        }

        public RCIExpansion(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public TimeSpan GetDuration()
        {
            return End.Subtract(Start);
        }

        public bool ContainsPoint(DateTime dt)
        {
            return dt.Ticks >= Start.Ticks && dt.Ticks <= End.Ticks;
        }
    }

    /// <summary>
    /// Creates RCIExpansions for the Recurring Capacity Interval
    /// </summary>
    private abstract class Expander
    {
        protected readonly RecurringCapacityInterval m_rci;
        protected RCIExpansion m_nextExpansion;
        private int m_nbrRecurrences;
        private readonly ExpansionTerminator m_terminator;

        internal RCIExpansion NextExpansion => m_nextExpansion;

        internal RecurringCapacityInterval RCI => m_rci;

        internal int NbrOfRecurrences => m_nbrRecurrences;

        public Expander(RecurringCapacityInterval a_rci, DateTime a_planningHorizonEnd)
        {
            m_rci = a_rci;
            if (a_rci.RecurrenceEndType == CapacityIntervalDefs.recurrenceEndTypes.AfterRecurrenceEndDateTime)
            {
                m_terminator = new EndDateExpansionTerminator(this, a_planningHorizonEnd);
            }
            else if (a_rci.RecurrenceEndType == CapacityIntervalDefs.recurrenceEndTypes.AfterMaxNbrRecurrences)
            {
                m_terminator = new MaxRecurrencesExpansionTerminator(this, a_planningHorizonEnd);
            }
            else
            {
                m_terminator = new PlanningHorizonExpansionTerminator(this, a_planningHorizonEnd);
            }
        }

        /// <summary>
        /// Create expansions, starting at the Clock time.
        /// </summary>
        /// <param name="clock"></param>
        internal void Expand(long a_clock, DateTime a_recurrenceStart)
        {
            m_nbrRecurrences = 0;

            //Update and get first expansion
            m_nextExpansion = JumpStartDateToNextExpansion(a_clock, a_recurrenceStart);

            if (m_nextExpansion != null)
            {
                do
                {
                    AddNextExpansion(m_nextExpansion, a_recurrenceStart);
                    m_nextExpansion = GetNextExpansion(m_nextExpansion);
                } while (!m_terminator.DoneExpanding());
            }
        }

        /// <summary>
        /// Change the recurrence start date to the next expansion.
        /// This will help reduce expansion calculations for expansions in the past.
        /// Returns first expansion
        /// </summary>
        private RCIExpansion JumpStartDateToNextExpansion(long a_clock, DateTime a_recurrenceStart)
        {
            //Immediately jump to the first time after the clock instead of expanding from an old date. This avoids some timezone daylight savings issues
            DateTime startDate = m_rci.StartDateTime;
            DateTime endDate;
            TimeSpan expansionSpan = m_rci.EndDateTime - m_rci.StartDateTime;

            endDate = startDate + expansionSpan;

            //Manually create the first expansion.
            m_nextExpansion = GetFirstExpansion(startDate, endDate);

            //Don't add expansions that end before the clock             
            while ((m_nextExpansion.End.Ticks < a_clock || m_nextExpansion.Start.Ticks < a_recurrenceStart.Ticks) && !m_terminator.DoneExpanding())
            {
                m_nbrRecurrences++;
                m_nextExpansion = GetNextExpansion(m_nextExpansion);
            }

            //Set the start time for the RCI to the start of the first valid expansion so that next time we don't have to generate up to the clock.
            //m_rci.StartDateTime = m_nextExpansion.Start;
            //m_rci.EndDateTime = m_nextExpansion.End;

            return m_nextExpansion;
        }

        private void AddNextExpansion(RCIExpansion expansion, DateTime recurrenceStart)
        {
            if (expansion.End >= recurrenceStart)
            {
                m_rci.m_expansions.Add(expansion);
                m_nbrRecurrences++;
            }
        }

        protected abstract RCIExpansion GetNextExpansion(RCIExpansion lastExpansion);
        protected abstract RCIExpansion GetFirstExpansion(DateTime start, DateTime end);

        /// <summary>
        /// Sets the Planning Horizon for PlanningHorizonExpansionTerminator which actually end at the planning horizon.
        /// </summary>
        /// <param name="planningHorizonEnd"></param>
        internal void ResetPlanningHorizon(DateTime planningHorizonEnd)
        {
            if (m_terminator is PlanningHorizonExpansionTerminator)
            {
                ((PlanningHorizonExpansionTerminator)m_terminator).PlanningHorizonEnd = planningHorizonEnd;
            }
        }
    }

    #region Expanders
    /// <summary>
    /// Creates RCIExpansions for a Daily Recurring Capacity Interval
    /// </summary>
    private class DailyExpander : Expander
    {
        public DailyExpander(RecurringCapacityInterval rci, DateTime planningHorizonEnd)
            : base(rci, planningHorizonEnd) { }

        protected override RCIExpansion GetFirstExpansion(DateTime start, DateTime end)
        {
            return new RCIExpansion(start, end);
        }

        /// <summary>
        /// Creates an RCIExpansion that has the same duration as the lastExpansion and starts at the same time
        /// of day after skipping the specified SkipFrequency.
        /// </summary>
        protected override RCIExpansion GetNextExpansion(RCIExpansion lastExpansion)
        {
            TimeSpan duration = lastExpansion.GetDuration();

            DateTime start = lastExpansion.Start.AddDays(m_rci.SkipFrequency + 1);
            DateTime end = start.Add(duration);

            return new RCIExpansion(start, end);
        }
    }

    /// <summary>
    /// Creates RCIExpansions for a Weekly Recurring Capacity Interval
    /// </summary>
    private class WeeklyExpander : Expander
    {
        public WeeklyExpander(RecurringCapacityInterval rci, DateTime planningHorizonEnd)
            : base(rci, planningHorizonEnd) { }

        public class WeeklyExpanderException : ApplicationException
        {
            public WeeklyExpanderException(string message)
                : base(message) { }
        }

        protected override RCIExpansion GetFirstExpansion(DateTime start, DateTime end)
        {
            //Find the first date after the start which has its Display Time with an included day of the week.
            //Iterate through the next seven days till a day is found for the first expansion.  At least one day of the week should always be turned on.
            for (int i = 0; i < 7; i++)
            {
                if (start.DayOfWeek == DayOfWeek.Monday && m_rci.Monday)
                {
                    break;
                }

                if (start.DayOfWeek == DayOfWeek.Tuesday && m_rci.Tuesday)
                {
                    break;
                }

                if (start.DayOfWeek == DayOfWeek.Wednesday && m_rci.Wednesday)
                {
                    break;
                }

                if (start.DayOfWeek == DayOfWeek.Thursday && m_rci.Thursday)
                {
                    break;
                }

                if (start.DayOfWeek == DayOfWeek.Friday && m_rci.Friday)
                {
                    break;
                }

                if (start.DayOfWeek == DayOfWeek.Saturday && m_rci.Saturday)
                {
                    break;
                }

                if (start.DayOfWeek == DayOfWeek.Sunday && m_rci.Sunday)
                {
                    break;
                }

                start = start.AddDays(1);
                end = end.AddDays(1);
            }

            return new RCIExpansion(start, end);
        }

        /// <summary>
        /// Creates an RCIExpansion that has the same start and end time of day as the last Expansion,
        /// occurs during one of the RCI's specified days of the week and respects the week skipping.
        /// Weeks are always considered to be from Monday thru Sunday.
        /// </summary>
        protected override RCIExpansion GetNextExpansion(RCIExpansion lastExpansion)
        {
            RCIExpansion nextExpansion = GetNextExpansion(lastExpansion.Start, lastExpansion.End);
            #if DEBUG
            if (nextExpansion.Start == lastExpansion.Start)
            {
                throw new DebugException("GetNextExpansion failed to return a future expansion");
            }
            #endif
            return nextExpansion;
        }

        private RCIExpansion GetNextExpansion(DateTime lastExpansionStart, DateTime lastExpansionEnd)
        {
            DateTime lastExpansionStartDisplayTime = lastExpansionStart;
            TimeSpan lastExpansionDuration = lastExpansionEnd.Subtract(lastExpansionStart);

            int offset = GetNextCheckedDayThisWeekOffset(lastExpansionStartDisplayTime.DayOfWeek);
            if (offset != -1) //More valid days this week
            {
                return new RCIExpansion(lastExpansionStart.AddDays(offset), lastExpansionEnd.AddDays(offset));
            }

            //Skip the required number of weeks and get the first valid interval in that week
            DateTime nextStartMonday = GetNextValidWeekStart(lastExpansionStartDisplayTime);
            offset = GetFirstCheckedDayOffset();
            return new RCIExpansion(nextStartMonday.AddDays(offset), nextStartMonday.AddDays(offset).Add(lastExpansionDuration));
        }

        /// <summary>
        /// Returns an integer specifying the number of days after Monday on which there is a valid interval.
        /// Returns -1 if there are no more days for intervals.
        /// </summary>
        /// <returns></returns>
        private int GetFirstCheckedDayOffset()
        {
            if (m_rci.Monday)
            {
                return 0;
            }

            if (m_rci.Tuesday)
            {
                return 1;
            }

            if (m_rci.Wednesday)
            {
                return 2;
            }

            if (m_rci.Thursday)
            {
                return 3;
            }

            if (m_rci.Friday)
            {
                return 4;
            }

            if (m_rci.Saturday)
            {
                return 5;
            }

            if (m_rci.Sunday)
            {
                return 6;
            }

            return -1;
        }

        /// <summary>
        /// Returns an integer specifying the number of days till the next day that is in the same week as the lastExpansion and is specified as being a valid interval day.
        /// Returns -1 if there are no more valid days in this week.
        /// </summary>
        /// <returns></returns>
        private int GetNextCheckedDayThisWeekOffset(DayOfWeek dow)
        {
            if (dow == DayOfWeek.Monday)
            {
                if (m_rci.Tuesday)
                {
                    return 1;
                }

                if (m_rci.Wednesday)
                {
                    return 2;
                }

                if (m_rci.Thursday)
                {
                    return 3;
                }

                if (m_rci.Friday)
                {
                    return 4;
                }

                if (m_rci.Saturday)
                {
                    return 5;
                }

                if (m_rci.Sunday)
                {
                    return 6;
                }
            }
            else if (dow == DayOfWeek.Tuesday)
            {
                if (m_rci.Wednesday)
                {
                    return 1;
                }

                if (m_rci.Thursday)
                {
                    return 2;
                }

                if (m_rci.Friday)
                {
                    return 3;
                }

                if (m_rci.Saturday)
                {
                    return 4;
                }

                if (m_rci.Sunday)
                {
                    return 5;
                }
            }
            else if (dow == DayOfWeek.Wednesday)
            {
                if (m_rci.Thursday)
                {
                    return 1;
                }

                if (m_rci.Friday)
                {
                    return 2;
                }

                if (m_rci.Saturday)
                {
                    return 3;
                }

                if (m_rci.Sunday)
                {
                    return 4;
                }
            }
            else if (dow == DayOfWeek.Thursday)
            {
                if (m_rci.Friday)
                {
                    return 1;
                }

                if (m_rci.Saturday)
                {
                    return 2;
                }

                if (m_rci.Sunday)
                {
                    return 3;
                }
            }
            else if (dow == DayOfWeek.Friday)
            {
                if (m_rci.Saturday)
                {
                    return 1;
                }

                if (m_rci.Sunday)
                {
                    return 2;
                }
            }
            else if (dow == DayOfWeek.Saturday)
            {
                if (m_rci.Sunday)
                {
                    return 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the date of the Monday of the next week where an interval can be created based on where the last one was created and the skip frequency of the m_rci.
        /// </summary>
        private DateTime GetNextValidWeekStart(DateTime lastStart)
        {
            DayOfWeek dow = lastStart.DayOfWeek;

            //Find the date for next monday
            DateTime nextMonday;
            if (dow == DayOfWeek.Monday)
            {
                nextMonday = lastStart.AddDays(7);
            }
            else if (dow == DayOfWeek.Tuesday)
            {
                nextMonday = lastStart.AddDays(6);
            }
            else if (dow == DayOfWeek.Wednesday)
            {
                nextMonday = lastStart.AddDays(5);
            }
            else if (dow == DayOfWeek.Thursday)
            {
                nextMonday = lastStart.AddDays(4);
            }
            else if (dow == DayOfWeek.Friday)
            {
                nextMonday = lastStart.AddDays(3);
            }
            else if (dow == DayOfWeek.Saturday)
            {
                nextMonday = lastStart.AddDays(2);
            }
            else
            {
                nextMonday = lastStart.AddDays(1);
            }

            return nextMonday.AddDays(m_rci.SkipFrequency * 7);
        }
    }

    /// <summary>
    /// Creates RCIExpansions for a Monthly by Day Recurring Capacity Interval
    /// </summary>
    private class MonthlyByDayExpander : Expander
    {
        public MonthlyByDayExpander(RecurringCapacityInterval rci, DateTime planningHorizonEnd)
            : base(rci, planningHorizonEnd) { }

        protected override RCIExpansion GetFirstExpansion(DateTime start, DateTime end)
        {
            return new RCIExpansion(start, end);
        }

        /// <summary>
        /// Creates an RCIExpansion that has the same duration as the lastExpansion and starts at the same time
        /// of day on the same day of the month -- after skipping the specified SkipFrequency.
        /// </summary>
        protected override RCIExpansion GetNextExpansion(RCIExpansion lastExpansion)
        {
            DateTime start = lastExpansion.Start.AddMonths(m_rci.SkipFrequency + 1);

            return new RCIExpansion(start, start.Add(lastExpansion.GetDuration()));
        }
    }

    /// <summary>
    /// Creates RCIExpansions for a Yearly by Day Recurring Capacity Interval
    /// </summary>
    private class YearlyByDayExpander : Expander
    {
        public YearlyByDayExpander(RecurringCapacityInterval rci, DateTime planningHorizonEnd)
            : base(rci, planningHorizonEnd) { }

        protected override RCIExpansion GetFirstExpansion(DateTime start, DateTime end)
        {
            return new RCIExpansion(start, end);
        }

        /// <summary>
        /// Creates an RCIExpansion that has the same duration as the lastExpansion and starts at the same time
        /// of day on the same day of the year.
        /// </summary>
        protected override RCIExpansion GetNextExpansion(RCIExpansion lastExpansion)
        {
            DateTime start = lastExpansion.Start.AddYears(1);

            return new RCIExpansion(start, start.Add(lastExpansion.GetDuration()));
        }
    }
    #endregion Expanders

    #region Terminators
    /// <summary>
    /// Keeps track of when the expansion should stop.
    /// </summary>
    private abstract class ExpansionTerminator
    {
        protected readonly Expander m_expander;

        public ExpansionTerminator(Expander expander)
        {
            m_expander = expander;
        }

        internal abstract bool DoneExpanding();
    }

    /// <summary>
    /// Keeps track of when the expansion should stop when no end date is specified.
    /// </summary>
    private class PlanningHorizonExpansionTerminator : ExpansionTerminator
    {
        internal DateTime PlanningHorizonEnd;

        public PlanningHorizonExpansionTerminator(Expander expander, DateTime planningHorizonEnd)
            : base(expander)
        {
            PlanningHorizonEnd = planningHorizonEnd;
        }

        internal override bool DoneExpanding()
        {
            return m_expander.NextExpansion.Start > PlanningHorizonEnd;
        }
    }

    /// <summary>
    /// Keeps track of when the expansion should stop when an end date is specified.
    /// </summary>
    private class EndDateExpansionTerminator : PlanningHorizonExpansionTerminator
    {
        public EndDateExpansionTerminator(Expander expander, DateTime planningHorizonEnd)
            : base(expander, planningHorizonEnd) { }

        internal override bool DoneExpanding()
        {
            return m_expander.NextExpansion.Start > m_expander.RCI.RecurrenceEndDateTime || base.DoneExpanding();
        }
    }

    /// <summary>
    /// Keeps track of when the expansion should stop when a max number of recurrences is specified.
    /// </summary>
    private class MaxRecurrencesExpansionTerminator : PlanningHorizonExpansionTerminator
    {
        public MaxRecurrencesExpansionTerminator(Expander expander, DateTime planningHorizonEnd)
            : base(expander, planningHorizonEnd) { }

        internal override bool DoneExpanding()
        {
            return m_expander.NbrOfRecurrences >= m_expander.RCI.MaxNbrRecurrences || base.DoneExpanding();
        }
    }
    #endregion Terminators
    #endregion RCI Expansions

    /// <summary>
    /// Remove the Expansions ending before the Clock from the Resources using this RCI.
    /// </summary>
    internal virtual void PurgeOldExpansionsFromResources(long a_newClockTime)
    {
        //Remove the old expansions from the past.  These are not purged by the Expander unless a re-Expansion is done.
        for (int i = m_expansions.Count - 1; i >= 0; i--)
        {
            RCIExpansion rciExpansion = m_expansions[i];
            if (rciExpansion.End.Ticks <= a_newClockTime)
            {
                m_expansions.Remove(rciExpansion);
            }
        }

        //Remove old references from all of its resources
        for (int i = CalendarResources.Count - 1; i >= 0; i--)
        {
            InternalResource r = CalendarResources[i];
            r.PurgeResourceCapacityIntervalsEndingBeforeClock(a_newClockTime);
        }
    }

    #region Transmission Creation
    public static Transmissions.RecurringCapacityInterval CreateRecurringCapacityIntervalForTransmission(RecurringCapacityInterval rci)
    {
        Transmissions.RecurringCapacityInterval rci4T = new ();
        InitCapacityIntervalForTransmission(rci4T, rci);

        rci4T.Recurrence = rci.Recurrence;
        rci4T.SkipFrequency = rci.SkipFrequency;
        rci4T.Monday = rci.Monday;
        rci4T.Tuesday = rci.Tuesday;
        rci4T.Wednesday = rci.Wednesday;
        rci4T.Thursday = rci.Thursday;
        rci4T.Friday = rci.Friday;
        rci4T.Saturday = rci.Saturday;
        rci4T.Sunday = rci.Sunday;
        rci4T.Occurrence = rci.Occurrence;
        rci4T.DayType = rci.DayType;
        rci4T.RecurrenceEndType = rci.RecurrenceEndType;
        rci4T.RecurrenceEndDateTime = rci.RecurrenceEndDateTime;
        rci4T.MaxNbrRecurrences = rci.MaxNbrRecurrences;
        rci4T.NbrOfPeopleOverride = rci.NbrOfPeopleOverride;
        rci4T.NbrIntervalsToOverride = rci.NbrIntervalsToOverride;

        return rci4T;
    }
    #endregion

    IEnumerator<ISearchableRange> IEnumerable<ISearchableRange>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return ExternalId + "; " + Name + "; " + Id;
    }
}