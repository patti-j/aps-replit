using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.APSCommon.Exceptions
{
    /// <summary>
    /// Alerts that the Server and Client Checksum values are no longer synchronized.
    /// </summary>
    public class PTDesyncException : PTValidationException
    {
        public PTDesyncException() { }

        public PTDesyncException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }

        public PTDesyncException(string a_message, Exception a_innerException, bool a_appendHelpUrl = false, object[] a_stringParameters = null)
            : base(a_message, a_innerException, a_appendHelpUrl, a_stringParameters) { }
    }
}
