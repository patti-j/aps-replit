using System;

namespace AhaIntegration.ResponseObjects
{

    public class Initiatives
    {
        public Initiative[] initiatives { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Initiative
    {
        public string id { get; set; }
        public string name { get; set; }
        public string reference_num { get; set; }
        public string status { get; set; }
        public int effort { get; set; }
        public int value { get; set; }
        public bool presented { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int position { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string product_id { get; set; }
        public object progress { get; set; }
        public string progress_source { get; set; }
        public string url { get; set; }
        public string resource { get; set; }
        public Project project { get; set; }
        public Workflow_Status workflow_status { get; set; }
        public Description description { get; set; }
        public object[] attachments { get; set; }
        public Assigned_To_User assigned_to_user { get; set; }
        public int comments_count { get; set; }
        public object[] goals { get; set; }
        public Integration_Fields[] integration_fields { get; set; }
        public Feature[] features { get; set; }
    }
}

