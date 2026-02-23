using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// This represents a followup action that needs to be run after all the data has been updated.
/// For example, we may need to auto delete some SOs, but we need to make sure Jobs have been updated first
/// </summary>
internal class PostProcessingAction
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a_transmission">The triggering transmission</param>
    /// <param name="a_alwaysRun">Whether to run this action even if there are exceptions processing the data</param>
    /// <param name="a_action">The action to run</param>
    internal PostProcessingAction(PTTransmission a_transmission, bool a_alwaysRun, Action a_action)
    {
        PostAction = a_action;
        PTTransmission = a_transmission;
        AlwaysRun = a_alwaysRun;
    }

    internal Action PostAction;
    internal PTTransmission PTTransmission;

    /// <summary>
    /// Whether this action should be run even if there were exceptions 
    /// </summary>
    internal bool AlwaysRun;
}