using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class InternalActivity
    {
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string JobExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string MoExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string OpExternalId { get; set; }
        [Column(TypeName = "DECIMAL")] public decimal RequiredFinishQty { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ProductionStatus { get; set; }

        public void SetStatus(string a_status)
        {
            switch (a_status.ToLower())
            {
                case "in development":
                    ProductionStatus = "In development";
                    break;
                case "ready to ship":
                    ProductionStatus = "Finished";
                    break;
                case "shipped":
                    ProductionStatus = "Finished";
                    break;
                default:
                    ProductionStatus = null;
                    break;
            }
        }
    }
}
