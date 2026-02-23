using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// For creating a RecurringCapacityInterval via ERP transmission.
/// </summary>
[Serializable]
public class RecurringCapacityInterval : CapacityInterval, IPTSerializable
{
    public new const int UNIQUE_ID = 98;

    #region IPTSerializable Members
    public RecurringCapacityInterval(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12501)
        {
            bools = new BoolVector32(reader);

            reader.Read(out int val);
            dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out maxNbrRecurrences);
            reader.Read(out monthlyDayNumber);
            reader.Read(out val);
            monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out skipFrequency);
            reader.Read(out val);
            yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out monday);
            reader.Read(out tuesday);
            reader.Read(out wednesday);
            reader.Read(out thursday);
            reader.Read(out friday);
            reader.Read(out saturday);
            reader.Read(out sunday);

            reader.Read(out recurrenceEndDateTime);
            reader.Read(out nbrOfPeopleOverride);
            reader.Read(out nbrIntervalsToOverride);
            reader.Read(out m_originalStartDateTime);
            reader.Read(out m_originalEndDateTime);
        }
        else if (reader.VersionNumber >= 12500)
        {
            bools = new BoolVector32(reader);

            reader.Read(out int val);
            dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out maxNbrRecurrences);
            reader.Read(out monthlyDayNumber);
            reader.Read(out val);
            monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out skipFrequency);
            reader.Read(out val);
            yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out monday);
            reader.Read(out tuesday);
            reader.Read(out wednesday);
            reader.Read(out thursday);
            reader.Read(out friday);
            reader.Read(out saturday);
            reader.Read(out sunday);

            reader.Read(out recurrenceEndDateTime);
            reader.Read(out nbrOfPeopleOverride);
            reader.Read(out nbrIntervalsToOverride);
        }
        else if (reader.VersionNumber >= 12416)
        {
            bools = new BoolVector32(reader);

            reader.Read(out int val);
            dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out maxNbrRecurrences);
            reader.Read(out monthlyDayNumber);
            reader.Read(out val);
            monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out skipFrequency);
            reader.Read(out val);
            yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out monday);
            reader.Read(out tuesday);
            reader.Read(out wednesday);
            reader.Read(out thursday);
            reader.Read(out friday);
            reader.Read(out saturday);
            reader.Read(out sunday);

            reader.Read(out recurrenceEndDateTime);
            reader.Read(out nbrOfPeopleOverride);
            reader.Read(out nbrIntervalsToOverride);
            reader.Read(out m_originalStartDateTime);
            reader.Read(out m_originalEndDateTime);
        }
        else if (reader.VersionNumber >= 12010)
        {
            bools = new BoolVector32(reader);

            reader.Read(out int val);
            dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out maxNbrRecurrences);
            reader.Read(out monthlyDayNumber);
            reader.Read(out val);
            monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out skipFrequency);
            reader.Read(out val);
            yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out monday);
            reader.Read(out tuesday);
            reader.Read(out wednesday);
            reader.Read(out thursday);
            reader.Read(out friday);
            reader.Read(out saturday);
            reader.Read(out sunday);

            reader.Read(out recurrenceEndDateTime);
            reader.Read(out nbrOfPeopleOverride);
            reader.Read(out nbrIntervalsToOverride);
        }
        else if (reader.VersionNumber >= 12009)
        {
            //This is the same as 96. Must do this for backwards compatibility
            int val;
            reader.Read(out val);
            dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out maxNbrRecurrences);
            reader.Read(out monthlyDayNumber);
            reader.Read(out val);
            monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out skipFrequency);
            reader.Read(out val);
            yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out monday);
            reader.Read(out tuesday);
            reader.Read(out wednesday);
            reader.Read(out thursday);
            reader.Read(out friday);
            reader.Read(out saturday);
            reader.Read(out sunday);

            reader.Read(out recurrenceEndDateTime);
            reader.Read(out DateTime recurrenceStart); //Obsolete
            reader.Read(out nbrOfPeopleOverride);
            reader.Read(out nbrIntervalsToOverride);

            bools = new BoolVector32(reader);
        }
        else if (reader.VersionNumber >= 746)
        {
            bools = new BoolVector32(reader);

            reader.Read(out int val);
            dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out maxNbrRecurrences);
            reader.Read(out monthlyDayNumber);
            reader.Read(out val);
            monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out skipFrequency);
            reader.Read(out val);
            yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out monday);
            reader.Read(out tuesday);
            reader.Read(out wednesday);
            reader.Read(out thursday);
            reader.Read(out friday);
            reader.Read(out saturday);
            reader.Read(out sunday);

            reader.Read(out recurrenceEndDateTime);
            reader.Read(out nbrOfPeopleOverride);
            reader.Read(out nbrIntervalsToOverride);
        }
        else if (reader.VersionNumber >= 96)
        {
            int val;
            reader.Read(out val);
            dayType = (CapacityIntervalDefs.dayTypes)val;
            reader.Read(out maxNbrRecurrences);
            reader.Read(out monthlyDayNumber);
            reader.Read(out val);
            monthlyOccurrence = (CapacityIntervalDefs.months)val;
            reader.Read(out val);
            occurrence = (CapacityIntervalDefs.occurrences)val;
            reader.Read(out val);
            recurrence = (CapacityIntervalDefs.recurrences)val;
            reader.Read(out val);
            recurrenceEndType = (CapacityIntervalDefs.recurrenceEndTypes)val;
            reader.Read(out skipFrequency);
            reader.Read(out val);
            yearlyMonth = (CapacityIntervalDefs.months)val;
            reader.Read(out monday);
            reader.Read(out tuesday);
            reader.Read(out wednesday);
            reader.Read(out thursday);
            reader.Read(out friday);
            reader.Read(out saturday);
            reader.Read(out sunday);

            reader.Read(out recurrenceEndDateTime);
            reader.Read(out DateTime recurrenceStart); //Obsolete
            reader.Read(out nbrOfPeopleOverride);
            reader.Read(out nbrIntervalsToOverride);

            bools = new BoolVector32(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        bools.Serialize(writer);

        writer.Write((int)dayType);
        writer.Write(maxNbrRecurrences);
        writer.Write(monthlyDayNumber);
        writer.Write((int)monthlyOccurrence);
        writer.Write((int)occurrence);
        writer.Write((int)recurrence);
        writer.Write((int)recurrenceEndType);
        writer.Write(skipFrequency);
        writer.Write((int)yearlyMonth);
        writer.Write(monday);
        writer.Write(tuesday);
        writer.Write(wednesday);
        writer.Write(thursday);
        writer.Write(friday);
        writer.Write(saturday);
        writer.Write(sunday);

        writer.Write(recurrenceEndDateTime);
        writer.Write(nbrOfPeopleOverride);
        writer.Write(nbrIntervalsToOverride);
        writer.Write(m_originalStartDateTime);
        writer.Write(m_originalEndDateTime);
    }

    public override int UniqueId => UNIQUE_ID;

    private BoolVector32 bools;
    private const int DayTypeSetIdx = 0;
    private const int MaxNbrRecurrencesSetIdx = 1;
    private const int MonthlyDayNumberSetIdx = 2;
    private const int MonthlyOccurrenceSetIdx = 3;
    private const int OccurrenceSetIdx = 4;
    private const int RecurrenceSetIdx = 5;
    private const int RecurrenceEndDateTimeSetIdx = 6;

    private const int RecurrenceEndTypeSetIdx = 7;

    //const int RecurrenceStartSetIdx = 8; Unused
    private const int SkipFrequencySetIdx = 9;
    private const int YearlyMonthSetIdx = 10;
    private const int NbrOfPeopleOverrideSetIdx = 11;
    private const int NbrIntervalsToOverrideSetIdx = 12;
    private const int MondaySetIdx = 13;
    private const int TuesdaySetIdx = 14;
    private const int WednesdaySetIdx = 15;
    private const int ThursdaySetIdx = 16;
    private const int FridaySetIdx = 17;
    private const int SaturdaySetIdx = 18;
    private const int SundaySetIdx = 19;
    #endregion

    public RecurringCapacityInterval(string externalId, string name)
        : base(externalId, name) { }

    public RecurringCapacityInterval()
        : base("", "") { }

    #region Shared Properties
    private CapacityIntervalDefs.dayTypes dayType = CapacityIntervalDefs.dayTypes.Day;

    /// <summary>
    /// Specifies which occurence of the day to use for MonthlyByDayType or YearlyByDayType Recurrence.   Such as 'Last Thursday of every month'.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public CapacityIntervalDefs.dayTypes DayType
    {
        get => dayType;
        set
        {
            dayType = value;
            bools[DayTypeSetIdx] = true;
        }
    }

    public bool DayTypeSet => bools[DayTypeSetIdx];

    private int maxNbrRecurrences = 1;

    /// <summary>
    /// No events are scheduled to start after this number of events recur.  Only used if RecurrenceEndType specifies MaxNbrRecurrences.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public int MaxNbrRecurrences
    {
        get => maxNbrRecurrences;
        set
        {
            maxNbrRecurrences = value;
            bools[MaxNbrRecurrencesSetIdx] = value != 0;
        }
    }

    public bool MaxNbrRecurrencesSet => bools[MaxNbrRecurrencesSetIdx];

    private int monthlyDayNumber = 1;

    /// <summary>
    /// The day of each month that the event occurs.  Applies to Monthly and Yearly Recurrences only.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public int MonthlyDayNumber
    {
        get => monthlyDayNumber;
        set
        {
            monthlyDayNumber = value;
            bools[MonthlyDayNumberSetIdx] = true;
        }
    }

    public bool MonthlyDayNumberSet => bools[MonthlyDayNumberSetIdx];

    private CapacityIntervalDefs.months monthlyOccurrence = CapacityIntervalDefs.months.January;

    /// <summary>
    /// Specifies the month of a monthly occurrence.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public CapacityIntervalDefs.months MonthlyOccurrence
    {
        get => monthlyOccurrence;
        set
        {
            monthlyOccurrence = value;
            bools[MonthlyOccurrenceSetIdx] = true;
        }
    }

    public bool MonthlyOccurrenceSet => bools[MonthlyOccurrenceSetIdx];

    private CapacityIntervalDefs.occurrences occurrence = CapacityIntervalDefs.occurrences.First;

    /// <summary>
    /// Specifies which occurence of the day to use for MonthlyByDayType or YearlyByDayType Recurrence.   Such as 'Last Thursday of every month'.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public CapacityIntervalDefs.occurrences Occurrence
    {
        get => occurrence;
        set
        {
            occurrence = value;
            bools[OccurrenceSetIdx] = true;
        }
    }

    public bool OccurrenceSet => bools[OccurrenceSetIdx];

    private CapacityIntervalDefs.recurrences recurrence = CapacityIntervalDefs.recurrences.Daily;

    /// <summary>
    /// Specifies if and how the event repeats.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    [Required(true)]
    public CapacityIntervalDefs.recurrences Recurrence
    {
        get => recurrence;
        set
        {
            recurrence = value;
            bools[RecurrenceSetIdx] = true;
        }
    }

    public bool RecurrenceSet => bools[RecurrenceSetIdx];

    private DateTime m_originalStartDateTime;
    public DateTime OriginalStartDateTime
    {
        get { return m_originalStartDateTime; }
        set { m_originalStartDateTime = value; }
    }

    private DateTime m_originalEndDateTime;
    public DateTime OriginalEndDateTime
    {
        get { return m_originalEndDateTime; }
        set { m_originalEndDateTime = value; }
    }

    private DateTime recurrenceEndDateTime = DateTime.UtcNow.AddYears(1);

    /// <summary>
    /// No events are scheduled to start after this DateTime.  Only used if RecurrenceEndType specifies RecurrenceEndDateTime.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public DateTime RecurrenceEndDateTime
    {
        get => recurrenceEndDateTime;
        set
        {
            recurrenceEndDateTime = value;
            bools[RecurrenceEndDateTimeSetIdx] = value != PTDateTime.MinValue.ToDateTime();
        }
    }

    public bool RecurrenceEndDateTimeSet => bools[RecurrenceEndDateTimeSetIdx];

    private CapacityIntervalDefs.recurrenceEndTypes recurrenceEndType = CapacityIntervalDefs.recurrenceEndTypes.NoEndDate;

    /// <summary>
    /// Specifies the indicator of when the recurrence stops.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public CapacityIntervalDefs.recurrenceEndTypes RecurrenceEndType
    {
        get => recurrenceEndType;
        set
        {
            recurrenceEndType = value;
            bools[RecurrenceEndTypeSetIdx] = true;
        }
    }

    public bool RecurrenceEndTypeSet => bools[RecurrenceEndTypeSetIdx];

    private int skipFrequency;

    /// <summary>
    /// Specifies how many intervals to skip between events.  For example, if Recurrence is Daily and SkipFrequency is 2 then an event is scheduled then two days are skipped and then another event is
    /// scheduled.
    /// </summary>
    public int SkipFrequency
    {
        get => skipFrequency;
        set
        {
            skipFrequency = value;
            bools[SkipFrequencySetIdx] = true;
        }
    }

    public bool SkipFrequencySet => bools[SkipFrequencySetIdx];

    private CapacityIntervalDefs.months yearlyMonth = CapacityIntervalDefs.months.January;

    /// <summary>
    /// Specifies the month in which the yearly event occurrs.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public CapacityIntervalDefs.months YearlyMonth
    {
        get => yearlyMonth;
        set
        {
            yearlyMonth = value;
            bools[YearlyMonthSetIdx] = true;
        }
    }

    public bool YearlyMonthSet => bools[YearlyMonthSetIdx];

    private decimal nbrOfPeopleOverride = 1;

    /// <summary>
    /// Can be used to override the Nbr Of People for a specified number of intervals at the beginning of the Recurring Capacity Interval.
    /// </summary>
    public decimal NbrOfPeopleOverride
    {
        get => nbrOfPeopleOverride;
        set
        {
            if (value <= 0)
            {
                throw new APSCommon.PTValidationException("Nbr Of People Override must be greater than zero.");
            }

            nbrOfPeopleOverride = value;
            bools[NbrOfPeopleOverrideSetIdx] = true;
        }
    }

    public bool NbrOfPeopleOverrideSet => bools[NbrOfPeopleOverrideSetIdx];

    private int nbrIntervalsToOverride;

    /// <summary>
    /// The number of Intervals in the Recurring Capacity Interval to override with the Nbr Of People Override.
    /// </summary>
    public int NbrIntervalsToOverride
    {
        get => nbrIntervalsToOverride;
        set
        {
            nbrIntervalsToOverride = value;
            bools[NbrIntervalsToOverrideSetIdx] = true;
        }
    }

    public bool NbrIntervalsToOverrideSet => bools[NbrIntervalsToOverrideSetIdx];

    private bool monday = true;

    /// <summary>
    /// Specifies whether to include Mondays in Weekly recurring events.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool Monday
    {
        get => monday;
        set
        {
            monday = value;
            bools[MondaySetIdx] = true;
        }
    }

    public bool MondaySet => bools[MondaySetIdx];

    private bool tuesday = true;

    /// <summary>
    /// Specifies whether to include Tuesdays in Weekly recurring events.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool Tuesday
    {
        get => tuesday;
        set
        {
            tuesday = value;
            bools[TuesdaySetIdx] = true;
        }
    }

    public bool TuesdaySet => bools[TuesdaySetIdx];

    private bool wednesday = true;

    /// <summary>
    /// Specifies whether to include Wednesdays in Weekly recurring events.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool Wednesday
    {
        get => wednesday;
        set
        {
            wednesday = value;
            bools[WednesdaySetIdx] = true;
        }
    }

    public bool WednesdaySet => bools[WednesdaySetIdx];

    private bool thursday = true;

    /// <summary>
    /// Specifies whether to include Thursdays in Weekly recurring events.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool Thursday
    {
        get => thursday;
        set
        {
            thursday = value;
            bools[ThursdaySetIdx] = true;
        }
    }

    public bool ThursdaySet => bools[ThursdaySetIdx];

    private bool friday = true;

    /// <summary>
    /// Specifies whether to include Fridays in Weekly recurring events.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool Friday
    {
        get => friday;
        set
        {
            friday = value;
            bools[FridaySetIdx] = true;
        }
    }

    public bool FridaySet => bools[FridaySetIdx];

    private bool saturday = true;

    /// <summary>
    /// Specifies whether to include Saturdays in Weekly recurring events.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool Saturday
    {
        get => saturday;
        set
        {
            saturday = value;
            bools[SaturdaySetIdx] = true;
        }
    }

    public bool SaturdaySet => bools[SaturdaySetIdx];

    private bool sunday = true;

    /// <summary>
    /// Specifies whether to include Sundays in Weekly recurring events.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool Sunday
    {
        get => sunday;
        set
        {
            sunday = value;
            bools[SundaySetIdx] = true;
        }
    }

    public bool SundaySet => bools[SundaySetIdx];
    #endregion Shared Properties
}