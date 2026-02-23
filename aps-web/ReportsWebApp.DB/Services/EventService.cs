using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.BryntumObjects;
using static ReportsWebApp.DB.Models.ResourceTimeRange;
using System;
using System.Collections.Generic;
using System.Linq;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services
{
    public class EventService : IEventService
    {
        public static int _lastEventId;
        public static int _lastMaterialId;

        public Event CreateEventFromDetail(DashtPlanning detail, List<Segment> segments)
        {
            _lastEventId++;
            return new Event
            {
                Id = _lastEventId,
                DashtPlanning = detail,
                OperationId = detail.Opid,
                Opname = detail.Opname,
                MoName = detail.Moname,
                ActivityExternalId = detail.ActivityExternalId,
                ActivityPercentFinished = detail.ActivityPercentFinished ?? 0,
                OPNeedDate = detail.OpneedDate ?? DateTime.MinValue,
                BlockLocked = detail.BlockLocked ?? false,
                OPDesc = detail.OPAttributesSummary,
                OPMaterialsList = detail.OPMaterialStatus,
                ActivityRequiredFinishQty = detail.ActivityRequiredFinishQty ?? 0,
                Customer = detail.Customer,
                LinkRelationType = detail.LinkRelationType,
                LinkDirection = detail.LinkDirection,
                SuccessorDependency = detail.SuccessorOPId ?? "",
                Name = detail.JobName,
                ResourceName = detail.BlockResource ?? "",
                ResourceId = Convert.ToInt32(detail.ResourceId),
                DepartmentId = Convert.ToInt32(detail.DepartmentId),
                PAScenarioId = detail.NewScenarioId,
                PlantId = Convert.ToInt32(detail.PlantId),
                BlockPlant = detail.BlockPlant ?? "",
                BlockDepartment = detail.BlockDepartment ?? "",
                BlockScenario = detail.ScenarioName ?? "",
                StartDate = detail.BlockScheduledStart ?? DateTime.MinValue,
                EndDate = detail.BlockScheduledEnd ?? DateTime.MinValue,
                BlockDurationHrs = Convert.ToDecimal(detail.BlockDurationHrs ?? 0),
                RenderSegments = segments
            };
        }
        public ResourceTimeRange CreateCapacityIntervalFromDetail(DashtCapacityPlanningShiftsCombined detail, string intervalText, string intervalTypeColor)
        {
            _lastEventId++;
            return new ResourceTimeRange(
                _lastEventId,
                Convert.ToInt32(detail.ResourceId),
                detail.StartDateTime ?? DateTime.MinValue,
                detail.EndDateTime ?? DateTime.MinValue,
                intervalText,
                intervalTypeColor
            );
        }

        public Material CreateMaterialFromDetail(Dasht_Materials detail, List<Event> events)
        {
            _lastMaterialId++;
            AssignDashtMaterialsReference(detail, events);
            Event supplier = FindEvent(events, detail.SupplyingJobName, detail.SupplyingMoname, detail.SupplyingOpname, detail.SupplyingActivityExternalId);
            Event receiver = FindEvent(events, detail.JobName, detail.Moname, detail.Opname, detail.ActivityExternalId);

            return new Material
            {
                Id = _lastMaterialId,
                Supplier = supplier,
                Receiver = receiver
            };
        }

        private void AssignDashtMaterialsReference(Dasht_Materials detail, List<Event> events)
        {
            events.ForEach(eventse =>
            {
                if (eventse.DashtPlanning.JobName == detail.JobName &&
                    eventse.DashtPlanning.Moname == detail.Moname &&
                    eventse.DashtPlanning.Opname == detail.Opname)
                {
                    eventse.DashtMaterials = detail;   
                }
            });
        }

        private Event FindEvent(List<Event> events, string jobName, string moName, string opName, string activityExternalId)
        {
            return events.FirstOrDefault(p =>
                p.DashtPlanning.JobName == jobName &&
                p.DashtPlanning.Moname == moName &&
                p.DashtPlanning.Opname == opName &&
                p.DashtPlanning.ActivityExternalId == activityExternalId);
        }
    }
}
