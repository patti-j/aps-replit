using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.Templates.Lists;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.APIs;

public class ApiCallHelper
{
    /// <summary>
    /// Get User Id from User name
    /// </summary>
    /// <param name="a_userName"></param>
    /// <param name="a_timeout"></param>
    /// <returns></returns>
    internal static BaseId GetUserId(string a_userName, int a_timeout)
    {
        BaseId userId = BaseId.NULL_ID;
        using (SystemController.Sys.UsersLock.TryEnterRead(out UserManager um, a_timeout))
        {
            User user = um.GetUserByName(a_userName);
            if (user != null)
            {
                userId = user.Id;
            }
        }

        if (userId == BaseId.NULL_ID)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedUndoNoUserFound, "No user found by the name provided.");
        }

        return userId;
    }
    
    /// <summary>
    /// Build a list of ExternalId objects from the PtObjectIds provided in the API call
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <param name="a_timeoutSpan"></param>
    /// <param name="a_objects"></param>
    /// <param name="o_lastTransmissionNumber"></param>
    /// <returns></returns>
    internal static List<ExternalIdObject> GenerateExternalIdObjects(long a_scenarioId, TimeSpan a_timeoutSpan, IEnumerable<PtObjectId> a_objects)
    {
        List<ExternalIdObject> objects = new ();

        int timeout = Convert.ToInt32(a_timeoutSpan.TotalMilliseconds);

        try
        {
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, timeout))
            {
                for (int i = 0; i < sm.LoadedScenarioCount; i++)
                {
                    Scenario scenario = sm.GetByIndex(i);

                    if (scenario.Id != a_scenarioId)
                    {
                        continue;
                    }
                    
                    using (scenario.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, timeout))
                    {
                        foreach (PtObjectId ptObjectId in a_objects)
                        {
                            if (string.IsNullOrEmpty(ptObjectId.JobExternalId))
                            {
                                return objects;
                            }

                            List<ExternalIdObject> holdJobObject = GetExternalIdObjects(ptObjectId, sd);
                            objects.AddRange(holdJobObject);
                        }
                    }
                }
            }
        }
        catch
        {
            foreach (PtObjectId ptObjectId in a_objects)
            {
                objects.Add(new ExternalIdObject(ptObjectId.JobExternalId, ptObjectId.MoExternalId, ptObjectId.OperationExternalId, ptObjectId.ActivityExternalId));
            }
        }


        return objects;
    }

    private static List<ExternalIdObject> GetExternalIdObjects(PtObjectId a_ptObjectId, ScenarioDetail a_sd)
    {
        List<ExternalIdObject> newObjects = new ();
        Job job = a_sd.JobManager.GetByExternalId(a_ptObjectId.JobExternalId);

        if (string.IsNullOrEmpty(a_ptObjectId.MoExternalId))
        {
            foreach (ManufacturingOrder mo in job.ManufacturingOrders)
            {
                for (int i = 0; i < mo.OperationManager.Count; i++)
                {
                    InternalOperation op = (InternalOperation)mo.OperationManager.GetByIndex(i);
                    foreach (InternalActivity act in op.Activities)
                    {
                        newObjects.Add(new ExternalIdObject(job.ExternalId, mo.ExternalId, op.ExternalId, act.ExternalId));
                    }
                }
            }
        }
        else
        {
            ManufacturingOrder mo = GetMOByExternalId(a_ptObjectId.MoExternalId, job);
            if (string.IsNullOrEmpty(a_ptObjectId.OperationExternalId))
            {
                for (int i = 0; i < mo.OperationManager.Count; i++)
                {
                    InternalOperation op = (InternalOperation)mo.OperationManager.GetByIndex(i);
                    foreach (InternalActivity act in op.Activities)
                    {
                        newObjects.Add(new ExternalIdObject(job.ExternalId, mo.ExternalId, op.ExternalId, act.ExternalId));
                    }
                }
            }
            else
            {
                InternalOperation op = GetOpByExternalId(a_ptObjectId.OperationExternalId, mo);
                if (string.IsNullOrEmpty(a_ptObjectId.ActivityExternalId))
                {
                    foreach (InternalActivity act in op.Activities)
                    {
                        newObjects.Add(new ExternalIdObject(job.ExternalId, mo.ExternalId, a_ptObjectId.OperationExternalId, act.ExternalId));
                    }
                }
                else
                {
                    InternalActivity internalActivity = GetActByExternalId(a_ptObjectId.ActivityExternalId, op);
                    newObjects.Add(new ExternalIdObject(job.ExternalId, mo.ExternalId, op.ExternalId, internalActivity.ExternalId));
                }
            }
        }

        return newObjects;
    }

    internal static Job GetJobByExternalId(string a_jobExternalId, ScenarioDetail a_sd)
    {
        if (string.IsNullOrEmpty(a_jobExternalId))
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidJob, "An external ID for the job was not provided");
        }

        Job job = a_sd.JobManager.GetByExternalId(a_jobExternalId);

        if (job == null)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidJob, "Could not find job with the external ID provided");
        }

        return job;
    }

    internal static ManufacturingOrder GetMOByExternalId(string a_moExternalId, Job a_job)
    {
        if (!string.IsNullOrEmpty(a_moExternalId))
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders.GetByExternalId(a_moExternalId);

            if (mo == null)
            {
                throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidMO, "Could not find MO with the external ID provided");
            }

            return mo;
        }

        if (a_job.ManufacturingOrderCount == 1)
        {
            return a_job.ManufacturingOrders[0];
        }

        throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidMO, $"Job with external ID '{a_job.ExternalId}' has multiple MOs. An external ID for the right MO must be provided to perform the move.");
    }

    internal static InternalOperation GetOpByExternalId(string a_opExternalId, ManufacturingOrder a_mo)
    {
        if (!string.IsNullOrEmpty(a_opExternalId))
        {
            InternalOperation op = a_mo.OperationManager[a_opExternalId] as InternalOperation;

            if (op == null)
            {
                throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidOp, "Could not find operation with the external ID provided");
            }

            return op;
        }

        if (a_mo.OperationManager.Count == 1)
        {
            return a_mo.OperationManager.GetByIndex(0) as InternalOperation;
        }

        throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidOp, $"MO with external ID '{a_mo.ExternalId}' has multiple operations. An external ID for the right operation must be provided to perform the move.");
    }

    internal static InternalActivity GetActByExternalId(string a_actExternalId, InternalOperation a_op)
    {
        if (!string.IsNullOrEmpty(a_actExternalId))
        {
            foreach (InternalActivity activity in a_op.Activities)
            {
                if (activity.ExternalId == a_actExternalId)
                {
                    return activity;
                }
            }

            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidActivity, "Could not find activity with the external ID provided");
        }

        if (a_op.Activities.Count == 1)
        {
            return a_op.Activities.GetByIndex(0);
        }

        throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidActivity, $"Operation with external ID '{a_op.ExternalId}' has multiple activities. An external ID for the right activity must be provided to perform the move.");
    }

    internal static MoveBlockKeyData GetMoveBlocksData(InternalActivity a_act)
    {
        ActivityKeyList actList = new ();
        Resource scheduledResource = a_act.Batch.PrimaryResource;

        if (scheduledResource == null)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidActivity, "Attempting to move an uncsheduled activity.");
        }

        ResourceKey resourceKey = new (scheduledResource.PlantId, scheduledResource.DepartmentId, scheduledResource.Id);
        BlockKey currentBlock = null;

        for (int blockI = 0; blockI < a_act.ResourceRequirementBlockCount; blockI++)
        {
            ResourceBlock block = a_act.GetResourceRequirementBlock(blockI);
            if (block != null)
            {
                currentBlock = new BlockKey(a_act.Operation.ManufacturingOrder.Job.Id, a_act.Operation.ManufacturingOrder.Id, a_act.Operation.Id, a_act.Id, block.Id);
                break;
            }
        }

        if (currentBlock == null)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedInvalidActivity, "Could not locate the schedule block with the activity external ID provided");
        }

        actList.Add(new ActivityKey(a_act.Operation.ManufacturingOrder.Job.Id, a_act.Operation.ManufacturingOrder.Id, a_act.Operation.Id, a_act.Id));

        return new MoveBlockKeyData(resourceKey, currentBlock, actList);
    }

    /// <summary>
    /// Returns a move time based on the desired move time taking into account current scheduling data.
    /// Considers Blocks scheduled at the time and the operation's latest constraint.
    /// Accepts and returns times in Display format.
    /// </summary>
    /// <returns>A time greater or equal to the initial time</returns>
    internal static DateTime AdjustMoveTimeForNonExactMove(ScenarioDetail a_sd, DateTime a_targetDateTime, ResourceKey a_targetResources, List<MoveBlockKeyData> a_blockKeyDataList)
    {
        DateTime originalDate = a_targetDateTime;

        long maxTime = originalDate.Ticks;
        //Set to EndTime of the target block if dragged onto a block.
        Resource r = (Resource)a_sd.PlantManager.GetResource(a_targetResources.Resource);

        List<ResourceBlockList.Node> blocksList = r.Blocks.FindAllBlocksContainingDate(maxTime);
        if (r.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
        {
            foreach (ResourceBlockList.Node block in blocksList)
            {
                //The block exists. Validate that the block being dropped on, is not one of the blocks moving
                bool blockAtDropTimeNotBeingMoved = true;
                if (block.Data.Batched)
                {
                    //This block may have activities that are not being moved.
                    blockAtDropTimeNotBeingMoved = false;
                    IEnumerator<InternalActivity> actList = block.Data.Batch.GetEnumerator();
                    while (actList.MoveNext())
                    {
                        foreach (MoveBlockKeyData keyData in a_blockKeyDataList)
                        {
                            foreach (ActivityKey actKey in keyData)
                            {
                                if (actKey.ActivityId != actList.Current.Id)
                                {
                                    //There is an activity in the block that isn't moving.
                                    blockAtDropTimeNotBeingMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (MoveBlockKeyData keyData in a_blockKeyDataList)
                    {
                        foreach (ActivityKey actKey in keyData)
                        {
                            if (block.Data.Batch.FindActivity(actKey.ActivityId) != null)
                            {
                                //The block contains an activity being moved.
                                blockAtDropTimeNotBeingMoved = false;
                                break;
                            }
                        }
                    }
                }

                if (blockAtDropTimeNotBeingMoved)
                {
                    maxTime = Math.Max(maxTime, block.Data.Batch.EndOfStorageTicks);
                }
            }
        }
        else if (r.CapacityType == InternalResourceDefs.capacityTypes.Infinite)
        {
            //No change to MaxTime. It can schedule at moved time
        }
        else if (r.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
        {
            //TODO: Calculate available people and attention %s to see if the job can schedule at the same time as other jobs
        }

        return new DateTime(maxTime);
    }

    internal static List<UndoAction> GetUndoActions(BaseId a_scenarioId, int a_timeout, int a_nbrOfActionsToUndo)
    {
        List<UndoAction> actionsToUndo = new ();
        SortedList<ulong, UndoAction> sortedTransmissions = GetSortedTransmissions(a_scenarioId, a_timeout);

        int count = sortedTransmissions.Values.Count;

        //If the call specifies more actions to undo than there are available, just start from the first one and undo them all.
        int startIdx = Math.Max(count - a_nbrOfActionsToUndo, 0);
        for (int i = startIdx; i < count; i++)
        {
            actionsToUndo.Add(sortedTransmissions.Values[i]);
        }

        return actionsToUndo;
    }

    internal static List<UndoAction> GetUndoActionsByUser(BaseId a_scenarioId, int a_timeout, BaseId a_instigator)
    {
        List<UndoAction> actionsToUndo = new ();
        SortedList<ulong, UndoAction> sortedTransmissions = GetSortedTransmissions(a_scenarioId, a_timeout);

        int startIdx = -1;
        for (int i = sortedTransmissions.Values.Count - 1; i >= 0; i--)
        {
            UndoAction undoAction = sortedTransmissions.Values[i];
            if (undoAction.Instigator == a_instigator)
            {
                startIdx = i;
                break;
            }
        }

        if (startIdx == -1)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedUndoNoUserFound, "Could not find any Undo actions performed by the provided user.");
        }

        int count = sortedTransmissions.Values.Count;
        for (int i = startIdx; i < count; i++)
        {
            actionsToUndo.Add(sortedTransmissions.Values[i]);
        }

        return actionsToUndo;
    }

    internal static List<UndoAction> GetUndoActionsByTransmissionNbr(BaseId a_scenarioId, int a_timeout, ulong a_transmissionNbr)
    {
        List<UndoAction> actionsToUndo = new ();
        SortedList<ulong, UndoAction> sortedTransmissions = GetSortedTransmissions(a_scenarioId, a_timeout);

        try
        {
            int startIdx = sortedTransmissions.IndexOfKey(a_transmissionNbr);
            int count = sortedTransmissions.Values.Count;

            for (int i = startIdx; i < count; i++)
            {
                actionsToUndo.Add(sortedTransmissions.Values[i]);
            }

            return actionsToUndo;
        }
        catch
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedUndoInvalidTransmissionNbr, "Could not find any Undo actions with the given transmission number.");
        }
    }

    private static SortedList<ulong, UndoAction> GetSortedTransmissions(BaseId a_scenarioId, int a_timeout)
    {
        SortedList<ulong, UndoAction> sortedTransmissions = new ();
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, a_timeout))
        {
            for (int s = 0; s < sm.LoadedScenarioCount; s++)
            {
                Scenario scenario = sm.GetByIndex(s);

                if (scenario.Id != a_scenarioId)
                {
                    continue;
                }

                //Lock and get the scenario undo sets
                using (scenario.UndoSetsLock.TryEnterRead(out Scenario.UndoSets undoSets, a_timeout))
                {
                    for (int i = 0; i < undoSets.Count; i++)
                    {
                        Scenario.UndoSet undoSet = undoSets[i];
                        for (int j = 0; j < undoSet.Count; j++)
                        {
                            Scenario.TransmissionJar jar = undoSet[j];

                            // Skip system transmissions and redo actions
                            if (jar.TransmissionInfo.IsInternal || !jar.Play)
                            {
                                continue;
                            }

                            BaseId instigator = jar.TransmissionInfo.Instigator;
                            sortedTransmissions.Add(jar.TransmissionInfo.TransmissionNbr, new UndoAction(undoSet.m_undoNbr, j, instigator, jar.TransmissionInfo.TransmissionNbr, jar.TransmissionInfo.TransmissionId));
                        }
                    }
                }
            }
        }

        if (sortedTransmissions.Count == 0)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.FailedUndoNoActions, "There are no undoable actions.");
        }

        return sortedTransmissions;
    }

    internal class UndoAction
    {
        internal Id UndoSetId;
        internal int TransmissionIdx;
        internal BaseId Instigator;
        internal ulong TransmissionNbr;
        internal Guid TransmissionId;

        internal UndoAction(Id a_undoSetId, int a_transmissionIdx, BaseId a_instigator, ulong a_transmissionNbr, Guid a_transmissionId)
        {
            UndoSetId = a_undoSetId;
            TransmissionIdx = a_transmissionIdx;
            Instigator = a_instigator;
            TransmissionNbr = a_transmissionNbr;
            TransmissionId = a_transmissionId;
        }
    }
}