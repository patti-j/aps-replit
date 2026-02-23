using PT.APSCommon;

namespace PT.Scheduler.TransmissionDispatchingAndReception
{
    public class QueuedTransmissionData
    {
        public BaseId InstigatorId;
        public string Description;
        public DateTimeOffset Timestamp;

        public QueuedTransmissionData(BaseId a_instigatorId, string a_description, DateTimeOffset a_timeStamp)
        {
            InstigatorId = a_instigatorId;
            Description = a_description;
            Timestamp = a_timeStamp;
        }
    }
}
