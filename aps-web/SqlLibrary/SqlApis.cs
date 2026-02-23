using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SqlLibrary
{
    public class SqlApis
    {
        private readonly string m_connectionString;

        public SqlApis(string a_connectionString)
        {
            m_connectionString = a_connectionString;
        }

        public Dictionary<string, PublishedJob> GetPublishedJobs()
        {
            DataTable dt = new DataTable();
            Dictionary<string, PublishedJob> publishedJobs = new Dictionary<string, PublishedJob>();
            using (SqlConnection connection = new SqlConnection(m_connectionString))
            {
                connection.Open();
                SqlCommand sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "Select * from Jobs";
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                da.Fill(dt);
            }

            foreach (DataRow dtRow in dt.Rows)
            {
                PublishedJob job = new PublishedJob();
                job.Scheduled = (bool)dtRow["Scheduled"];
                if (!job.Scheduled)
                {
                    //No need to update unscheduled orders
                    continue;
                }
                job.ExternalId = (string)dtRow["ExternalId"];
                job.ScheduledStartDate = (DateTime)dtRow["ScheduledStartDateTime"];
                job.ScheduledEndDate = (DateTime)dtRow["ScheduledEndDateTime"];
                publishedJobs.Add(job.ExternalId, job);
            }

            return publishedJobs;
        }
    }
}
