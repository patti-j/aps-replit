namespace PT.Scheduler;

public partial class ScenarioDetail
{
    /// <summary>
    /// Details of a simulation specific to batching
    /// </summary>
    private class BatchSimDetails
    {
        internal MoveMergeDetails MoveMerge { get; set; }

        internal class MoveMergeDetails
        {
            internal Batch MergeIntoBatch { get; set; }
        }
    }

    /// <summary>
    /// Used to group details about a specific aspect of a simulation.
    /// </summary>
    private class SimDetailsGroupings
    {
        internal BatchSimDetails BatchSimDetails { get; set; }
    }
}