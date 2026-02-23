using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.DB.Services
{
    public class DarkModeService : IDarkModeService
    {
        private readonly IDbContextFactory<DbReportsContext> _factory;

        public DarkModeService(IDbContextFactory<DbReportsContext> factory)
        {
            _factory = factory;
        }

        private DbReportsContext GetDbContext()
        {
            // Resolve a new scoped DbContext using the IServiceProvider
            return _factory.CreateDbContext();
        }
        public void SetDarkModeForUser(int? userId)
        {
            using var context = GetDbContext();
            var user = context.Users.Find(userId);
            if (user != null)
            {
                user.DarkModeActive = isDarkMode; // Assuming you're storing this as a string "On"/"Off"
                context.SaveChanges();
            }
        }
        public event Action<bool> OnDarkModeChanged; // I need that when the system callls this action I have to update the SetDarkModeForUser
        private bool isDarkMode;

        public bool IsDarkMode
        {
            get => isDarkMode;
            set
            {
                if (isDarkMode != value)
                {
                    isDarkMode = value;
                    OnDarkModeChanged?.Invoke(isDarkMode);
                }
            }
        }
    }

}
