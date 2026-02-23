using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class NavigationItem
    {
        public string Text { get; set; }
        public string NavigateUrl { get; set; }
        public string IconCssClass { get; set; }
        public NavigationItem ParentItem { get; set; }        
        public List<NavigationItem> Children { get; set; } = new List<NavigationItem>();
    }

}
