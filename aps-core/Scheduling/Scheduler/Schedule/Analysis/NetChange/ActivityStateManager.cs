namespace PT.Scheduler.Schedule.Analysis.NetChange;

public class ActivityStateManager
{
    /// <summary>
    /// This also clears out any existing states.
    /// </summary>
    /// <param name="sd"></param>
    public void GetStartStates(ScenarioDetail sd)
    {
        Clear();
        GetValues(sd, true);
    }

    public void GetFinishStates(ScenarioDetail sd)
    {
        GetValues(sd, false);
    }

    private void GetValues(ScenarioDetail sd, bool startDates)
    {
        for (int jI = 0; jI < sd.JobManager.Count; ++jI)
        {
            Job job = sd.JobManager[jI];
            for (int mI = 0; mI < job.ManufacturingOrders.Count; ++mI)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[mI];
                AlternatePath path = mo.CurrentPath;
                IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = path.AlternateNodeSortedList.GetEnumerator();

                while (alternateNodesEnumerator.MoveNext())
                {
                    AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                    InternalOperation op = (InternalOperation)node.Operation;

                    for (int actI = 0; actI < op.Activities.Count; ++actI)
                    {
                        InternalActivity ia = op.Activities.GetByIndex(actI);
                        ActivityState state;

                        if (states.ContainsKey(ia))
                        {
                            state = states[ia];
                        }
                        else
                        {
                            state = new ActivityState(ia);
                            states.Add(ia, state);
                        }

                        for (int rrI = 0; rrI < ia.ResourceRequirementBlockCount; ++rrI)
                        {
                            ResourceBlock rb = ia.GetResourceRequirementBlock(rrI);
                            List<ResourceRequirementState> reqStates;

                            if (startDates)
                            {
                                reqStates = state.startStates;
                            }
                            else
                            {
                                reqStates = state.endStates;
                            }

                            if (rb != null && rb.Scheduled)
                            {
                                reqStates.Add(new ResourceRequirementState(rb));
                            }
                            else
                            {
                                reqStates.Add(new ResourceRequirementState());
                            }
                        }
                    }
                }
            }
        }
    }

    public List<ActivityState> DetermineNetChange()
    {
        List<ActivityState> netChanges = new ();
        Dictionary<InternalActivity, ActivityState>.Enumerator statesEnumerator = states.GetEnumerator();

        while (statesEnumerator.MoveNext())
        {
            ActivityState state = statesEnumerator.Current.Value;
            if (!state.EqualResourceRequirementStates())
            {
                netChanges.Add(state);
            }
        }

        return netChanges;
    }

    public static List<Job> GetChangedJobs(List<ActivityState> netChanges)
    {
        Dictionary<Job, Job> jobs = new ();
        for (int ncI = 0; ncI < netChanges.Count; ++ncI)
        {
            Job j = netChanges[ncI].ia.Job;
            if (!jobs.ContainsKey(j))
            {
                jobs.Add(j, j);
            }
        }

        List<Job> netChangeJobs = new ();
        Dictionary<Job, Job>.Enumerator en = jobs.GetEnumerator();
        while (en.MoveNext())
        {
            netChangeJobs.Add(en.Current.Key);
        }

        return netChangeJobs;
    }

    private readonly Dictionary<InternalActivity, ActivityState> states = new ();

    internal void Clear()
    {
        states.Clear();
    }
}