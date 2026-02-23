using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.Customizations;

public class CapacityIntervalChanger
{
    private readonly List<Tuple<CapacityIntervalsCollection, CapacityInterval>> m_deleteIntervals;
    private readonly List<Tuple<CapacityIntervalManager, CapacityIntervalAddHelper>> m_addIntervals;
    private readonly List<CapacityIntervalValueChanges> m_changeIntervals;

    public CapacityIntervalChanger()
    {
        m_deleteIntervals = new List<Tuple<CapacityIntervalsCollection, CapacityInterval>>();
        m_addIntervals = new List<Tuple<CapacityIntervalManager, CapacityIntervalAddHelper>>();
        m_changeIntervals = new List<CapacityIntervalValueChanges>();
    }

    public void AddToDeleteSet(CapacityIntervalsCollection a_capSet, CapacityInterval a_interval)
    {
        m_deleteIntervals.Add(new Tuple<CapacityIntervalsCollection, CapacityInterval>(a_capSet, a_interval));
    }

    /// <summary>
    /// Adds the changes for processesing later.
    /// </summary>
    /// <param name="a_capSet"></param>
    /// <param name="a_changesHelper"></param>
    public void AddToAddSet(CapacityIntervalManager a_capSet, CapacityIntervalAddHelper a_changesHelper)
    {
        m_addIntervals.Add(new Tuple<CapacityIntervalManager, CapacityIntervalAddHelper>(a_capSet, a_changesHelper));
        //if (a_updateImmediately)
        //{
        //    ExecuteAdds();
        //}
    }

    public void AddToChangeSet(CapacityIntervalValueChanges a_intervalChanges)
    {
        m_changeIntervals.Add(a_intervalChanges);
    }

    /// <summary>
    /// Perform all pending deletes
    /// </summary>
    internal void ExecuteDeletes()
    {
        foreach (Tuple<CapacityIntervalsCollection, CapacityInterval> deleteInterval in m_deleteIntervals)
        {
            deleteInterval.Item1.Remove(deleteInterval.Item2);
        }

        m_deleteIntervals.Clear();
    }

    /// <summary>
    /// Add and update all capaccity intervals added to AddSet so far.
    /// </summary>
    internal void ExecuteAdds()
    {
        foreach (Tuple<CapacityIntervalManager, CapacityIntervalAddHelper> ciData in m_addIntervals)
        {
            CapacityInterval newCi = ciData.Item1.AddNew(ciData.Item2.IntervalType,
                ciData.Item2.Scope,
                ciData.Item2.PlantId,
                ciData.Item2.DepartmentId,
                ciData.Item2.ResourceId,
                ciData.Item2.IntervalStart,
                ciData.Item2.IntervalEnd,
                IntervalProfile.DefaultProfile,
                ciData.Item2.IntervalColor,
                ciData.Item2.CanDragAndResize,
                ciData.Item2.CanDelete); //TODO: Update this whole process to pass the interval profile values
            CapacityIntervalValueChanges ciChanges = new (newCi);
            if (!string.IsNullOrEmpty(ciData.Item2.Name))
            {
                ciChanges.Name = ciData.Item2.Name;
            }

            if (!string.IsNullOrEmpty(ciData.Item2.Description))
            {
                ciChanges.Description = ciData.Item2.Description;
            }

            if (!string.IsNullOrEmpty(ciData.Item2.Notes))
            {
                ciChanges.Notes = ciData.Item2.Notes;
            }

            ciChanges.Execute();
        }

        m_addIntervals.Clear();
    }

    /// <summary>
    /// Update all pending changes to all capacity intervals
    /// </summary>
    internal void ExecuteChanges()
    {
        foreach (CapacityIntervalValueChanges change in m_changeIntervals)
        {
            change.Execute();
        }

        m_changeIntervals.Clear();
    }
}