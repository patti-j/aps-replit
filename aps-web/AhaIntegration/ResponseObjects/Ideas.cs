using System;

namespace AhaIntegration.ResponseObjects
{

    public class Ideas
    {
        public Idea[] ideas { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Idea
    {
        public string id { get; set; }
        public string name { get; set; }
        public string reference_num { get; set; }
        public DateTime created_at { get; set; } 
        public string product_id { get; set; }
        public int votes { get; set; }
        public Workflow_Status workflow_status { get; set; }
        public Description description { get; set; }
        public string url { get; set; }
        public int endorsements_count { get; set; }
        public Category[] categories { get; set; }
        

    }

}