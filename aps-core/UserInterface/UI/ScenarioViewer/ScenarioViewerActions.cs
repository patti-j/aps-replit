using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.PackageDefinitionsUI.UserSettings;
using PT.Scheduler;
using PT.SchedulerData;
using PT.SchedulerExtensions.Job;
using PT.Transmissions;
using PT.UIDefinitions;

namespace PT.UI.ScenarioViewer;

public partial class ScenarioViewer
{
    #region Action Dialogs
    //private void ConfiguratorControl_RecurringCapacityAssignedResourcesClicked(object sender, BaseId rciId)
    //{
    //    ganttEventProcessor.ConfiguratorControl_RecurringCapacityAssignedResourcesClicked(sender, rciId);
    //}

    //private void ConfiguratorControl_CapacityAssignedResourcesClicked(object sender, BaseId ciId)
    //{
    //    ganttEventProcessor.ConfiguratorControl_CapacityAssignedResourcesClicked(sender, ciId);
    //}

    internal void BrowseAlerts()
    {
        //try
        //{
        //    ScenarioManager sm;
        //    using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        //    {
        //        Scenario s = sm.Find(m_scenarioInfo.ScenarioId);

        //        ScenarioDetail sd;
        //        using (s.ScenarioDetailLock.TryEnterRead(out sd, 1000))
        //        {
        //            MultiLevelHourglass.TurnOn();

        //            BrowseAlerts dlg = new BrowseAlerts();
        //            dlg.Populate(sd);
        //            dlg.Owner = ParentForm;
        //            dlg.Show();
        //        }
        //    }
        //}
        //catch (AutoTryEnterException)
        //{
        //    m_messageProvider.ShowBusyMessage();
        //}
    }

    //TODO: V12 Make sure the extensions dialog is in a package, then remove
    public void ShowAddInsForm()
    {
        //MultiLevelHourglass.TurnOn();
        //try
        //{
        //    InternalAddInsForm dlg = new InternalAddInsForm(m_scenarioInfo);
        //    ScenarioManager sm;
        //    using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        //    {
        //        Scenario s = sm.Find(m_scenarioInfo.ScenarioId);

        //        ScenarioDetail sd;
        //        using (s.ScenarioDetailLock.TryEnterRead(out sd, 1000))
        //        {
        //            dlg.Populate(sm, sd, this);
        //        }
        //    }
        //    dlg.Owner = ParentForm;
        //    dlg.Show();
        //}
        //catch (AutoTryEnterException)
        //{
        //    m_messageProvider.ShowBusyMessage();
        //}
        //MultiLevelHourglass.TurnOff(false);
    }

    internal void WatchLateOverdueJobs()
    {
        try
        {
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
            {
                Scenario s = sm.Find(m_scenarioInfo.ScenarioId);
                using (s.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, 1000))
                {
                    IEnumerable<Job> jobList = sd.JobManager.GetLateJobs();
                    if (jobList.Any())
                    {
                        BaseIdList jobIdList = new ();
                        foreach (Job job in jobList)
                        {
                            jobIdList.Add(job.Id);
                        }

                        FireUINavEvent(jobIdList);
                    }
                    else
                    {
                        m_messageProvider.ShowMessage("There are no Late or Overdue Jobs.");
                    }
                }
            }
        }
        catch (AutoTryEnterException)
        {
            m_messageProvider.ShowBusyMessage();
        }
    }

    private void FireUINavEvent(BaseIdList a_jobList)
    {
        UINavigationEvent navEvent = new (UINavigationEvent.WatchJobsKey);
        navEvent.Data.Add(UINavigationEvent.WatchJobsKey, a_jobList);

        m_mainForm.FireNavigationEvent(navEvent);
    }

    internal void WatchOverdueJobs()
    {
        try
        {
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 1000))
            {
                Scenario s = sm.Find(m_scenarioInfo.ScenarioId);
                using (s.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, 1000))
                {
                    IEnumerable<Job> jobList = sd.JobManager.GetOverdueJobs();
                    if (jobList.Any())
                    {
                        BaseIdList jobIdList = new ();
                        foreach (Job job in jobList)
                        {
                            jobIdList.Add(job.Id);
                        }

                        FireUINavEvent(jobIdList);
                    }
                    else
                    {
                        m_messageProvider.ShowMessage("There are no Overdue Jobs.");
                    }
                }
            }
        }
        catch (AutoTryEnterException)
        {
            m_messageProvider.ShowBusyMessage();
        }
    }
    #endregion Action Dialogs

    private static void ShowActionGetUserString(UserManager a_um, params object[] a_params)
    {
        ScenarioBaseT a_t = (ScenarioBaseT)a_params[0];
        List<string> returnString = (List<string>)a_params[1];
        string userStr;
        if (a_t.Instigator == BaseId.ERP_ID)
        {
            userStr = string.Format("{0}: {1}", Localizer.GetString("Data Import"));
        }
        else if (a_t.Instigator == BaseId.NULL_ID)
        {
            userStr = string.Format("{0}", Localizer.GetString("System Action"));
        }
        else
        {
            User instigatorUser = a_um.GetById(a_t.Instigator);
            if (instigatorUser != null)
            {
                userStr = string.Format("{0}: {1}", Localizer.GetString("User"), instigatorUser.Name);
            }
            else
            {
                userStr = string.Format("{0}: {1}", Localizer.GetString("User"), Localizer.GetString("UNKNOWN"));
            }
        }

        returnString.Add(userStr);
    }

    private void GetScenarioString(ScenarioManager a_sm, Scenario a_s, ScenarioDetail a_sd, params object[] a_params)
    {
        ScenarioBaseT transmission = (ScenarioBaseT)a_params[0];
        List<string> returnString = (List<string>)a_params[1];
        List<Tuple<ResourceOperation, MoveBlockKeyData>> ops = (List<Tuple<ResourceOperation, MoveBlockKeyData>>)a_params[2];
        List<Job> jobs = (List<Job>)a_params[3];
        if (a_sm.LoadedScenarioCount > 1)
        {
            returnString.Add(string.Format("{0}: {1}", Localizer.GetString("Scenario"), a_s.Name));
        }

        if (transmission is ScenarioDetailMoveT)
        {
            ops.AddRange(ShowActionGetMoveTOperations((ScenarioDetailMoveT)transmission, a_sd));
        }
        else if (transmission is JobT) //JobT
        {
            jobs.AddRange(a_sd.JobManager.GetAllNewJobs());
        }
    }

    private static List<Tuple<ResourceOperation, MoveBlockKeyData>> ShowActionGetMoveTOperations(ScenarioDetailMoveT a_moveT, ScenarioDetail a_sd)
    {
        List<Tuple<ResourceOperation, MoveBlockKeyData>> ops = new ();

        using (IEnumerator<MoveBlockKeyData> moveBlockIterator = a_moveT.GetEnumerator())
        {
            while (moveBlockIterator.MoveNext())
            {
                MoveBlockKeyData currentBlock = moveBlockIterator.Current;
                ResourceOperation op = (ResourceOperation)a_sd.JobManager.FindOperation(currentBlock.BlockKey.JobId, currentBlock.BlockKey.MOId, currentBlock.BlockKey.OperationId);

                ops.Add(new Tuple<ResourceOperation, MoveBlockKeyData>(op, currentBlock));
            }
        }

        return ops;
    }

    internal void ShowCollaborativeUserAction(BaseId a_scenarioId, ScenarioBaseT a_t)
    {
        List<Tuple<ResourceOperation, MoveBlockKeyData>> ops = new ();
        List<Job> jobs = new ();

        string msg;
        string actionDescStr = "";

        PTCorePreferences corePreferences = m_mainForm.UserPreferenceInfo.LoadSetting(new PTCorePreferences());
        if (!corePreferences.ShowOtherUserActionMessages || a_t is ScenarioChecksumT or CoPilotStatusUpdateT)
        {
            //Don't show any messages if collaboration is turned off or the transmission is not important
            return;
        }

        List<string> returnString = new ();
        using (BackgroundLock asyncLock = new (m_scenarioInfo.ScenarioId))
        {
            asyncLock.RunLockCodeBackground(ShowActionGetUserString, a_t, returnString);
            if (asyncLock.Status == BackgroundLock.EResultStatus.Error)
            {
                //Some error has occurred.
                return;
            }
        }

        string userStr = returnString[0];

        using (BackgroundLock asyncLock = new (m_scenarioInfo.ScenarioId))
        {
            asyncLock.RunLockCodeBackground(GetScenarioString, a_t, returnString, ops, jobs);
            if (asyncLock.Status == BackgroundLock.EResultStatus.Error)
            {
                //Some error has occurred.
                return;
            }
        }

        string scenarioStr = "";
        if (returnString.Count > 1)
        {
            scenarioStr = returnString[1];
        }

        if (a_t is ScenarioDetailMoveT)
        {
            if (ops.Count == 0)
            {
                return;
            }

            ScenarioDetailMoveT moveT = (ScenarioDetailMoveT)a_t;

            foreach (Tuple<ResourceOperation, MoveBlockKeyData> opBlock in ops)
            {
                ResourceOperation op = opBlock.Item1;
                MoveBlockKeyData currentBlock = opBlock.Item2;
                ResourceBlock block = (ResourceBlock)op.Activities.FindBlock(currentBlock.BlockKey.ActivityId, currentBlock.BlockKey.BlockId);
                if (block != null) //in case the move called a failed schedule of the job
                {
                    string latenessStr;
                    if (op.ManufacturingOrder.Late)
                    {
                        latenessStr = string.Format("{0} {1:N} {2}", Localizer.GetString("LATE"), Math.Round(op.ManufacturingOrder.Lateness.TotalDays, 1), Localizer.GetString("days"));
                    }
                    else
                    {
                        latenessStr = Localizer.GetString("On-Time");
                    }

                    Job job = op.ManufacturingOrder.Job;
                    string customerString = job.Customers.Count > 1 ? "Customers".Localize() : "Customer".Localize();

                    if (currentBlock.ResourceKey.Equals(moveT.ToResourceKey))
                    {
                        actionDescStr += string.Format("{0}:{11}\t{1}:{2}{11}\t{3}:{4}{11}\t{5}:{6}{11}\t{7}:{8}{11}\t{9}:{10}",
                            Localizer.GetString("Moved Operation to a different time"),
                            Localizer.GetString("Operation"),
                            op.Name,
                            Localizer.GetString("Job"),
                            job.Name,
                            Localizer.GetString(customerString),
                            job.Customers.GetCustomerNamesList(),
                            Localizer.GetString("New Start"),
                            block.StartDateTime.ToDisplayTime(),
                            Localizer.GetString("Status"),
                            latenessStr,
                            Environment.NewLine);
                    }
                    else
                    {
                        actionDescStr += string.Format("{0}:{13}\t{1}:{2}{13}\t{3}:{4}{13}\t{5}:{6}{13}\t{7}:{8}{13}\t{9}:{10}{13}\t{11}:{12}",
                            Localizer.GetString("Moved Operation to a different Resource"),
                            Localizer.GetString("Operation"),
                            op.Name,
                            Localizer.GetString("Job"),
                            job.Name,
                            Localizer.GetString(customerString),
                            job.Customers.GetCustomerNamesList(),
                            Localizer.GetString("New Start"),
                            block.StartDateTime.ToDisplayTime(),
                            Localizer.GetString("New Resource"),
                            block.ResourceUsed,
                            Localizer.GetString("Status"),
                            latenessStr,
                            Environment.NewLine);
                    }
                }

                actionDescStr += Environment.NewLine;
            }
        }
        else if (a_t is JobT)
        {
            if (jobs.Count == 0)
            {
                return;
            }

            JobT jobT = (JobT)a_t;

            StringBuilder descBuilder = new ();
            descBuilder.Append(string.Format("{0} {1} {2} {3}",
                jobT.Count,
                Localizer.GetString("Jobs were imported."),
                Localizer.GetString("New Jobs count:"),
                jobs.Count
            ));
            if (jobs.Count < 10)
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    Job job = jobs[i];
                    descBuilder.Append(string.Format("{10}\t{0}:{1}{10}\t\t{2}:{3}{10}\t\t{4}:{5}{10}\t\t{6}:{7:N}{10}\t\t{8}:{9}",
                        Localizer.GetString("Job"),
                        job.Name,
                        Localizer.GetString("Customer"),
                        job.Customers.GetCustomerExternalIdsList(),
                        Localizer.GetString("Product"),
                        job.Product,
                        Localizer.GetString("Qty"),
                        job.Qty,
                        Localizer.GetString("NeedDate"),
                        job.NeedDateTime.ToDisplayTime(),
                        Environment.NewLine));
                }
            }

            actionDescStr = descBuilder.ToString();
        }
        else
        {
            actionDescStr = Localizer.GetString(a_t.Description);
        }

        if (actionDescStr != Transmission.DEFAULT_DESCRIPTION)
        {
            msg = string.Format("{0}{3}{1}{3}{2}", actionDescStr, userStr, scenarioStr, Environment.NewLine);
        }
        else
        {
            msg = string.Format("{0}{2}{1}", userStr, scenarioStr, Environment.NewLine);
        }

        BeginInvoke(new Action(() => InvokeMessageProvider(msg)));
    }
}