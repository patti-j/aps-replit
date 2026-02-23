using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models.BryntumObjects
{
    public class Material
    {
        public long Id { get; set; }
        public Event Supplier { get; set; }
        public Event Receiver { get; set; }
    }
}