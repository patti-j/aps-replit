using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Scheduler.AuditObject
{
    public class ActivityMoveFailedAudit : ActivityMoveAudit
    {
        public string FailedReason { get; set; }

        public override AuditEntry GetAuditEntry()
        {
            AuditEntry auditEntry = base.GetAuditEntry() as AuditEntry ?? new AuditEntry();
            auditEntry.ObjectType = nameof(ActivityMoveFailedAudit);

            auditEntry.AddManualChange("MovedFailed", null, true);
            auditEntry.AddManualChange("FailedReason", null, FailedReason);

            return auditEntry;
        }
    }
}
