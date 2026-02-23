using ReportsWebApp.DB.Models.BryntumObjects;
using System.Globalization;

namespace ReportsWebApp.DB.Models;

public class SchedulerConfiguration
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool InfiniteScroll { get; set; } = false;
    public bool RowResize { get; set; } = true;
    public bool FilterBar { get; set; } = true;
    public bool Stripe { get; set; } = false;
    public bool TimeRanges { get; set; } = true;
    public bool ScheduleTooltip { get; set; } = false;
    public bool DeselectOnClick { get; set; } = true;
    public List<object> Columns { get; set; } = new();
    public bool ShowActivityTitle { get; set; } = false;
    public double AverageTaskDuration { get; set; }
    public int RowHeight { get; set; }

    public EventTooltipConfiguration EventTooltipConfig { get; set; } = new();

    public Dictionary<string, object> ToDictionary() => new Dictionary<string, object>
    {
        ["startDate"] = StartDate.ToString("s"),
        ["endDate"] = EndDate.ToString("s"),
        ["infiniteScroll"] = InfiniteScroll,
        ["deselectOnClick"] = DeselectOnClick,
        ["columns"] = Columns,
        ["rowHeight"] = RowHeight,
        ["features"] = GetFeatureDictionary(),
        ["barMargin"] = 4
    };

    private Dictionary<string, object> GetFeatureDictionary() => new()
    {
        ["rowResize"] = RowResize,
        ["filterBar"] = FilterBar,
        ["stripe"] = Stripe,
        ["timeRanges"] = TimeRanges,
        ["resourceTimeRanges"] = TimeRanges,
        ["scheduleTooltip"] = ScheduleTooltip,
        ["eventTooltip"] = (EventTooltipConfig.Disabled = !ShowActivityTitle) ? EventTooltipConfig.ToDictionary() : EventTooltipConfig.ToDictionary(),
        ["dependencies"] = new Dictionary<string, object>
        {
            ["highlightDependenciesOnEventHover"] = true,
            ["radius"] = 3,
            ["markerDef"] = "M3,0 L8,3 L3,6"
        }
    };

    public List<DurationOption> GetDurationOptions() => new List<DurationOption>
    {
        new("Shift", 1),
        new("Double Shift", 2),
        new("Workweek", 5),
        new("Weekly", 7),
        new("Biweekly", 14),
        new("Monthly", 30),
        new("Bimonthly", 60),
        new("Quarterly", 90),
        new("Quadrimester", 120),
        new("Semester", 180),
        new("Annual", 365)
    }.Select(o => { o.IsSelected = Math.Abs(o.Days - AverageTaskDuration) < 2; return o; }).ToList();
}

public class DurationOption
{
    public string Name { get; set; }
    public int Days { get; set; }
    public bool IsSelected { get; set; }
    public DurationOption(string a_name, int a_days) => (Name, Days) = (a_name, a_days);
}

public class ZoomSnapshot
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CenterDate { get; set; }
}