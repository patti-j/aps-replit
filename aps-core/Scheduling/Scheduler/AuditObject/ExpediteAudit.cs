using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.APSCommon;

namespace PT.Scheduler.AuditObject;
public class ExpediteAudit : IScheduleAuditEntry
{
    private string JobName { get; set; }

    private string ManufacturingOrderName { get; set; }

    public DateTime FromStartDate { get; set; }

    public DateTime ToStartDate { get; set; }

    public bool ExpediteStatus { get; set; }

    private bool FromDepartmentFrozenSpan { get; set; }

    public bool ToDepartmentFrozenSpan { get; set; }

    private bool FromPlantStableSpan { get; set; }

    public bool ToPlantStableSpan { get; set; }

    public ExpediteAudit(ManufacturingOrder a_mo, DateTime a_clockDate)
    {
        JobName = a_mo.Job.Name;
        ManufacturingOrderName = a_mo.Name;
        FromStartDate = a_mo.ScheduledStartDate;
        ResourceOperation leadOperation = a_mo.GetLeadOperation() as ResourceOperation;
        if (leadOperation != null)
        {
            Resource scheduledPrimaryResource = leadOperation.GetScheduledPrimaryResource();
            FromDepartmentFrozenSpan = a_clockDate + scheduledPrimaryResource.Department.FrozenSpan > FromStartDate;
            FromPlantStableSpan = a_clockDate + scheduledPrimaryResource.Department.Plant.StableSpan > FromStartDate;
        }

    }

    public IAuditEntry GetAuditEntry()
    {
        AuditEntry auditEntry = new AuditEntry();
        auditEntry.ObjectType = nameof(ExpediteAudit);

        auditEntry.AddManualChange("JobName", JobName, JobName);
        auditEntry.AddManualChange("ManufacturingOrderName", ManufacturingOrderName, ManufacturingOrderName);
        auditEntry.AddManualChange("ExpediteStatus", ExpediteStatus, ExpediteStatus);

        auditEntry.AddManualChange("FromStartDate", FromStartDate, FromStartDate);
        auditEntry.AddManualChange("FromDepartmentFrozenSpan", FromDepartmentFrozenSpan, FromDepartmentFrozenSpan);
        auditEntry.AddManualChange("FromPlantStableSpan", FromPlantStableSpan, FromPlantStableSpan);

        auditEntry.AddManualChange("ToStartDate", ToStartDate, ToStartDate);
        auditEntry.AddManualChange("ToDepartmentFrozenSpan", ToDepartmentFrozenSpan, ToDepartmentFrozenSpan);
        auditEntry.AddManualChange("ToPlantStableSpan", ToPlantStableSpan, ToPlantStableSpan);

        auditEntry.SkipAutomaticComparison();
        return auditEntry;
    }
}

