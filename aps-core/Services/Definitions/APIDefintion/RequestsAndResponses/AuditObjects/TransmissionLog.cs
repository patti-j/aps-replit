using PT.APSCommon;
using PT.Common.Sql.SqlServer;

namespace PT.APIDefinitions.RequestsAndResponses.AuditObjects
{
    public class TransmissionLog
    {
        public long ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public long InstigatorId { get; set; }
        public string Description { get; set; }
        public string ScenarioType { get; set; }
        public string TransmissionType { get; set; }
        public DateTime Timestamp { get; set; }
        public long TransmissionNumber { get; set; }

        public TransmissionLog(){}
        public TransmissionLog(BaseId a_scenarioId, string a_scenarioName, BaseId a_instigator, string a_description, string a_scenarioType, string a_transmissionType, DateTime a_timestamp, ulong a_transmissionNumber)
        {
            ScenarioId = a_scenarioId.Value;
            ScenarioName = Filtering.FilterString(a_scenarioName);
            InstigatorId = a_instigator.Value;
            Description = Filtering.FilterString(a_description);
            ScenarioType = Filtering.FilterString(a_scenarioType);
            TransmissionType = Filtering.FilterString(a_transmissionType);
            Timestamp = a_timestamp;
            TransmissionNumber = unchecked((long)a_transmissionNumber); // in the (unlikely) event of overflow, negatives will be logged. This shouldn't impact logging if primary use case is matching up TransmissionNbr values
        }
    }
}
