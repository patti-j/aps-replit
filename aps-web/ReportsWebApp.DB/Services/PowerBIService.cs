using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services
{
    public class PowerBISettings
    {
        public string TenantId { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
    }
    public class DropdownOptions
    {
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
    }

    public class DTOWorkspaces
    {
        public string Id { get; set; }
        public string WorkspaceName { get; set; }
        public string Type { get; set; }
    }

    public class DTOReports
    {
        public string Id { get; set; }
        public string Reportname { get; set; }
    }

    public class DTODatasets
    {
        public string Id { get; set; }
        public string Datasetname { get; set; }
        public string CreateReportEmbedURL { get; set; }
    }

    public class GetWorkspacesResults
    {
        public List<DTOWorkspaces> workspacesList;
        public int totalRows;
    }

    public class GetReportsResults
    {
        public List<DTOReports> reportsList;
        public int totalRows;
    }

    public class GetDatasetsResults
    {
        public List<DTODatasets> datasetsList;
        public int totalRows;
    }
    public class PowerBIService
    {
        AuthenticationStateProvider _AuthenticationStateProvider;
        PowerBISettings _PowerBISettings;

        public PowerBIService(
            AuthenticationStateProvider AuthenticationStateProvider,
            PowerBISettings PowerBISettings)
        {
            _AuthenticationStateProvider = AuthenticationStateProvider;
            _PowerBISettings = PowerBISettings;
        }

        private const string AuthorityFormat = "https://login.microsoftonline.com/{0}/v2.0";
        private const string MSGraphScope = "https://analysis.windows.net/powerbi/api/.default";

        public string GetPowerBIAccessToken()
        {
            var TenantId = _PowerBISettings.TenantId;

            IConfidentialClientApplication daemonClient;

            daemonClient = ConfidentialClientApplicationBuilder.Create(_PowerBISettings.ApplicationId)
                .WithAuthority(string.Format(AuthorityFormat, _PowerBISettings.TenantId))
                .WithClientSecret(_PowerBISettings.ApplicationSecret)
                .Build();

            AuthenticationResult authResult =
                 daemonClient.AcquireTokenForClient(new[] { MSGraphScope }).ExecuteAsync().Result;

            return authResult.AccessToken;
        }

        public async Task<string> GetPowerBIAccessTokenAsync()
        {
            var TenantId = _PowerBISettings.TenantId;

            IConfidentialClientApplication daemonClient;

            daemonClient = ConfidentialClientApplicationBuilder.Create(_PowerBISettings.ApplicationId)
                .WithAuthority(string.Format(AuthorityFormat, _PowerBISettings.TenantId))
                .WithClientSecret(_PowerBISettings.ApplicationSecret)
                .Build();

            AuthenticationResult authResult =
                await daemonClient.AcquireTokenForClient(new[] { MSGraphScope }).ExecuteAsync();

            return authResult.AccessToken;
        }

        public async Task<GetWorkspacesResults> GetWorkspacesAsync()
        {
            GetWorkspacesResults groupsResults = new GetWorkspacesResults();
            groupsResults.totalRows = 0;

            List<DTOWorkspaces> colWorkspaces = new List<DTOWorkspaces>();

            string accessToken = GetPowerBIAccessToken();
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

            using (var client = new PowerBIClient(
                new Uri("https://api.powerbi.com/"), tokenCredentials))
            {
                Groups groups = await client.Groups.GetGroupsAsync();

                if (groups.Value.Count() > 0)
                {
                    foreach (Microsoft.PowerBI.Api.Models.Group workspace in groups.Value)
                    {
                        DTOWorkspaces objWorkspace = new DTOWorkspaces();

                        objWorkspace.Id = workspace.Id.ToString();
                        objWorkspace.WorkspaceName = workspace.Name;
                        //objWorkspace.Type = workspace.Type;

                        colWorkspaces.Add(objWorkspace);
                    }
                }

                colWorkspaces = colWorkspaces.OrderBy(x => x.WorkspaceName).ToList();
                groupsResults.workspacesList = colWorkspaces;
                groupsResults.totalRows = colWorkspaces.Count();
            }

            return groupsResults;
        }

        public async Task<GetReportsResults> GetReportsAsync(string workspaceID)
        {
            GetReportsResults reportsResults = new GetReportsResults();
            reportsResults.totalRows = 0;

            List<DTOReports> colReports = new List<DTOReports>();

            string accessToken = GetPowerBIAccessToken();
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

            using (var client = new PowerBIClient(
                new Uri("https://api.powerbi.com/"), tokenCredentials))
            {
                Reports reports = await client.Reports.GetReportsInGroupAsync(new Guid(workspaceID));

                if (reports.Value.Count() > 0)
                {
                    foreach (Microsoft.PowerBI.Api.Models.Report report in reports.Value)
                    {
                        if (report.ReportType == "PowerBIReport")
                        {
                            DTOReports objReport = new DTOReports();

                            objReport.Id = report.Id.ToString();
                            objReport.Reportname = report.Name;

                            colReports.Add(objReport);
                        }
                        else if (report.ReportType == "PaginatedReport")
                        {
                            DTOReports objReport = new DTOReports();

                            objReport.Id = report.Id.ToString();
                            objReport.Reportname = report.Name;

                            colReports.Add(objReport);
                        }
                    }
                }
                colReports = colReports.OrderBy(x => x.Reportname).ToList();
                reportsResults.reportsList = colReports;
                reportsResults.totalRows = colReports.Count();
            }

            return reportsResults;
        }

        public async Task<GetDatasetsResults> GetDatasetsAsync(string workspaceID)
        {
            GetDatasetsResults datasetsResults = new GetDatasetsResults();
            datasetsResults.totalRows = 0;

            List<DTODatasets> colDatasets = new List<DTODatasets>();

            string accessToken = GetPowerBIAccessToken();
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

            using (var client = new PowerBIClient(
                new Uri("https://api.powerbi.com/"), tokenCredentials))
            {
                Datasets datasets = await client.Datasets.GetDatasetsInGroupAsync(new Guid(workspaceID));

                if (datasets.Value.Count() > 0)
                {
                    foreach (Dataset dataset in datasets.Value)
                    {
                        DTODatasets objDataset = new DTODatasets();

                        objDataset.Id = dataset.Id;
                        objDataset.Datasetname = dataset.Name;
                        objDataset.CreateReportEmbedURL = dataset.CreateReportEmbedURL;

                        colDatasets.Add(objDataset);
                    }
                }

                colDatasets = colDatasets.OrderBy(x => x.Datasetname).ToList();
                datasetsResults.datasetsList = colDatasets;
                datasetsResults.totalRows = colDatasets.Count();
            }

            return datasetsResults;
        }

        public Dashboard GetDashboard(string workspaceID)
        {
            Dashboard dashboard;
            string accessToken = GetPowerBIAccessToken();
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

            using (var client = new PowerBIClient(
                       new Uri("https://api.powerbi.com/"), tokenCredentials))
            {
                var dashboards =  client.Dashboards.GetDashboards(new Guid(workspaceID));

                if (dashboards.Value.Count() > 0)
                {
                    dashboard = dashboards.Value.FirstOrDefault();
                }
                else dashboard = null;
            }

            return dashboard;
        }
        public List<Dashboard> GetDashboards(string workspaceID)
        {
            Dashboards dashboards;
            string accessToken = GetPowerBIAccessToken();
            var tokenCredentials = new TokenCredentials(accessToken, "Bearer");

            using (var client = new PowerBIClient(
                       new Uri("https://api.powerbi.com/"), tokenCredentials))
            {
                dashboards = client.Dashboards.GetDashboards(new Guid(workspaceID));
            }

            return dashboards.Value.ToList();
        }
    }
}
