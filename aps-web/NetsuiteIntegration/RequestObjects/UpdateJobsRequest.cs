using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetsuiteIntegration.ResponseObjects;

namespace NetsuiteIntegration.RequestObjects
{
    public class UpdateJobsRequest
    {
        public async Task<List<UpdatedJobs>> GetUpdatedJobs(string a_connectionString)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(a_connectionString))
            {
                connection.Open();
                SqlCommand sqlCommand = connection.CreateCommand();
                //sqlCommand.CommandText = "SELECT [WOID], [JobID], [JobStart], [JobEnd], [MfgRouteID], [RouteID], [BOMID], [BOMName], [Operation Internal ID], [OperationId], [OpStart], [OpEnd], [ResourcesUsed], [Status] FROM [PublishData]";
                sqlCommand.CommandText = "SELECT [WOID], [JobID], [JobStart], [JobEnd], [MfgRouteID], [RouteID], [BOMID], [BOMName] FROM [PublishData]";
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                da.Fill(dt);
            }

            List<UpdatedJobs> updatedJobs = new List<UpdatedJobs>();

            foreach (DataRow dtRow in dt.Rows)
            {
                UpdatedJobs jobs = new UpdatedJobs();

                jobs.PT_WorkOrderID = Convert.ToInt32(dtRow["WOID"]);
                jobs.PT_WorkOrderName = Convert.ToString(dtRow["JobID"]);
                jobs.PT_WorkOrderStartDate = Convert.ToDateTime(dtRow["JobStart"]);
                jobs.PT_WorkOrderEndDate = Convert.ToDateTime(dtRow["JobEnd"]);
                jobs.PT_WorkOrderMFGRouteID = Convert.ToInt32(dtRow["MfgRouteID"]);
                jobs.PT_WorkOrderMFGRouteName = Convert.ToString(dtRow["RouteID"]);
                jobs.PT_WorkOrderBOMID = Convert.ToInt32(dtRow["BOMID"]);
                jobs.PT_WorkOrderBOMName = Convert.ToString(dtRow["BOMName"]);
                //jobs.PT_OpID = Convert.ToInt32(dtRow["Operation Internal ID"]);
                //jobs.PT_OpName = Convert.ToString(dtRow["OperationId"]);
                //jobs.PT_OpStartDate = Convert.ToDateTime(dtRow["OpStart"]);
                //jobs.PT_OpEndDate = Convert.ToDateTime(dtRow["OpEnd"]);
                //jobs.PT_ResourcesUsed = Convert.ToString(dtRow["ResourcesUsed"]);
                //jobs.PT_Status = Convert.ToString(dtRow["Status"]);                

                updatedJobs.Add(jobs);
            }

            return updatedJobs;         
        }
    }
}
