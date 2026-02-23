using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models
{
    public class PBIWorkspace : BaseEntity
    { 
        public string PBIWorkspaceId { get; set; }
        // Foreign key property
        public int CompanyId { get; set; }

        // Navigation property for the relationship
        public Company Company { get; set; }
        public List<Report> Reports { get; } = new();
        public override string ToString()
        {
            return Name + ": " + PBIWorkspaceId;
        }
    }

    public class PBIWorkspaceOption
    {
        public int Id;
        public string OptionName;
        public string OptionValue;
        public override string ToString()
        {
            return OptionName + ": " + OptionValue;
        }
    }
    public class PBIReportNameDropdownOption
    {
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
    }

    public class PBIDataSetDropdownOption
    {
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
    }
}
