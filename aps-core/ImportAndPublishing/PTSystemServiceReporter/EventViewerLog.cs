using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.SystemServiceReporter
{
    public class EventViewerLog
    {
        public EventLogEntry Entry { get; set; }
        public string FullSourceName { get; set; }
        public bool SystemLog { get; set; }
        public string WindowsErrorMessage { get; set; }
    }
}
