namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Provides change helper classes for the RequiredCapacity customization.
/// </summary>
public class RequiredCapacityChangeHelper
{
    private RequiredCapacityPlus m_requiredCapacityChanger;
    private readonly List<ActivityValueChanges> m_actChangesList;

    public RequiredCapacityChangeHelper()
    {
        m_actChangesList = new List<ActivityValueChanges>();
    }

    public RequiredCapacityPlus RequiredCapacityChanger
    {
        get => m_requiredCapacityChanger;
        internal set => m_requiredCapacityChanger = value;
    }

    /// <summary>
    /// Add pending changes for an activity
    /// </summary>
    /// <param name="a_actChanges"></param>
    public void AddActivityChanges(ActivityValueChanges a_actChanges)
    {
        m_actChangesList.Add(a_actChanges);
    }

    internal void Execute()
    {
        foreach (ActivityValueChanges actChange in m_actChangesList)
        {
            actChange.ExecuteChanges();
        }

        m_actChangesList.Clear();
    }
}