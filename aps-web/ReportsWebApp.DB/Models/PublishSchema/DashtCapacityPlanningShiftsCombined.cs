using System;
using System.Collections.Generic;

namespace ReportsWebApp.DB.Models;

    public class DashtCapacityPlanningShiftsCombined
    {
        public string? PlanningAreaName { get; set; }
        public string? PlanningAreaId { get; set; }
        public string? PlantName { get; set; }
        public string? PlantId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentId { get; set; }
        public string? Workcenter { get; set; }
        public string? ResourceName { get; set; }
        public string? ResourceId { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? IntervalName { get; set; }
        public string? IntervalId { get; set; }
        public string? IntervalType { get; set; }
        public double? NbrOfPeople { get; set; }
        public string? CapacityType { get; set; }
        public bool? Active { get; set; }
        public string? ResourceType { get; set; }
        public bool? ResourceBottleneck { get; set; }
        public bool? ResourceDrum { get; set; }
        public DateTime? PublishDate { get; set; }
        public string? ScenarioName { get; set; }
        public string? ScenarioType { get; set; }
        public bool? LastLiveSchedulePublished { get; set; }
    }

