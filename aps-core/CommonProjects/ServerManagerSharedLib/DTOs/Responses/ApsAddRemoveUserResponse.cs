using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class ApsAddRemoveUserResponse
    {
        public int ModifiedUsers { get; set; }
        bool Exception = false;
        string FullExceptionText = "";
    }
}
