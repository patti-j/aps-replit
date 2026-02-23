namespace ReportsWebApp.DB.Models
{
    public class Dependency
    {
        public int Id { get; set; } // A unique identifier for the dependency
        public bool IsActive { get; set; }
        public long From { get; set; } // The ID of the predecessor event
        public long To { get; set; } // The ID of the successor event
        public DependencyType DependencyType { get; set; } 
        public int Type { get; set; }
        public string Color { get; set; }
        public string TooltipLabel { get; set; }
        public LinkType LinkType { get; set; }
        public string Cls
        {
            get
            {
                return ColorClassMapper.GetCssClassFromHex(Color);
            }
        }

    }
    public class LinkType
    {
        public static readonly LinkType ActivityLink = new LinkType(0, "ActivityLink");
        public static readonly LinkType MaterialsLink = new LinkType(1, "MaterialsLink");

        public int Value { get; private set; }
        public string StringValue { get; private set; }

        public LinkType()
        {

        }

        private LinkType(int value, string stringValue)
        {
            Value = value;
            StringValue = stringValue;
        }

        public static LinkType FromInt(int value)
        {
            switch (value)
            {
                case 0: return ActivityLink;
                case 1: return MaterialsLink;
                default: throw new ArgumentException("Invalid value", nameof(value));
            }
        }

        public static LinkType FromString(string? value)
        {
            if (value == null)
            {
                return ActivityLink;
            }

            switch (value.ToUpper())
            {
                case "ActivityLink": return ActivityLink;
                case "MaterialsLink": return MaterialsLink;
                default: throw new ArgumentException("Invalid string value", nameof(value));
            }
        }

        public override string ToString()
        {
            return StringValue;
        }
    }
    public class DependencyType
    {
        public static readonly DependencyType StartToStart = new DependencyType(0, "SS");
        public static readonly DependencyType StartToEnd = new DependencyType(1, "SF");
        public static readonly DependencyType EndToStart = new DependencyType(2, "FS");
        public static readonly DependencyType EndToEnd = new DependencyType(3, "FF");

        public int Value { get; private set; }
        public string StringValue { get; private set; }

        public DependencyType()
        {
            
        }

        private DependencyType(int value, string stringValue)
        {
            Value = value;
            StringValue = stringValue;
        }

        public static DependencyType FromInt(int value)
        {
            switch (value)
            {
                case 0: return StartToStart;
                case 1: return StartToEnd;
                case 2: return EndToStart;
                case 3: return EndToEnd;
                default: throw new ArgumentException("Invalid value", nameof(value));
            }
        }

        public static DependencyType FromString(string? value)
        {
            if (value == null)
            {
                return EndToStart;
            }

            switch (value.ToUpper())
            {
                case "SS": return StartToStart;
                case "SF": return StartToEnd;
                case "FS": return EndToStart;
                case "FF": return EndToEnd;
                default: throw new ArgumentException("Invalid string value", nameof(value));
            }
        }

        public override string ToString()
        {
            return StringValue;
        }
    }
}