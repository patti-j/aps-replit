using System;
using System.Net.Mail;

using Newtonsoft.Json.Linq;

namespace AhaIntegration.ResponseObjects
{
    public class Pagination
    {
        public int total_records { get; set; }
        public int total_pages { get; set; }
        public int current_page { get; set; }
    }

    public class Workflow_Kind
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Workflow_Status
    {
        public string id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public bool complete { get; set; }
        public string color { get; set; }
    }

    public class Product_Roles
    {
        public int role { get; set; }
        public string role_description { get; set; }
        public string product_id { get; set; }
        public string product_name { get; set; }
    }


    public class Description
    {
        public string id { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public object[] attachments { get; set; }
    }

    public class Category
    {
        public string id { get; set; }
        public string name { get; set; }
        public string parent_id { get; set; }
        public DateTime created_at { get; set; }
    }
    public class Owner
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string avatar_url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string reference_prefix { get; set; }
        public string name { get; set; }
        public bool product_line { get; set; }
        public DateTime created_at { get; set; }
    }

    public class Integration_Fields
    {
        public string id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string integration_id { get; set; }
        public string service_name { get; set; }
        public DateTime created_at { get; set; }
    }

    public class Created_By_User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string avatar_url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Assigned_To_User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool default_assignee { get; set; }
    }

    public class Custom_Fields
    {
        public string key { get; set; }
        public string name { get; set; }
        public JToken value { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public object[] attachments { get; set; }
    }

    public class Parent
    {
        public string id { get; set; }
        public string reference_num { get; set; }
        public string name { get; set; }
        public string start_date { get; set; }
        public string release_date { get; set; }
        public DateTime created_at { get; set; }
        public string url { get; set; }
        public string resource { get; set; }
    }

    public class Theme
    {
        public string id { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public Attachment[] attachements { get; set; }
    }
}
