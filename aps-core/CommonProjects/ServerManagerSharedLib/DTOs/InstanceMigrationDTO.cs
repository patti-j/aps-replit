using System.Collections.Generic;
using Newtonsoft.Json;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs
{
    public class InstanceMigrationDto
    {
        public List<InstanceMigration> data { get; set; }
        public int totalCount { get; set; }

        public InstanceMigrationDto(IEnumerable<APSInstanceEntity> instances) 
        {
            totalCount = instances.Count();
            data = instances.Select(x => new InstanceMigration(x)).ToList();
        }
    }

    public class InstanceMigration
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Environment { get; set; }
        public string PlanningAreaKey { get; set; }
        public string Settings { get; set; }

        public InstanceMigration(APSInstanceEntity instance)
        {
            Name = instance.PublicInfo.InstanceName;
            Version = instance.PublicInfo.SoftwareVersion;
            Environment = Enum.GetName(instance.PublicInfo.EnvironmentType);
            PlanningAreaKey = instance.Settings.InstanceId;
            Settings = JsonConvert.SerializeObject(instance);
        }
    }
}