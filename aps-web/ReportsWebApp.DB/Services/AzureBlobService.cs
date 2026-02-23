using Azure.Data.Tables;
using System.Net;
using Azure.Storage;
using Azure.Storage.Blobs;

namespace ReportsWebApp.DB.Services
{
    /// <summary>
    /// Connects with the Azure Blob of deployed objects and returns data as a <see cref="Stream"/> to avoid needing to write it to memory.
    /// We use the convention of naming blobs as "{ObjectName}-{FourPartVersionNumber}.zip", since blobs require unique names.
    /// The api controller will suggest a generic name to the client, with the version number trimmed (leaving "ObjectName.zip).
    /// </summary>
    public class AzureBlobService
    {
        private const string c_storageAccountName = "ptwebhostv2dev";
        private const string c_blobServiceEndpoint = @"https://ptwebhostv2dev.blob.core.windows.net/software";

        private string m_serverAgentPath => $"{c_blobServiceEndpoint}/serveragent";
        private string m_clientAgentPath => $"{c_blobServiceEndpoint}/clientagent";

        public async Task<Stream>GetServerAgentZip(string a_version)
        {
            string objectName = "ServerAgent";
            string versionedName = GetVersionedBlobName(objectName, a_version);

            try
            {
                BlobClient client = InitClient($"{m_serverAgentPath}/{versionedName}");

                if (!await client.ExistsAsync())
                {
                    return null;
                }

                return await client.OpenReadAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {objectName} version {a_version}: {ex}");
            }
        }       
        
        public async Task<Stream>GetClientAgentZip(string a_version)
        {
            string objectName = "PlanetTogetherClientAgent";
            string versionedName = GetVersionedBlobName(objectName, a_version);

            try
            {
                BlobClient client = InitClient($"{m_clientAgentPath}/{versionedName}");

                if (!await client.ExistsAsync())
                {
                    return null;
                }

                return await client.OpenReadAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {objectName} version {a_version}: {ex}");
            }
        }

        private BlobClient InitClient(string a_blobUri)
        {
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(c_storageAccountName, "AqEmag99NCYtuFzobE2LM/4HCuNFIRADoGnz6lrq7u+dUkO2vpU8CHV0XKTgODrNPnb4L08RYGYI+AStTf4Byw==");
            BlobClient client = new BlobClient(new Uri(a_blobUri), credential);

            return client;
        }

        private static string GetVersionedBlobName(string objectName, string a_version)
        {
            return $"{objectName}-{a_version}.zip";
        }
    }
}
