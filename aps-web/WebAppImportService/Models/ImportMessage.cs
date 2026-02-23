using System;

namespace WebAppImportService.Models;

public class ImportMessage : IQueueMessage
{
	public bool IsSuccess { get; set; }
	public string Errors { get; set; }
	public string FileName { get; set; }
	public string OriginalFileName { get; set; }
	public string Message { get; set; }
	public DateTime ImportCompletedDateTime { get; set; }
	public string Environment { get; set; }
	public string CompanyId { get; set; }
	public string InstanceName { get; set; }
	public string Sender { get; set; }
	public int RetryCount { get; set; } = 0;
	public string MessageType { get; set; }
}