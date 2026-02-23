using ReportsWebApp.DB.Services;
using System;

namespace ReportsWebApp.DB.Models
{
    public class BryntumProject 
    {
        public Guid id { get; set; }
        public string FavoriteName { get; set; }
        public string ScenarioId { get; set; }
        public SchedulerConfiguration Configuration { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
        public List<ResourceTimeRange> Concurrencies { get; set; } = new List<ResourceTimeRange>();
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
        public List<BryntumResource> Resources { get; set; } = new List<BryntumResource>();
        public BryntumSettings Settings { get; set; } = new BryntumSettings();
        public List<TreeNode> HierarchyFilterNodes { get; set; } = new List<TreeNode>();
        public List<ColumnVisibility> ColumnVisibilities { get; set; } = new List<ColumnVisibility>();

        public BryntumProject()
        {
            
        }

        public BryntumProject(Guid id, string favoriteName, SchedulerConfiguration presetConfig,
                                 List<Event> events, List<BryntumResource> resources,
                                 List<ResourceTimeRange> resourceTimeRanges,
                                 BryntumSettings settings, List<TreeNode> hierarchyFilterNodes)
        {
            this.id = id;
            this.FavoriteName = favoriteName;
            this.Configuration = presetConfig;
            this.Concurrencies = resourceTimeRanges;
            this.Events = events ?? new List<Event>(); // Ensure the list is initialized if null is passed
            this.Resources = resources ?? new List<BryntumResource>(); // Ensure the list is initialized if null is passed
            this.Settings = settings ?? new BryntumSettings(); // Ensure the list is initialized if null is passed
            this.HierarchyFilterNodes = hierarchyFilterNodes ?? new List<TreeNode>(); // Ensure the list is initialized if null is passed
        }
    }
    public class ColumnVisibility
    {
        public string Id { get; set; }
        public bool Hidden { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is ColumnVisibility visibility)
            {
                return this.Id == visibility.Id && this.Hidden == visibility.Hidden;
            }
            return base.Equals(obj);
        }
    }

    /// <summary>
    /// Represents a data point used for Gantt chart zooming visualization. 
    /// Each point has an argument value (typically time) and up to three Y-series values.
    /// </summary>
    public class ZoomingData
    {
        /// <summary>
        /// Gets or sets the X-axis value, typically representing a timestamp or sequential index.
        /// </summary>
        public DateTime Arg { get; set; }

        /// <summary>
        /// Gets or sets the first Y-series value, used for Line/Area chart rendering.
        /// </summary>
        public double Y1 { get; set; }

        /// <summary>
        /// Gets or sets the second Y-series value (optional).
        /// </summary>
        public double Y2 { get; set; }

        /// <summary>
        /// Gets or sets the third Y-series value (optional).
        /// </summary>
        public double Y3 { get; set; }
    }

}
