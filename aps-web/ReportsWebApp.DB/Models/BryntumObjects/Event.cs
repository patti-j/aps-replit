using System.Numerics;

namespace ReportsWebApp.DB.Models
{
    public class Event
    {
        public long Id { get; set; }
        public bool ManuallyScheduled { get; set; } = true;        
        public virtual DashtPlanning DashtPlanning { get; set; }
        public virtual Dasht_Materials DashtMaterials { get; set; }
        public string OperationId { get; set; }
        public string Opname { get; set; }
        public string MoName { get; set; }
        public string ActivityExternalId { get; set; }
        public decimal ActivityPercentFinished { get; set; }
        public DateTime OPNeedDate { get; set; } // Scheduled start date and time
        public bool BlockLocked { get; set; } // Scheduled end date and time
        public string OPDesc { get; set; }
        public object OPMaterialsList { get; set; }
        public decimal ActivityRequiredFinishQty { get; set; }
        public string? Customer { get; set; }
        public string? SuccessorDependency { get; set; }
        public List<long> DependenciesAsSuccessor { get; set; } = new List<long>();
        public List<long> MaterialsAsSuccessor { get; set; } = new List<long>();
        public List<long> MaterialsAsPredecessor { get; set; } = new List<long>();
        public int ResourceId { get; set; }
        public EventStyle EventStyleEnum { get; set; }
        public string EventStyle { get { return EventStyleEnum.ToString().ToLower(); } }
        public string? ResourceName { get; set; }
        public int PlantId { get; set; }
        public int DepartmentId { get; set; }
        public string PAScenarioId { get; set; }
        public string? BlockPlant { get; set; }
        public string? BlockDepartment { get; set; }
        public string? BlockScenario { get; set; }
        public string? TooltipHTML { get; set; }
        public string? Resource { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; } // Scheduled start date and time
        public DateTime EndDate { get; set; } // Scheduled end date and time
        public decimal BlockDurationHrs { get; set; }
        public List<Segment> RenderSegments { get; set; } = new List<Segment>();
        public string? LinkRelationType { get; internal set; }
        public string? LinkDirection { get; internal set; }
    }

    public enum EventStyle
    {
        Plain,
        Border,
        Colored,
        Hollow,
        Line,
        Dashed,
        Minimal,
        Rounded
    }

    public static class EventStyleExtensions
    {
        public static string GetStringName(this EventStyle eventStyle)
        {
            return eventStyle.ToString().ToLower();
        }
    }
}
