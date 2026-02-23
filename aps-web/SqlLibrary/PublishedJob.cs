using System;
using System.Collections.Generic;
using System.Text;

namespace SqlLibrary
{
    public class PublishedJob
    {
        public string ExternalId;
        public bool Scheduled;
        public DateTime ScheduledStartDate;
        public DateTime ScheduledEndDate;
    }
}
