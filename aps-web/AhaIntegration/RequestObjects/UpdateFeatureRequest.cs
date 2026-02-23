using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AhaIntegration.ResponseObjects;

namespace AhaIntegration.RequestObjects
{
    public class UpdateFeatureRequest
    {
        public UpdateFeatureRequest()
        {
        }

        internal UpdateFeatureRequest(FeatureUpdate a_feature)
        {
            id = a_feature.Id;
            start_date = a_feature.ScheduledStartDateTime;
            due_date = a_feature.ScheduledEndDateTime;
            description = a_feature.Description;
            //release = a_feature.ReleaseId;
        }

        public string id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime due_date { get; set; }
        public string description { get; set; }
        //public string release { get; set; }

        public List<FeatureUpdate> GetUpdatedFeatures(string a_connectionString)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(a_connectionString))
            {
                connection.Open();
                SqlCommand sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "SELECT ExternalId, ScheduledStartDate, ScheduledEndDate FROM [PublishData]";
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                da.Fill(dt);
            }

            List<FeatureUpdate> updatedFeatures = new List<FeatureUpdate>();

            foreach (DataRow dtRow in dt.Rows)
            {

                FeatureUpdate featureUpdate = new FeatureUpdate();

                featureUpdate.Id = (string)dtRow["ExternalId"];
                featureUpdate.ScheduledStartDateTime = (DateTime)dtRow["ScheduledStartDate"];
                featureUpdate.ScheduledEndDateTime = (DateTime)dtRow["ScheduledEndDate"];
                featureUpdate.Description = ((DateTime)dtRow["ScheduledEndDate"]).ToString();
                updatedFeatures.Add(featureUpdate);
            }

            return updatedFeatures;

            //TODO:Push Data back to Aha!
            //Old code written by Cavan, will need a complete rework and/or remove going forwad
            /*
            SqlLibrary.SqlApis sqlApi = new SqlApis(a_dbConnectionString);
            Dictionary<string, PublishedJob> jobs = sqlApi.GetPublishedJobs();

            AhaApiLibrary ahaApiLibrary = new AhaApiLibrary(m_token, "");

            //TODO: find a better way to get releases and their dates
            List<Product> products = await ahaApiLibrary.GetProducts();

            foreach (Product product in products)
            {
                if (product.id != c_integratedProductId)
                {
                    //TODO: Support multiple products
                    continue;
                }

                //Release parkingLot =
                List<Release> releases = await ahaApiLibrary.GetReleasesByProduct(product.id, true);
                Release plannedBacklogRelease = releases.Find(r => r.parking_lot);
                releases.Remove(plannedBacklogRelease);
                releases = releases.Where(r => !r.released && r.end_date != null).ToList(); //Remove shipped releases
                releases = releases.OrderBy(r => r.start_date).ToList();

                List<Feature> features = await ahaApiLibrary.GetFeaturesByProduct(product.id, false);
                List<FeatureUpdate> updatedFeatures = new List<FeatureUpdate>();
                foreach (Feature feature in features)
                {
                    if (jobs.TryGetValue(feature.id, out PublishedJob job))
                    {
                        FeatureUpdate featureUpdate = new FeatureUpdate();
                        featureUpdate.Id = job.ExternalId;
                        featureUpdate.ScheduledStartDateTime = job.ScheduledStartDate;
                        featureUpdate.ScheduledEndDateTime = job.ScheduledEndDate;

                        //Determine release
                        foreach (Release release in releases)
                        {
                            if (job.ScheduledStartDate > release.start_date && job.ScheduledStartDate <= release.end_date)
                            {
                                featureUpdate.ReleaseId = release.id;
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(featureUpdate.ReleaseId))
                        {
                            //No planned release found. Put in Planned Backlog
                            featureUpdate.ReleaseId = plannedBacklogRelease.id;
                        }

                        updatedFeatures.Add(featureUpdate);
                    }
                }

                await ahaApiLibrary.PublishJobUpdates(updatedFeatures);
            }
            */

        }


    }
}
