namespace ReportsWebApp.Controllers.Models
{
	public class TriggerImportMessage: QueueMessage
	{
		public string Sender { get; set; }
		public string MessageType { get; set; }
		public string CompanyId { get; set; }
		public string InstanceName { get; set; }
		public bool IsSuccess { get; set; }
		public string Errors { get; set; }
		public string Message { get; set; }
		public string Environment { get; set; }
	}
}
