using PT.Logging.Entities;

namespace PT.Logging.Interfaces
{
    public interface IErrorLogger
    {
        public void EnsureErrorLogConfigured();

        public Task LogExceptionAsync(ErrorLog a_ex);

        public Task LogExceptionsAsync(List<ErrorLog> a_errs);

        public Task<string> GetLogContentsAsync(GetLogsRequest a_logsRequest);
    }
}
