using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class BreadcrumbItem
    {
        public string Text { get; set; }
        public string? NavigateUrl { get; set; }
        public bool HasValue { get; set; }

        public BreadcrumbItem(string text, string? navigateUrl)
        {
            Text = text;
            NavigateUrl = navigateUrl;
        }

        public BreadcrumbItem() // Parameterless constructor for initialization convenience
        {
        }
    }

}
