using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models.WebApp
{
    public interface INamedEntity
    {
        public int Id { get; }
        public string Name { get; }
        public string TypeDisplayName { get; }
    }
}
