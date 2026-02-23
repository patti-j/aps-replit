using System.Collections.Generic;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs
{
    public class InstancesDistributionDto
    {//here add a QA type
        public List<InstanceDistribution> data { get; set; } = new();

    }
}