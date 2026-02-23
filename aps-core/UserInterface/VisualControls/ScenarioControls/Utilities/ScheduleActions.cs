using PT.APSCommon;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SchedulerExtensions.Job;
using PT.Transmissions;

namespace PT.ScenarioControls.Utilities;

public class ScheduleActions
{
    public static void ExpediteJobMOs(IMainForm a_mainForm, ScenarioDetail a_sd, BaseIdList a_jobs, PackageEnums.EJobExpediteType a_jobExpediteType
                                      , DateTime a_specifiedLocalTimeForToDateType, BlockKey a_lastBlockMouseDown, bool a_anchor, bool a_lockResource, bool a_expediteSupplies)
    {
        ScenarioDetailExpediteBaseT.ExpediteStartDateType expediteStartType;
        DateTime expediteTimeInServerTime = PTDateTime.MinDateTime;
        List<PTTransmission> transmissions = new ();
        PTTransmission t;
        MOKeyList mosToExpedite;
        List<ManufacturingOrder> mos;
        List<Job> jobs = new (a_jobs.Count);

        foreach (BaseId jobId in a_jobs)
        {
            jobs.Add(a_sd.JobManager.GetById(jobId));
        }

        switch (a_jobExpediteType)
        {
            case PackageEnums.EJobExpediteType.ASAP:
                expediteStartType = ScenarioDetailExpediteBaseT.ExpediteStartDateType.Clock;
                expediteTimeInServerTime = a_sd.ClockDate;
                //Get list of all Preds and Material supplying MOs.
                mos = a_sd.JobManager.GetMoExpediteListConstrainedByDate(a_sd, jobs, expediteStartType, expediteTimeInServerTime, a_expediteSupplies);
                mosToExpedite = SupplyingMoList.ConvertToMOKeyList(mos);
                t = GetExpediteMOTransmission(a_sd, a_mainForm.ScenarioInfo.ScenarioId, mosToExpedite, expediteStartType, expediteTimeInServerTime, a_anchor, a_lockResource, true);
                transmissions.Add(t);
                break;
            case PackageEnums.EJobExpediteType.PreserveFrozenSpan:
                expediteStartType = ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfFrozenSpan;

                mos = a_sd.JobManager.GetMoExpediteListConstrainedByDate(a_sd, jobs, expediteStartType, expediteTimeInServerTime, a_expediteSupplies);
                mosToExpedite = SupplyingMoList.ConvertToMOKeyList(mos);
                t = GetExpediteMOTransmission(a_sd, a_mainForm.ScenarioInfo.ScenarioId, mosToExpedite, expediteStartType, expediteTimeInServerTime, a_anchor, a_lockResource, true);
                transmissions.Add(t);
                break;
            case PackageEnums.EJobExpediteType.PreserveStableSpan:
                if (a_lastBlockMouseDown != null)
                {
                    expediteStartType = ScenarioDetailExpediteBaseT.ExpediteStartDateType.EndOfStableSpan;
                    mos = a_sd.JobManager.GetMoExpediteListConstrainedByDate(a_sd, jobs, expediteStartType, expediteTimeInServerTime, a_expediteSupplies);
                    mosToExpedite = SupplyingMoList.ConvertToMOKeyList(mos);
                    t = GetExpediteMOTransmission(a_sd, a_mainForm.ScenarioInfo.ScenarioId, mosToExpedite, expediteStartType, expediteTimeInServerTime, a_anchor, a_lockResource, true);
                    transmissions.Add(t);
                }
                else //no block so don't allow this for now
                {
                    throw new PTValidationException("2030");
                }

                break;
            case PackageEnums.EJobExpediteType.JIT:
                expediteStartType = ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime;
                foreach (Job job in jobs)
                {
                    DateTime expediteTime = PTDateTime.Max(a_sd.ClockDate, job.EarliestOpJitStart());
                    mos = a_sd.JobManager.GetMoExpediteListConstrainedByDate(a_sd, new List<Job>(1) { job }, expediteStartType, expediteTimeInServerTime, a_expediteSupplies);
                    mosToExpedite = SupplyingMoList.ConvertToMOKeyList(mos);

                    t = GetExpediteMOTransmission(a_sd, a_mainForm.ScenarioInfo.ScenarioId, mosToExpedite, expediteStartType, expediteTime, a_anchor, a_lockResource, true);
                    transmissions.Add(t);
                }

                break;
            default:
                expediteStartType = ScenarioDetailExpediteBaseT.ExpediteStartDateType.SpecificDateTime;
                expediteTimeInServerTime = a_specifiedLocalTimeForToDateType.ToServerTime().RemoveSeconds();
                mos = a_sd.JobManager.GetMoExpediteListConstrainedByDate(a_sd, jobs, expediteStartType, expediteTimeInServerTime, a_expediteSupplies);
                mosToExpedite = SupplyingMoList.ConvertToMOKeyList(mos);
                t = GetExpediteMOTransmission(a_sd, a_mainForm.ScenarioInfo.ScenarioId, mosToExpedite, expediteStartType, expediteTimeInServerTime, a_anchor, a_lockResource, true);
                transmissions.Add(t);
                break;
        }

        if (transmissions.Count == 1)
        {
            a_mainForm.ClientSession.SendClientAction(transmissions[0]);
        }
        else if (transmissions.Count > 1)
        {
            a_mainForm.ClientSession.SendClientActionsPacket(transmissions);
        }
    }

    public static void ExpediteJobs(IMainForm a_mainForm, BaseIdList a_jobKeyList, ScenarioDetailExpediteBaseT.ExpediteStartDateType a_specificDateTime, DateTime a_expediteTimeInServerTime, bool a_anchorMove, bool a_lockMove, BaseId a_resourceId)
    {
        ScenarioDetailExpediteJobsT t = new(a_mainForm.ScenarioInfo.ScenarioId,
            a_jobKeyList,
            a_specificDateTime,
            a_expediteTimeInServerTime.Ticks,
            a_anchorMove,
            a_lockMove,
            true);
        t.ResToScheduleEligibleLeadActivitiesOn = a_resourceId;

        a_mainForm.ClientSession.SendClientAction(t);
    }

    private static PTTransmission GetExpediteMOTransmission(ScenarioDetail a_sd, BaseId a_scenarioId, MOKeyList a_mosToExpedite, ScenarioDetailExpediteBaseT.ExpediteStartDateType a_expediteType
                                                            , DateTime a_expediteTimeInServerTime, bool a_anchor, bool a_lockResource, bool a_manuallyTriggered)
    {
        return new ScenarioDetailExpediteMOsT(a_scenarioId, a_mosToExpedite, a_expediteType, a_expediteTimeInServerTime.Ticks, a_anchor, a_lockResource, a_manuallyTriggered);
    }

    /// <summary>
    /// Unschedule jobs
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <param name="a_jobKeyList"></param>
    /// <param name="a_schedule"></param>
    public static void UnscheduleJobs(IMainForm a_mainForm, BaseIdList a_jobKeyList, bool a_schedule)
    {
        ScenarioDetailScheduleJobsT t = new (a_mainForm.ScenarioInfo.ScenarioId, a_jobKeyList, a_schedule);
        a_mainForm.ClientSession.SendClientAction(t);
    }
}