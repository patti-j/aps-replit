using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppImportService.Models
{
	public class TriggerImportMessage : IQueueMessage
	{
		public string Sender { get; set; }
		public string MessageType { get; set; }
		public bool IsSuccess { get; set; }
		public string Errors { get; set; }
		public string Message { get; set; }
		public string CompanyId { get; set; }
		public string InstanceName { get; set; }
		public string Environment { get; set; }
	}
}
