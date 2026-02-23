using PT.APSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PT.Scheduler
{
    public class UnscheduleAudit : IScheduleAuditEntry
    {
        private readonly string m_jobName;
        private readonly DateTime m_scheduledStartDateTime;
        private readonly DateTime m_scheduledEndDateTime;
        private readonly bool m_scheduledInDepartmentFrozenSpan;
        private readonly bool m_scheduledInPlantStableSpan;

        private readonly Job m_updatedJob;
        public UnscheduleAudit(Job a_job)
        {
            m_jobName = a_job.Name;
            m_scheduledStartDateTime = a_job.ScheduledStartDate;
            m_scheduledEndDateTime = a_job.ScheduledEndDate;
            m_scheduledInDepartmentFrozenSpan = a_job.ScheduledInDepartmentFrozenSpan;
            m_scheduledInPlantStableSpan = a_job.ScheduledInPlantStableSpan;
            m_updatedJob = a_job;
        }
        public IAuditEntry GetAuditEntry()
        {
            AuditEntry auditEntry = new AuditEntry();
            auditEntry.ObjectType = GetType().Name;


            auditEntry.AddManualChange("JobName", m_jobName, m_updatedJob.Name);
            auditEntry.AddManualChange("FromScheduledStartDate", m_scheduledStartDateTime, m_updatedJob.ScheduledStartDate);
            auditEntry.AddManualChange("FromScheduledEndDate", m_scheduledEndDateTime, m_updatedJob.ScheduledEndDate);

            auditEntry.AddManualChange("FromDepartmentFrozenSpan", m_scheduledInDepartmentFrozenSpan, m_updatedJob.ScheduledInDepartmentFrozenSpan);
            auditEntry.AddManualChange("FromPlantStableSpan", m_scheduledInPlantStableSpan, m_updatedJob.ScheduledInPlantStableSpan);


            auditEntry.SkipAutomaticComparison();
            return auditEntry;
        }
    }
}
