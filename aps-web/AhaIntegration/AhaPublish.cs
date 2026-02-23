using AhaIntegration.RequestObjects;
using AhaIntegration.ResponseObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhaIntegration
{
    public class AhaPublish
    {
        public async Task<Task> PublishData(string a_username, string a_password, string a_dbConnectionString, string a_apiToken)
        {
            AhaApiLibrary ahaApiLibrary = new AhaApiLibrary(a_apiToken, a_username);

            UpdateFeatureRequest updatedFeatures = new UpdateFeatureRequest();

            List<FeatureUpdate> features = updatedFeatures.GetUpdatedFeatures(a_dbConnectionString);

            await ahaApiLibrary.PublishJobUpdates(features);
           
            return Task.CompletedTask;
        }
    }
}
