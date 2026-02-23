using System;

namespace AhaIntegration.ResponseObjects
{

    public class Requirements
    {
        public Requirement[] requirements { get; set; }
        public Pagination pagination { get; set; }
    }
    
    public class Requirement
    {
        public string id { get; set; }
        public string name { get; set; }
        public string reference_num { get; set; }
        public int position { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string release_id { get; set; }
        public Workflow_Status workflow_status { get; set; }
        public string url { get; set; }
        public string resource { get; set; }
        public Description description { get; set; }
        public Feature feature { get; set; }
        public object assigned_to_user { get; set; }
        public Created_By_User created_by_user { get; set; }
        public object[] attachments { get; set; }
        public object[] custom_fields { get; set; }
        public Integration_Fields[] integration_fields { get; set; }
        public int comments_count { get; set; }
        public object[] time_tracking_events { get; set; }
    }

}
