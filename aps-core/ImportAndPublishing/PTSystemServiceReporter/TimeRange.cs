using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.SystemServiceReporter
{
    public class TimeRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Downtime Downtime { get; set; }

        public TimeRange(DateTime a_start, DateTime a_end, Downtime a_downtime = null)
        {
            Start = a_start;
            End = a_end;
            Downtime = a_downtime;
        }

        public TimeSpan Duration
        {
            get
            {
                return End - Start;
            }
        }

        public static bool Overlap(TimeRange a_timeRange1, TimeRange a_timeRange2)
        {
            return (a_timeRange2.Start < a_timeRange1.End && a_timeRange1.Start < a_timeRange2.End);
        }

        public static bool Overlap(TimeRange a_timeRange, IEnumerable<TimeRange> a_timeRanges)
        {
            foreach(TimeRange tr in a_timeRanges)
            {
                if(Overlap(a_timeRange, tr))
                {
                    return true;
                }
            }

            return false;
        }

        public static TimeRange Merge(TimeRange a_timeRange1, TimeRange a_timeRange2)
        {
            return new TimeRange(
                (a_timeRange1.Start < a_timeRange2.Start) ? a_timeRange1.Start : a_timeRange2.Start,
                (a_timeRange1.End > a_timeRange2.End) ? a_timeRange1.End : a_timeRange2.End,
                (a_timeRange1.Start < a_timeRange2.Start) ? a_timeRange1.Downtime : a_timeRange2.Downtime
            );
        }
    }
}
