using PT.APSCommon;
using PT.Common.Extensions;
using PT.Database;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.SchedulerData;

internal class Metric
{
    private readonly decimal m_onTimeJobs;
    private readonly decimal m_overdueJobs;
    private readonly decimal m_outputPerWeek;
    private readonly long m_averageLeadTimeTicks;
    private readonly int m_usersWhoOptimize;
    private readonly decimal m_salesRevenue30;
    private readonly decimal m_salesRevenue60;
    private readonly decimal m_salesRevenue90;
    private readonly decimal m_salesRevenue120;
    private readonly int m_salesOrderCount;
    private readonly int m_purchaseOrderCount;
    private readonly int m_scheduledOrderCount;
    private readonly int m_operationsCount;
    private readonly int m_failedToScheduleCount;
    private readonly int m_jobsPastHorizon;
    private readonly int m_numberOfUsers;
    private readonly DateTime m_lastAutomaticAction;
    private readonly DateTime m_lastManualAction;

    public Metric(ScenarioDetail a_sd)
    {
        m_onTimeJobs = GetOnTimeJobsPercent(a_sd.JobManager);
        m_overdueJobs = GetOverdueJobsPercent(a_sd.JobManager);
        m_outputPerWeek = GetShortTermOutput(a_sd.JobManager, a_sd.GetEndOfShortTerm());
        ;
        m_averageLeadTimeTicks = GetAverageLeadTimeTicks(a_sd.JobManager);
        m_usersWhoOptimize = GetUsersWhoOptimize(a_sd.Scenario);
        m_salesRevenue30 = GetSalesRevenue(a_sd.JobManager, a_sd.GetEndOfShortTerm(), 30);
        m_salesRevenue60 = GetSalesRevenue(a_sd.JobManager, a_sd.GetEndOfShortTerm(), 60);
        m_salesRevenue90 = GetSalesRevenue(a_sd.JobManager, a_sd.GetEndOfShortTerm(), 90);
        m_salesRevenue120 = GetSalesRevenue(a_sd.JobManager, a_sd.GetEndOfShortTerm(), 120);
        m_salesOrderCount = GetSalesOrderCount(a_sd.SalesOrderManager);
        m_purchaseOrderCount = GetPurchaseOrderCount(a_sd.PurchaseToStockManager);
        m_scheduledOrderCount = GetScheduledJobsCount(a_sd.JobManager);
        m_operationsCount = GetOperationsCount(a_sd.JobManager);
        m_failedToScheduleCount = GetFailedToSchedule(a_sd.JobManager);
        m_jobsPastHorizon = GetJobsPastHorizon(a_sd.JobManager);
        m_numberOfUsers = GetNumberOfUsers();
        m_lastManualAction = GetLastManualAction(a_sd.Scenario);
        m_lastAutomaticAction = GetLastAutomaticAction(a_sd.Scenario);
    }

    public void PtDbPopulate(ref PtDbDataSet a_dataSet, PTDatabaseHelper a_dbHelper, PtDbDataSet.SchedulesRow a_schedulesRow)
    {
        a_dataSet.Metrics.AddMetricsRow(
            a_schedulesRow,
            a_schedulesRow.InstanceId,
            OnTimeJobs,
            OverdueJobs,
            OutputPerWeek,
            AverageLeadTimeTicks,
            UsersWhoOptimize,
            SalesRevenue30,
            SalesRevenue60,
            SalesRevenue90,
            SalesRevenue120,
            SalesOrderCount,
            PurchaseOrderCount,
            ScheduledOrderCount,
            OperationsCount,
            FailedToScheduleCount,
            JobsPastHorizon,
            NumberOfUsers,
            LastManualAction,
            LastAutomaticAction);
    }

    internal static decimal GetOnTimeJobsPercent(JobManager a_jm)
    {
        return 100 - GetOverdueJobsPercent(a_jm);
    }

    internal static decimal GetOverdueJobsPercent(JobManager a_jm)
    {
        decimal overDue = a_jm.Count(a_x => a_x.Overdue);
        int jobCount = a_jm.Count(a_x => !a_x.Template);
        return jobCount == 0 ? 0 : 100 * overDue / jobCount;
    }

    internal static decimal GetShortTermOutput(JobManager a_jm, DateTime a_endOfShortTermDate)
    {
        try
        {
            decimal shortTermOutput = a_jm.Where(a_x =>
                                              a_x.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled &&
                                              a_x.ScheduledEndDate <= a_endOfShortTermDate)
                                          .Sum(a_x => a_x.Qty);
            return shortTermOutput;
        }
        catch
        {
            //Return -1 in case of an overflow exception
            return -1;
        }
    }

    internal static long GetAverageLeadTimeTicks(JobManager a_jm)
    {
        try
        {
            long totalLeadTime = a_jm.Where(a_x => a_x.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
                                     .Sum(a_x => a_x.LeadTime.Ticks);
            long totalScheduled = GetScheduledJobsCount(a_jm);
            return totalScheduled == 0 ? 0 : totalLeadTime / totalScheduled;
        }
        catch
        {
            //Return -1 in case of an overflow exception
            return -1;
        }
    }

    internal static int GetUsersWhoOptimize(Scenario a_S)
    {
        HashSet<BaseId> userList = new ();
        using (a_S.UndoSetsLock.EnterRead(out Scenario.UndoSets uss))
        {
            for (int i = 0; i < uss.Count; i++)
            {
                Scenario.UndoSet us = uss[i];
                for (int j = 0; j < us.Count; j++)
                {
                    Scenario.TransmissionJar jar = us[j];
                    BaseId instigator = jar.TransmissionInfo.Instigator;

                    if (jar.TransmissionInfo.IsInternal || instigator == BaseId.ERP_ID || instigator == BaseId.NULL_ID)
                    {
                        continue;
                    }

                    if (jar.TransmissionInfo.TransmissionUniqueId == ScenarioDetailOptimizeT.UNIQUE_ID)
                    {
                        userList.AddIfNew(instigator);
                    }
                }
            }
        }

        return userList.Count;
    }

    internal static decimal GetSalesRevenue(JobManager a_jm, DateTime a_clockDate, int a_days)
    {
        try
        {
            decimal salesRevenue = a_jm.Where(a_x =>
                                           a_x.ScheduledEndDate >= a_clockDate &&
                                           a_x.ScheduledEndDate <= a_clockDate.AddDays(a_days))
                                       .Sum(a_x => a_x.Revenue);
            return salesRevenue;
        }
        catch
        {
            //Return -1 in case of an overflow exception
            return -1;
        }
    }

    internal static int GetSalesOrderCount(SalesOrderManager a_sm)
    {
        return a_sm.Count(a_x => !a_x.Cancelled);
    }

    internal static int GetPurchaseOrderCount(PurchaseToStockManager a_pm)
    {
        return a_pm.Count(a_x => !a_x.Closed);
    }

    internal static int GetScheduledJobsCount(JobManager a_jm)
    {
        return a_jm.Count(a_x => a_x.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled && !a_x.Template);
    }

    internal static int GetOperationsCount(JobManager a_jm)
    {
        return a_jm.Where(a_x => !a_x.Template).Sum(a_x => a_x.OperationCount);
    }

    internal static int GetFailedToSchedule(JobManager a_jm)
    {
        return a_jm.Count(a_x => a_x.ScheduledStatus == JobDefs.scheduledStatuses.FailedToSchedule);
    }

    internal static int GetJobsPastHorizon(JobManager a_jm)
    {
        return a_jm.Count(a_x => a_x.PastPlanningHorizon);
    }

    internal static int GetNumberOfUsers()
    {
        HashSet<BaseId> userList = new ();
        using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
        {
            foreach (User user in um)
            {
                if (user.Id == BaseId.ERP_ID || user.Id == BaseId.NULL_ID || userList.Contains(user.Id))
                {
                    continue;
                }

                userList.Add(user.Id);
            }
        }

        return userList.Count;
    }

    internal static DateTime GetLastAutomaticAction(Scenario a_s)
    {
        using (a_s.UndoSetsLock.EnterRead(out Scenario.UndoSets uss))
        {
            Scenario.UndoSet us = null;
            for (int i = uss.Count - 1; i >= 0; i--)
            {
                us = uss[i];

                if (us != null && us.Count > 0)
                {
                    for (int j = us.Count - 1; j >= 0; j--)
                    {
                        Scenario.TransmissionJar jar = us[j];
                        BaseId instigator = jar.TransmissionInfo.Instigator;

                        //TODO: V12
                        //if (instigator == BaseId.SCHEDULING_AGENT_ID)
                        //{
                        //    return jar.TransmissionInfo.TimeStamp;
                        //}
                    }
                }
            }
        }

        return PTDateTime.MinDateTime;
    }

    internal static DateTime GetLastManualAction(Scenario a_s)
    {
        using (a_s.UndoSetsLock.EnterRead(out Scenario.UndoSets uss))
        {
            Scenario.UndoSet undoSet = null;
            for (int i = uss.Count - 1; i >= 0; i--)
            {
                undoSet = uss[i];
                if (undoSet != null && undoSet.Count > 0)
                {
                    for (int x = undoSet.Count - 1; x >= 0; x--)
                    {
                        Scenario.TransmissionJar jar = undoSet[x];
                        BaseId instigator = jar.TransmissionInfo.Instigator;

                        //Skip any system transmissions, actions triggered automatically, and actions trigged by the server.
                        if (jar.TransmissionInfo.IsInternal || instigator == BaseId.NULL_ID) //Server id is null id
                        {
                            continue;
                        }

                        return jar.TransmissionInfo.TimeStamp.ToDateTime();
                    }
                }
            }
        }

        return PTDateTime.MinDateTime;
    }

    public DateTime LastAutomaticAction => m_lastAutomaticAction;

    public DateTime LastManualAction => m_lastManualAction;

    public int NumberOfUsers => m_numberOfUsers;

    public int JobsPastHorizon => m_jobsPastHorizon;

    public int FailedToScheduleCount => m_failedToScheduleCount;

    public int OperationsCount => m_operationsCount;

    public int ScheduledOrderCount => m_scheduledOrderCount;

    public int PurchaseOrderCount => m_purchaseOrderCount;

    public int SalesOrderCount => m_salesOrderCount;

    public decimal SalesRevenue120 => m_salesRevenue120;

    public decimal SalesRevenue90 => m_salesRevenue90;

    public decimal SalesRevenue60 => m_salesRevenue60;

    public decimal SalesRevenue30 => m_salesRevenue30;

    public int UsersWhoOptimize => m_usersWhoOptimize;

    public long AverageLeadTimeTicks => m_averageLeadTimeTicks;

    public decimal OutputPerWeek => m_outputPerWeek;

    public decimal OverdueJobs => m_overdueJobs;

    public decimal OnTimeJobs => m_onTimeJobs;
}