using System;

namespace AhaIntegration.ResponseObjects
{

    public class Epcis
    {
        public Epic[] epics { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Epic
    {
        public string id { get; set; }
        public string reference_num { get; set; }
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public string url { get; set; }
        public string resource { get; set; }
    }

}
