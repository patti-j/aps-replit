using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.InventoryManagement.Adjustment
{
    public class AdjustmentCondensed : IAdjustment
    {
        public AdjustmentCondensed(Scheduler.Adjustment a_adjustment)
        {
            Time = a_adjustment.Time;
            Inventory = a_adjustment.Inventory;
            ChangeQty = a_adjustment.ChangeQty;
            AdjustmentType = a_adjustment.AdjustmentType;
            ReasonDescription = a_adjustment.GetAdjustmentReason();
            ReasonPriority = a_adjustment.ReasonPriority;
            m_reason = a_adjustment.GetReason();

            HasStorage = a_adjustment.HasStorage;
            HasLotStorage = a_adjustment.HasLotStorage;
            Storage = a_adjustment.Storage;

            if (a_adjustment is LotExpirationAdjustment lotExpiration)
            {
                SaveExpiredMaterial = lotExpiration.SaveExpiredMaterial;
            }
        }

        public bool SaveExpiredMaterial { get; }

        public long Time { get; }
        public Inventory Inventory { get; }
        public decimal ChangeQty { get; set; }
        public InventoryDefs.EAdjustmentType AdjustmentType { get; }
        public string ReasonDescription { get; }
        public int ReasonPriority { get; }
        private readonly BaseIdObject m_reason;
        public BaseIdObject GetReason()
        {
            return m_reason;
        }
        public bool HasStorage { get; }
        public bool HasLotStorage { get; }
        public StorageAdjustment Storage { get; }
    }
}
