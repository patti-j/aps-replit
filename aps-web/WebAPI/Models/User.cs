using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ReportsWebApp.DB.Models;

namespace WebAPI.Models
{
    public class User : BaseEntity, PAUserSettings
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public bool DarkModeActive { get; set; } = false;
        [Required]
        public string Email { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public string FullName => $"{Name} {LastName}";

        public int CompanyId { get; set; } // Changed to int?
        public virtual Company Company { get; set; }
        public List<Role> Groups { get; set; } = new();
        public virtual List<PlanningAreaTag> PAGroups { get; set; }
        public EUserType UserType { get; set; } = EUserType.Web;
        public string UpdatedBy { get; set; }


        public string TaskNotes { get; set; } = DefaultTaskNotesEnglish;
        public string DisplayLanguage { get; set; } = "English";
        public ECompressionType CompressionType { get; set; } = ECompressionType.Normal;
        public string ExternalId { get; set; } = Guid.NewGuid().ToString();

        public User()
        {
            CompanyId = 0; // Set a default value (e.g., 0) for CompanyId
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
