namespace ReportsWebApp.Controllers.Models;

public class ImportInitiatedMessage : QueueMessage
{
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string Message { get; set; }
    public DateTime ImportCompletedDateTime { get; set; }
    public string Environment { get; set; }
    public string CompanyId { get; set; }
    public string InstanceName { get; set; }
    public string Sender { get; set; }
    public string MessageType { get; set; }
}