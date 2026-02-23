
using PT.APSCommon;

namespace PT.Scheduler.AuditObject
{
    public class ActivityMoveAudit : IScheduleAuditEntry
    {
        public string ActivityId { get; set; }
        public string OperationName { get; set; }
        public string ManufacturingOrderName { get; set; }
        public string JobName { get; set; }

        public string FromPlantName { get; set; }
        public string FromDepartmentName { get; set; }
        public string FromResourceName { get; set; }

        public string ToPlantName { get; set; }
        public string ToDepartmentName { get; set; }
        public string ToResourceName { get; set; }

        public DateTime MoveFromDate { get; set; }
        public DateTime MoveToDate { get; set; }

        public bool MovedFromDepartmentFrozenSpan { get; set; }
        public bool MovedToDepartmentFrozenSpan { get; set; }
        public bool MovedFromPlantStableSpan { get; set; }
        public bool MovedToPlantStableSpan { get; set; }
        public ActivityMoveAudit() {}

        public virtual IAuditEntry GetAuditEntry()
        {
            AuditEntry auditEntry = new AuditEntry();
            auditEntry.ObjectType = GetType().Name;


            auditEntry.AddManualChange("ActivityId", ActivityId, ActivityId);
            auditEntry.AddManualChange("OperationName", OperationName, OperationName);
            auditEntry.AddManualChange("ManufacturingOrderName", ManufacturingOrderName, ManufacturingOrderName);
            auditEntry.AddManualChange("JobName", JobName, JobName);
            auditEntry.AddManualChange("PlantName", FromPlantName, ToPlantName);
            auditEntry.AddManualChange("DepartmentName", FromDepartmentName, ToDepartmentName);
            auditEntry.AddManualChange("ResourceName", FromResourceName, ToResourceName);
            auditEntry.AddManualChange("ScheduledDate", MoveFromDate, MoveToDate);
        
            auditEntry.AddManualChange("MovedFromDepartmentFrozenSpan", null, MovedFromDepartmentFrozenSpan);
            auditEntry.AddManualChange("MovedToDepartmentFrozenSpan", null, MovedToDepartmentFrozenSpan);
            auditEntry.AddManualChange("MovedFromPlantStableSpan", null, MovedFromPlantStableSpan);
            auditEntry.AddManualChange("MovedToPlantStableSpan", null, MovedToPlantStableSpan);


            return auditEntry;
        }
    }

}
