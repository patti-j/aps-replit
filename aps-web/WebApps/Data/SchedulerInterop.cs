using Microsoft.JSInterop;
using ReportsWebApp.DB.Models;
using ReportsWebApp.Shared;
using ReportsWebApp.DB.Services;
using System.ComponentModel.Design;

namespace ReportsWebApp.Data
{
    public class SchedulerInterop
    {
        private IJSObjectReference _jsObjectReference;
        private IJSObjectReference _jsModule;
        private IJSObjectReference _jsSchedulerManager;
        private readonly IJSRuntime _jsRuntime;
        private readonly IAppInsightsLogger _logger;
        public string Id { get; private set; }

        public SchedulerInterop(IJSRuntime jsRuntime, string elementId, IAppInsightsLogger logger)
        {
            _jsRuntime = jsRuntime;
            Id = elementId;
            _logger = logger;
        }

        public async Task<string> InitializeAsync(object dotNetObjectReference, List<ShortcutSection> shortcutSections)
        {
            try
            {
                _jsObjectReference = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/scheduler.js");
                _jsSchedulerManager = await _jsObjectReference.InvokeAsync<IJSObjectReference>("createSchedulerManager");
                await _jsRuntime.InvokeVoidAsync("initKeydownListener", shortcutSections, dotNetObjectReference);
                return null;
            }
            catch (JSException jsEx)
            {
                _logger.LogError(jsEx);
                return jsEx.Message;
            }
        }

        public async Task<object> PrepareScheduler()
        {
            var result = await _jsSchedulerManager.InvokeAsync<object>("prepareScheduler");
            return ProcessResult(result);
        }

        public class TemplateInfo
        {
            public class SegmentTemplateInfo
            {
                public SegmentTemplateInfo(SegmentType segment)
                {
                    SegmentName = segment.SegmentTypeId.ToString();
                    Template = segment.Template;
                }
                public string SegmentName { get; set; }
                public string Template  { get; set; }
            }
            public string BlockTemplate { get; set; }
            public string ActivityLinkTemplate { get; set; }
            public string MaterialLinkTemplate { get; set; }
            public string CapacityTemplate { get; set; }
            public SegmentTemplateInfo[] Segments { get; set; }
        }
        public async Task<object> InitializeScheduler(BryntumProject a_bryntumProject, BryntumGridSettings? a_bryntumGridSettings = null, DotNetObjectReference<GanttNoteInteropContext> a_noteCtx = null)
        {
            try
            {
                await PrepareScheduler();

                TemplateInfo templateInfo = new TemplateInfo
                {
                    BlockTemplate = a_bryntumProject.Settings.TooltipDetailsTemplate,
                    ActivityLinkTemplate = a_bryntumProject.Settings.ActivityLinksLabelsTemplate,
                    MaterialLinkTemplate = a_bryntumProject.Settings.MaterialsLinksLabelsTemplate,
                    CapacityTemplate = a_bryntumProject.Settings.CapacityLabelsTemplate,
                    Segments = a_bryntumProject.Settings.SegmentTypes.Select(x => new TemplateInfo.SegmentTemplateInfo(x)).ToArray()
                };
                
                
                var result = await _jsSchedulerManager.InvokeAsync<object>("initializeScheduler", Id, 
                    a_bryntumProject, a_bryntumProject.Configuration.ToDictionary(), a_bryntumGridSettings, a_noteCtx, templateInfo);

                return ProcessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
        
        public async Task<BryntumGridSettings> CollectBryntumGridSettings()
        {
            try
            {
                return await _jsSchedulerManager.InvokeAsync<BryntumGridSettings>("collectBryntumSettings");
            }
            catch (Exception e)
            {
                //_logger.LogError(e);
                return new ();
            }
        }

        public async Task<object> ResizeRowsToFit()
        {
            try
            {
                var result = await _jsSchedulerManager.InvokeAsync<object>("uiManager.resizeRowsToFit");
                return ProcessResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; 
            }
        }

        public async Task<object> Empty()
        {
            return new object();
        }
        public async Task<object> SetFullScreen()
        {
            try
            {
                var result = await _jsSchedulerManager.InvokeAsync<object>("uiManager.setFullScreen");
                return ProcessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                // Log the exception or handle it as needed
                // For example, you can log the exception details:
                Console.WriteLine($"An error occurred: {ex.Message}");

                // You might want to return null or a default object in case of failure
                // Depending on your application's needs, you might also throw, or handle the exception differently
                return null; // or return a default/error-specific object
            }
        }
        public async Task ChangeBlockFontSize(int value)
        {
            await _jsSchedulerManager.InvokeVoidAsync("uiManager.changeBlockFontSize", value);
        }
        public async Task FilterEvents(string value)
        {
            await _jsSchedulerManager.InvokeVoidAsync("dataManager.filterEvents", value);
        }
        public async Task HighlightEvents(string value)
        {
            await _jsSchedulerManager.InvokeVoidAsync("uiManager.highlightEvents", value);
        }        

        public async Task IncreaseEventHeight()
        {
            await _jsSchedulerManager.InvokeVoidAsync("uiManager.increaseEventHeight");
        }

        public async Task DecreaseEventHeight()
        {
            await _jsSchedulerManager.InvokeVoidAsync("uiManager.decreaseEventHeight");
        }
        public async Task ZoomToTimeSpan(DateTime startDate, DateTime endDate, DateTime? centerDate = null)
        {
            await _jsSchedulerManager.InvokeVoidAsync("schedulerZoomManager.zoomToTimeSpan", new
            {
                startDate = startDate.ToString("s"),
                endDate = endDate.ToString("s"),
                centerDate = centerDate?.ToString("s")
            });
        }

        public async Task<object> ZoomIn()
        {
            try
            {
                var result = await _jsSchedulerManager.InvokeAsync<object>("schedulerZoomManager.zoomIn", 1);
                return ProcessResult(result);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                // For example, you can log the exception details:
                Console.WriteLine($"An error occurred: {ex.Message}");

                // You might want to return null or a default object in case of failure
                // Depending on your application's needs, you might also throw, or handle the exception differently
                return null; // or return a default/error-specific object
            }
        }

        public async Task<object> ZoomOut()
        {
            try
            {
                var result = await _jsSchedulerManager.InvokeAsync<object>("schedulerZoomManager.zoomOut", 1);
                return ProcessResult(result);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                // For example, you can log the exception details:
                Console.WriteLine($"An error occurred: {ex.Message}");

                // You might want to return null or a default object in case of failure
                // Depending on your application's needs, you might also throw, or handle the exception differently
                return null; // or return a default/error-specific object
            }
        }
        public async Task Shift(int amount, string unit)
        {
            await _jsSchedulerManager.InvokeVoidAsync("schedulerZoomManager.shift", new { amount, unit });
        }

        public async Task ShiftNext(int amount = 1)
        {
            await _jsSchedulerManager.InvokeVoidAsync("schedulerZoomManager.shiftNext", amount);
        }

        public async Task ShiftPrevious(int amount = 1)
        {
            await _jsSchedulerManager.InvokeVoidAsync("schedulerZoomManager.shiftPrevious", amount);
        }

        public async Task<int?> ZoomInFull(dynamic options = null)
        {
            return await _jsSchedulerManager.InvokeAsync<int?>("schedulerZoomManager.zoomInFull", options ?? new { });
        }
        public async Task<List<ColumnVisibility>> GetColumnVisibilityAsync()
        {
            try
            {
                var columns = await _jsSchedulerManager.InvokeAsync<List<ColumnVisibility>>("getColumnVisibility");
                return columns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get column visibility.");
                return new List<ColumnVisibility>(); // Return an empty list or handle accordingly
            }
        }

        public async Task<ZoomSnapshot> GetCurrentZoomSnapshot()
        {
            try
            {
                var snapshot = await _jsSchedulerManager.InvokeAsync<ZoomSnapshot>("schedulerZoomManager.getCurrentZoomSnapshot");
                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current zoom snapshot.");
                return new ZoomSnapshot
                {
                    StartDate = DateTime.Today.AddDays(-7),
                    EndDate = DateTime.Today.AddDays(7),
                    CenterDate = DateTime.Today
                };
            }
        }

        public async Task<int?> ZoomOutFull(dynamic options = null)
        {
            return await _jsSchedulerManager.InvokeAsync<int?>("schedulerZoomManager.zoomOutFull", options ?? new { });
        }
        private object ProcessResult(object result)
        {
            if (result is IDictionary<string, object> dict && dict.ContainsKey("error"))
            {
                return new { Error = dict["error"].ToString() };
            }
            return result;
        }        
    }
}
