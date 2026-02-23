using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.SystemServiceReporter
{
    public class Downtime
    {
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Reason { get; set; }
        public DateTime ServiceStartTime { get; set; }
        public TimeSpan DurationSinceServiceStart { get; set; }
        public EventViewerLog StoppedLog { get; set; }
        public bool RequiresManualInput { get; set; }
    }
}
