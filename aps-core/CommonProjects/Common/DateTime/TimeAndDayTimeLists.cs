namespace PT.Common;

public class TimeAndDayTimeLists
{
    /// <summary>
    /// Throws a PT.Common.Validation exception if the string doesn't contain valid TimeSpans and Day-times.
    /// </summary>
    /// <param name="a_aTimesAndDateTimesString"></param>
    public void CreateLists(string a_timesAndDateTimesString)
    {
        //Validate time of day list
        //Possible values:
        //Time only: 08:00 (means every day at 8)
        //Day only: Monday (means Monday at 00:00)
        //Day time: Monday 08:00
        string[] entries = a_timesAndDateTimesString.Split(SEP_CHAR.ToCharArray());

        for (int i = 0; i < entries.Length; i++)
        {
            string nextEntry = ((string)entries.GetValue(i)).Trim();
            string[] entrySegments = nextEntry.Split(" ".ToCharArray());
            if (entrySegments.Length > 2)
            {
                ThrowException(nextEntry);
            }

            if (entrySegments.Length > 0)
            {
                string segment1 = entrySegments[0].Trim();
                TimeSpan timeOfDay1;
                bool isTimeOfDay1 = IsTimeOfDay(segment1, out timeOfDay1);
                DayOfWeek dayOfWeek1;
                bool isDayOfWeek1 = IsDayOfWeek(segment1, out dayOfWeek1);

                bool isTimeOfDay2 = false;
                bool isDayOfWeek2 = false;
                TimeSpan timeOfDay2 = new ();
                DayOfWeek dayOfWeek2 = DayOfWeek.Sunday;

                if (entrySegments.Length > 1)
                {
                    string segment2 = entrySegments[1].Trim();

                    isTimeOfDay2 = IsTimeOfDay(segment2, out timeOfDay2);
                    isDayOfWeek2 = IsDayOfWeek(segment2, out dayOfWeek2);
                }

                //Now create the values based on what we have
                if (entrySegments.Length == 1)
                {
                    if (isDayOfWeek1)
                    {
                        DayTime dt = new (dayOfWeek1, new TimeSpan());
                        m_dayTimes.Add(dt);
                    }
                    else if (isTimeOfDay1)
                    {
                        m_times.Add(timeOfDay1);
                    }
                    else
                    {
                        ThrowException(nextEntry);
                    }
                }
                else if (entrySegments.Length > 1)
                {
                    if (isDayOfWeek1 && isTimeOfDay2)
                    {
                        DayTime dt = new (dayOfWeek1, timeOfDay2);
                        m_dayTimes.Add(dt);
                    }
                    else if (isDayOfWeek2 && isTimeOfDay1)
                    {
                        DayTime dt = new (dayOfWeek2, timeOfDay1);
                        m_dayTimes.Add(dt);
                    }
                    else
                    {
                        ThrowException(nextEntry);
                    }
                }
            }
        }

        m_dayTimes = m_dayTimes.OrderBy(x => x.Day).ThenBy(y => y.Time).ToList();
        m_times = m_times.OrderBy(x => x.TotalMilliseconds).ToList();
    }

    public DateTime GetNextDayTime(DateTime a_timeElementRunUtc)
    {
        //Used to keep track of the next time from the Times list
        DateTime nextTime = PTDateTime.MaxDateTime;
        if (TimesOfDay.Count > 0)
        {
            nextTime = GetNextTimeFromTimelist(DateTime.UtcNow);
            if (nextTime == PTDateTime.MaxDateTime)
            {
                // There are no more happening today.  Check tomorrow
                nextTime = GetNextTimeFromTimelist(DateTime.UtcNow.Date.AddDays(1));
            }
        }

        DateTime nextDayTime = GetNextTimeFromDayTimeList();
        return PTDateTime.Min(nextTime, nextDayTime);
    }

    private DateTime GetNextTimeFromDayTimeList()
    {
        foreach (DayTime dayTime in DayTimes)
        {
            if (dayTime.Day == DateTime.UtcNow.DayOfWeek && dayTime.Time > DateTime.UtcNow.TimeOfDay)
            {
                return DateTime.UtcNow.Date + dayTime.Time;
            }
        }

        return PTDateTime.MaxDateTime;
    }

    //Find next start time after time
    private DateTime GetNextTimeFromTimelist(DateTime time)
    {
        foreach (TimeSpan ts in TimesOfDay)
        {
            if (time < PTDateTime.UserDateTimeNow.ToDateNoTime().ToDateTime() + ts)
            {
                return PTDateTime.UserDateTimeNow.ToDateNoTime().ToDateTime() + ts;
            }
        }

        return PTDateTime.MaxDateTime;
    }

    protected virtual void ThrowException(string a_nextEntry)
    {
        throw new CommonException(string.Format("Input string contains one value that is not a time or day: '{0}'", a_nextEntry));
    }

    private bool IsDayOfWeek(string a_inString, out DayOfWeek a_dayOfWeek)
    {
        try
        {
            a_dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), a_inString);
            return true;
        }
        catch
        {
            a_dayOfWeek = DayOfWeek.Sunday;
            return false;
        }
    }

    private bool IsTimeOfDay(string a_inString, out TimeSpan a_timeOfDay)
    {
        try
        {
            a_timeOfDay = TimeSpan.Parse(a_inString);
            return true;
        }
        catch
        {
            a_timeOfDay = new TimeSpan();
            return false;
        }
    }

    private List<TimeSpan> m_times = new ();

    public List<TimeSpan> TimesOfDay => m_times;

    private List<DayTime> m_dayTimes = new ();

    public List<DayTime> DayTimes => m_dayTimes;

    public bool DayTimeExists(DayOfWeek a_dayOfWeek, TimeSpan a_timeOfDay)
    {
        DayTime dt = new (a_dayOfWeek, a_timeOfDay);
        return m_dayTimes.Contains(dt);
    }

    public class DayTime
    {
        public DayTime(DayOfWeek a_dayOfWeek, TimeSpan a_timeOfDay)
        {
            m_dayOfWeek = a_dayOfWeek;
            m_timeOfDay = a_timeOfDay;
        }

        private readonly DayOfWeek m_dayOfWeek;

        public DayOfWeek Day => m_dayOfWeek;

        private readonly TimeSpan m_timeOfDay;

        public TimeSpan Time => m_timeOfDay;

        public override string ToString()
        {
            return string.Format("{0} {1}", Day, Time);
        }
    }

    public static readonly string SEP_CHAR = ",";
}