using PT.APSCommon;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Demand;

public class TransferOrder : BaseObject, IPTSerializable
{
    #region IPTSerializable Members
    public TransferOrder(IReader reader, BaseIdGenerator aIdGen)
        : base(reader)
    {
        if (reader.VersionNumber >= 725)
        {
            reader.Read(out closed);
            reader.Read(out priority);
            _distributions = new TransferOrderDistributionManager(reader, aIdGen, this);
            bools = new BoolVector32(reader);

            reader.Read(out int val);
            m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(closed);
        writer.Write(priority);
        _distributions.Serialize(writer);
        bools.Serialize(writer);
        writer.Write((int)m_maintenanceMethod);
    }

    public new const int UNIQUE_ID = 647;

    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(WarehouseManager warehouses, ItemManager items)
    {
        _distributions.RestoreReferences(warehouses, items);
    }    
    
    internal void AfterRestoreAdjustmentReferences()
    {
        _distributions.AfterRestoreAdjustmentReferences();
    }
    #endregion

    internal TransferOrder(BaseId aId, TransferOrderT.TransferOrder tTO, ItemManager items, WarehouseManager warehouses, BaseIdGenerator aIdGen, ScenarioDetail a_sd, PTTransmission a_t, UserFieldDefinitionManager a_udfManager)
        : base(aId, tTO)
    {
        _distributions = new TransferOrderDistributionManager(aIdGen);
        UpdateHelper(tTO, items, warehouses, a_sd, a_t, a_udfManager);
    }

    internal void Update(UserFieldDefinitionManager a_udfManager, TransferOrderT.TransferOrder tTO, ItemManager items, WarehouseManager warehouses, ScenarioDetail a_sd, Transmissions.PTTransmission a_t)
    {
        UpdateHelper(tTO, items, warehouses, a_sd, a_t, a_udfManager);
    }

    internal void UpdateHelper(TransferOrderT.TransferOrder tTO, ItemManager items, WarehouseManager warehouses, ScenarioDetail a_sd, PTTransmission a_t, UserFieldDefinitionManager a_udfManager)
    {
        base.Update(tTO, a_t, a_udfManager, UserField.EUDFObjectType.TransferOrders);

        if (Firm != tTO.Firm)
        {
            Firm = tTO.Firm;
        }

        if (Closed != tTO.Closed)
        {
            Closed = tTO.Closed;
        }

        if (Priority != tTO.Priority)
        {
            Priority = tTO.Priority;
            Distributions.SetNetChangeMRPFlags();
        }

        MaintenanceMethod = tTO.MaintenanceMethod;

        _distributions.Update(tTO, items, warehouses, this, a_sd, a_t);
    }

    private readonly TransferOrderDistributionManager _distributions;

    public TransferOrderDistributionManager Distributions => _distributions;

    internal void DeletingOrClearingDistributions(ScenarioDetail a_sd, Transmissions.PTTransmissionBase a_t)
    {
        a_sd.PurchaseToStockManager.DeletingTransferOrderDistributions(this, a_t);
        a_sd.JobManager.DeletingDemand(this, a_t);
        foreach (TransferOrderDistribution dist in Distributions)
        {
            dist.Deleting();
        }
    }

    /// <summary>
    /// Removes specified distributions and their associated demands
    /// </summary>
    internal void DeleteDistributions(ScenarioDetail a_sd, BaseIdList a_distributionIdList, Transmissions.PTTransmissionBase a_t)
    {
        a_sd.PurchaseToStockManager.DeletingTransferOrderDistributions(this, a_distributionIdList, a_t);
        a_sd.JobManager.DeletingDemand(this, a_t, a_distributionIdList);
        foreach (BaseId baseId in a_distributionIdList)
        {
            Distributions.GetById(baseId)?.Deleting();
            Distributions.RemoveById(baseId);
        }
    }

    #region Shared Properties
    private bool closed;

    /// <summary>
    /// If Closed then the Transfer Order has no further affect on the Inventory Plan.
    /// </summary>
    public bool Closed
    {
        get => closed;
        internal set => closed = value;
    }

    private int priority;

    /// <summary>
    /// Sets the Priority for Jobs created by MRP to satisify this demand.
    /// </summary>
    public int Priority
    {
        get => priority;
        internal set => priority = value;
    }

    private BoolVector32 bools;
    private const int FirmIdx = 0;

    /// <summary>
    /// If the Purchase is Firm then the MRP logic will not modify or delete it.
    /// Users can still change Firm Purchases and imports can affect them.
    /// </summary>
    public bool Firm
    {
        get => bools[FirmIdx];
        internal set => bools[FirmIdx] = value;
    }

    private JobDefs.EMaintenanceMethod m_maintenanceMethod = JobDefs.EMaintenanceMethod.ERP;

    /// <summary>
    /// How the TransferOrder was entered into the system.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public JobDefs.EMaintenanceMethod MaintenanceMethod
    {
        get => m_maintenanceMethod;
        internal set => m_maintenanceMethod = value;
    }
    #endregion Shared Properties

    #region Miscellaneous
    [System.ComponentModel.Browsable(false)]
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    public override string DefaultNamePrefix => "Transfer Order";
    #endregion

    #region Simulation
    internal void ResetSimulationStateVariables()
    {
        Distributions.ResetSimulationStateVariables();
    }
    #endregion
}