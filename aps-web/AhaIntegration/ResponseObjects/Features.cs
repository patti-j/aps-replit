using System;

namespace AhaIntegration.ResponseObjects
{
    public class FeatureUpdate
    {
        public string Id { get; set; }
        public DateTime ScheduledStartDateTime;
        public DateTime ScheduledEndDateTime;
        public string Description;
        //public string ReleaseId;
    }

    public class Features
    {
        public Feature[] features { get; set; }
        public Pagination pagination { get; set; }
    }
    
    public class Feature
    {
        public string id { get; set; }
        public string name { get; set; }
        public string reference_num { get; set; }
        public int position { get; set; }
        public int score { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object start_date { get; set; }
        public object due_date { get; set; }
        public string product_id { get; set; }
        public string product_name { get; set; }
        public object progress { get; set; }
        public string progress_source { get; set; }
        public Workflow_Kind workflow_kind { get; set; }
        public Workflow_Status workflow_status { get; set; }
        public decimal? original_estimate { get; set; }
        public decimal? remaining_estimate { get; set; }
        public object work_done { get; set; }
        public int work_units { get; set; }
        public bool use_requirements_estimates { get; set; }
        public Description description { get; set; }
        public object[] attachments { get; set; }
        public Integration_Fields[] integration_fields { get; set; }
        public string url { get; set; }
        public string resource { get; set; }
        public Release release { get; set; }
        public Created_By_User created_by_user { get; set; }
        public Assigned_To_User assigned_to_user { get; set; }
        public Requirement[] requirements { get; set; }
        public object[] goals { get; set; }
        public int comments_count { get; set; }
        public object[] score_facts { get; set; }
        public object[] tags { get; set; }
        public object[] full_tags { get; set; }
        public Custom_Fields[] custom_fields { get; set; }
        public object[] time_tracking_events { get; set; }
        public object[] feature_links { get; set; }
        public object feature_only_original_estimate { get; set; }
        public object feature_only_remaining_estimate { get; set; }
        public object feature_only_work_done { get; set; }
        public Epic epic { get; set; }
    }
}
