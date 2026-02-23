//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using PTIntegrationAPI.Models.V1;

//namespace PTIntegrationAPI.DAL
//{
//    public partial class InstanceDBContext : DbContext
//    {
//        public InstanceDBContext(DbContextOptions options) : base(options)
//        {
//        }

//        public virtual DbSet<Capability> Capabilities { get; set; }

//        public virtual DbSet<CapacityInterval> CapacityIntervals { get; set; }

//        public virtual DbSet<CapacityIntervalResourceAssignment> CapacityIntervalResourceAssignments { get; set; }

//        public virtual DbSet<Department> Departments { get; set; }

//        public virtual DbSet<Forecast> Forecasts { get; set; }

//        public virtual DbSet<ForecastShipment> ForecastShipments { get; set; }

//        public virtual DbSet<ForecastShipmentInventoryAdjustment> ForecastShipmentInventoryAdjustments { get; set; }

//        public virtual DbSet<Inventory> Inventories { get; set; }

//        public virtual DbSet<Item> Items { get; set; }

//        public virtual DbSet<Job> Jobs { get; set; }

//        public virtual DbSet<JobActivity> JobActivities { get; set; }

//        public virtual DbSet<JobActivityInventoryAdjustment> JobActivityInventoryAdjustments { get; set; }

//        public virtual DbSet<JobMaterial> JobMaterials { get; set; }

//        public virtual DbSet<JobMaterialSupplyingActivity> JobMaterialSupplyingActivities { get; set; }

//        public virtual DbSet<JobOperation> JobOperations { get; set; }

//        public virtual DbSet<JobOperationAttribute> JobOperationAttributes { get; set; }

//        public virtual DbSet<JobPath> JobPaths { get; set; }

//        public virtual DbSet<JobPathNode> JobPathNodes { get; set; }

//        public virtual DbSet<JobProduct> JobProducts { get; set; }

//        public virtual DbSet<JobProductDeletedDemand> JobProductDeletedDemands { get; set; }

//        public virtual DbSet<JobProductForecastDemand> JobProductForecastDemands { get; set; }

//        public virtual DbSet<JobProductSafetyStockDemand> JobProductSafetyStockDemands { get; set; }

//        public virtual DbSet<JobProductSalesOrderDemand> JobProductSalesOrderDemands { get; set; }

//        public virtual DbSet<JobProductTransferOrderDemand> JobProductTransferOrderDemands { get; set; }

//        public virtual DbSet<JobResource> JobResources { get; set; }

//        public virtual DbSet<JobResourceBlock> JobResourceBlocks { get; set; }

//        public virtual DbSet<JobResourceBlockInterval> JobResourceBlockIntervals { get; set; }

//        public virtual DbSet<JobResourceCapability> JobResourceCapabilities { get; set; }

//        public virtual DbSet<JobSuccessorManufacturingOrder> JobSuccessorManufacturingOrders { get; set; }

//        public virtual DbSet<KPI> KPIs { get; set; }

//        public virtual DbSet<Lot> Lots { get; set; }

//        public virtual DbSet<ManufacturingOrder> ManufacturingOrders { get; set; }

//        public virtual DbSet<Metric> Metrics { get; set; }

//        public virtual DbSet<Plant> Plants { get; set; }

//        public virtual DbSet<PlantWarehouse> PlantWarehouses { get; set; }

//        public virtual DbSet<ProductRule> ProductRules { get; set; }

//        public virtual DbSet<PurchaseToStockDeletedDemand> PurchaseToStockDeletedDemands { get; set; }

//        public virtual DbSet<PurchaseToStockForecastDemand> PurchaseToStockForecastDemands { get; set; }

//        public virtual DbSet<PurchaseToStockInventoryAdjustment> PurchaseToStockInventoryAdjustments { get; set; }

//        public virtual DbSet<PurchaseToStockSafetyStockDemand> PurchaseToStockSafetyStockDemands { get; set; }

//        public virtual DbSet<PurchaseToStockSalesOrderDemand> PurchaseToStockSalesOrderDemands { get; set; }

//        public virtual DbSet<PurchaseToStockTransferOrderDemand> PurchaseToStockTransferOrderDemands { get; set; }

//        public virtual DbSet<PurchasesToStock> PurchasesToStocks { get; set; }

//        public virtual DbSet<RecurringCapacityInterval> RecurringCapacityIntervals { get; set; }

//        public virtual DbSet<RecurringCapacityIntervalRecurrence> RecurringCapacityIntervalRecurrences { get; set; }

//        public virtual DbSet<RecurringCapacityIntervalResourceAssignment> RecurringCapacityIntervalResourceAssignments { get; set; }

//        public virtual DbSet<ReportBlock> ReportBlocks { get; set; }

//        public virtual DbSet<ResourceCapability> ResourceCapabilities { get; set; }

//        public virtual DbSet<SalesOrder> SalesOrders { get; set; }

//        public virtual DbSet<SalesOrderDistributionInventoryAdjustment> SalesOrderDistributionInventoryAdjustments { get; set; }

//        public virtual DbSet<SalesOrderLine> SalesOrderLines { get; set; }

//        public virtual DbSet<SalesOrderLineDistribution> SalesOrderLineDistributions { get; set; }

//        public virtual DbSet<Schedule> Schedules { get; set; }

//        public virtual DbSet<SystemDatum> SystemData { get; set; }

//        public virtual DbSet<TransferOrder> TransferOrders { get; set; }

//        public virtual DbSet<TransferOrderDistribution> TransferOrderDistributions { get; set; }

//        public virtual DbSet<TransferOrderDistributionInventoryAdjustment> TransferOrderDistributionInventoryAdjustments { get; set; }

//        public virtual DbSet<Warehouse> Warehouses { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.HasDefaultSchema("import");
//            modelBuilder.Entity<Capability>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.CapabilityId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//            });

//            modelBuilder.Entity<CapacityInterval>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.CapacityIntervalId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.EndDateTime).HasColumnType("datetime");
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.IntervalType).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.StartDateTime).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<CapacityIntervalResourceAssignment>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.CapacityIntervalId, e.ResourceId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<Department>(entity =>
//            {
//                entity.Property(e => e.DepartmentId).HasMaxLength(10);
//                entity.Property(e => e.Name).HasMaxLength(60);
//                entity.Property(e => e.PlantId)
//                    .IsRequired()
//                    .HasMaxLength(10);
//            });

//            modelBuilder.Entity<Forecast>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.ForecastId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Customer).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.Planner).IsUnicode(false);
//                entity.Property(e => e.SalesOffice).IsUnicode(false);
//                entity.Property(e => e.SalesPerson).IsUnicode(false);
//                entity.Property(e => e.Version)
//                    .IsRequired()
//                    .IsUnicode(false);
//            });

//            modelBuilder.Entity<ForecastShipment>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.ForecastShipmentId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.ConsumptionDetails).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<ForecastShipmentInventoryAdjustment>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.InventoryId, e.ForecastShipmentId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentReason).IsUnicode(false);
//            });

//            modelBuilder.Entity<Inventory>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.InventoryId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.ForecastConsumption).IsUnicode(false);
//                entity.Property(e => e.ForecastInterval).IsUnicode(false);
//                entity.Property(e => e.MaterialAllocation).IsUnicode(false);
//                entity.Property(e => e.MrpNotes).IsUnicode(false);
//                entity.Property(e => e.MrpProcessing).IsUnicode(false);
//                entity.Property(e => e.PlannerExternalId).IsUnicode(false);
//            });

//            modelBuilder.Entity<Item>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.ItemId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AttributesSummary).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.ItemGroup).IsUnicode(false);
//                entity.Property(e => e.ItemType).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.Source).IsUnicode(false);
//            });

//            modelBuilder.Entity<Job>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AgentAlert).IsUnicode(false);
//                entity.Property(e => e.AgentEmail).IsUnicode(false);
//                entity.Property(e => e.Anchored).IsUnicode(false);
//                entity.Property(e => e.AttributesSummary).IsUnicode(false);
//                entity.Property(e => e.Bottlenecks).IsUnicode(false);
//                entity.Property(e => e.Classification).IsUnicode(false);
//                entity.Property(e => e.ColorCode).IsUnicode(false);
//                entity.Property(e => e.Commitment).IsUnicode(false);
//                entity.Property(e => e.CustomerAlert).IsUnicode(false);
//                entity.Property(e => e.CustomerEmail).IsUnicode(false);
//                entity.Property(e => e.CustomerExternalId).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.Destination).IsUnicode(false);
//                entity.Property(e => e.EarliestDelivery).HasColumnType("datetime");
//                entity.Property(e => e.EntryDate).HasColumnType("datetime");
//                entity.Property(e => e.EntryMethod).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.FailedToScheduleReason).IsUnicode(false);
//                entity.Property(e => e.HoldReason).IsUnicode(false);
//                entity.Property(e => e.HoldUntil).HasColumnType("datetime");
//                entity.Property(e => e.HotReason).IsUnicode(false);
//                entity.Property(e => e.LeadResource).IsUnicode(false);
//                entity.Property(e => e.Locked).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.NeedDateTime).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.OnHold).IsUnicode(false);
//                entity.Property(e => e.OnHoldReason).IsUnicode(false);
//                entity.Property(e => e.OrderNumber).IsUnicode(false);
//                entity.Property(e => e.Product).IsUnicode(false);
//                entity.Property(e => e.ProductDescription).IsUnicode(false);
//                entity.Property(e => e.ResourceNames).IsUnicode(false);
//                entity.Property(e => e.ScheduledEndDateTime).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledStartDateTime).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledStatus).IsUnicode(false);
//                entity.Property(e => e.Shipped).IsUnicode(false);
//                entity.Property(e => e.SuccessorOrderNumbers).IsUnicode(false);
//                entity.Property(e => e.TravelerReportFileName).IsUnicode(false);
//                entity.Property(e => e.Type).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobActivity>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.ManufacturingOrderId, e.JobId, e.OperationId, e.ActivityId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.ActualResourcesUsed).IsUnicode(false);
//                entity.Property(e => e.AnchorStartDate).HasColumnType("datetime");
//                entity.Property(e => e.Comments).IsUnicode(false);
//                entity.Property(e => e.Comments2).IsUnicode(false);
//                entity.Property(e => e.EndOfRunDate).HasColumnType("datetime");
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.JitStartDate).HasColumnType("datetime");
//                entity.Property(e => e.Locked).IsUnicode(false);
//                entity.Property(e => e.MaxDelayRequiredStartBy).HasColumnType("datetime");
//                entity.Property(e => e.OptimizationScoreDetails).IsUnicode(false);
//                entity.Property(e => e.PeopleUsage).IsUnicode(false);
//                entity.Property(e => e.ProductionStatus).IsUnicode(false);
//                entity.Property(e => e.ReportedEndOfRunDate).HasColumnType("datetime");
//                entity.Property(e => e.ReportedFinishDate).HasColumnType("datetime");
//                entity.Property(e => e.ReportedStartDate).HasColumnType("datetime");
//                entity.Property(e => e.ReportedStartOfProcessingDate).HasColumnType("datetime");
//                entity.Property(e => e.ResourcesUsed).IsUnicode(false);
//                entity.Property(e => e.ScheduledEndDate).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledEndOfRunDate).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledEndOfSetupDate).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledStartDate).HasColumnType("datetime");
//                entity.Property(e => e.Timing).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobActivityInventoryAdjustment>(entity =>
//            {
//                entity.HasKey(e => e.ActivityId);

//                entity.Property(e => e.ActivityId).ValueGeneratedNever();
//                entity.Property(e => e.AdjustmentDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentReason).IsUnicode(false);
//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobMaterial>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId, e.ManufacturingOrderId, e.OperationId, e.MaterialRequirementId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AllowedLotCodes).IsUnicode(false);
//                entity.Property(e => e.AvailableDateTime).HasColumnType("datetime");
//                entity.Property(e => e.ConstraintType).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.ItemExternalId).IsUnicode(false);
//                entity.Property(e => e.MaterialAllocation).IsUnicode(false);
//                entity.Property(e => e.MaterialDescription).IsUnicode(false);
//                entity.Property(e => e.MaterialName).IsUnicode(false);
//                entity.Property(e => e.MaterialSourcing).IsUnicode(false);
//                entity.Property(e => e.RequirementType).IsUnicode(false);
//                entity.Property(e => e.Source).IsUnicode(false);
//                entity.Property(e => e.Supply).IsUnicode(false);
//                entity.Property(e => e.TankStorageReleaseTiming).IsUnicode(false);
//                entity.Property(e => e.UOM).IsUnicode(false);
//                entity.Property(e => e.WarehouseExternalId).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobMaterialSupplyingActivity>(entity =>
//            {
//                entity.HasKey(e => e.SupplyingActivityId);

//                entity.Property(e => e.SupplyingActivityId).ValueGeneratedNever();
//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobOperation>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId, e.ManufacturingOrderId, e.OperationId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Anchored).IsUnicode(false);
//                entity.Property(e => e.Attributes).IsUnicode(false);
//                entity.Property(e => e.AttributesSummary).IsUnicode(false);
//                entity.Property(e => e.BatchCode).IsUnicode(false);
//                entity.Property(e => e.BuyDirectMaterialsList).IsUnicode(false);
//                entity.Property(e => e.BuyDirectMaterialsListNotAvailable).IsUnicode(false);
//                entity.Property(e => e.CommitEndDate).HasColumnType("datetime");
//                entity.Property(e => e.CommitStartDate).HasColumnType("datetime");
//                entity.Property(e => e.CompatibilityCode).IsUnicode(false);
//                entity.Property(e => e.ConstraintType).IsUnicode(false);
//                entity.Property(e => e.DBRJitStartDate).HasColumnType("datetime");
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.EndOfMatlPostProcDate).HasColumnType("datetime");
//                entity.Property(e => e.EndOfResourceTransferTimeDate).HasColumnType("datetime");
//                entity.Property(e => e.EndOfRunDate).HasColumnType("datetime");
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.HoldReason).IsUnicode(false);
//                entity.Property(e => e.HoldUntilDateTime).HasColumnType("datetime");
//                entity.Property(e => e.JITStartDate).HasColumnType("datetime");
//                entity.Property(e => e.LatestConstraint).IsUnicode(false);
//                entity.Property(e => e.LatestConstraintDate).HasColumnType("datetime");
//                entity.Property(e => e.LatestPredecessorFinish).HasColumnType("datetime");
//                entity.Property(e => e.Locked).IsUnicode(false);
//                entity.Property(e => e.MSProjectPredecessorOperations).IsUnicode(false);
//                entity.Property(e => e.MaterialList).IsUnicode(false);
//                entity.Property(e => e.MaterialStatus).IsUnicode(false);
//                entity.Property(e => e.MaterialsNotAvailable).IsUnicode(false);
//                entity.Property(e => e.MaterialsNotPlanned).IsUnicode(false);
//                entity.Property(e => e.MaxDelayRequiredStartBy).HasColumnType("datetime");
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.NeedDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.Omitted).IsUnicode(false);
//                entity.Property(e => e.OutputName).IsUnicode(false);
//                entity.Property(e => e.ProductsList).IsUnicode(false);
//                entity.Property(e => e.RequiredCapabilities).IsUnicode(false);
//                entity.Property(e => e.ResourcesUsed).IsUnicode(false);
//                entity.Property(e => e.ScheduledEnd).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledPrimaryWorkCenterExternalId).IsUnicode(false);
//                entity.Property(e => e.ScheduledStart).HasColumnType("datetime");
//                entity.Property(e => e.SetupCode).IsUnicode(false);
//                entity.Property(e => e.SetupColor).IsUnicode(false);
//                entity.Property(e => e.SetupColorName).IsUnicode(false);
//                entity.Property(e => e.StockMaterialsList).IsUnicode(false);
//                entity.Property(e => e.StockMaterialsListAwaitingAllocation).IsUnicode(false);
//                entity.Property(e => e.SuccessorProcessing).IsUnicode(false);
//                entity.Property(e => e.UOM).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobOperationAttribute>(entity =>
//            {
//                entity.HasKey(e => e.OperationId);

//                entity.Property(e => e.OperationId).ValueGeneratedNever();
//                entity.Property(e => e.Code).IsUnicode(false);
//                entity.Property(e => e.ColorCodeColorName).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.IncurSetupWhen).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobPath>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PathId, e.ManufacturingOrderId, e.JobId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AutoUse).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobPathNode>(entity =>
//            {
//                entity.HasKey(e => e.PathId);

//                entity.Property(e => e.PathId).ValueGeneratedNever();
//                entity.Property(e => e.OverlapType).IsUnicode(false);
//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobProduct>(entity =>
//            {
//                entity.HasKey(e => e.JobId);

//                entity.Property(e => e.JobId).ValueGeneratedNever();
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.InventoryAvailableTiming).IsUnicode(false);
//                entity.Property(e => e.LotCode).IsUnicode(false);
//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobProductDeletedDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobProductId, e.DateDeleted, e.DeletedDemandId, e.JobOperationId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.DateDeleted).HasColumnType("datetime");
//                entity.Property(e => e.Description)
//                    .IsRequired()
//                    .IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobProductForecastDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.ForecastShipmentId, e.JobOperationId, e.JobProductId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobProductSafetyStockDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.InventoryId, e.JobOperationId, e.JobProductId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobProductSalesOrderDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobProductId, e.JobOperationId, e.SalesOrderDistributionId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobProductTransferOrderDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.TransferOrderDistributionId, e.JobOperationId, e.JobProductId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobResource>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId, e.ManufacturingOrderId, e.OperationId, e.ResourceRequirementId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.UsageEnd).IsUnicode(false);
//                entity.Property(e => e.UsageStart).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobResourceBlock>(entity =>
//            {
//                entity.HasKey(e => new { e.BatchId, e.BlockId, e.ActivityId, e.OperationId, e.ManufacturingOrderId, e.JobId, e.importDate });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledEnd).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledStart).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<JobResourceBlockInterval>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId, e.OperationId, e.ManufacturingOrderId, e.BlockId, e.ActivityId, e.IntervalIndex });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.PostProcessingEnd).HasColumnType("datetime");
//                entity.Property(e => e.PostProcessingStart).HasColumnType("datetime");
//                entity.Property(e => e.RunEnd).HasColumnType("datetime");
//                entity.Property(e => e.RunStart).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledEnd).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledStart).HasColumnType("datetime");
//                entity.Property(e => e.SetupEnd).HasColumnType("datetime");
//                entity.Property(e => e.SetupStart).HasColumnType("datetime");
//                entity.Property(e => e.ShiftDescription).IsUnicode(false);
//                entity.Property(e => e.ShiftEnd).HasColumnType("datetime");
//                entity.Property(e => e.ShiftName).IsUnicode(false);
//                entity.Property(e => e.ShiftStart).HasColumnType("datetime");
//                entity.Property(e => e.ShiftType).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobResourceCapability>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId, e.OperationId, e.ManufacturingOrderId, e.ResourceRequirementId, e.CapabilityId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.CapabilityExternalId).IsUnicode(false);
//            });

//            modelBuilder.Entity<JobSuccessorManufacturingOrder>(entity =>
//            {
//                entity.HasKey(e => new { e.JobId, e.ManufacturingOrderId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<KPI>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.CalculatorId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.CalculatorName)
//                    .IsRequired()
//                    .IsUnicode(false);
//            });

//            modelBuilder.Entity<Lot>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.LotId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Code).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.LotSource).IsUnicode(false);
//                entity.Property(e => e.ProductionDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<ManufacturingOrder>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId, e.ManufacturingOrderId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Anchored).IsUnicode(false);
//                entity.Property(e => e.AttributesSummary).IsUnicode(false);
//                entity.Property(e => e.AutoJoinGroup).IsUnicode(false);
//                entity.Property(e => e.Bottlenecks).IsUnicode(false);
//                entity.Property(e => e.BreakOffSourceMOName).IsUnicode(false);
//                entity.Property(e => e.DbrReleaseDate).HasColumnType("datetime");
//                entity.Property(e => e.DbrShippingDueDate).HasColumnType("datetime");
//                entity.Property(e => e.DefaultPathName).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.Family).IsUnicode(false);
//                entity.Property(e => e.Frozen).IsUnicode(false);
//                entity.Property(e => e.HoldReason).IsUnicode(false);
//                entity.Property(e => e.HoldUntil).HasColumnType("datetime");
//                entity.Property(e => e.Locked).IsUnicode(false);
//                entity.Property(e => e.LockedPlantName).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.NeedDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.OnHold).IsUnicode(false);
//                entity.Property(e => e.ProductColor).IsUnicode(false);
//                entity.Property(e => e.ProductDescription).IsUnicode(false);
//                entity.Property(e => e.ProductName).IsUnicode(false);
//                entity.Property(e => e.ReleaseDate).HasColumnType("datetime");
//                entity.Property(e => e.ReleaseDateTime).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledEnd).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledStart).HasColumnType("datetime");
//                entity.Property(e => e.UOM).IsUnicode(false);
//            });

//            modelBuilder.Entity<Metric>(entity =>
//            {
//                entity.HasKey(e => e.importDate);

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.LastAutomaticActionDate).HasColumnType("datetime");
//                entity.Property(e => e.LastManualActionDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<Plant>(entity =>
//            {
//                entity.HasKey(e => e.EntityID);

//                entity.Property(e => e.EntityID).HasMaxLength(4);
//            });

//            modelBuilder.Entity<PlantWarehouse>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PlantId, e.WarehouseId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<ProductRule>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.ItemId, e.PlantId, e.DepartmentId, e.ResourceId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.OperationName)
//                    .IsRequired()
//                    .IsUnicode(false);
//            });

//            modelBuilder.Entity<PurchaseToStockDeletedDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PurchaseToStockId, e.DateDeleted, e.DeletedDemandId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.DateDeleted).HasColumnType("datetime");
//                entity.Property(e => e.Description)
//                    .IsRequired()
//                    .IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<PurchaseToStockForecastDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PurchaseToStockId, e.ForecastShipmentId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<PurchaseToStockInventoryAdjustment>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.InventoryId, e.PurchaseToStockId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentReason).IsUnicode(false);
//            });

//            modelBuilder.Entity<PurchaseToStockSafetyStockDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PurchaseToStockId, e.InventoryId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<PurchaseToStockSalesOrderDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PurchaseToStockId, e.SalesOrderDistributionId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<PurchaseToStockTransferOrderDemand>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PurchaseToStockId, e.TransferOrderDistributionId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.RequiredDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<PurchasesToStock>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PurchaseToStockId });

//                entity.ToTable("PurchasesToStock");

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AttributesSummary).IsUnicode(false);
//                entity.Property(e => e.AvailableDate).HasColumnType("datetime");
//                entity.Property(e => e.BuyerExternalId).IsUnicode(false);
//                entity.Property(e => e.DbrReceiptDate).HasColumnType("datetime");
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.MaintenanceMethod).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.ScheduledReceiptDate).HasColumnType("datetime");
//                entity.Property(e => e.UnloadEndDate).HasColumnType("datetime");
//                entity.Property(e => e.VendorExternalId).IsUnicode(false);
//            });

//            modelBuilder.Entity<RecurringCapacityInterval>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.RecurringCapacityIntervalId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.DayType).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.EndDateTime).HasColumnType("datetime");
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.IntervalType).IsUnicode(false);
//                entity.Property(e => e.MonthlyOccurrence).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.Occurrence).IsUnicode(false);
//                entity.Property(e => e.Recurrence).IsUnicode(false);
//                entity.Property(e => e.RecurrenceEndDateTime).HasColumnType("datetime");
//                entity.Property(e => e.RecurrenceEndType).IsUnicode(false);
//                entity.Property(e => e.StartDateTime).HasColumnType("datetime");
//                entity.Property(e => e.YearlyMonth).IsUnicode(false);
//            });

//            modelBuilder.Entity<RecurringCapacityIntervalRecurrence>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.RecurringCapacityIntervalId, e.StartDateTime });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.StartDateTime).HasColumnType("datetime");
//                entity.Property(e => e.EndDateTime).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<RecurringCapacityIntervalResourceAssignment>(entity =>
//            {
//                entity.HasKey(e => e.importDate);

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<ReportBlock>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.JobId, e.OperationId, e.ManufacturingOrderId, e.ActivityId, e.BlockId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.ActivityComments).IsUnicode(false);
//                entity.Property(e => e.ActivityComments2).IsUnicode(false);
//                entity.Property(e => e.Customer).IsUnicode(false);
//                entity.Property(e => e.DepartmentName).IsUnicode(false);
//                entity.Property(e => e.Duration).HasColumnType("datetime");
//                entity.Property(e => e.HoldReason).IsUnicode(false);
//                entity.Property(e => e.HoldUntil).HasColumnType("datetime");
//                entity.Property(e => e.JitStartDate).HasColumnType("datetime");
//                entity.Property(e => e.JobDescription).IsUnicode(false);
//                entity.Property(e => e.JobName).IsUnicode(false);
//                entity.Property(e => e.JobNeedDate).HasColumnType("datetime");
//                entity.Property(e => e.JobNotes).IsUnicode(false);
//                entity.Property(e => e.ManufacturingOrderName).IsUnicode(false);
//                entity.Property(e => e.Materials).IsUnicode(false);
//                entity.Property(e => e.OpAttributes).IsUnicode(false);
//                entity.Property(e => e.OperationDescription).IsUnicode(false);
//                entity.Property(e => e.OperationName).IsUnicode(false);
//                entity.Property(e => e.OperationNeedDate).HasColumnType("datetime");
//                entity.Property(e => e.OperationNotes).IsUnicode(false);
//                entity.Property(e => e.OtherResourcesUsed).IsUnicode(false);
//                entity.Property(e => e.PlantName).IsUnicode(false);
//                entity.Property(e => e.Product).IsUnicode(false);
//                entity.Property(e => e.ProductDescription).IsUnicode(false);
//                entity.Property(e => e.ProductionStatus).IsUnicode(false);
//                entity.Property(e => e.RequiredCapabilities).IsUnicode(false);
//                entity.Property(e => e.ResourceDescription).IsUnicode(false);
//                entity.Property(e => e.ResourceName).IsUnicode(false);
//                entity.Property(e => e.RunEnd).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledEnd).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledStart).HasColumnType("datetime");
//                entity.Property(e => e.SetupCode).IsUnicode(false);
//                entity.Property(e => e.SetupEnd).HasColumnType("datetime");
//                entity.Property(e => e.UOM).IsUnicode(false);
//            });

//            modelBuilder.Entity<ResourceCapability>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.PlantId, e.CapabilityId, e.DepartmentId, e.ResourceId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<SalesOrder>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.SalesOrderId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Customer).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//                entity.Property(e => e.Planner).IsUnicode(false);
//                entity.Property(e => e.Project).IsUnicode(false);
//                entity.Property(e => e.SalesOffice).IsUnicode(false);
//                entity.Property(e => e.SalesPerson).IsUnicode(false);
//            });

//            modelBuilder.Entity<SalesOrderDistributionInventoryAdjustment>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.InventoryId, e.SalesOrderDistributionId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentReason).IsUnicode(false);
//            });

//            modelBuilder.Entity<SalesOrderLine>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.SalesOrderLineId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.LineNumber).IsUnicode(false);
//            });

//            modelBuilder.Entity<SalesOrderLineDistribution>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.SalesOrderLineDistributionId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AvailableDate).HasColumnType("datetime");
//                entity.Property(e => e.EligibleLots).IsUnicode(false);
//                entity.Property(e => e.HoldReason).IsUnicode(false);
//                entity.Property(e => e.MaterialAllocation).IsUnicode(false);
//                entity.Property(e => e.MaterialSourcing).IsUnicode(false);
//                entity.Property(e => e.RequiredAvailableDate).HasColumnType("datetime");
//                entity.Property(e => e.SalesRegion).IsUnicode(false);
//                entity.Property(e => e.ShipToZone).IsUnicode(false);
//                entity.Property(e => e.StockShortageRule).IsUnicode(false);
//            });

//            modelBuilder.Entity<Schedule>(entity =>
//            {
//                entity.HasKey(e => e.importDate);

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.Clock).HasColumnType("datetime");
//                entity.Property(e => e.PlanningHorizonEnd).HasColumnType("datetime");
//                entity.Property(e => e.ScenarioDescription).IsUnicode(false);
//                entity.Property(e => e.ScenarioName).IsUnicode(false);
//                entity.Property(e => e.ScenarioType).IsUnicode(false);
//                entity.Property(e => e.importHorizonEnd).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<SystemDatum>(entity =>
//            {
//                entity.HasKey(e => e.Version);

//                entity.Property(e => e.Version).HasMaxLength(13);
//            });

//            modelBuilder.Entity<TransferOrder>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.TransferOrderId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AttributesSummary).IsUnicode(false);
//                entity.Property(e => e.Description).IsUnicode(false);
//                entity.Property(e => e.ExternalId).IsUnicode(false);
//                entity.Property(e => e.MaintenanceMethod).IsUnicode(false);
//                entity.Property(e => e.Name).IsUnicode(false);
//                entity.Property(e => e.Notes).IsUnicode(false);
//            });

//            modelBuilder.Entity<TransferOrderDistribution>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.TransferOrderDistributionId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.MaterialAllocation).IsUnicode(false);
//                entity.Property(e => e.MaterialSourcing).IsUnicode(false);
//                entity.Property(e => e.ScheduledReceiveDate).HasColumnType("datetime");
//                entity.Property(e => e.ScheduledShipDate).HasColumnType("datetime");
//            });

//            modelBuilder.Entity<TransferOrderDistributionInventoryAdjustment>(entity =>
//            {
//                entity.HasKey(e => new { e.importDate, e.InventoryId, e.TransferOrderDistributionId });

//                entity.Property(e => e.importDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentDate).HasColumnType("datetime");
//                entity.Property(e => e.AdjustmentReason).IsUnicode(false);
//            });

//            modelBuilder.Entity<Warehouse>(entity =>
//            {
//                entity.HasKey(e => e.ExternalId);

//                entity.Property(e => e.ExternalId).HasMaxLength(10);
//                entity.Property(e => e.Name).HasMaxLength(60);
//                entity.Property(e => e.PlantExternalId).HasMaxLength(10);
//            });

//            OnModelCreatingPartial(modelBuilder);
//        }

//        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
//    }
//}
