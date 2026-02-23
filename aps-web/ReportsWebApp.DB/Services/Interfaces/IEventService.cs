using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.BryntumObjects;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IEventService
{
    Event CreateEventFromDetail(DashtPlanning detail, List<Segment> segments);
    ResourceTimeRange CreateCapacityIntervalFromDetail(DashtCapacityPlanningShiftsCombined detail, string intervalText, string intervalTypeColor);
    Material CreateMaterialFromDetail(Dasht_Materials detail, List<Event> events);
}