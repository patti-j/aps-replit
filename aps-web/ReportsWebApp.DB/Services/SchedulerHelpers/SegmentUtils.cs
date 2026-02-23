using ReportsWebApp.DB.Models;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace ReportsWebApp.DB.Services
{
    public static class SegmentUtils
    {
        public static List<SegmentType> SegmentTypes => new ()
        {
            new (SegmentTypeEnum.Timing, "Timing", "red", true, "Name: {{ DashtPlanning.JobName }} \r\nOperation name: {{ DashtPlanning.Opname }} ",
                true, ("Too Early", "green"), ("On-Time", "blue"), ("Almost Late", "orange"),
                ("Late", "red"), ("Capacity Bottleneck", "purple"), ("Material Bottleneck", "pink"),
                ("Release Bottleneck", "maroon")),
            
            new (SegmentTypeEnum.Commitment, "Commitment", "orange", false, String.Empty, true,
                ("Estimate", "lightblue"), ("Planned", "blue"), ("Firm", "darkblue"), ("Released", "navy")),

            new (SegmentTypeEnum.Attributes, "Attribute(s)", "green", false, String.Empty, false),

            new (SegmentTypeEnum.Process, "Process", "green", false, String.Empty, false,
                ("Setup", "orangered"), ("Run", "green"), ("Post-process", "lightgreen"),
                ("Storage", "yellow"), ("Storage Post-process", "lightblue")),

            new (SegmentTypeEnum.Status, "Status", "blue", true,
                "{{ ({ 'Ready': '{fa-star}', 'Running': '{fa-cog}', 'SettingUp': '{fa-wrench}', 'Started': '{fa-play}', 'Waiting': '{fa-clock-o}' })[DashtPlanning.BlockProductionStatus] || '{fa-question}'; }} {{ DashtPlanning.BlockProductionStatus }} \r\n",
                true, ("On-Hold", "yellow"), ("Waiting", "orange"), ("Setting-Up", "orangered"), ("Running", "red"),
                ("Ready", "lime"), ("Started", "green"), ("Post Processing", "darkgreen"), ("Transferring", "navy"), ("Paused", "blue")
            ),

            new (SegmentTypeEnum.MaterialStatus, "Material Status", "purple", false, String.Empty, true,
                ("Material Sources Unknown", "violet"), ("Material Sources Firmed", "indigo"), ("Material Sources Planned", "purple"),
                ("Materials Available", "aqua")),

            new (SegmentTypeEnum.Priority, "Priority", "lime", false, String.Empty, true,
                ("Less than 1", "maroon"), ("1", "red"), ("2", "orangered"), ("3", "yellow"), ("Higher than 3", "olive")),

            new (SegmentTypeEnum.Buffer, "Buffer", "orange", false, String.Empty, true,
                ("Ok", "green"), ("Warning", "orange"), ("Critical", "orangered"), ("Late", "red")),

            new (SegmentTypeEnum.PercentFinished, "Percent Finished", "blue", false, String.Empty, false),
        };
            
        public static Task<List<SegmentConfig>> GetAllSegments()
        {
            var configs = new List<SegmentConfig>();
            var dtoType = typeof(DashtPlanning);

            foreach (var segmentType in SegmentTypes)
            {
                var prop = dtoType.GetProperties().FirstOrDefault(p => p.Name == segmentType.Name);
                var config = new SegmentConfig
                {
                    Id = segmentType.SegmentTypeId,
                    IdentifierKey = prop?.Name ?? segmentType.Name,
                    Weight = 1,
                    BackgroundColor = segmentType.Color,
                    TextColor = "#000000"
                };
                configs.Add(config);
            }

            return Task.FromResult(configs);
        }
    }
}
