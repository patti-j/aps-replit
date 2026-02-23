using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class CustomFieldCFGroup
    {
        public int CustomFieldId { get; set; }
        public CustomField CustomField { get; set; }
        public int CFGroupId { get; set; }
        public CFGroup CFGroup { get; set; }
    }
}
