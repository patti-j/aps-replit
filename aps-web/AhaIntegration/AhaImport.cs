using System;
using System.Collections.Generic;
using System.Text;
using AhaIntegration.ResponseObjects;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace AhaIntegration
{
    public class AhaImport
    {

        public async Task<Task> ImportData(string a_username, string a_password, string a_dbConnectionString, string a_apiToken, string a_schema)
        {            
            AhaApiLibrary ahaApiLibrary = new AhaApiLibrary(a_apiToken, a_username);

            List<Feature> features = await ahaApiLibrary.GetFeatureTest();
            await SqlDump.DumpAsync(features, a_dbConnectionString, "AhaFeatures", a_schema);

            List<Idea> ideas = await ahaApiLibrary.GetIdeas();
            await SqlDump.DumpAsync(ideas, a_dbConnectionString, "AhaIdeas", a_schema);

            List<Product> products = await ahaApiLibrary.GetProducts();
            await SqlDump.DumpAsync(products, a_dbConnectionString, "AhaProducts", a_schema);

            List<User> users = await ahaApiLibrary.GetUsers();
            await SqlDump.DumpAsync(users, a_dbConnectionString, "AhaUsers", a_schema);

            List<Release> releases = await ahaApiLibrary.GetReleases();
            await SqlDump.DumpAsync(releases, a_dbConnectionString, "AhaReleases", a_schema);            

            return Task.CompletedTask;
        }
    }



    //private DateTime GetEndOfReleasePlanning(List<Release> a_releases)
    //{
    //    DateTime endOfReleasePlanning = DateTime.MinValue;
    //    foreach (Release release in a_releases)
    //    {
    //        if (!release.parking_lot && !release.released && release.release_date.HasValue)
    //        {
    //            DateTime releaseDate = Convert.ToDateTime(release.release_date);
    //            if (releaseDate > endOfReleasePlanning)
    //            {
    //                endOfReleasePlanning = releaseDate;
    //            }
    //        }
    //    }

    //    return endOfReleasePlanning;
    //}

    //private DateTime GetFeatureNeedDateTime(DateTime a_endOfRelease, Release a_release)
    //{
    //    DateTime needDateTime = a_endOfRelease.Add(c_defaultSprintDuration).AddHours(23.9); //Aha date is at 12am, we need to account for the last work day
    //    if (!a_release.parking_lot && !a_release.released && a_release.release_date.HasValue)
    //    {
    //        needDateTime = a_release.release_date.Value.AddHours(23.9); //Aha date is at 12am, we need to account for the last work day
    //    }

    //    return needDateTime;
    //}


    //private string GetCustomFieldValue(Feature feature, string fieldName)
    //{
    //    var field = feature.custom_fields.FirstOrDefault(f => f.name == fieldName);
    //    if (field == null || field.value == null)
    //        return "";

    //    if (field.value.Type == JTokenType.Array)
    //    {
    //        // Unimos los valores con coma o como prefieras
    //        var values = field.value.ToObject<List<string>>();
    //        return string.Join(", ", values);
    //    }

    //    return field.value.ToString();
    //}


    //private decimal GetOperationsEstimate(Feature a_feature)
    //{
    //    decimal operationEstimate = 1;
    //    //Use original estimate unless remaining work is specified
    //    if (a_feature.original_estimate.HasValue && a_feature.original_estimate.Value != 0m)
    //    {
    //        operationEstimate = a_feature.original_estimate.Value;
    //    }

    //    if (a_feature.remaining_estimate.HasValue && a_feature.remaining_estimate.Value != 0m)
    //    {
    //        operationEstimate = a_feature.remaining_estimate.Value;
    //    }

    //    return operationEstimate;
    //}

}
