using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class ColorMeaning
    {
        public string Color { get; set; } // Hex code or color name
        public string Name { get; set; } // Meaning name
        public string Action { get; set; } // Action or description
        public string TypeBar { get; set; } // Differentiates types of bars or segments if needed
        public SegmentTypeEnum SegmentTypeId { get; set; } // Foreign key

        // Additional properties or methods here
    }
}
