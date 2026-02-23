using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using ReportsWebApp.Controllers.Models;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ReportsWebApp.Shared;

namespace ReportsWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAppInsightsLogger _logger;

        public UploadController(IConfiguration configuration, IAppInsightsLogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        [HttpPost("[action]")]
        public async Task<ActionResult>  Upload(IFormFile myFile, string companyId, string environment, string instanceName, string userEmail)
        {
            var guid = Guid.NewGuid();
            var x = companyId;
            try
            {
                var fileShareConnectionString = _configuration["AzureSMBStorageConnectionString"];
                string shareName = _configuration["AzureSMBStorageName"];
                string directoryName = _configuration["AzureSMBStorageInDirectory"];
                QueueManager queueManager = new QueueManager(fileShareConnectionString, _configuration["AzureImportQueueName"]);

                // Create a temporary MemoryStream to hold the file content
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Copy the content of the IFormFile to the MemoryStream
                    await myFile.CopyToAsync(memoryStream);

                    // Reset the position of the MemoryStream to the beginning
                    memoryStream.Position = 0;
                    
                        AzureSMBStorageManager storageManager = new AzureSMBStorageManager(fileShareConnectionString, shareName, directoryName);
                        await storageManager.CopyExcelToAzureSMBStorage(memoryStream, guid.ToString()+".xlsx");

                    ImportInitiatedMessage message = new ImportInitiatedMessage()
                    {
                        MessageType = "ExcelImport", FileName = guid.ToString() +".xlsx",ImportCompletedDateTime = DateTime.UtcNow, Message = "Import file copied to Azure storage",
                        Sender = userEmail, CompanyId = companyId, InstanceName = instanceName, Environment = environment, OriginalFileName = myFile.FileName
                    };
                        queueManager.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message))));
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, userEmail);
                return BadRequest();
            }
            return Ok("importRequestId:" + guid);
        }

        public string GetValueFromAzureKeyVault(string key)
        {
            string kvURL = _configuration["KeyValultUrl"];
            string tenentId = _configuration["TenantId"];
            string clientId = _configuration["ClientId"];
            string clientSecret = _configuration["ClientSecret"];

            var credential = new ClientSecretCredential(tenentId, clientId, clientSecret);

            var client = new SecretClient(new Uri(kvURL), credential);
            KeyVaultSecret secret = client.GetSecret(key);
            return secret.Value;

        }
    }
}
