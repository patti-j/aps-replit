using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models.JsonModels
{
    public class TreeGridState : GridState
    {
        // Which tree keys should be expanded
        public List<string> ExpandedKeys { get; set; }
    }
}
