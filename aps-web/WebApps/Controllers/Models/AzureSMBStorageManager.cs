using Azure.Storage.Files.Shares;

namespace ReportsWebApp.Controllers.Models;

public class AzureSMBStorageManager
{
    private string _connectionString;
    private string _shareName;
    private string _directoryName;

    public AzureSMBStorageManager(string connectionString, string shareName, string directoryName)
    {
        _connectionString = connectionString;
        _shareName = shareName;
        _directoryName = directoryName;
    }

    public async Task CopyExcelToAzureSMBStorage(Stream excelFileStream, string excelFileName)
    {
        // Create a ShareClient object to interact with the Azure File Share
        var shareClient = new ShareClient(_connectionString, _shareName);

        //var x = shareClient.GetRootDirectoryClient();
        // Ensure the share exists
        await shareClient.CreateIfNotExistsAsync();

        // Get a reference to the directory in the Azure File Share
        var directoryClient = shareClient.GetDirectoryClient(_directoryName);

        // Ensure the directory exists
        await directoryClient.CreateIfNotExistsAsync();

        // Get a reference to the file in the directory
        var fileClient = directoryClient.GetFileClient(excelFileName);
        fileClient.Create(excelFileStream.Length);

        // Upload the Excel file to Azure SMB storage
        await fileClient.UploadAsync(excelFileStream);
    }

    public async Task<byte[]> ReadFileFromAzureSMBStorage(string transactionId, string originalFileName)
    {
        try
        {
            // Create a ShareClient object to interact with the Azure File Share
            var shareClient = new ShareClient(_connectionString, _shareName);
           
            // Get a reference to the directory in the Azure File Share
            var directoryClient = shareClient.GetDirectoryClient(_directoryName);
            // Get a reference to the file in the share
            ShareFileClient fileClient = directoryClient.GetFileClient(transactionId + Path.GetExtension(originalFileName));

            // Download the file content
            var downloadInfo = await fileClient.DownloadAsync();
            using (var memoryStream = new MemoryStream())
            {
                await downloadInfo.Value.Content.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}