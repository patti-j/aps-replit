using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services;

namespace ReportsWebApp.Resources.Shared
{
    public class VisibleEventProcessor
    {
        private readonly List<BryntumResource> _visibleResources;
        private readonly BryntumSettings _settings;
        private readonly Dictionary<SegmentTypeEnum, SegmentType> _segmentTypes;
        private readonly Dictionary<SegmentTypeEnum, SegmentConfig> _segmentConfigs;
        private readonly TemplateSegmentContext templateSegmentContext;
        private readonly BryntumProject _scheduler;
        private readonly List<Event> _visibleEvents;

        public VisibleEventProcessor(
            List<SegmentConfig> segmentConfigs,
            BryntumProject scheduler,
            TemplateSegmentContext templatesegmentContext)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            templateSegmentContext = templatesegmentContext ?? throw new ArgumentNullException(nameof(templatesegmentContext));

            if (scheduler.Resources == null)
                throw new ArgumentNullException(nameof(scheduler.Resources), "Scheduler.Resources cannot be null");

            if (scheduler.Events == null)
                throw new ArgumentNullException(nameof(scheduler.Events), "Scheduler.Events cannot be null");

            _settings = scheduler.Settings ?? throw new ArgumentNullException(nameof(scheduler.Settings));
            _segmentTypes = _settings.SegmentTypes?.ToDictionary(st => st.SegmentTypeId, st => st)
                             ?? throw new ArgumentNullException(nameof(_settings.SegmentTypes));

            _segmentConfigs = segmentConfigs?.ToDictionary(sc => sc.Id, sc => sc)
                              ?? throw new ArgumentNullException(nameof(segmentConfigs));

            _visibleResources = scheduler.Resources
                .Where(r => r.Visible && r.Enabled)
                .ToList();

            var visibleIds = new HashSet<int>(_visibleResources.Select(r => r.Id));
            _visibleEvents = scheduler.Events
                .Where(e => visibleIds.Contains(e.ResourceId))
                .ToList();
        }

        /// <summary>
        /// Processes visible resources and updates scheduler.Events and scheduler.Dependencies
        /// </summary>
        public void Process()
        {
            Parallel.ForEach(_visibleEvents, RecalculateSegmentHeights);

            _scheduler.Events = _visibleEvents;

            var generator = new DependencyGenerator(_visibleEvents, _settings, templateSegmentContext);
            _scheduler.Dependencies = generator.GenerateDependencies();
        }

        private void RecalculateSegmentHeights(Event e)
        {
            var totalWeight = e.RenderSegments
                .Sum(s => _segmentConfigs.TryGetValue(s.Id, out var config) ? config.Weight : 0);

            var heightUnit = totalWeight > 0 ? 100.0 / totalWeight : 0;

            foreach (var segment in e.RenderSegments)
            {
                SetSegmentHeight(segment, heightUnit);
            }
        }

        private void SetSegmentHeight(Segment segment, double heightUnit)
        {
            if (_segmentConfigs.TryGetValue(segment.Id, out var config) && segment.SegmentStyle != null)
            {
                segment.SegmentStyle.Height = (heightUnit * config.Weight).ToString("0.##") + "%";
            }
        }
    }
}