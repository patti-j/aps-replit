namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class GetPostImportWebhookResponse
    {
        public string PostImportURL { get; set; }
        public int WebApiTimeoutSeconds { get; set; }
    }
}
