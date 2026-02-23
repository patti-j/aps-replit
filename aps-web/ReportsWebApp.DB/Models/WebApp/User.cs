using ReportsWebApp.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Authorization;

namespace ReportsWebApp.DB.Models
{
    public class User : BaseEntity, PAUserSettings
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public string UpdatedBy { get; set; }
        public bool DarkModeActive { get; set; } = false;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public string FullName => $"{Name} {LastName ?? ""}".TrimEnd();

        public int CompanyId { get; set; } // Changed to int?
        public virtual Company Company { get; set; }

        [NotMapped]
        public IEnumerable<int> AuthorizedCompanyIds => Roles.GroupBy(x => x.CompanyId).Select(x => x.First().CompanyId);
        [NotMapped]
        public IEnumerable<Company> AuthorizedCompanies => Roles.GroupBy(x => x.CompanyId).Select(x => x.First().Company);

        public List<Role> Roles { get; set; } = new();
        public List<ENotificationType> SubscribedNotifications { get; set; } = new();
        public virtual List<SchedulerFavorite> GanttFavorites { get; } = new();
        public virtual List<Team> Teams { get; set; } = new();
        public string TaskNotes { get; set; } = DefaultTaskNotesEnglish;
        public string DisplayLanguage { get; set; } = "English";
        public string? TimeZone { get; set; }
        public ECompressionType CompressionType { get; set; } = ECompressionType.Normal;
        public EUserType UserType { get; set; } = EUserType.Web;
        [NotMapped]
        public string? ServiceToken { get; set; }
        public string ExternalId { get; set; }
        public virtual List<SavedGridLayout> SavedGridLayouts { get; set;}
        
        [NotMapped]
        public AuthenticationState? AuthState { get;set; }

        public User()
        {
            CompanyId = 0; // Set a default value (e.g., 0) for CompanyId
        }

        public bool IsAuthorizedFor(Permission permission)
        {
            return IsAuthorizedFor(permission.Key, CompanyId);
        }
        public bool IsAuthorizedFor(Permission permission, int companyId)
        {
            return IsAuthorizedFor(permission.Key, companyId);
        }
        public bool IsAuthorizedForAny(params Permission[] permissions)
        {
            return permissions.Any(IsAuthorizedFor);
        }
        public bool IsAuthorizedForAll(params Permission[] permissions)
        {
            return permissions.All(IsAuthorizedFor);
        }
        public bool IsAuthorizedForCompany(int companyId)
        {
            return AuthorizedCompanies.Any(x => x.Id == companyId);
        }

        public bool IsAuthorizedFor(string permissionKey, int companyId)
        {
            if (Roles.Any(g => g.Permissions.Any(p => p == Permission.PTAdmin.Key)))
            {
                return true;
            }
            if (permissionKey == Permission.PTAdmin.Key)
            {
                return Roles.Any(g => g.Permissions.Any(p => p == permissionKey));
            }
            return Roles.Any(g => g.Permissions.Any(p => g.CompanyId == companyId && p == permissionKey));
        }

        public bool IsPTAdmin()
        {
            return IsAuthorizedFor(Permission.PTAdmin.Key, 1);
        }

        public bool Exists()
        {
            return Roles.Any();
        }

        private static string DefaultTaskNotesEnglish
        {
            get
            {
                string line1 = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n{\\colortbl ;\\red0\\green0\\blue0;\\red0\\green0\\blue255;}\r\n\\viewkind4\\uc1\\pard\\cf1\\f0\\fs17 You can enter custom notes and Shortcuts here.  \\par\r\n\\par\r\nTo create a shortcut to a menu item, enter the text as it appears in the menu item, select it, and click 'Make Shortcut' above.  If it is a valid menu command then it will turn blue.  Otherwise it will turn red.  To modify an existing Shortcut, first select it and click 'Revert to Text'.\\par\r\n\\par\r\nHere are some sample Task Notes with Shortcuts.\\par\r\n\\par\r\n----------------------------------------------------\\par\r\nSetting up your Shop\\par\r\n----------------------------------------------------\\par\r\n\\par\r\nDefine your \\cf2\\ul\\b\\protect\\fs16 Plants\\cf1\\ulnone\\b0\\protect0\\fs17 , \\cf2\\ul\\b\\protect\\fs16 Departments\\cf1\\ulnone\\b0\\protect0\\fs17 , \\cf2\\ul\\b\\protect\\fs16 ";
                string line2 = "Resources\\cf1\\ulnone\\b0\\protect0\\fs17 , and \\cf2\\ul\\b\\protect\\fs16 Capabilities\\cf1\\ulnone\\b0\\protect0\\fs17 .\\par\r\n\\par\r\nLink your Resources to their \\cf2\\ul\\b\\protect\\fs16 Capabilities\\cf1\\ulnone\\b0\\protect0\\fs17 .\\par\r\n\\par\r\nDefine Resource availability by creating \'Online\' Capacity Intervals from beneath the Gantt.\\par\r\n\\par\r\nCreate a New Job from the \\cf2\\ul\\b\\protect\\fs16 Jobs \\cf1\\ulnone\\b0\\protect0\\fs17  pane.\\par\r\n\\par\r\nClick \\cf2\\ul\\b\\protect\\fs16 Optimize \\cf1\\ulnone\\b0\\protect0\\fs17 to schedule your new Job(s).\\par\r\n\\par\r\nManually adjust the schedule from the Gantt.\\par\r\n\\par\r\nPrint schedules from the Gantt for \'All\', one \'Plant\', or one \'Dept\'.\\par\r\n\\par\r\n\\par\r\n--------------------------\\par\r\nMy Daily Tasks\\par\r\n--------------------------\\par\r\n\\par\r\n\\cf2\\ul\\b\\protect\\fs16 Perform Import\\cf1\\ulnone\\b0\\protect0\\fs17  of data from the ERP System\\par\r\n\\par\r\nReview recent production activity \\cf2\\ul\\b\\protect Performance Monitor \\cf1\\ulnone\\b0\\protect0\\par\r\n  \\par\r\nReview recent Job changes \\cf2\\ul\\b\\protect Job Changes Monitor \\cf1\\ulnone\\b0\\protect0  \\par\r\n\\par\r\n\\cf2\\ul\\b\\protect\\fs16 Advance Clock\\cf1\\ulnone\\b0\\protect0\\fs17  to update to current time\\par\r\n\\par\r\n\\cf2\\ul\\b\\protect\\fs16 Optimize ";
                string line3 = "\\cf1\\ulnone\\b0\\protect0\\fs17 with new data\\par\r\n\\par\r\nSwitch to \\cf2\\ul\\b\\protect\\fs16 Full-Screen Gantt \\cf1\\ulnone\\b0\\protect0\\fs17 to make manual adjustments\\par\r\n\\par\r\nCheck the \\cf2\\ul\\b\\protect KPI \\cf1\\ulnone\\b0\\protect0  to evaluate schedule quality\\par\r\n\\par\r\n\\par\r\n--------------------------- \\par\r\nPeriodic Tasks\\par\r\n---------------------------\\par\r\n\\par\r\nCreate a \\cf2\\ul\\b\\protect New What-If\\cf1\\ulnone\\b0\\protect0  Scenario\\par\r\n         \\par\r\nReview proposed changes with the \\cf2\\ul\\b\\protect\\fs16 Impact Monitor\\cf1\\ulnone\\b0\\protect0\\fs17 .\\par\r\n        \\par\r\n\\par\r\n\\par\r\n-------------------------------------\\par\r\nCommonly Used Tools\\par\r\n-------------------------------------\\par\r\n\\par\r\n\\cf2\\ul\\b\\protect\\fs16 Undo\\cf1\\ulnone\\b0\\protect0\\fs17  your last action\\par\r\n\\par\r\nModify the model using the \\cf2\\ul\\b\\protect\\fs16 Configurator \\cf1\\ulnone\\b0\\protect0\\fs17 window\\par\r\n\\par\r\n\\cf2\\ul\\b\\protect Send a New Message\\cf1\\ulnone\\b0\\protect0  to a colleague\\par\r\n\\cf0\\par\r\n\\cf2\\ul\\b\\protect Exit \\cf1\\ulnone\\b0\\protect0  PlanetTogether (changes are saved automatically)\\cf0\\par\r\n}\r\n\0";
                return line1 + line2 + line3;
            }
        }
    }

    public enum EUserType
    {
        Web = 0,
        Service
    }

    public interface PAUserSettings
    {
        
        private static readonly Dictionary<int, string> DisplayLanguagesForBackwardsCompatibilityDict = new()
    {
        { 0, "English" },
        { 1, "Polish" },
        { 2, "German" },
        { 3, "Chinese_PRC" },
        { 4, "Spanish" },
        { 5, "Japanese" },
        { 6, "Dutch" },
        { 7, "French" },
        { 8, "Italian" },
        { 9, "English" },
        { 10, "Indonesian" },
        { 11, "Turkish" },
        { 12, "Portuguese" },
        { 13, "InvariantCulture" }
    };

        public const long NULL_ID = long.MinValue;
        public string TaskNotes { get; set; }
        public string DisplayLanguage { get; set; }
        public ECompressionType CompressionType { get; set; }
        public string ExternalId { get; set; }


        public static string GetDisplayLanguageStringForBackwardsCompatibility(int a_val)
        {
            return DisplayLanguagesForBackwardsCompatibilityDict[a_val];
        }

    }
}
