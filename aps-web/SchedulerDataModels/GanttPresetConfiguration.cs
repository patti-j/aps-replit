namespace SchedulerDataModels
{
    public class SchedulerPresetConfiguration
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool InfiniteScroll { get; set; } = false;
        public bool RowResize { get; set; } = true;
        public List<object> Columns { get; set; } = new List<object>();
        public double AverageTaskDuration { get; set; } // Average task duration in days
        public int RowHeight { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["startDate"] = StartDate.ToString("s"),
                ["endDate"] = EndDate.ToString("s"),
                ["viewPreset"] = CustomPreset(),
                ["infiniteScroll"] = InfiniteScroll,
                ["columns"] = Columns,
                ["rowHeight"] = RowHeight,
                ["features"] = CustomFeatures(),
                ["barMargin"] = 4
            };
        }
        public Dictionary<string, object> CustomFeatures()
        {
            return new Dictionary<string, object>
            {
                ["rowResize"] = RowResize
            };
        }

        public Dictionary<string, object> CustomPreset()
        {
            // Dynamic adjustment based on the average task duration
            if (AverageTaskDuration <= 30)
            {
                // Settings for short durations (up to one month)
                return GetDayHourPreset();
            }
            else if (AverageTaskDuration <= 90)
            {
                // Settings for medium durations (up to three months)
                return GetWeekDayPreset();
            }
            else if (AverageTaskDuration <= 365)
            {
                // Settings for long durations (up to one year)
                return GetMonthWeekPreset();
            }
            else if (AverageTaskDuration <= 1825) // 5 years
            {
                // Settings for "extralarge" (up to five years)
                return GetYearMonthPreset();
            }
            else if (AverageTaskDuration <= 5475) // 15 years
            {
                // Settings for "gigantic" (up to fifteen years)
                return GetDecadeYearPreset();
            }
            else
            {
                // Settings for "extragigantic" and beyond
                return GetCenturyDecadePreset();
            }
        }
        private Dictionary<string, object> GetDayHourPreset()
        {
            return new Dictionary<string, object>
            {
                ["timeResolution"] = new { unit = "hour", increment = 1 },
                ["headers"] = new object[]
                {
            new { unit = "day", increment = 1, dateFormat = "ddd MM/dd" },
            new { unit = "hour", increment = 1, dateFormat = "HH:mm" }
                },
                ["tickWidth"] = 25
            };
        }
        private Dictionary<string, object> GetWeekDayPreset()
        {
            return new Dictionary<string, object>
            {
                ["timeResolution"] = new { unit = "day", increment = 1 },
                ["headers"] = new object[]
                {
            new { unit = "week", increment = 1, dateFormat = "'Week' W" },
            new { unit = "day", increment = 1, dateFormat = "ddd MM/dd" }
                },
                ["tickWidth"] = 50
            };
        }
        private Dictionary<string, object> GetMonthWeekPreset()
        {
            return new Dictionary<string, object>
            {
                ["timeResolution"] = new { unit = "week", increment = 1 },
                ["headers"] = new object[]
                {
            new { unit = "month", increment = 1, dateFormat = "MMMM" },
            new { unit = "week", increment = 1, dateFormat = "'Week' W" }
                },
                ["tickWidth"] = 75
            };
        }
        private Dictionary<string, object> GetYearMonthPreset()
        {
            return new Dictionary<string, object>
            {
                ["timeResolution"] = new { unit = "month", increment = 1 },
                ["headers"] = new object[]
                {
            new { unit = "year", increment = 1, dateFormat = "YYYY" },
            new { unit = "month", increment = 1, dateFormat = "MMM" }
                },
                ["tickWidth"] = 100
            };
        }
        private Dictionary<string, object> GetDecadeYearPreset()
        {
            return new Dictionary<string, object>
            {
                ["timeResolution"] = new { unit = "year", increment = 1 },
                ["headers"] = new object[]
                {
            new { unit = "decade", increment = 10, dateFormat = "'Decade' YYYYs" },
            new { unit = "year", increment = 1, dateFormat = "YYYY" }
                },
                ["tickWidth"] = 120
            };
        }
        private Dictionary<string, object> GetCenturyDecadePreset()
        {
            return new Dictionary<string, object>
            {
                ["timeResolution"] = new { unit = "decade", increment = 10 },
                ["headers"] = new object[]
                {
            new { unit = "century", increment = 100, dateFormat = "YYYY" },
            new { unit = "decade", increment = 10, dateFormat = "YYYY's" }
                },
                ["tickWidth"] = 150
            };
        }

    }
}
