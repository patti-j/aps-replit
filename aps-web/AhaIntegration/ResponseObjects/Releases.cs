using System;

namespace AhaIntegration.ResponseObjects
{

    public class Releases
    {
        public Release[] releases { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Release
    {
        public string id { get; set; }
        public string product_id { get; set; }
        public string reference_num { get; set; }
        public string name { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public DateTime? development_started_on { get; set; }
        public DateTime? release_date { get; set; }
        public string external_release_date_description { get; set; }
        public string external_date_resolution { get; set; }
        public bool released { get; set; }
        public bool parking_lot { get; set; }
        public bool master_release { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int position { get; set; }
        public int progress { get; set; }
        public string progress_source { get; set; }
        public Theme theme { get; set; }
        public string url { get; set; }
        public string resource { get; set; }
        public Integration_Fields[] integration_fields { get; set; }
        public Custom_Fields[] custom_fields { get; set; }
        public int comments_count { get; set; }
        public object original_estimate { get; set; }
        public decimal remaining_estimate { get; set; }
        public Workflow_Status workflow_status { get; set; }
        public Owner owner { get; set; }
        public object[] goals { get; set; }
        public Project project { get; set; }
        public Parent parent { get; set; }
    }
}
