using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetsuiteIntegration.RequestObjects;
using NetsuiteIntegration.ResponseObjects;
using NetsuiteIntegration.Utils;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using static NetsuiteIntegration.Utils.CustomTransactionBuilder;

namespace NetsuiteIntegration
{
    public class NetsuitePublish
    {        

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public async Task<Task> PublishData(string a_accountId, string a_clientId, string a_clientSecret, string a_tokenId, string a_tokenSecret, string a_dbConnectionString, string a_PublishUrl)
        {
            using HttpClient http = new HttpClient { Timeout = TimeSpan.FromMinutes(10)};

            NetSuiteApiConfig cfg = new NetSuiteApiConfig
            {
                AccountId = a_accountId,
                ClientId = a_clientId,
                ClientSecret = a_clientSecret,
                TokenId = a_tokenId,
                TokenSecret = a_tokenSecret
            };            

            NetSuiteApiLibrary netSuiteApiLibrary = new NetSuiteApiLibrary(cfg, http);
            UpdateJobsRequest updateJobsRequest = new UpdateJobsRequest();
            List<UpdatedJobs> updatedJobs = await updateJobsRequest.GetUpdatedJobs(a_dbConnectionString);
            
            CustomTransactionBuilder builder = new CustomTransactionBuilder();
            List<WoUpdate> payload = await builder.BuildWoEndOnlyAsync(updatedJobs);
            
            string json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            
            await netSuiteApiLibrary.PublishJobUpdates(json, a_accountId, a_PublishUrl);

            return Task.CompletedTask;
        }
    }
}
