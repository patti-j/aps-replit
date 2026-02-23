using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ReportsWebApp.Resources.Shared;
using System.Threading.Tasks;

namespace ReportsWebApp.Data
{
    public class Interop
    {
        internal static ValueTask<object> ShowReport(
            IJSRuntime jsRuntime,
            ElementReference reportContainer,
            string accessToken,
            string embedUrl,
            string embedReportId,
            string pageName,
            DotNetObjectReference<ReportDisplay> reference,
            bool isPaginated)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.showReport",
                reportContainer, accessToken, embedUrl,
                embedReportId, pageName, reference, isPaginated);
        }

        internal static ValueTask SwitchReportPage(IJSRuntime jsRuntime, string embedReportId, string pageName)
        {
            return jsRuntime.InvokeVoidAsync("PowerBI.switchReportPage", embedReportId, pageName);
        }

        internal static ValueTask<object> EditReport(
            IJSRuntime jsRuntime,
            ElementReference reportContainer,
            string accessToken,
            string embedUrl,
            string embedReportId)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.editReport",
                reportContainer, accessToken, embedUrl,
                embedReportId);
        }
        internal static ValueTask<object> PrintReport(
            IJSRuntime jsRuntime,
            ElementReference reportContainer,
            string embedReportId)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.printReport",
                reportContainer,
                embedReportId);
        }
        internal static ValueTask<object> FullScreenReport(
            IJSRuntime jsRuntime,
            ElementReference reportContainer,
            string embedReportId)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.fullScreenReport",
                reportContainer,
                embedReportId);
        }
        internal static ValueTask<object> ReloadReport(
            IJSRuntime jsRuntime,
            ElementReference reportContainer,
            string embedReportId)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.reloadReport",
                reportContainer,
                embedReportId);
        }
        internal static ValueTask<object> RefreshReport(
            IJSRuntime jsRuntime,
            ElementReference reportContainer,
            string embedReportId)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.refreshReport",
                reportContainer,
                embedReportId);
        }
        internal static ValueTask<object> CreateReport(
            IJSRuntime jsRuntime,
            ElementReference reportContainer,
            string accessToken,
            string embedUrl,
            string datasetId)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.createReport",
                reportContainer, accessToken, embedUrl,
                datasetId);
        }

        internal static ValueTask<object> ShowDashboard(IJSRuntime jsRuntime, ElementReference dashboardContainer, string accessToken, string dashboardEmbedUrl, string dashboardId)
        {
            return jsRuntime.InvokeAsync<object>(
                "PowerBI.showDashboard",
                dashboardContainer, accessToken, dashboardEmbedUrl,
                dashboardId);
        }
    }
}