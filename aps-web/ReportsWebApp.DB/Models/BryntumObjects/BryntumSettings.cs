using System.Diagnostics;

using Newtonsoft.Json;

using ReportsWebApp.DB.Services;

namespace ReportsWebApp.DB.Models
{
    public enum PenetrationType
    {
        CurrentPenetration,
        ProjectedPenetration
    }
    
    public class BryntumGridSettings
    {
        public bool hidden { get; set; }
        public List<ColumnVisibility> columnVisibilities { get; set; } 
        public List<Sorter> sorters { get; set; }
        public Grouper? grouper { get; set; }
        public DateRange? dateRange { get; set; }
        public int? zoomLevel { get; set; }
        public string? filterText { get; set; }
        public override bool Equals(object? obj)
        {
            if (obj is BryntumGridSettings other)
            {
                if (other.columnVisibilities.SequenceEqual(this.columnVisibilities) &&
                    other.sorters.SequenceEqual(this.sorters) &&
                    ((other.grouper == this.grouper) || (other.grouper?.Equals(this.grouper) ?? false)) &&
                    other.hidden.Equals(this.hidden) &&
                    (other.dateRange?.Equals(this.dateRange) ?? other == this) &&
                    other.zoomLevel == this.zoomLevel &&
                    other.filterText == this.filterText)
                {
                    return true;
                }
            }
            
            return base.Equals(obj);
        }
    }

    public class DateRange
    {
        public long startMs { get; set; }
        public long endMs { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is DateRange other)
            {
                if (other.startMs.Equals(this.startMs) && other.endMs.Equals(this.endMs))
                {
                    return true;
                }
            }
            return base.Equals(obj);
        }
    }
    
    public class Sorter
    {
        public string field { get; set; }
        public bool ascending { get; set; }
        
        public override bool Equals(object? obj)
        {
            if (obj is Sorter other)
            {
                if (other.field.Equals(this.field)
                    && other.ascending.Equals(this.ascending))
                {
                    return true;
                }
            }
            return base.Equals(obj);
        }
    }
        
    public class Grouper
    {
        public string field { get; set; }
        public bool ascending { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Grouper other)
            {
                if (other.field.Equals(this.field)
                    && other.ascending.Equals(this.ascending))
                {
                    return true;
                }
            }
            return base.Equals(obj);
        }
    }

    public class BryntumSettings
    {
        [System.Text.Json.Serialization.JsonConstructor]
        [JsonConstructor]
        public BryntumSettings()
        {
            
        }
        
        public BryntumSettings(bool init = true)
        {
            if (init)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            SegmentTypes = new List<SegmentType>();
            SegmentTypes = SegmentUtils.SegmentTypes;
            ShowActivityTitle = false;
            ShowEventTooltips = false;
            ShowCapacityLabels = true;
            IntervalColors = new Dictionary<int, IntervalColor>
                {
                    { 1, new IntervalColor { Id = 1, Type = "Online", Color = "#7cbf00" } }, // Green
                    { 2, new IntervalColor { Id = 2, Type = "Offline", Color = "#d3d3d3" } },      // Light Gray
                    { 3, new IntervalColor { Id = 3, Type = "Overtime", Color = "#7e0044" } },     // Dark Red
                    { 4, new IntervalColor { Id = 4, Type = "PotentialOvertime", Color = "#e4acb8" } }, // Light Pink
                    { 5, new IntervalColor { Id = 5, Type = "Cleanout", Color = "#147bd1" } }      // Blue
                };
            TooltipDetailsTemplate = "{fa-th-large} Job Name {{DashtPlanning.JobName}} \r\n{fa-calendar-o} Operation's End DateTime {{DashtPlanning.BlockScheduledEnd}} ";
            CapacityLabelsTemplate = "{fa-cog} {{DashtCapacityPlanningShiftsCombined.IntervalName}}";
            ActivityLinksLabelsTemplate = "<p>{{DashtPlanning.JobName}}</p><p> <span style=\"color: red;\">FROM: </span> {{ DashtPlanning.Opname }}</p><p><span style=\"color: rgb(55, 054, 55);\" > TO:</span> {{ DashtPlanningTo.Opname }}</p> ";
            MaterialsLinksLabelsTemplate = "FROM: {{Dasht_Materials.SupplyingJobName}} / {{Dasht_Materials.SupplyingMoname}} / {{Dasht_Materials.SupplyingOpname}} / {{Dasht_Materials.SupplyingActivityExternalId}} " +
                                           "TO: {{Dasht_Materials.JobName}} / {{Dasht_Materials.Moname}} / {{Dasht_Materials.Opname}} / {{Dasht_Materials.ActivityExternalId}}";
            PenetrationType = PenetrationType.CurrentPenetration;
            LinkColor = "#00FF00";
            MaterialColor = "#0000FF";
        }

        public List<SegmentType> SegmentTypes { get; set; }
        public bool ShowActivityTitle { get; set; }
        public bool ShowEventTooltips { get; set; }
        public bool ShowCapacityLabels { get; set; }
        public Dictionary<int, IntervalColor> IntervalColors { get; set; }
        public string TooltipDetailsTemplate { get; set; }
        public string CapacityLabelsTemplate { get; set; }
        public string ActivityLinksLabelsTemplate { get; set; }
        public string MaterialsLinksLabelsTemplate { get; set; }
        public PenetrationType PenetrationType { get; set; }
        public string LinkColor { get; set; }
        public string MaterialColor { get; set; }
        public int HourInterval { get; set; } = 6;
    }
}
