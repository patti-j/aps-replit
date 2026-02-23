using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerDataModels
{
    public class SegmentsConfiguration
    {
        public List<SegmentConfig> SegmentConfigs { get; set; } = new List<SegmentConfig>();
    }

    public class SegmentConfig
    {
        public int Id { get; set; }
        public int Weight { get; set; } 
        public string IdentifierKey { get; set; } // e.g., "activitycomments", "jobName"
        public string NameKey { get; set; } 
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
    }

}
