using System.ComponentModel.DataAnnotations;

using ReportsWebApp.Shared;

namespace ReportsWebApp.Data
{
    public class NetsuiteCredentials
    {
        public string AccountId { get; set; } = "";
        public string ClientId { get; set; } = "";
        [Password] public string ClientSecret { get; set; } = "";
        public string TokenId { get; set; } = "";
        [Password] public string TokenSecret { get; set; } = "";
    }

    public class NetsuiteImportEndpoints
    {
        public List<string> URLs { get; set; } = new();
    }

    public class NetsuitePublishEndpoint
    {
        public string URL { get; set; } = "";
    }

    public class NetsuiteIntegrationModel
    {
        public NetsuiteCredentials Credentials { get; set; } = new();

        public NetsuiteImportEndpoints ImportEndpoints { get; set; } = new();

        public NetsuitePublishEndpoint PublishEndpoint { get; set; } = new();

        // Checkboxes
        public bool WorkOrders { get; set; } = true;
        public bool BOMs { get; set; } = true;
        public bool Routings { get; set; } = true;
        public bool Items { get; set; } = true;  
        public bool PurchaseOrders { get; set; } = false;
        public bool SalesOrders { get; set; } = false;
    }
}
