using System.Reflection;

using Microsoft.AspNetCore.Components;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;

using TimeZoneConverter;

namespace ReportsWebApp.Shared
{
    public class ActionItem<TItem>
    {
        public string ActionText { get; set; }
        public string IconCssClass { get; set; }
        public Func<TItem, bool> ShouldActionButtonBeEnabled { get; set; } = _ => true; // Default: Always enabled

        public Func<TItem, Task> Action { get; set; }
    }
    public static class Utils
    {
        public const string DateTimeOfficialFormat = "MMM dd yyyy - h:mm tt";
        public const string DateOfficialFormat = "MMM dd yyyy";
        public static readonly string[] DevExpressThemes = { "blazing-berry", "purple", "office-white", "blazing-dark" };

        /**
         * <summary>
         * Method <c>BlockUsersByAuthStatus</c> checks if the current user is authenticated and authorized to view the current page, 
         * and if not, redirects the user to the appropriate page.
         * </summary>
         * 
         * <param name="user">The current user</param>
         * <param name="permission">The permission to check against. If null, the user will be allowed as long as they are authenticated</param>
         * <param name="navigation">The navigation manager to use when redirecting the user</param>
         * <param name="ptAdminOverride">If true, users with PTAdmin permission will override the standard parmission check</param>
         * 
         * <returns><c>True</c> if access should be blocked. <c>NOTE: When this happens, the User will be redirected as a side-effect!</c><br/>
         * <c>False</c> if access should be allowed</returns>
         */
        public static bool BlockUsersByAuthStatus(User user, Permission? permission, NavigationManager navigation, bool ptAdminOverride = false)
        {
            if (!user.Exists())
            {
                if (string.IsNullOrEmpty(user.Email))
                {
                    // If email is null, user is not logged in
                    navigation.NavigateTo($"./login?redirectUri={navigation.GetLocalRoute()}", true);
                } 
                else
                {
                    // If email exists, user is logged in with SSO, but has no matching user
                    navigation.NavigateTo($"./usernotfound?redirectUri={navigation.GetLocalRoute()}", true);
                }
                return true;
            }

            if (!navigation.ValidateAuth(user.AuthState))
            {
                return true;
            }

            //if the user is a service user they are not allowed to do anything
            if (user.UserType == EUserType.Service)
            {
                navigation.NavigateTo($"./serviceUserNotAllowedHere", true);
                return true;
            }

            // If the user only exists in one company that doesn't use SSO, make sure the user account is verified, or send them to setup
            if (user.Company.UseSSOLogin == false && user.AuthorizedCompanies.Count() == 1 && navigation.IsUnregistered(user.AuthState))
            {
                navigation.NavigateTo($"/UserAccountSetup", true);
                return true;
            }

            // If user is PTAdmin and allowPtAdmin is set, override permission check
            if (ptAdminOverride && user.IsPTAdmin()) {
                return false;
            }
            // If permission is null, allow access as long as the user is logged in
            if (permission != null && !user.IsAuthorizedFor(permission))
            {
                navigation.NavigateTo("/", true);
                return true;
            }
            return false;
        }

        public static string EncodeStringForWeb(string a_inputString)
        {
            return a_inputString
                .Trim()
                .Replace(":", "%3a")
                .Replace(" ", "%20");
        }

        public static string DecodeStringForWeb(string a_inputString)
        {
            return a_inputString
                   .Trim()
                   .Replace("%3a", ":")
                   .Replace("%20", " ");
        }
        public static string FormatUtcDate(DateTime? utcDate, User user)
        {
            if (utcDate is null) return null;
            if (string.IsNullOrEmpty(user.TimeZone)) return utcDate.Value.ToString(DateTimeOfficialFormat);
            try
            {
                DateTime localTime;
                switch (utcDate.Value.Kind)
                {
                    case DateTimeKind.Local:
                        localTime = utcDate.Value; // already local
                        break;
                    default:
                        DateTime utcToLocalTime = utcDate.Value.Add(TZConvert.GetTimeZoneInfo(user.TimeZone).GetUtcOffset(utcDate.Value));
                        localTime = DateTime.SpecifyKind(utcToLocalTime, DateTimeKind.Local);
                        break;
                }

                return localTime.ToString(DateTimeOfficialFormat);
            }
            catch
            {
                return utcDate.Value.ToString(DateTimeOfficialFormat);
            }
        }
    }
    
    
    
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class HiddenAttribute : Attribute
    {
        public HiddenAttribute()
        {
            
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class PasswordAttribute : Attribute
    {
        public PasswordAttribute()
        {
        }
    }
}
