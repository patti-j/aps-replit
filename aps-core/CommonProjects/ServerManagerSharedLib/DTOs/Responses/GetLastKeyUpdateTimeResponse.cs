using System;
using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class GetLastKeyUpdateTimeResponse
    {
        public DateTime LastUpdatedDateTimeUTC { get; set; }
        public KeyResultBase ResultKey { get; set; }
    }
}
