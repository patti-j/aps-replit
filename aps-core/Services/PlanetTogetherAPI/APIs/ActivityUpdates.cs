using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.Templates.Lists;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.APIs;

internal class ActivityUpdates
{
    /// <summary>
    /// Validates DateTime is in the future but less than 5 years away. broadcasts a ScenarioClockAdvanceT
    /// </summary>
    internal static ApsWebServiceResponseBase SendHold(long a_scenarioId, TimeSpan a_timeoutSpan, IEnumerable<PtObjectId> a_objects, DateTime a_dateTime, string a_holdReason, BaseId a_instigator, ApiLogger a_apiLogger)
    {
        try
        {
            if (a_objects == null)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.NoIdObjects, "No Ids were provided to perform the job hold.");
            }

            if (a_dateTime < PTDateTime.MinDateTime)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.InvalidDatePast, "Date is too far in the past.");
            }

            if (a_dateTime > PTDateTime.MaxDateTime)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.InvalidDateFuture, "Date is too far in the future.");
            }

            List<ExternalIdObject> objects = ApiCallHelper.GenerateExternalIdObjects(a_scenarioId, a_timeoutSpan, a_objects);

            //Success, send transmission
            ApiHoldT successClockT = new (new BaseId(a_scenarioId), objects, true, a_dateTime, a_holdReason);
            successClockT.Instigator = a_instigator;
            string broadcastArgs = $"ScenarioId: {successClockT.scenarioId} | Instigator = {successClockT.Instigator} | Hold Until Date: {successClockT.HoldUntilDate} | Hold Reason {successClockT.HoldReason}";
            a_apiLogger.LogBroadcast("ApiHoldT", broadcastArgs);
            SystemController.ClientSession.SendClientAction(successClockT);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission.");
        }
    }

    internal static ApsWebServiceResponseBase SendLock(long a_scenarioId, TimeSpan a_timeoutSpan, IEnumerable<PtObjectId> a_ptObjectIds, bool a_lock, BaseId a_instigator, ApiLogger a_apiLogger)
    {
        try
        {
            if (a_ptObjectIds == null)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.NoIdObjects, "No Ids were provided to perform the job hold.");
            }

            List<ExternalIdObject> objects = ApiCallHelper.GenerateExternalIdObjects(a_scenarioId, a_timeoutSpan, a_ptObjectIds);

            //Success, send transmission
            ApiLockT apiLockT = new (new BaseId(a_scenarioId), objects, a_lock);
            apiLockT.Instigator = a_instigator;
            string broadcastArgs = $"ScenarioId: {apiLockT.scenarioId} | Instigator = {apiLockT.Instigator}";
            a_apiLogger.LogBroadcast("ApiLockT", broadcastArgs);
            SystemController.ClientSession.SendClientAction(apiLockT);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendAnchor(long a_scenarioId, TimeSpan a_timeoutSpan, IEnumerable<PtObjectId> a_ptObjectIds, bool a_anchor, bool a_lock, DateTime a_anchorDate, BaseId a_instigator, ApiLogger a_apiLogger)
    {
        try
        {
            if (a_ptObjectIds == null)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.NoIdObjects, "No Ids were provided to perform the job hold.");
            }

            if (a_anchorDate > PTDateTime.MaxDateTime)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.InvalidDateFuture, "Date is too far in the future.");
            }

            List<ExternalIdObject> objects = ApiCallHelper.GenerateExternalIdObjects(a_scenarioId, a_timeoutSpan, a_ptObjectIds);

            //Success, send transmission
            ApiAnchorT apiAnchorT = new (new BaseId(a_scenarioId), objects, a_anchor, a_lock, a_anchorDate);
            apiAnchorT.Instigator = a_instigator;
            SystemController.ClientSession.SendClientAction(apiAnchorT);
            string broadcastArgs = $"ScenarioId: {apiAnchorT.scenarioId} | Instigator = {apiAnchorT.Instigator}";
            a_apiLogger.LogBroadcast("apiAnchorT", broadcastArgs);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendUnschedule(long a_scenarioId, TimeSpan a_timeoutSpan, IEnumerable<PtObjectId> a_ptObjectIds, BaseId a_instigator)
    {
        try
        {
            if (a_ptObjectIds == null)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.NoIdObjects, "No Ids were provided to perform the job hold.");
            }

            List<ExternalIdObject> objects = ApiCallHelper.GenerateExternalIdObjects(a_scenarioId, a_timeoutSpan, a_ptObjectIds);

            //Success, send transmission
            ApiUnscheduleT apiUnscheduleT = new (new BaseId(a_scenarioId), objects);
            apiUnscheduleT.Instigator = a_instigator;
            SystemController.ClientSession.SendClientAction(apiUnscheduleT);

            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendUndoByTransmissionNbr(long a_scenarioId, TimeSpan a_timeoutSpan, ulong a_transmissionNbr, BaseId a_instigator)
    {
        int timeout = Convert.ToInt32(a_timeoutSpan.TotalMilliseconds);
        try
        {
            BaseId scenarioId = new (a_scenarioId);
            List<ApiCallHelper.UndoAction> undoActionsByTransmissionNbr = ApiCallHelper.GetUndoActionsByTransmissionNbr(scenarioId, timeout, a_transmissionNbr);

            if (undoActionsByTransmissionNbr.Count == 0)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "No undo actions specified");
            }

            ScenarioUndoT undoT = new (scenarioId, undoActionsByTransmissionNbr[0].UndoSetId);
            undoT.Instigator = a_instigator;

            foreach (ApiCallHelper.UndoAction undoAction in undoActionsByTransmissionNbr)
            {
                undoT.UndoIds.Add(undoAction.TransmissionId);
            }

            SystemController.ClientSession.SendClientAction(undoT);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendLastUserActionUndo(long a_scenarioId, TimeSpan a_timeoutSpan, string a_userName, BaseId a_instigator)
    {
        int timeout = Convert.ToInt32(a_timeoutSpan.TotalMilliseconds);
        try
        {
            BaseId userId = ApiCallHelper.GetUserId(a_userName, timeout);
            BaseId scenarioId = new (a_scenarioId);

            List<ApiCallHelper.UndoAction> undoActionsByUser = ApiCallHelper.GetUndoActionsByUser(scenarioId, timeout, userId);

            if (undoActionsByUser.Count == 0)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "No undo actions specified");
            }

            ScenarioUndoT undoT = new (scenarioId, undoActionsByUser[0].UndoSetId);
            undoT.Instigator = a_instigator;

            foreach (ApiCallHelper.UndoAction undoAction in undoActionsByUser)
            {
                undoT.UndoIds.Add(undoAction.TransmissionId);
            }

            SystemController.ClientSession.SendClientAction(undoT);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendUndoActions(long a_scenarioId, TimeSpan a_timeoutSpan, int a_nbrOfActionsToUndo, BaseId a_instigator)
    {
        int timeout = Convert.ToInt32(a_timeoutSpan.TotalMilliseconds);
        try
        {
            BaseId scenarioId = new (a_scenarioId);
            List<ApiCallHelper.UndoAction> undoActions = ApiCallHelper.GetUndoActions(scenarioId, timeout, a_nbrOfActionsToUndo);

            if (undoActions.Count == 0)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "No undo actions specified");
            }

            ScenarioUndoT undoT = new (scenarioId, undoActions[0].UndoSetId);
            undoT.Instigator = a_instigator;

            foreach (ApiCallHelper.UndoAction undoAction in undoActions)
            {
                undoT.UndoIds.Add(undoAction.TransmissionId);
            }

            SystemController.ClientSession.SendClientAction(undoT);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendMove(long a_scenarioId, TimeSpan a_timeoutSpan, IEnumerable<PtObjectId> a_ptObjectIds, string a_resourceExternalId, DateTime a_moveDateTime, bool a_lockMove, bool a_anchorMove, bool a_expediteSuccessors, bool a_exactMove, BaseId a_instigator)
    {
        try
        {
            if (a_ptObjectIds == null)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.NoIdObjects, "No Ids were provided to perform the job hold.");
            }

            if (string.IsNullOrEmpty(a_resourceExternalId))
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedInvalidResource, "A target resource Id was not provided.");
            }

            if (a_moveDateTime < PTDateTime.MinDateTime)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.InvalidDateFuture, "Date is too far in the past.");
            }

            if (a_moveDateTime > PTDateTime.MaxDateTime)
            {
                return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.InvalidDateFuture, "Date is too far in the future.");
            }

            int timeout = Convert.ToInt32(a_timeoutSpan.TotalMilliseconds);
            ResourceKey targetResource = null;
            List<MoveBlockKeyData> blockKeyDataList = new ();
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
                        List<Resource> resList = sd.PlantManager.GetResourceList();

                        foreach (Resource res in resList)
                        {
                            if (res.ExternalId == a_resourceExternalId)
                            {
                                targetResource = new ResourceKey(res.PlantId, res.DepartmentId, res.Id);
                                break;
                            }
                        }

                        if (targetResource == null)
                        {
                            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedInvalidResource, "Failed to get resource from target resource external Id provided.");
                        }

                        foreach (PtObjectId ptObjectId in a_ptObjectIds)
                        {
                            try
                            {
                                Job job = ApiCallHelper.GetJobByExternalId(ptObjectId.JobExternalId, sd);
                                ManufacturingOrder mo = ApiCallHelper.GetMOByExternalId(ptObjectId.MoExternalId, job);
                                InternalOperation op = ApiCallHelper.GetOpByExternalId(ptObjectId.OperationExternalId, mo);
                                InternalActivity act = ApiCallHelper.GetActByExternalId(ptObjectId.ActivityExternalId, op);
                                MoveBlockKeyData mbk = ApiCallHelper.GetMoveBlocksData(act);
                                blockKeyDataList.Add(mbk);
                            }
                            catch (WebServicesErrorException e)
                            {
                                return new ApsWebServiceResponseBase(e.Code, e.Message);
                            }
                        }

                        //No validation errors, perform the move.
                        if (!a_exactMove)
                        {
                            a_moveDateTime = ApiCallHelper.AdjustMoveTimeForNonExactMove(sd, a_moveDateTime, targetResource, blockKeyDataList);
                        }
                    }
                }
            }

            ScenarioDetailMoveT t = new (new BaseId(a_scenarioId), targetResource, a_moveDateTime.Ticks, a_expediteSuccessors, a_lockMove, a_anchorMove, a_exactMove, false /*TODO: CampaignMove*/);
            t.Instigator = a_instigator;

            foreach (MoveBlockKeyData moveBlock in blockKeyDataList)
            {
                t.AddMoveBlock(moveBlock);
            }

            t.JoinWithBatchDroppedOntoIfPossible = false;

            SystemController.ClientSession.SendClientAction(t);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendActivityStatusUpdate(long a_scenarioId, TimeSpan a_timeoutSpan, ActivityStatusUpdateObject[] a_activityStatusUpdates, BaseId a_instigator)
    {
        try
        {
            ApiActivityUpdateT t = new (new BaseId(a_scenarioId));
            t.Instigator = a_instigator;

            foreach (ActivityStatusUpdateObject activityStatusUpdate in a_activityStatusUpdates)
            {
                PtObjectId objectId = activityStatusUpdate.PtObjectId;
                ExternalIdObject externalIdObject = new (objectId.JobExternalId, objectId.MoExternalId, objectId.OperationExternalId, objectId.ActivityExternalId);

                ApiActivityUpdateT.ActivityUpdate update = new (externalIdObject);

                if (!string.IsNullOrEmpty(activityStatusUpdate.ProductionStatus))
                {
                    update.SetProductionStatus(activityStatusUpdate.ProductionStatus);
                }

                if (!string.IsNullOrEmpty(activityStatusUpdate.Comments))
                {
                    update.Comments = activityStatusUpdate.Comments;
                }

                if (!string.IsNullOrEmpty(activityStatusUpdate.HoldReason))
                {
                    update.HoldReason = activityStatusUpdate.HoldReason;
                }

                if (activityStatusUpdate.HoldUntil != DateTime.MinValue)
                {
                    update.HoldUntil = activityStatusUpdate.HoldUntil;
                }

                update.OnHold = activityStatusUpdate.OnHold;
                update.Paused = activityStatusUpdate.Paused;

                t.Add(update);
            }

            SystemController.ClientSession.SendClientAction(t);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendActivityQuantitiesUpdate(long a_scenarioId, TimeSpan a_timeoutSpan, ActivityQuantitiesUpdateObject[] a_activityQuantitiesUpdates, BaseId a_instigator)
    {
        try
        {
            ApiActivityUpdateT t = new (new BaseId(a_scenarioId));
            t.Instigator = a_instigator;

            foreach (ActivityQuantitiesUpdateObject activityQuantitiesUpdate in a_activityQuantitiesUpdates)
            {
                PtObjectId objectId = activityQuantitiesUpdate.PtObjectId;
                ExternalIdObject externalIdObject = new (objectId.JobExternalId, objectId.MoExternalId, objectId.OperationExternalId, objectId.ActivityExternalId);

                ApiActivityUpdateT.ActivityUpdate update = new (externalIdObject);

                if (activityQuantitiesUpdate.ReportedGoodQty != decimal.MinValue)
                {
                    update.ReportedGoodQty = activityQuantitiesUpdate.ReportedGoodQty;
                }

                if (activityQuantitiesUpdate.ReportedScrapQty != decimal.MinValue)
                {
                    update.ReportedScrapQty = activityQuantitiesUpdate.ReportedScrapQty;
                }

                t.Add(update);
            }

            SystemController.ClientSession.SendClientAction(t);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    internal static ApsWebServiceResponseBase SendActivityDatesUpdate(long a_scenarioId, TimeSpan a_timeoutSpan, ActivityDatesUpdateObject[] a_activityDatesUpdates, BaseId a_instigator)
    {
        try
        {
            ApiActivityUpdateT t = new (new BaseId(a_scenarioId));
            t.Instigator = a_instigator;

            foreach (ActivityDatesUpdateObject activityQuantitiesUpdate in a_activityDatesUpdates)
            {
                PtObjectId objectId = activityQuantitiesUpdate.PtObjectId;
                ExternalIdObject externalIdObject = new (objectId.JobExternalId, objectId.MoExternalId, objectId.OperationExternalId, objectId.ActivityExternalId);

                ApiActivityUpdateT.ActivityUpdate update = new (externalIdObject);

                if (activityQuantitiesUpdate.ReportedRunHrs >= 0)
                {
                    update.ReportedRunHrs = activityQuantitiesUpdate.ReportedRunHrs;
                }

                if (activityQuantitiesUpdate.ReportedSetupHrs >= 0)
                {
                    update.ReportedSetupHrs = activityQuantitiesUpdate.ReportedSetupHrs;
                }

                if (activityQuantitiesUpdate.ReportedPostProcessingHrs >= 0)
                {
                    update.ReportedPostProcessingHrs = activityQuantitiesUpdate.ReportedPostProcessingHrs;
                }

                if (activityQuantitiesUpdate.ReportedFinishDate != DateTime.MinValue)
                {
                    update.ReportedFinishDate = activityQuantitiesUpdate.ReportedFinishDate;
                }

                if (activityQuantitiesUpdate.ReportedStartDate != DateTime.MinValue)
                {
                    update.ReportedStartDate = activityQuantitiesUpdate.ReportedStartDate;
                }

                if (activityQuantitiesUpdate.ReportedProcessingStartDate != DateTime.MinValue)
                {
                    update.ReportedProcessingEndDate = activityQuantitiesUpdate.ReportedProcessingStartDate;
                }

                if (activityQuantitiesUpdate.ReportedStartDate != DateTime.MinValue)
                {
                    update.ReportedProcessingEndDate = activityQuantitiesUpdate.ReportedProcessingEndDate;
                }

                if (activityQuantitiesUpdate.ReportedCleanHrs >= 0)
                {
                    update.ReportedCleanHrs = activityQuantitiesUpdate.ReportedCleanHrs;
                }

                if (activityQuantitiesUpdate.ReportedCleanoutGrade >= 0)
                {
                    update.ReportedCleanoutGrade = activityQuantitiesUpdate.ReportedCleanoutGrade;
                }

                t.Add(update);
            }

            SystemController.ClientSession.SendClientAction(t);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }

    public static ApsWebServiceResponseBase SendChat(TimeSpan a_timeoutSpan, long a_scenarioId, string a_chatMessage, string a_userName, BaseId a_instigator)
    {
        try
        {
            int timeout = Convert.ToInt32(a_timeoutSpan.TotalMilliseconds);

            BaseId userId = ApiCallHelper.GetUserId(a_userName, timeout);

            UserChatT chatT = new (userId, a_instigator, a_chatMessage);
            chatT.Instigator = a_instigator;

            SystemController.ClientSession.SendClientAction(chatT);
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.Success);
        }
        catch (WebServicesErrorException e)
        {
            return new ApsWebServiceResponseBase(e.Code, e.Message);
        }
        catch
        {
            return new ApsWebServiceResponseBase(EApsWebServicesResponseCodes.FailedToBroadcast, "Failed to broadcast the transmission");
        }
    }
}