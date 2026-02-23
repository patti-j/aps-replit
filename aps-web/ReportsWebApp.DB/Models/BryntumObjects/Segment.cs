namespace ReportsWebApp.DB.Models
{
    public class Segment
    {
        public SegmentTypeEnum Id { get; set; }
        public string SegmentName {get => Id.ToString();}
        public string Title { get; set; }
        public string Color { get; set; }
        public CSSType SegmentStyle { get; set; } 
        public string Style { get { return SegmentStyle.ToStyleString(); } }
        public bool CustomHTML { get; set; }
        public string CustomCode { get; set; }
        public CSSType ParagraphStyle { get; set; } = new CSSType() 
        {
            Position = "absolute",
            Top = 2,
            PaddingLeft = 2,
            PaddingTop = 2,
            WhiteSpace = "pre-wrap",
            OverflowWrap = "break-word",
            Overflow = "hidden",
            Height = "100%"
        };
        public string StyleParragraph { get { return ParagraphStyle.ToStyleString(); } }
        public CSSType SegmentProcessStyle { get; set; } = new CSSType()
        {
            Display = "flex",
            Height = "100%",
            Position = "absolute",
            Top = 0,
            Width = "100%"
        };
    public string StyleSegmentProcess { get { return SegmentProcessStyle.ToStyleString(); } }
    public bool TransparentBorder { get; set; }
    }
}
