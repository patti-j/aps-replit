using PT.APSCommon;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Simulation.Customizations.Material;

namespace PT.Scheduler.Demand;

public partial class TransferOrderDistribution : ISourceQtyData, ILotGeneratorObject, ISupplySource
{
    public override string ToString()
    {
        return string.Format("Qty: {0}; Send: {1}; Receive: {2}", QtyOrdered, DateTimeHelper.ToLocalTimeFromUTCTicks(ScheduledShipDateTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(ScheduledReceiveDateTicks));
    }

    /// <summary>
    /// Whether the lot can be used to satisfy this material requirement.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <returns></returns>
    internal bool IsLotElig(Lot a_lot, object a_data)
    {
        if (a_lot == null && MustUseEligLot)
        {
            return false;
        }

        return m_eligibleLots.IsLotElig(a_lot, (EligibleLots)a_data);
    }

    /// <summary>
    /// Whether the MaterialRequirement must be supplied by the lots specified as Eligible Lots.
    /// </summary>
    public bool MustUseEligLot => m_eligibleLots.Count > 0;

    /// <summary>
    /// Not used by transfer order distributions, hence it always returns true.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal bool IsUsable(Lot a_lot, object args)
    {
        return true;
    }

    /// <summary>
    /// Whether a lot code is contained in the set of lots eligible to fulfill this object.
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <returns></returns>
    public bool ContainsEligibleLot(string a_lotCode)
    {
        return m_eligibleLots.Contains(a_lotCode);
    }

    private readonly SourceQtyData m_sourceQtyData = new ();

    /// <summary>
    /// A simulation only value used to keep track of quantity for some of the IMaterialAllocation functionality.
    /// </summary>
    public SourceQtyData SourceQtyData => m_sourceQtyData;

    internal void ResetSimulationStateVariables()
    {
        m_sourceQtyData.ResetSimulationStateVariables();
        m_demandAdjustments.Clear();
        m_simAdjustments.Clear();
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
        BaseId lotId;
        if (m_generatedLotIds.TryGetValue(a_inventoryId, out lotId))
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

    public Adjustment GenerateDiscardAdjustment(long a_simClock, decimal a_excess)
    {
        return new TransferOrderDiscardAdjustment(ToInventory, a_simClock, -a_excess, this);
    }

    private readonly AdjustmentArray m_demandAdjustments; //Adjustments that don't affect scheduling such as MRP demands
    private readonly AdjustmentArray m_simAdjustments; //Adjustments that are stored on another object (Lot)

    public AdjustmentArray Adjustments
    {
        get
        {
            AdjustmentArray combinedArray = new ();
            combinedArray.Add(m_demandAdjustments);
            combinedArray.Add(m_simAdjustments);
            return combinedArray;
        }
    }

    internal void LinkSimAdjustment(TransferOrderAdjustment a_adjustment)
    {
        m_simAdjustments.Add(a_adjustment);
    }

    internal Adjustment GenerateDemandAdjustment(Inventory a_inv)
    {
        TransferOrderMrpDemandAdjustment demandAdjustment = new (this, a_inv, -QtyOpenToShip);
        m_demandAdjustments.Add(demandAdjustment);
        return demandAdjustment;
    }

    internal void SimulationActionComplete()
    {
        //Sort adjustments now so when accessed they don't sort and get collection modified exceptions
        m_demandAdjustments.Sort();
        m_simAdjustments.Sort();
    }
}