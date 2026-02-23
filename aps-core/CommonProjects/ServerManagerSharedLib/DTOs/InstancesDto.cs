using System.Collections.Generic;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs
{
    public class InstancesDto
    {
        public List<Instance> data { get; set; }
        public int totalCount { get; set; }
    }
}