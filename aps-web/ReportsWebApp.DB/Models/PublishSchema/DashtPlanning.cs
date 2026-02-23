using System;

namespace ReportsWebApp.DB.Models
{
    /// <summary>
    /// Data model for the published planning data. Represents operational and job scheduling metadata.
    /// </summary>
    public class DashtPlanning
    {
        public string? JobName { get; set; }
        public string? JobId { get; set; }
        public string? Moname { get; set; }
        public string? Moid { get; set; }
        public string? Opname { get; set; }
        public string? LinkPredecessorOPId { get; set; }
        public string? Opid { get; set; }
        public string? LinkSuccessorOPId { get; set; }
        public string? ActivityExternalId { get; set; }
        public string? ActivityId { get; set; }
        public string? ActivityTiming { get; set; }
        public decimal? ActivityPercentFinished { get; set; }
        public int? BlockId { get; set; }
        public int? SuccessorBlockId { get; set; }
        public string? LinkRelationType { get; set; }
        public string? LinkDirection { get; set; }
        public DateTime? OpneedDate { get; set; }
        public decimal? OPRequiredFinishQty { get; set; }
        public DateTime? BlockScheduledStart { get; set; }
        public DateTime? BlockScheduledEnd { get; set; }
        public string? PlanningAreaName { get; set; }
        public string? BlockPlant { get; set; }
        public string? PlantId { get; set; }
        public string? BlockDepartment { get; set; }
        public string? DepartmentId { get; set; }
        public string? BlockResource { get; set; }
        public string? ResourceId { get; set; }
        public string? BlockWorkcenter { get; set; }
        public string? BlockResourceType { get; set; }
        public bool? BlockLocked { get; set; }
        public int? BlockSequence { get; set; }
        public int? BlockRunNbr { get; set; }
        public double? BlockDurationHrs { get; set; }
        public double? OPTrasnferHrs { get; set; }
        public decimal? BlockLaborCost { get; set; }
        public decimal? BlockMachineCost { get; set; }
        public bool? BlockScheduled { get; set; }
        public string? OPDesc { get; set; }
        public bool? OponHold { get; set; }
        public string? OPHoldReason { get; set; }
        public bool? OPLate { get; set; }
        public bool? OPBottleneck { get; set; }
        public string? OPBatchCode { get; set; }
        public string? OPProductsList { get; set; }
        public string? OPMaterialsList { get; set; }
        public string? OPMaterialStatus { get; set; }
        public DateTime? OPCommitStartDate { get; set; }
        public DateTime? OPCommitEndDate { get; set; }
        public DateTime? OPEndOfResourceTransferTimeDate { get; set; }
        public string? OPCurrentBufferWarningLevel { get; set; }
        public string? OPProjectedBufferWarningLevel { get; set; }
        public string? OPProductGroups { get; set; }
        public string? PredecessorOPId { get; set; }
        public string? SuccessorOPId { get; set; }
        public string? JobColorCode { get; set; }
        public string? MOProductColorCode { get; set; }
        public string? MOProduct { get; set; }
        public string? OPSetupColorCode { get; set; }
        public string? OPAttributesExternalIds { get; set; }
        public string? OPAttributesSummary { get; set; }
        public string? OPAttributesColorCodes { get; set; }
        public string? OPProductIds { get; set; }
        public string? OPLotCodes { get; set; }
        public DateTime? JobNeedDateTime { get; set; }
        public DateTime? JobScheduledStartDateTime { get; set; }
        public DateTime? JobScheduledEndDateTime { get; set; }
        public bool? JobScheduled { get; set; }
        public bool? JobDoNotSchedule { get; set; }
        public string? JobScheduledStatus { get; set; }
        public DateTime? JobEntryDate { get; set; }
        public bool? JobStarted { get; set; }
        public bool? JobFinished { get; set; }
        public bool? JobLate { get; set; }
        public decimal? JobLatenessDays { get; set; }
        public decimal? JobLatePenaltyCost { get; set; }
        public bool? JobOverdue { get; set; }
        public decimal? JobOverdueDays { get; set; }
        public decimal? JobQty { get; set; }
        public string? JobProduct { get; set; }
        public string? JobProductDescription { get; set; }
        public decimal? JobRevenue { get; set; }
        public decimal? JobTotalCost { get; set; }
        public string? JobCommitment { get; set; }
        public string? JobOnHold { get; set; }
        public string? JobHoldReason { get; set; }
        public double? JobThroughput { get; set; }
        public string? JobNotes { get; set; }
        public decimal? MORequiredQty { get; set; }
        public bool? MOScheduled { get; set; }
        public bool? MOLate { get; set; }
        public DateTime? ActivityReportedStartDate { get; set; }
        public DateTime? ActivityReportedFinishDate { get; set; }
        public decimal? ActivityReportedGoodQty { get; set; }
        public decimal? ActivityRequiredFinishQty { get; set; }
        public string? Customer { get; set; }
        public string? CustomerExternalId { get; set; }
        public int? Priority { get; set; }
        public string? OtherResourcesUsed { get; set; }
        public string? SetupCode { get; set; }
        public double? BlockSetupHours { get; set; }
        public double? BlockRunHours { get; set; }
        public double? BlockPostProcessingHours { get; set; }
        public double? OPStoragePostProcessingHours { get; set; }
        public DateTime? ActivityJitStartDate { get; set; }
        public double? ActivitySlackDays { get; set; }
        public string? BlockProductionStatus { get; set; }
        public DateTime? PublishDate { get; set; }
        public long? ScenarioId { get; set; }
        public string? NewScenarioId { get; set; }
        public string? ScenarioName { get; set; }
        public string? ScenarioType { get; set; }
    }

}