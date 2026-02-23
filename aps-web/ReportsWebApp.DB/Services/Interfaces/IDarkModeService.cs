namespace ReportsWebApp.DB.Services.Interfaces;

public interface IDarkModeService
{
    void SetDarkModeForUser(int? userId);
    event Action<bool> OnDarkModeChanged; // I need that when the system callls this action I have to update the SetDarkModeForUser
    bool IsDarkMode { get; set; }
}