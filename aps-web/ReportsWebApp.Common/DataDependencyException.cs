using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.Common
{
    public class DataDependencyException : Exception
    {
        public DataDependencyException(string message) : base(message) { }
    }
}
