using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class BatchManager
{
    /// <summary>
    /// Create a new batch during simulation.
    /// </summary>
    /// <param name="a_si">Schedulable info</param>
    /// <param name="a_rrSatiators">The resources being scheduled for the resource requirements.</param>
    /// <param name="a_primaryResource"></param>
    /// <param name="a_actProductionStatus"></param>
    /// <param name="a_useTank"></param>
    /// <returns>Created Batch</returns>
    internal Batch CreateBatch(SchedulableInfo a_si, ResourceSatiator[] a_rrSatiators, Resource a_primaryResource, InternalActivityDefs.productionStatuses a_actProductionStatus)
    {
        Batch batch = new Batch(m_batchIdManager.NextID(), a_si, a_rrSatiators, a_primaryResource);

        Add(batch);
        return batch;
    }

    /// <summary>
    /// Create a batch based on an existing batch, such as when an activity is split.
    /// </summary>
    /// <param name="a_si"></param>
    /// <param name="a_sourceBatch"></param>
    /// <param name="a_primaryResource"></param>
    /// <param name="a_actProductionStatus"></param>
    /// <returns></returns>
    internal Batch CreateBatch(SchedulableInfo a_si, Batch a_sourceBatch, Resource a_primaryResource, InternalActivityDefs.productionStatuses a_actProductionStatus)
    {
        Batch batch = new Batch(m_batchIdManager.NextID(), a_si, a_sourceBatch, a_primaryResource);

        Add(batch);
        return batch;
    }
}