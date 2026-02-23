using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using AhaIntegration.RequestObjects;
using AhaIntegration.ResponseObjects;

using Microsoft.AspNetCore.WebUtilities;

using Newtonsoft.Json;

namespace AhaIntegration
{
    public class AhaApiLibrary
    {
        public AhaApiLibrary(string baseUrl, string a_apiToken, string a_username)
        {
            m_httpClient.BaseAddress = new Uri(baseUrl);
            m_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", a_apiToken);
            m_httpClient.DefaultRequestHeaders.Add("User-Agent", $"GetFeatures Data Import ({a_username})");
            m_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public AhaApiLibrary(string a_apiToken, string a_username)
    : this("https://planettogether.aha.io/api/v1/", a_apiToken, a_username)
        {
        }

        readonly HttpClient m_httpClient = new HttpClient();

        public async Task<List<User>> GetUsers()
        {
            List<User> users = new List<User>();
            Users usersResponse = new Users();
            usersResponse.pagination = new Pagination();
            usersResponse.pagination.current_page = 0;
            usersResponse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (usersResponse.pagination.current_page < usersResponse.pagination.total_pages)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add("page", (usersResponse.pagination.current_page + 1).ToString());
                queryParams.Add("per_page", "100");
                queryParams.Add("fields",
                "id,name,product_id,product_name,product_roles");
                string urlSuffix = QueryHelpers.AddQueryString("users", queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);
                usersResponse = await Utilities.GetResponseObject<Users>(response);
                users.AddRange(usersResponse.users);
            }

            return users;
        }

        public async Task<List<Feature>> GetFeatures(string a_url, bool a_includeShipped)
        {
            List<Feature> features = new List<Feature>();
            Features featuresResponse = new Features();
            featuresResponse.pagination = new Pagination();
            featuresResponse.pagination.current_page = 0;
            featuresResponse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (featuresResponse.pagination.current_page < featuresResponse.pagination.total_pages)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add("page", (featuresResponse.pagination.current_page + 1).ToString());
                queryParams.Add("per_page", "100");
                queryParams.Add("fields", "id,reference_num,name,url,product_id,workflow_status,original_estimate,remaining_estimate,assigned_to_user,custom_fields,score,epic,release,description,created_by_user,workflow_kind,integration_fields");
                //queryParams.Add("fields", "*");

                string urlSuffix = QueryHelpers.AddQueryString(a_url, queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);

                featuresResponse = await Utilities.GetResponseObject<Features>(response);
                features.AddRange(featuresResponse.features);
            }

            //TODO:Performance, slightly improve by removing shipped first
            List<Feature> noRemovedFeatures = features.Where(f => !f.workflow_status.name.ToLower().Contains("not")).ToList(); //will not implement


            if (!a_includeShipped)
            {
                List<Feature> notShippedFeatures = noRemovedFeatures.Where(f => !f.workflow_status.complete).ToList();
                return notShippedFeatures;
            }

            return noRemovedFeatures;
        }

        public async Task<List<Initiative>> GetInitiatives(string a_url, bool a_includeShipped)
        {
            List<Initiative> initiatives = new List<Initiative>();
            Initiatives initiativeResponse = new Initiatives();
            initiativeResponse.pagination = new Pagination();
            initiativeResponse.pagination.current_page = 0;
            initiativeResponse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (initiativeResponse.pagination.current_page < initiativeResponse.pagination.total_pages)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add("page", (initiativeResponse.pagination.current_page + 1).ToString());
                queryParams.Add("per_page", "100");
                queryParams.Add("fields", "id,reference_num,name,url,product_id,workflow_status,assigned_to_user,description,created_by_user,features");
                //queryParams.Add("fields", "*");

                string urlSuffix = QueryHelpers.AddQueryString(a_url, queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);

                initiativeResponse = await Utilities.GetResponseObject<Initiatives>(response);
                initiatives.AddRange(initiativeResponse.initiatives);
            }

            //TODO:Performance, slightly improve by removing shipped first
            List<Initiative> noRemovedFeatures = initiatives.Where(f => !f.workflow_status.name.ToLower().Contains("not")).ToList(); //will not implement

            if (!a_includeShipped)
            {
                List<Initiative> notShippedFeatures = noRemovedFeatures.Where(f => !f.workflow_status.complete).ToList();
                return notShippedFeatures;
            }

            return noRemovedFeatures;
        }

        public async Task<List<Feature>> GetFeaturesByProduct(string a_productId, bool a_includeShipped)
        {
            List<Feature> features = await GetFeatures($"products/{a_productId}/features", a_includeShipped);

            return features;
        }

        public async Task<List<Initiative>> GetInitiativesByProduct(string a_productId, bool a_includeShipped)
        {
            List<Initiative> initiatives = await GetInitiatives($"products/{a_productId}/initiatives", a_includeShipped);

            return initiatives;
        }

        public async Task<List<Feature>> GetFeaturesByIdea(string a_ideaId, bool a_includeShipped)
        {
            List<Feature> features = await GetFeatures($"products/{a_ideaId}/ideas", a_includeShipped);

            return features;
        }


        public async Task<List<Product>> GetProducts()
        {
            List<Product> products = new List<Product>();
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("fields", "id,name");
            queryParams.Add("include_teams", "true");
            string urlSuffix = QueryHelpers.AddQueryString("products", queryParams);

            HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
            await VerifyResult(response);

            Products productsResponse = await Utilities.GetResponseObject<Products>(response);
            products.AddRange(productsResponse.products);
            return products;
        }

        public async Task<List<Idea>> GetIdeas()
        {
            List<Idea> ideas = new List<Idea>();
            Ideas ideasResponse = new Ideas();
            ideasResponse.pagination = new Pagination();
            ideasResponse.pagination.current_page = 0;
            ideasResponse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (ideasResponse.pagination.current_page < ideasResponse.pagination.total_pages)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add("page", (ideasResponse.pagination.current_page + 1).ToString());
                queryParams.Add("per_page", "100");
                // Just get name id
                queryParams.Add("fields",
                    "id,name,reference_num,created_at,product_id,votes,workflow_status,description,url,endorsements_count,categories");
                string urlSuffix = QueryHelpers.AddQueryString("ideas", queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);
                ideasResponse = await Utilities.GetResponseObject<Ideas>(response);
                ideas.AddRange(ideasResponse.ideas);
            }

            return ideas;
        }

        public async Task<List<Feature>> GetSimpleFeatures()
        {
            List<Feature> features = new List<Feature>();
            Features featuresResponse = new Features();
            featuresResponse.pagination = new Pagination();
            featuresResponse.pagination.current_page = 0;
            featuresResponse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (featuresResponse.pagination.current_page < featuresResponse.pagination.total_pages)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add("page", (featuresResponse.pagination.current_page + 1).ToString());
                queryParams.Add("per_page", "100");
                // Just get name id
                //var startOfApril = new DateTime(DateTime.Now.Year, 4, 1).ToString("yyyy-MM-dd");
                //queryParams.Add("created_after", startOfApril);                
                string urlSuffix = QueryHelpers.AddQueryString("features", queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);
                featuresResponse = await Utilities.GetResponseObject<Features>(response);
                features.AddRange(featuresResponse.features);
            }

            return features;
        }

        // Test for getting Features. GetFeature not yet tested with this process
        public async Task<List<Feature>> GetFeatureTest()
        {
            List<Feature> ideas = new List<Feature>();
            Features featuresResponse = new Features();
            featuresResponse.pagination = new Pagination();
            featuresResponse.pagination.current_page = 0;
            featuresResponse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (featuresResponse.pagination.current_page < featuresResponse.pagination.total_pages)
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["page"] = (featuresResponse.pagination.current_page + 1).ToString(),
                    ["per_page"] = "100",
                    ["fields"] = "id,reference_num,name,url,product_id,product_name,original_estimate,remaining_estimate,assigned_to_user,score,epic,description,created_by_user,workflow_kind,workflow_status,created_at,start_date,due_date,product_name,progress,progress_source,work_done,epic,custom_fields,release,start_date,due_date,integration_fields",
                    ["updated_since"] = "=>2026-01-01T00:00:00Z"
                };

                string urlSuffix = QueryHelpers.AddQueryString("features", queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);
                featuresResponse = await Utilities.GetResponseObject<Features>(response);
                ideas.AddRange(featuresResponse.features);
            }

            return ideas;
        }

        private async Task VerifyResult(HttpResponseMessage a_message)
        {
            if (!a_message.IsSuccessStatusCode)
            {
                //Error
                string content = await a_message.Content.ReadAsStringAsync();
                ApiException e = new ApiException
                {
                    StatusCode = (int)a_message.StatusCode,
                    Content = content
                };

                throw e;
            }
        }

        public async Task<List<Requirement>> GetRequirements(string a_featureId)
        {
            List<Requirement> requirements = new List<Requirement>();
            HttpResponseMessage response = await m_httpClient.GetAsync($"features/{a_featureId}/requirements");
            await VerifyResult(response);

            Requirements responseObject = await Utilities.GetResponseObject<Requirements>(response);
            requirements.AddRange(responseObject.requirements);

            return requirements;
        }

        public async Task PublishJobUpdates(List<FeatureUpdate> a_updatedFeatures)
        {
            foreach (FeatureUpdate updatedFeature in a_updatedFeatures)
            {
                UpdateFeatureRequest request = new UpdateFeatureRequest(updatedFeature);
                string serializeObject = JsonConvert.SerializeObject(request);
                HttpResponseMessage response = await m_httpClient.PatchAsync($"features/{updatedFeature.Id}", new StringContent(serializeObject, Encoding.UTF8, "application/json"));
                await VerifyResult(response);
            }
        }

        public async Task<List<Release>> GetReleasesByProduct(string a_productId, bool a_includeParkingLot)
        {
            List<Release> releases = new List<Release>();
            Releases releasesRepsonse = new Releases();
            releasesRepsonse.pagination = new Pagination();
            releasesRepsonse.pagination.current_page = 0;
            releasesRepsonse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (releasesRepsonse.pagination.current_page < releasesRepsonse.pagination.total_pages)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add("page", (releasesRepsonse.pagination.current_page + 1).ToString());
                queryParams.Add("per_page", "100");
                queryParams.Add("fields", "id,release_date,parking_lot");

                string urlSuffix = QueryHelpers.AddQueryString($"products/{a_productId}/releases", queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);

                releasesRepsonse = await Utilities.GetResponseObject<Releases>(response);
                releases.AddRange(releasesRepsonse.releases);
            }

            if (!a_includeParkingLot)
            {
                List<Release> noParkingLotReleases = releases.Where(f => !f.parking_lot).ToList();
                return noParkingLotReleases;
            }

            return releases;
        }


        public async Task<List<Release>> GetReleases()
        {
            List<Release> releases = new List<Release>();
            Releases releasesRepsonse = new Releases();
            releasesRepsonse.pagination = new Pagination();
            releasesRepsonse.pagination.current_page = 0;
            releasesRepsonse.pagination.total_pages = 1; //initial setup, will change to actual on response

            while (releasesRepsonse.pagination.current_page < releasesRepsonse.pagination.total_pages)
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add("page", (releasesRepsonse.pagination.current_page + 1).ToString());
                queryParams.Add("per_page", "100");
                queryParams.Add("fields", "id,product_id,reference_num,name,start_date,end_date,workflow_status");

                string urlSuffix = QueryHelpers.AddQueryString($"releases", queryParams);

                HttpResponseMessage response = await m_httpClient.GetAsync(urlSuffix);
                await VerifyResult(response);

                releasesRepsonse = await Utilities.GetResponseObject<Releases>(response);
                releases.AddRange(releasesRepsonse.releases);
            }

            return releases;
        }

    }
}
