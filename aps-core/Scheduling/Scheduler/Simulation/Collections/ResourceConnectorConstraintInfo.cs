namespace PT.Scheduler.Simulation.Collections;

internal class ResourceConnectorConstraintInfo
{
    private readonly List<ResourceConnector> m_resourceConnectors;
    internal BaseResource PredecessorResource;
    internal InternalOperation PredecessorOperation;
    internal int OperationResourceRequirementIdx;

    /// <summary>
    /// The start date to use to calculate the begin of transfer. This is when the successor was released
    /// This will account for overlaps and transfer
    /// </summary>
    internal long PredecessorReleaseTicks;

    internal ResourceConnectorConstraintInfo()
    {
    }

    internal ResourceConnectorConstraintInfo(BaseResource a_predConnectorRes, InternalOperation a_predecessorOp)
    {
        PredecessorResource = a_predConnectorRes;
        PredecessorOperation = a_predecessorOp;
        m_resourceConnectors = new List<ResourceConnector>();
    }

    internal List<ResourceConnector> ResourceConnectors => m_resourceConnectors;

    public void AddPotentialConnector(ResourceConnector a_connector)
    {
        m_resourceConnectors.Add(a_connector);
    }
}