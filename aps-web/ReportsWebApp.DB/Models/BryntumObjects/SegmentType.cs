using System.Text.Json.Serialization;

namespace ReportsWebApp.DB.Models
{
    public class SegmentType
    {
        [JsonConstructor]
        public SegmentType()
        {
            
        }
        public SegmentType(SegmentTypeEnum type, string name, string colorName, bool show, string template, bool hasLabelTemplate, params (string Name, string Color)[] colorMeanings)
        {

            SegmentTypeId = type;
            Name = name;
            Color = ColorClassMapper.GetHexValue(colorName);
            Show = show;
            ColorMeanings = CreateColorMeanings(colorMeanings, type);
            Template = template;
            HasLabelTemplate = hasLabelTemplate;
        }
        
        private static List<ColorMeaning> CreateColorMeanings((string Name, string Color)[] colorMeanings, SegmentTypeEnum segmentTypeId)
        {
            return colorMeanings.Select(cm => new ColorMeaning
            {
                Name = cm.Name,
                Color = ColorClassMapper.GetHexValue(cm.Color),
                SegmentTypeId = segmentTypeId
            }).ToList();
        }
        
        public SegmentTypeEnum SegmentTypeId { get; set; } // Primary key, now using enum
        public string Name { get; set; }
        public string Template { get; set; }
        public string Color { get; set; } // Hex code or color name
        public bool Show { get; set; } // Indicates if the segment type should be shown
        public bool IsConfigPopupVisible { get; set; }
        public List<ColorMeaning> ColorMeanings { get; set; } = new();
        public bool HasLabelTemplate { get; set; } = true;

        //Attributes members
        public bool ShowJobColor { get; set; }
        public bool ShowProductColor { get; set; }
        public bool ShowSetupColor { get; set; }
        public bool ShowCustomAttributeColor { get; set; }

    }
    public enum SegmentTypeEnum
    {
        Timing = 1,
        Commitment = 2,
        Attributes = 3, // Changed from "Attribute(s)" for a cleaner name
        Status = 4,
        MaterialStatus = 5, // Concatenated "Material Status" for a valid enum name
        Priority = 6,
        Buffer = 7,
        PercentFinished = 8, // Concatenated "Percent Finished" for a valid enum name
        Process = 9
    }

}
