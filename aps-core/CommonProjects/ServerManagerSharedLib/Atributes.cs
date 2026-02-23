using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerWebService
{
    /// <summary>
    /// Stores the software version
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MinimumSerializationVersion : Attribute
    {
        readonly string m_version;

        public MinimumSerializationVersion(string a_versionNumber)
        {
            m_version = a_versionNumber;
        }

        public string GetVersion()
        {
            return m_version;
        }
    }
}
