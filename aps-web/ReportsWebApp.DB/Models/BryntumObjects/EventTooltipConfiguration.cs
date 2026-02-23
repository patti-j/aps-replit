namespace ReportsWebApp.DB.Models.BryntumObjects
{
    public class EventTooltipConfiguration
    {
        public bool AutoHide { get; set; } = false;
        public string Align { get; set; } = "b-t"; 
        public bool ShowOnHover { get; set; } = true;
        public bool Disabled { get; set; } = true;

        // Converts the configuration into a dictionary suitable for integration with JS
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["autoHide"] = AutoHide,
                ["align"] = Align,
                ["showOnHover"] = ShowOnHover,
                ["disabled"] = Disabled
            };
        }
    }
}
