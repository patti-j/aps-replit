using Azure;
using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;

namespace ReportsWebApp.DB.Services
{
    /// <summary>
    /// Not a generic client. This client hard codes the endpoints for PT
    /// </summary>
    public class AzureTableService
    {
        private TableServiceClient m_tableServiceClient;
        private Uri m_azureStorageContainerUri;

        private const string c_storageAccountName = "ptwebhostv2dev";

        // New Tables
        private const string c_clientAgentVersionsTable = "ClientAgentVersions";
        private const string c_serverAgentVersionsTable = "ServerAgentVersions";

        // Existing pre-webapp tables (TODO: need review for use)
        private const string c_workspaceTableName = "StandardWorkspacesTable";
        private const string c_scenariosTableName = "StandardScenariosTable";
        private const string c_standardVersionsTableName = "WebInstallerStandardVersionsTable";
        private const string c_softwareVersionsTableName = "SoftwareWebVersions";
        private const string c_integrationFilesTableName = "WebInstallerIntegrationsV12Table";
        private const string c_configTableName = "WebInstallerConfigTable";
        private const string c_earlyAccessVersionsTableName = "WebInstallerEarlyAccessVersionsTable";
        private const string c_specificVersionsTable = "WebInstallerCompanySpecificVersionsTable";
        private bool isProduction = false;

        public AzureTableService(IConfiguration config)
        {
            m_azureStorageContainerUri = new Uri("https://ptwebhostv2dev.table.core.windows.net");
            if (config["Environment"].StartsWith("p", StringComparison.InvariantCultureIgnoreCase))
            {
                isProduction = true;
            }
            BuildTableClient();
        }
        private void BuildTableClient()
        {
            // TODO: Stephen mentioned that Shared Access Signature (Sas) credentials are a little more cumbersome to work with. Using standard key for now.
            TableSharedKeyCredential credential = new TableSharedKeyCredential(c_storageAccountName, "AqEmag99NCYtuFzobE2LM/4HCuNFIRADoGnz6lrq7u+dUkO2vpU8CHV0XKTgODrNPnb4L08RYGYI+AStTf4Byw==");
            //AzureSasCredential credential = new AzureSasCredential("?st=2023-03-28T14%3A32%3A14Z&se=2100-03-29T14%3A32%3A00Z&sp=r&sv=2018-03-28&tn=standardworkspacestable&sig=vbPXNZ3WK%2Fa5PXM1JQeoUu53%2Fx%2Bq3ZfRBqLN37pu%2Bh8%3D");
            m_tableServiceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
        }

        // Not in db yet
        public async Task<IEnumerable<StandardVersionEntity>> GetAllClientAgentVersions()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_clientAgentVersionsTable);

            List<StandardVersionEntity> versionList = new List<StandardVersionEntity>();

            var versions = tableClient.QueryAsync<StandardVersionEntity>("", 10);

            await foreach (var version in versions.ConfigureAwait(false))
            {
                // If in production, only show released versions. Otherwise, show all
                if (!isProduction || version.ReleaseState == 1)
                {
                    versionList.Add(version);
                }
            }

            return versionList;
        }

        public async Task<StandardVersionEntity> GetLatestClientAgentVersion()
        {
            var versions = await GetAllClientAgentVersions();

            var latest = versions.MaxBy(x => x.VersionDate);
            if (latest == null)
            {
                throw new Exception("Couldn't connect to azure to retrieve server versions");
            }

            return latest;
        }

        public async Task<IEnumerable<StandardVersionEntity>> GetAllServerAgentVersions()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_serverAgentVersionsTable);

            List<StandardVersionEntity> versionList = new List<StandardVersionEntity>();

            var versions = tableClient.QueryAsync<StandardVersionEntity>("", 10);

            await foreach (var version in versions.ConfigureAwait(false))
            {
                // If in production, only show released versions. Otherwise, show all
                if (!isProduction || version.ReleaseState == 1)
                {
                    versionList.Add(version);
                }
            }

            return versionList;
        }

        public async Task<StandardVersionEntity> GetLatestServerAgentVersion()
        {
            var versions = await GetAllServerAgentVersions();
            var latest = versions.MaxBy(x => x.VersionDate);
            if (latest == null)
            {
                throw new Exception("Couldn't connect to azure to retrieve server versions");
            }

            return latest;
        }

        public async Task<IEnumerable<StandardVersionEntity>> GetAllSoftwareVersions()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_softwareVersionsTableName);

            List<StandardVersionEntity> versionList = new List<StandardVersionEntity>();

            var versions = tableClient.QueryAsync<StandardVersionEntity>("", 10);

            await foreach (var version in versions.ConfigureAwait(false))
            {
                // If in production, only show released versions. Otherwise, show all
                if (!isProduction || version.ReleaseState == 1)
                {
                    versionList.Add(version);
                }
            }

            return versionList;
        }

        public async Task<StandardVersionEntity> GetLatestSoftwareVersion()
        {
            var versions = await GetAllSoftwareVersions();
            var latest = versions.MaxBy(x => x.VersionDate);
            if (latest == null)
            {
                throw new Exception("Couldn't connect to azure to retrieve server versions");
            }

            return latest;
        }


        // Not in db yet
        public async Task<IEnumerable<FileEntity>> GetAllStandardWorkspaces()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_workspaceTableName);

            IList<FileEntity> modelList = new List<FileEntity>();
            var celebs = tableClient.QueryAsync<FileEntity>("", 10);
            await foreach (var celeb in celebs)
            {
                modelList.Add(celeb);
            }
            return modelList;
        }

        // Not in db yet
        public async Task<IEnumerable<StandardVersionEntity>> GetAllStandardVersions()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_standardVersionsTableName);

            List<StandardVersionEntity> versionList = new List<StandardVersionEntity>();
            var versions = tableClient.QueryAsync<StandardVersionEntity>("", 10);

            await foreach (var version in versions.ConfigureAwait(false))
            {
                versionList.Add(version);
            }

            return versionList;
        }

        // Not in db yet
        public async Task<IEnumerable<IntegrationFilesEntity>> GetAllIntegrationFiles()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_integrationFilesTableName);

            List<IntegrationFilesEntity> integrationList = new List<IntegrationFilesEntity>();
            var integrations = tableClient.QueryAsync<IntegrationFilesEntity>("", 10);
            await foreach (var integration in integrations)
            {
                integrationList.Add(integration);
            }

            return integrationList;
        }

        // Not in db yet
        public async Task<IEnumerable<ConfigurationEntity>> GetAllConfigurations()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_configTableName);

            var configurations = tableClient.QueryAsync<ConfigurationEntity>();

            List<ConfigurationEntity> configList = new List<ConfigurationEntity>();
            await foreach (var config in configurations.ConfigureAwait(false))
            {
                configList.Add(config);
            }

            return configList;
        }

        // Not in db yet
        public async Task<IEnumerable<FileEntity>> GetAllStandardScenarios()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_scenariosTableName);

            IList<FileEntity> modelList = new List<FileEntity>();
            var celebs = tableClient.QueryAsync<FileEntity>("", 10);
            await foreach (var celeb in celebs)
            {
                modelList.Add(celeb);
            }
            return modelList;
        }

        // Not in db yet
        public async Task<IEnumerable<EarlyAccessVersionEntity>> GetAllEarlyAccessVersions()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_earlyAccessVersionsTableName);

            List<EarlyAccessVersionEntity> earlyAccessVersions = new List<EarlyAccessVersionEntity>();
            var versions = tableClient.QueryAsync<EarlyAccessVersionEntity>("", 10);
            await foreach (var version in versions)
            {
                earlyAccessVersions.Add(version);
            }

            return earlyAccessVersions;
        }

        // Not in db yet
        public async Task<IEnumerable<BetaVersionEntity>> GetAllBetaVersions()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient("todoBetaVersionName(ifNeeded)");

            List<BetaVersionEntity> betaVersions = new List<BetaVersionEntity>();

            var entities = tableClient.QueryAsync<BetaVersionEntity>();
            await foreach (var entity in entities)
            {
                betaVersions.Add(entity);
            }

            return betaVersions;
        }

        // Not in db yet
        public async Task<IEnumerable<CompanyEntity>> GetAllCompanies()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient("todoCompanyTableNameAddConst");

            List<CompanyEntity> companyEntities = new List<CompanyEntity>();

            var companies = tableClient.QueryAsync<CompanyEntity>("", 10);
            await foreach (var company in companies)
            {
                companyEntities.Add(company);
            }

            return companyEntities;
        }

        // Not in db yet
        public async Task<IEnumerable<CompanySpecificVersionEntity>> GetAllCompanySpecificVersions()
        {
            TableClient tableClient = m_tableServiceClient.GetTableClient(c_specificVersionsTable);

            List<CompanySpecificVersionEntity> companySpecificVersions = new List<CompanySpecificVersionEntity>();

            var versions = tableClient.QueryAsync<CompanySpecificVersionEntity>("", 10);
            await foreach (var version in versions)
            {
                companySpecificVersions.Add(version);
            }

            return companySpecificVersions;
        }
    }
}
