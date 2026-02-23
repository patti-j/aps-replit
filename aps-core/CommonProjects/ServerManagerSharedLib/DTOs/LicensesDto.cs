using System.Collections.Generic;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs
{
    public class LicensesDto
    {
        public List<License> data { get; set; }
        public int totalCount { get; set; }
    }
}
