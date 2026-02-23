using PT.Logging.Entities;

namespace PT.Logging.Interfaces
{
    public interface IAuditLogger
    {
        public void EnsureAuditLogConfigured();

        public Task LogAuditAsync(AuditLog a_auditData);

        public Task<string> GetLogContentsAsync(GetLogsRequest a_logsRequest);
    }
}
