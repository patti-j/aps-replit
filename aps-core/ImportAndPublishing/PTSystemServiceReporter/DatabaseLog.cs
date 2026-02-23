using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.SystemServiceReporter
{
    public class DatabaseLog
    {
        public enum EventTypes
        {
            STARTED,
            STOPPING,
            STOPPED,
            ERROR
        }

        public string InstanceName { get; set; }
        public string SoftwareVersion { get; set; }
        public string Message { get; set; }
        public EventTypes EventType { get; set; }
        public string Reason { get; set; }
        public DateTime TimeOfEvent { get; set; }
        public int DurationSinceStartInSeconds { get; set; }

        public TimeSpan DowntimeDuration { get; set; }
    }
}
