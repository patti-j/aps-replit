using System.Collections.Generic;


namespace ReportsWebApp.DB.Models
{
    /// <summary>
    /// This class represents a collection of legacy Instances from the ServerManager that can be converted into PlanningAreas
    /// </summary>
    public class InstanceMigrationDto
    {
        public List<InstanceMigration> Data { get; set; }
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// This class represents legacy Instance data from the ServerManager that can be converted into a PlanningArea
    /// </summary>
    public class InstanceMigration
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Environment { get; set; }
        public string PlanningAreaKey { get; set; }
        public string Settings { get; set; }
        public PADetails? existingPA;

        public PADetails ToPlanningArea(CompanyServer companyServer, User requester)
        {
            return new PADetails
            {
                ServerId = companyServer.Id,
                CreatedBy = requester.Email,
                Environment = Environment,
                CompanyId = companyServer.ManagingCompanyId,
                CreationDate = DateTime.UtcNow,
                Name = Name,
                Version = Version,
                Settings = Settings,
                PlanningAreaKey = PlanningAreaKey,
                RegistrationStatus = ERegistrationStatus.Created.ToString()
            };
        }
    }
}