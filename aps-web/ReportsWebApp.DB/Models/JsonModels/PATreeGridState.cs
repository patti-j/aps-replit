using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models.JsonModels
{
    public class PATreeGridState : TreeGridState
    {
        // Which tree keys should be expanded
        public List<int> ShownServerIds { get; set; }
        public List<int> HiddenServerIds { get; set; }
    }
}
