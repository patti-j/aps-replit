using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text;

using AhaIntegration.ResponseObjects;

namespace PlanetTogetherContext.Entities
{
    public class Job
    {
        [Key]
        [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }

        [Column(TypeName = "NVARCHAR(MAX)")] public string Description { get; set; }

        [Column(TypeName = "DATETIME")] public DateTime NeedDateTime { get; set; }

        [Column(TypeName = "NVARCHAR(250)")] public string Commitment { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")] public string UDF { get; set; }
        [Column] public int Priority { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")] public string Notes { get; set; }
        
        [Column(TypeName = "NVARCHAR(250)")] public string Customer { get; set; }
        [Column(TypeName = "VARCHAR(250)")] public string OrderNo { get; set; }
        [Column(TypeName = "BIT")] public bool Hot { get; set; }
        [Column(TypeName = "INT")] public int ColorCodeAlpha { get; set; }
        [Column(TypeName = "INT")] public int ColorCodeBlue { get; set; }
        [Column(TypeName = "INT")] public int ColorCodeGreen { get; set; }
        [Column(TypeName = "INT")] public int ColorCodeRed { get; set; }
        [Column(TypeName = "BIT")] public bool Template { get; set; }
        [Column(TypeName = "BIT")] public bool DoNotSchedule { get; set; }
        [Column(TypeName = "VARCHAR(250)")] public string DevOpsId { get; set; }
        [Column(TypeName = "VARCHAR(250)")] public string DevOpsUrl { get; set; }
        [Column(TypeName = "VARCHAR(250)")] public string DevOpsTitle { get; set; }
        [Column(TypeName = "VARCHAR(250)")] public string DevOpsType { get; set; }

        //public void SetCommitment(string a_status)
        //{
        //    switch (a_status.ToLower())
        //    {
        //        case "will not implement":
        //            DoNotSchedule = true;
        //            break;
        //        case "under consideration":
        //            Commitment = ECommitment.Estimate.ToString();
        //            break;
        //        case "in design":
        //            Commitment = ECommitment.Estimate.ToString();
        //            break;
        //        case "ready to develop":
        //            Commitment = ECommitment.Planned.ToString();
        //            break;
        //        case "in development":
        //            Commitment = ECommitment.Firm.ToString();
        //            break;
        //        case "ready to ship":
        //        case "shipped":
        //            Commitment = ECommitment.Released.ToString();
        //            break;
        //        default:
        //            throw new Exception("Invalid status");
        //    }
        //}

        public void SetCommitment(string a_status, bool a_integrationFlag)
        {
            if (a_status.ToLower() == "will not implement")
            {
                DoNotSchedule = true;
            }
            else
            {
                //some bool value for status
                //t estimate
                //f planned
                Commitment = a_integrationFlag ? ECommitment.Planned.ToString() : ECommitment.Estimate.ToString();
            }
        }

        public void SetUDF(string[] a_integrationData)
        // 0, id
        // 1, url
        // 2, title
        // 3, type
        {
            //UserFields =
            //    $" {a_integrationData[0]}, {a_integrationData[1]}, {a_integrationData[2]}, {a_integrationData[3]}";
            DevOpsId = a_integrationData[0];
            DevOpsUrl = a_integrationData[1];
            DevOpsTitle = StripTilde(a_integrationData[2]);
            DevOpsType = a_integrationData[3];
        }
        internal enum ECommitment { Estimate, Planned, Firm, Released }
        internal static string StripTilde(string a_s)
        {
            StringBuilder st = new StringBuilder(a_s);
            st.Replace("~", "");
            return st.ToString();
        }

        public void SetInitiative(Initiative a_initiative)
        {
            Color color = (Color)ColorTranslator.FromHtml(a_initiative.workflow_status.color);
            ColorCodeAlpha = color.A;
            ColorCodeBlue = color.B;
            ColorCodeGreen = color.G;
            ColorCodeRed = color.R;
            OrderNo = a_initiative.name;
            Hot = true;
        }
        

    }
}