using System;
using System.Collections.Generic;
using System.Drawing;

namespace ReportsWebApp.DB.Models
{
    public class ResourceTimeRange
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; }
        public string RecurrenceRule { get; set; } // Using iCal format for recurrence rules
        public string TimeRangeColor { get; set; }
        public string Cls
        {
            get
            {
                return "child-"+ColorClassMapper.GetCssClassFromHex(TimeRangeColor);
            }
        }
        public bool ReadOnly { get; set; } // Indicates if the time range is editable

        // Modify the constructor to accept an instance of TimeIntervalColorManager
        public ResourceTimeRange(int id, int resourceId, DateTime startDate, DateTime endDate, string name, string intervalTypeColor)
        {
            Id = id;
            ResourceId = resourceId;
            StartDate = startDate;
            EndDate = endDate;
            Name = name;
            TimeRangeColor = intervalTypeColor;
            RecurrenceRule = ""; // Default value if not set
            ReadOnly = false; // Default value if not set
        }
    }
    public class IntervalColor
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
    }
}
