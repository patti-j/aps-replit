using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetsuiteIntegration.Utils
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }

        public override string Message
        {
            get { return $"Status {StatusCode}: {Content}"; }
        }
    }
}
