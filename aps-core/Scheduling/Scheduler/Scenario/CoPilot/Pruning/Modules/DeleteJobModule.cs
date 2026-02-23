using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

/// <summary>
/// Deletes a Job in a given scenario
/// </summary>
internal class DeleteJobModule : BaseDeleteModule, PruneScenario.IPruneScenarioRecursiveModule
{
    public bool Prune(Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        using (a_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            Transmissions.JobDeleteJobsT delT = new (a_scenario.Id, m_jobKeyGroupList[m_listIdx]);
            sd.Receive(delT, a_dataChanges);
            return true;
        }
    }

    protected override bool PopulateGroupLists(Scenario a_scenario)
    {
        //Break the remaining jobs into groups based on the group modifier
        ScenarioDetail sd;
        using (a_scenario.ScenarioDetailLock.EnterWrite(out sd))
        {
            //End conditions
            //All jobs deleted
            if (sd.JobManager.Count == 0)
            {
                return false;
            }

            //Jobs remain, but we are attempting to delete them one by one
            if (m_groupModifier >= sd.JobManager.Count)
            {
                //We already treid deleting them individually, don't try anymore
                if (m_lastGroupSplit)
                {
                    return false;
                }

                m_lastGroupSplit = true;
            }

            //Determine jobs per group
            int groupCount = sd.JobManager.Count / m_groupModifier;
            groupCount = Math.Max(groupCount, 1);
            int listCount = 0;
            BaseIdList newList = new ();
            //For each remaining job, add it to a group
            for (int jobI = 0; jobI < sd.JobManager.Count; jobI++)
            {
                //This group is full, start a new group
                if (listCount == groupCount)
                {
                    listCount = 0;
                    m_jobKeyGroupList.Add(newList);
                    newList = new BaseIdList();
                }

                listCount++;
                newList.Add(sd.JobManager[jobI].Id);
            }

            //Add any remaining items that didn't fill a full group
            if (newList.Count > 0)
            {
                m_jobKeyGroupList.Add(newList);
            }
        }

        return true;
    }
}