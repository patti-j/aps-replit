namespace PT.UIDefinitions.Interfaces;

public interface IExceptionManager
{
    void UnhandledException(Exception a_e);

    void LogSimpleException(Exception a_e);

    void LogException(Exception a_e, bool a_showMessage);
    void LogNotification(string a_message, string a_info);
    void ClearNotificationsLog();
    string GetLoggedErrorMessage(Guid a_transmissionId);
}