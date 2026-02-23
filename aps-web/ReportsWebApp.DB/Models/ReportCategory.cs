using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class ReportCategory
    {
        public int ReportsId { get; set; }
        public Report Report { get; set; }
        public int CategoriesId { get; set; }
        public Category Category { get; set; }
    }
}
