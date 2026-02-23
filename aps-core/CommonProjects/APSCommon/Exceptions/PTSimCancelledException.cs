using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.Common.Exceptions;

namespace PT.APSCommon.Exceptions
{
    /// <summary>
    /// PTException thrown to cancel a simulation action
    /// </summary>
    public class PTSimCancelledException : PTException
    {
        public PTSimCancelledException(): base("8000") { }
    }
}
