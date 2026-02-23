using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class GetCompanyLicenseDataResponse
    {
        public Result ResultKey { get; set; }
        public License[] CompanyLicensesList { get; set; }
    }
}
