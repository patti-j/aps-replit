using Microsoft.AspNetCore.Components;
using System.Configuration;

namespace ReportsWebApp.DB.Models
{
    public class BryntumResource
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string Workcenter { get; set; }
        public string ResourceType { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string Scenario { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public ElementReference DraggableReference { get; set; }

    }

}

