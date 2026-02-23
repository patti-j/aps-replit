namespace SchedulerDataModels
{
    public class Event
    {
        public long Id { get; set; }
        public int ResourceId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; } //ScheduledStartDateTime
        public DateTime EndDate { get; set; } //ScheduledEndDateTime 
        public List<Segment> RenderSegments { get; set; } = new List<Segment>();
        public List<Segment> OriginalSegments { get; set; } = new List<Segment>();
    }
}
