using System;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class InstanceDistribution
    {
        public string Instance { get; set; }
        public int Area { get; set; }

        public InstanceDistribution(string instance, float area)
        {
            Instance = instance;
            if(!float.IsNaN(area))
                Area = Convert.ToInt32(area);
        }
    }
}