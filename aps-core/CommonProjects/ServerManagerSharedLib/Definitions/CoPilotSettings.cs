using PT.ServerManagerSharedLib.DTOs.Requests;
using System.Runtime.Serialization;

namespace PT.ServerManagerSharedLib.Definitions
{
    [Serializable]
    public class CoPilotSettings
    {
        public CoPilotSettings()
        {
            Enabled = false;
            AverageCpuUsage = 1;
            BoostPercentage = 200;
            BurstDuration = new TimeSpan(1,0,0);
        }
        public CoPilotSettings(CopilotRequest copilotRequest)
        {
            Enabled = copilotRequest.Enabled;
            AverageCpuUsage = copilotRequest.AverageCpuUsage;
            BoostPercentage = copilotRequest.BoostPercentage;
            BurstDuration = TimeSpan.FromTicks((long)(copilotRequest.BurstDuration * TimeSpan.TicksPerHour));
        }

        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public decimal AverageCpuUsage { get; set; }
        [DataMember]
        public decimal BoostPercentage { get; set; }
        [DataMember]
        public TimeSpan BurstDuration { get; set; }
    }
}
