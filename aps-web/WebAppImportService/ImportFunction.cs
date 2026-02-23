using System;
using System.Threading.Tasks;
using WebAppImportService.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using WebAppImportService.Models;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json.Linq;

namespace WebAppImportService
{
    public class ImportFunction
    {
        [FunctionName("ImportFunction")]
        public async Task Run([QueueTrigger("%ImportQueueName%", Connection = "AzureSMBStorageConnectionString")] QueueMessage queueItem, ILogger log)
        {
	        JObject json = JObject.Parse(queueItem.Body.ToString());
			switch ((string)json["MessageType"])
			{
				case "ExcelImport":
					ImportMessage importMessage = new ImportMessage()
					{
						FileName = (string)json["FileName"], OriginalFileName = (string)json["OriginalFileName"], Environment = (string)json["Environment"],
						CompanyId = (string)json["CompanyId"], InstanceName = (string)json["InstanceName"], Sender = (string)json["Sender"]
					};
					await DoExcelImport(importMessage, log); break;
				case "TriggerImport":
					TriggerImportMessage triggerImportMessage = new TriggerImportMessage() 
						{CompanyId = (string)json["CompanyId"], InstanceName = (string)json["InstanceName"], Sender = (string)json["Sender"], Environment =(string)json["Environment"] };
					await InitiateImport(triggerImportMessage, log); break;
			}
		}

        private async Task InitiateImport(TriggerImportMessage myQueueItem, ILogger log)
        {
	        //TriggerImportMessage myQueueItem = (TriggerImportMessage)queueItem;
	        CommonUtils.TriggerImport(myQueueItem, log);
        }

		private async Task DoExcelImport(ImportMessage queueItem, ILogger log)
		{
			ImportMessage myQueueItem = (ImportMessage)queueItem;
			try
			{
				string result = await CommonUtils.ConvertExcelToTable(myQueueItem);
				if (result == null)
				{
					log.LogInformation($"Queue trigger function processed: {myQueueItem}");
					//publish message to notification-queue//
					myQueueItem.IsSuccess = true;
					myQueueItem.Message = $"Import Complete: FileName: {myQueueItem.FileName}, User: {myQueueItem.Sender}, Date: {DateTime.Now}";
					await CommonUtils.PublishMessageToQueue(myQueueItem, Environment.GetEnvironmentVariable("NotificationQueueName"));
					log.LogInformation("Notification published to notification queue");
				}
				else
				{
					log.LogInformation($"Queue trigger function processed with errors: {myQueueItem}");
					//publish message to notification-queue//
					myQueueItem.IsSuccess = false;
					myQueueItem.Errors = result;
					myQueueItem.Message = result;
					await CommonUtils.PublishMessageToQueue(myQueueItem, Environment.GetEnvironmentVariable("NotificationQueueName"));
					log.LogInformation("Notification published to notification queue");
				}
			}
			catch (Exception ex)
			{
				log.LogError($"Exception during import {myQueueItem}: {ex.Message}");
				myQueueItem.IsSuccess = false;
				myQueueItem.Message = $"Import failed: FileName: {myQueueItem.FileName}, User: {myQueueItem.Sender}, Date: {DateTime.Now} Error: {ex.Message}";
				myQueueItem.Errors = ex.Message;
				myQueueItem.RetryCount++;
				if (myQueueItem.RetryCount == 3)
				{
					await CommonUtils.PublishMessageToQueue(myQueueItem, Environment.GetEnvironmentVariable("NotificationQueueName"));
					await CommonUtils.PublishMessageToQueue(myQueueItem, Environment.GetEnvironmentVariable("ImportQueueName") + "-poison");
				}
				else
				{
					await CommonUtils.PublishMessageToQueue(myQueueItem, Environment.GetEnvironmentVariable("ImportQueueName"));
				}
			}
		}
	}
}
