using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReportsWebApp.DB.Models
{
	public class ImportHistoryItem
	{
		[JsonProperty("file_name")]
		public string FileName { get; set; }
		[JsonProperty("import_date")]
		public DateTime ImportDate { get; set; }
		[JsonProperty("import_user")]
		public string ImportUser { get; set; }
		[JsonProperty("transaction_id")]
		public string? TransactionId { get; set; }
        [JsonProperty("import_details")]
        public string? Details { get; set; }
    }
}
