using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppImportService.Models
{
	public interface IQueueMessage
	{
		string CompanyId { get; set; }
		string Sender { get; set; }
		string MessageType { get; set; }
		public bool IsSuccess { get; set; }
		public string Errors { get; set; }
		public string Message { get; set; }
	}
}
