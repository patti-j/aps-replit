using PT.APSCommon;
using PT.Scheduler.Simulation.Customizations.Material;

namespace PT.Scheduler;

public partial class PurchaseToStock : ISourceQtyData, ILotGeneratorObject, ISupplySource
{
    private readonly SourceQtyData m_sourceQtyData = new ();

    /// <summary>
    /// A simulation only value used to keep track of quantity for some of the IMaterialAllocation functionality.
    /// </summary>
    public SourceQtyData SourceQtyData => m_sourceQtyData;

    internal void ResetSimulationStateVariables()
    {
        m_sourceQtyData.ResetSimulationStateVariables();
    }

    /// <summary>
    /// Dictionary of previously created LotIds. Each LotId is associated with a source.
    /// To allow different simulations to produce the same BaseId for each Lot source,
    /// this dictionary isn't cleared between simulations. This collection is also serialized
    /// so clients also generate the same ids.
    /// </summary>
    private readonly Dictionary<BaseId, BaseId> m_generatedLotIds = new ();

    public BaseId CreateLotId(BaseId a_inventoryId, IIdGenerator a_idGen)
    {
        if (m_generatedLotIds.TryGetValue(a_inventoryId, out BaseId lotId))
        {
            return lotId;
        }

        lotId = a_idGen.NextID();
        m_generatedLotIds.Add(a_inventoryId, lotId);
        return lotId;
    }

    private Lot m_producedLot;
    Lot ISupplySource.SupplyLot => m_producedLot;

    public void LinkCreatedLot(Lot a_newLot)
    {
        m_producedLot = a_newLot;
    }

    //TODO: I'm not sure if this is right
    public long MaterialPostProcessingTicks => AvailableDateTicks - m_actualReceiptDate;
    
    public Adjustment GenerateDiscardAdjustment(long a_simClock, decimal a_excess)
    {
        return new PurchaseOrderDiscardAdjustment(Inventory, a_simClock, -a_excess, this);
    }

    public Adjustment GenerateAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea)
    {
        throw new NotImplementedException();
    }

    public int WearDurability => 0; //TODO: Storage Areas
    
    
}