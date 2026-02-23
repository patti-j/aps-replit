using PT.Scheduler;

namespace PT.SchedulerData;

public static class ResourceKeyListData
{
    /// <summary>
    /// Returns comma delimited string of actual resources used ExternalIds
    /// </summary>
    /// <param name="a_resourceKeyList"></param>
    /// <param name="a_sd"></param>
    /// <returns></returns>
    public static string ToResourceExternalIdsCSV(this ResourceKeyList a_resourceKeyList, ScenarioDetail a_sd)
    {
        string actualResourceUsedCSV = string.Empty;
        ResourceKeyList.Node current = a_resourceKeyList.First;

        while (current != null)
        {
            BaseResource res = a_sd.PlantManager.GetResource(current.Data.Resource);
            if (res == null)
            {
                //The resource has been removed from the scenario
                actualResourceUsedCSV += $"Removed Resource:'{current.Data.Resource}',";
            }
            else
            {
                actualResourceUsedCSV += $"{res.ExternalId},";
            }

            current = current.Next;
        }

        string actualResourcesUsedTrimmed = actualResourceUsedCSV.TrimEnd(',');
        return actualResourcesUsedTrimmed;
    }
}