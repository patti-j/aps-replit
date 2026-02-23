using System.Text;

namespace SchedulerDataModels
{
    public class Segment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Color { get; set; } // CSS color
        public CSSType CSSType { get; set; } // CSS style
        public string Style { get { return CSSType.ToStyleString(); } } // CSS style
    }

    public class CSSType
    {
        // Define CSS properties
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; } // CSS color
        public string BackgroundImage { get; set; }
        public string BackgroundSize { get; set; }
        public string BackgroundPosition { get; set; }
        public string Border { get; set; }
        public string Padding { get; set; }
        public string Height { get; set; } // Height in pixels or other units

        // Add other CSS properties as needed

        // Method to convert CSSType properties to a valid CSS style string
        public string ToStyleString()
        {
            var styleBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(BackgroundColor)) styleBuilder.Append($"background-color: {BackgroundColor}; ");
            if (!string.IsNullOrEmpty(BackgroundImage)) styleBuilder.Append($"background-image: {BackgroundImage}; ");
            if (!string.IsNullOrEmpty(BackgroundSize)) styleBuilder.Append($"background-size: {BackgroundSize}; ");
            if (!string.IsNullOrEmpty(BackgroundPosition)) styleBuilder.Append($"background-position: {BackgroundPosition}; ");
            if (!string.IsNullOrEmpty(Border)) styleBuilder.Append($"border: {Border}; ");
            if (!string.IsNullOrEmpty(Padding)) styleBuilder.Append($"padding: {Padding}; ");
            if (!string.IsNullOrEmpty(Height)) styleBuilder.Append($"height: {Height}; ");
            if (!string.IsNullOrEmpty(TextColor)) styleBuilder.Append($"color: {TextColor}; ");
            // Append other CSS properties as needed

            return styleBuilder.ToString();
        }
    }
}
