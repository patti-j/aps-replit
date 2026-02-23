
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class Scenario
    {
        /// <summary>
        /// Id that unique identifies where the event came from. ScenarioIds alone won't be unique across PAs, but this combination should be within a company.
        /// </summary>
        public string Id { get; set; }
        public string PlanningAreaName { get; set; }
        public string Environment { get; set; }
        public List<DashtPlanning> DetailDashtPlanning { get; set; }
        public List<DashtCapacityPlanningShiftsCombined> DetailDashtCapacityPlanningShiftsCombined { get; set; }
        public List<Dasht_Materials> DetailDasht_Materials { get; set; }
        public CompanyDb AnalyticalDb { get; set; }
        public BryntumSettings? BryntumSettings { get; set; }
        /// <summary>
        /// The Scenario's Id from the context of its Planning Area. Not unique across PAs, instead use <see cref="Id"/>
        /// </summary>
        //public long ScenarioId { get; set; }
        public string Name { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is Scenario typedObj)
            {
                return Id == typedObj.Id;
            }
            return base.Equals(obj);
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class ScenarioDto
    {
        public string NewScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public string PlanningAreaName { get; set; }
        
    }

}
