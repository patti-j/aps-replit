using Azure;
using Azure.Data.Tables;

namespace PT.ServerManagerSharedLib.Azure
{
    /// <summary>
    /// Not a generic client. This client hard codes the endpoints for PT
    /// </summary>
    public class AzureTableClient
    {
        private TableClient m_tableClient;
        private Uri m_azureStorageContainerUri;
        private const string c_workspaceTableName = "StandardWorkspacesTable";
        private const string c_scenariosTableName = "StandardScenariosTable";
        private const string c_standardVersionsTableName = "WebInstallerStandardVersionsTable";
        private const string c_integrationFilesTableName = "WebInstallerIntegrationsV12Table";
        private const string c_configTableName = "WebInstallerConfigTable";
        private const string c_earlyAccessVersionsTableName = "WebInstallerEarlyAccessVersionsTable";         
        private const string c_specificVersionsTable = "WebInstallerCompanySpecificVersionsTable";         

        public AzureTableClient()
        {
            m_azureStorageContainerUri = new Uri("https://ptwebhost.table.core.windows.net");
        }

        public async Task<IEnumerable<FileEntity>> GetAllStandardWorkspaces()
        {
            AzureSasCredential credential = new AzureSasCredential("?st=2023-03-28T14%3A32%3A14Z&se=2100-03-29T14%3A32%3A00Z&sp=r&sv=2018-03-28&tn=standardworkspacestable&sig=vbPXNZ3WK%2Fa5PXM1JQeoUu53%2Fx%2Bq3ZfRBqLN37pu%2Bh8%3D");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient(c_workspaceTableName);

            IList<FileEntity> modelList = new List<FileEntity>();
            var celebs = m_tableClient.QueryAsync<FileEntity>("", 10);
            await foreach (var celeb in celebs)
            {
                modelList.Add(celeb);
            }
            return modelList;
        }

        public async Task<IEnumerable<StandardVersionEntity>> GetAllStandardVersions()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2018-03-28&si=WI%20Read%20Policy&tn=webinstallerstandardversionstable&sig=3b56DZmiNIh1i8lY1tBTOtn4nBk4xw1EYxX6ku%2FAioY%3D");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient(c_standardVersionsTableName);

            List<StandardVersionEntity> versionList = new List<StandardVersionEntity>();
            var versions = m_tableClient.QueryAsync<StandardVersionEntity>("", 10);
            await foreach (var version in versions.ConfigureAwait(false))
            {
                versionList.Add(version);
            }

            return versionList;
        }

        public async Task<IEnumerable<IntegrationFilesEntity>> GetAllIntegrationFiles()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2019-02-02&st=2023-09-01T21%3A32%3A21Z&se=2033-09-02T21%3A32%3A00Z&sp=r&sig=NTE%2Ft5RsL1O6TNV4SZ9gi308FnF6qSs1xMLXhW2E51g%3D&tn=WebInstallerIntegrationsV12Table");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient(c_integrationFilesTableName);

            List<IntegrationFilesEntity> integrationList = new List<IntegrationFilesEntity>();
            var integrations = m_tableClient.QueryAsync<IntegrationFilesEntity>("", 10);
            await foreach (var integration in integrations)
            {
                integrationList.Add(integration);
            }

            return integrationList;
        }

        public async Task<IEnumerable<ConfigurationEntity>> GetAllConfigurations()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2018-03-28&si=WI%20ConfigTable%20Read%20Policy&tn=webinstallerconfigtable&sig=JktBmjvzKhBf9myCF5Vof3mAJ3k3mGGT9iEykpw%2Fx%2B8%3D");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient(c_configTableName);

            var configurations = m_tableClient.QueryAsync<ConfigurationEntity>();

            List<ConfigurationEntity> configList = new List<ConfigurationEntity>();
            await foreach (var config in configurations.ConfigureAwait(false))
            {
                configList.Add(config);
            }

            return configList;
        }



    public async Task<IEnumerable<FileEntity>> GetAllStandardScenarios()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2019-02-02&se=2100-05-14T03%3A49%3A00Z&sp=r&sig=6gXt5JVCNP2NK6fEvSLtbdpswfRdi4XqpruDg1ftm9U%3D&tn=StandardScenariosTable");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient(c_scenariosTableName);

            IList<FileEntity> modelList = new List<FileEntity>();
            var celebs = m_tableClient.QueryAsync<FileEntity>("", 10);
            await foreach (var celeb in celebs)
            {
                modelList.Add(celeb);
            }
            return modelList;
        }

        public async Task<IEnumerable<EarlyAccessVersionEntity>> GetAllEarlyAccessVersions()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2018-03-28&si=EarlyAccessVersionsTable%20Read%20Policy&tn=webinstallerearlyaccessversionstable&sig=gMo4pWiDR7Mrf3ropT5EsIQ%2B614uSV%2FtflZ6kyfy6dM%3D");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient(c_earlyAccessVersionsTableName);

            List<EarlyAccessVersionEntity> earlyAccessVersions = new List<EarlyAccessVersionEntity>();
            var versions = m_tableClient.QueryAsync<EarlyAccessVersionEntity>("", 10);
            await foreach (var version in versions)
            {
                earlyAccessVersions.Add(version);
            }

            return earlyAccessVersions;
        }
        public async Task<IEnumerable<BetaVersionEntity>> GetAllBetaVersions()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2018-03-28&si=BetaVersionsTable%20Read%20Policy&tn=webinstallerbetaversionstable&sig=9de%2B1lSIF9nt6fQ57aTg8LQfCPi3cASC%2FHafRNjxgz4%3D");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            TableClient tableClient = serviceClient.GetTableClient("WebInstallerBetaVersionsTable");

            List<BetaVersionEntity> betaVersions = new List<BetaVersionEntity>();

            var entities = tableClient.QueryAsync<BetaVersionEntity>();
            await foreach (var entity in entities)
            {
                betaVersions.Add(entity);
            }

            return betaVersions;
        }
        public async Task<IEnumerable<CompanyEntity>> GetAllCompanies()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2018-03-28&si=CompaniesTable%20Read%20Policy&tn=webinstallercompaniestable&sig=0MaxdbmqgPYvxPncecaPOsG3u4%2FLNKSazOcyeagQ8w8%3D");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient("WebInstallerCompaniesTable");

            List <CompanyEntity> companyEntities = new List<CompanyEntity>();

            var companies = m_tableClient.QueryAsync<CompanyEntity>("", 10);
            await foreach (var company in companies)
            {
                companyEntities.Add(company);
            }

            return companyEntities;
        }
        public async Task<IEnumerable<CompanySpecificVersionEntity>> GetAllCompanySpecificVersions()
        {
            AzureSasCredential credential = new AzureSasCredential("?sv=2019-02-02&st=2023-06-01T17%3A01%3A00Z&se=2099-06-03T17%3A01%3A00Z&sp=r&sig=gWC%2BkjNwV2%2BsHcO7BZUNpBrBaX9Bz3eTmKbpH3c5IqQ%3D&tn=WebInstallerCompanySpecificVersionsTable");
            TableServiceClient serviceClient = new TableServiceClient(m_azureStorageContainerUri, credential);
            m_tableClient = serviceClient.GetTableClient(c_specificVersionsTable);

            List<CompanySpecificVersionEntity> companySpecificVersions = new List<CompanySpecificVersionEntity>();

            var versions = m_tableClient.QueryAsync<CompanySpecificVersionEntity>("", 10);
            await foreach (var version in versions)
            {
                companySpecificVersions.Add(version);
            }

            return companySpecificVersions;
        }
        
    }
}
