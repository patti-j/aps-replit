namespace PlanetTogetherExtraServicesDefinitions.cs;

public interface IPTSystem
{
    Task<bool> BroadcastTransmissionAsync(string a_message, bool a_shutdown, bool a_shutdownWarning, List<long> a_selectedUsers);
    void SetLastUserLogon(DateTime? a_dateTime);
    void SetTransmissionReceived(bool a_received);
    long? ValidateCredentials(string a_userName, string a_currentPassword);
    void ResetCredentials(long a_userIdVal, string a_currentPassword, string a_newPassword, bool a_resetPwdOnNextLogin);
    bool ValidatePassword(string a_password);
}