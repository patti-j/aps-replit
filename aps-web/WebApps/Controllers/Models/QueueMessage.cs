namespace ReportsWebApp.Controllers.Models
{
	public interface QueueMessage
	{
		string CompanyId { get; set; }
		string Sender { get; set; }
		string MessageType { get; set; }
	}
}
