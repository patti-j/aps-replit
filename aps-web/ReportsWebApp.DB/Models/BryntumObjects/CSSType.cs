using System.Text;

public class CSSType
{
    public string TextColor { get; set; } // CSS color
    public string BackgroundImage { get; set; }
    public int? FontSize { get; set; }
    public string BackgroundPosition { get; set; }
    public string Border { get; set; }
    public string Padding { get; set; }
    public string Height { get; set; }
    public string Width { get; set; }    
    public string Position { get; set; } = "relative";
    public int? Top { get; set; }
    public int? PaddingLeft { get; set; }
    public int? PaddingTop { get; set; }
    public string WhiteSpace { get; set; }
    public string OverflowWrap { get; set; }
    public string Overflow { get; set; }
    public string Display { get; set; } // Add Display property

    // Method to convert CSSType properties to a valid CSS style string
    public string ToStyleString()
    {
        var styleBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(BackgroundImage)) styleBuilder.Append($"background-image: {BackgroundImage}; ");
        if (FontSize.HasValue) styleBuilder.Append($"font-size: {FontSize}px; ");
        if (!string.IsNullOrEmpty(BackgroundPosition)) styleBuilder.Append($"background-position: {BackgroundPosition}; ");
        if (!string.IsNullOrEmpty(Border)) styleBuilder.Append($"border: {Border}; ");
        if (!string.IsNullOrEmpty(Padding)) styleBuilder.Append($"padding: {Padding}; ");
        if (!string.IsNullOrEmpty(Height)) styleBuilder.Append($"height: {Height}; ");
        if (!string.IsNullOrEmpty(Width)) styleBuilder.Append($"width: {Width}; ");
        if (!string.IsNullOrEmpty(TextColor)) styleBuilder.Append($"color: {TextColor}; ");
        if (!string.IsNullOrEmpty(Position)) styleBuilder.Append($"position: {Position}; ");
        if (Top.HasValue) styleBuilder.Append($"top: {Top}px; ");
        if (PaddingLeft.HasValue) styleBuilder.Append($"padding-left: {PaddingLeft}px; ");
        if (PaddingTop.HasValue) styleBuilder.Append($"padding-top: {PaddingTop}px; ");
        if (!string.IsNullOrEmpty(WhiteSpace)) styleBuilder.Append($"white-space: {WhiteSpace}; ");
        if (!string.IsNullOrEmpty(OverflowWrap)) styleBuilder.Append($"overflow-wrap: {OverflowWrap}; ");
        if (!string.IsNullOrEmpty(Overflow)) styleBuilder.Append($"overflow: {Overflow}; ");
        if (!string.IsNullOrEmpty(Display)) styleBuilder.Append($"display: {Display}; ");
        // Append other CSS properties as needed

        return styleBuilder.ToString();
    }
}
