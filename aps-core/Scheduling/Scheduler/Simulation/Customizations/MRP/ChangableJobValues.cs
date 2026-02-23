using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.Customizations.MRP;

/// <summary>
/// For use with JobCreationCustomization.
/// Allows changes to some job values that can't be directly changed in the job.
/// </summary>
public class ChangableJobValues
{
    private readonly List<Tuple<ManufacturingOrder, Plant>> m_lockedPlants = new ();
    private readonly List<Tuple<InternalOperation, Resource>> m_defaultResources = new ();

    public List<Tuple<ManufacturingOrder, Plant>>.Enumerator GetLockedPlantsEnumerator()
    {
        return m_lockedPlants.GetEnumerator();
    }

    public List<Tuple<InternalOperation, Resource>>.Enumerator GetDefaultResourcesEnumerator()
    {
        return m_defaultResources.GetEnumerator();
    }

    /// <summary>
    /// To change the LockedPlant of a ManufacturingOrder, call this function  passing in the ManufacturingOrder and plant you want to lock it to.
    /// </summary>
    /// <param name="a_mo">The ManufacturingOrder whose LockedPlant you want to change.</param>
    /// <param name="a_lockedPlant">The plant you want to lock the ManufacturingOrder to. </param>
    public void AddLockedPlant(ManufacturingOrder a_mo, Plant a_lockedPlant)
    {
        m_lockedPlants.Add(new Tuple<ManufacturingOrder, Plant>(a_mo, a_lockedPlant));
    }

    public void AddDefaultResource(InternalOperation a_op, Resource a_resource)
    {
        m_defaultResources.Add(new Tuple<InternalOperation, Resource>(a_op, a_resource));
    }
}

/// <summary>
/// This class contains nullable values, that when set will be used to modify a PO
/// </summary>
public class ChangeablePoValues
{
    public DateTime? NeedDate;
    public string LotCode;
    public PurchaseToStockDefs.EMaintenanceMethod? MaintenanceMethod;
    public bool? Firm;
    public bool? Closed;
    public TimeSpan? TransferSpan;
    public decimal? QtyOrdered;
    public decimal? QtyReceived;
    public string BuyerExternalId;
    public string VendorExternalId;
}