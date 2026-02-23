using System.Collections;

using PT.APSCommon;
using PT.Database;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class BaseOperationManagerData
{
    public static void PtDbPopulate(this BaseOperationManager a_baseOperationManager,
                                    ScenarioDetail sd,
                                    ref PtDbDataSet dataSet,
                                    PtDbDataSet.ManufacturingOrdersRow moRow,
                                    bool publishInventory,
                                    bool limitToResourceList,
                                    HashSet<BaseId> resourceIds,
                                    PTDatabaseHelper a_dbHelper)
    {
        //Calculate row indexes for MS Project Predecessor export.  Assumes operations are sorted by start date and op id.
        SortedList msProjectOpsSortedList = new ();
        Hashtable msProjectIndexesHash = new ();
        for (int i = 0; i < a_baseOperationManager.OperationsHash.Count; i++)
        {
            BaseOperation bOp = (BaseOperation)a_baseOperationManager.OperationsHash.GetByIndex(i);
            //Exclude Omitted Ops since we don't want them in the project
            if (bOp.Omitted == BaseOperationDefs.omitStatuses.NotOmitted)
            {
                msProjectOpsSortedList.Add(new MsProjectSortableOp(bOp), bOp.Id);
            }
        }

        for (int sortedI = 0; sortedI < msProjectOpsSortedList.Count; sortedI++)
        {
            BaseId opId = (BaseId)msProjectOpsSortedList.GetByIndex(sortedI);
            msProjectIndexesHash.Add(opId, sortedI); //store the sort index by OpId for use later.
        }

        //Determine which operations to export.  
        List<ResourceOperation> allOperations = new ();
        List<ResourceOperation> opsToExport = new ();
        IDictionaryEnumerator enumerator = a_baseOperationManager.OperationsHashInternal.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            if (op is ResourceOperation)
            {
                ResourceOperation resOp = (ResourceOperation)op;
                allOperations.Add(resOp);
                if (!(resOp.Scheduled && resOp.StartDateTime.Ticks > a_dbHelper.MaxPublishDate.Ticks)) //not excluded based on date
                {
                    if (limitToResourceList)
                    {
                        List<InternalResource> resourcesUsed = resOp.GetResourcesScheduled();
                        for (int resI = 0; resI < resourcesUsed.Count; resI++)
                        {
                            if (resourceIds.Contains(resourcesUsed[resI].Id))
                            {
                                opsToExport.Add(resOp);
                                break;
                            }
                        }
                    }
                    else
                    {
                        opsToExport.Add(resOp);
                    }
                }
            }
        }
        ScenarioPublishDestinations publishDestinations = new ScenarioPublishDestinations ();
        using (sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
        {
            publishDestinations =  scenarioSummary.ScenarioSettings.LoadSetting(publishDestinations);
        }
        List<ResourceOperation> finalOpsToExport;
        if (publishDestinations.PublishAllActivitesForMO && opsToExport.Count > 0) //export all
        {
            finalOpsToExport = allOperations;
        }
        else
        {
            finalOpsToExport = opsToExport;
        }

        for (int opI = 0; opI < finalOpsToExport.Count; opI++)
        {
            finalOpsToExport[opI].PtDbPopulate(sd, ref dataSet, moRow, publishInventory, msProjectIndexesHash, a_dbHelper);
        }
    }
}