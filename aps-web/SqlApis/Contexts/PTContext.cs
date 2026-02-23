using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using PlanetTogetherContext.Entities;

namespace PlanetTogetherContext.Contexts
{
    public class PTContext : DbContext
    {
        public PTContext(DbContextOptions<PTContext> a_options) : base(a_options)
        {
            
        }

        public DbSet<Plant> Plants { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<PlantWarehouse> PlantWarehouses { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Lots> Lots { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
        public DbSet<SalesOrderLineDistribution> SalesOrderLineDistributions { get; set; }
        public DbSet<PTProduct> PTProducts { get; set; }
        public DbSet<Material> Material { get; set; }
        public DbSet<ManufacturingOrder> ManufacturingOrders { get; set; }
        public DbSet<ResourceOperation> ResourceOperations { get; set; }
        public DbSet<InternalActivity> InternalActivities { get; set; }
        public DbSet<ResourceRequirement> ResourceRequirements { get; set; }
        public DbSet<Capability> Capabilities { get; set; }
        public DbSet<ResourceCapability> ResourceCapabilities { get; set; }
        public DbSet<RequiredCapability> RequiredCapabilities { get; set; }
        public DbSet<OperationAttribute> OperationAttributes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Plant>().ToTable("Plants");

            modelBuilder.Entity<Department>().ToTable("Departments").HasKey(d => new { d.PlantExternalId, d.ExternalId });

            modelBuilder.Entity<Resource>().ToTable("Resources").HasKey(d => new { d.PlantExternalId, d.ExternalId, d.DepartmentExternalId });

            modelBuilder.Entity<Capability>().ToTable("Capabilities");

            modelBuilder.Entity<ResourceCapability>().HasKey(rc => new { rc.PlantExternalId, rc.DepartmentExternalId, rc.CapabilityExternalId, rc.ResourceExternalId });
            modelBuilder.Entity<ResourceCapability>().ToTable("ResourceCapabilities");

            modelBuilder.Entity<Job>().ToTable("Jobs").HasKey(d => new { d.ExternalId });
    ;

            modelBuilder.Entity<Item>().HasKey(i => new {i.ExternalId});
            modelBuilder.Entity<Item>().ToTable("Items");

            modelBuilder.Entity<Warehouse>().HasKey(i => new { i.ExternalId});
            modelBuilder.Entity<Warehouse>().ToTable("Warehouse").HasData(
                new Warehouse() { ExternalId = "AhaWarehouse", Name = "AhaIdeas", Description = "Ideas from Aha!" }
            );

            modelBuilder.Entity<PlantWarehouse>().HasKey(pw => new { pw.PlantExternalId, pw.WarehouseExternalId });
            modelBuilder.Entity<PlantWarehouse>().ToTable("PlantWarehouse").HasData(
                new PlantWarehouse() { PlantExternalId = "6786756851059831831", WarehouseExternalId = "AhaWarehouse" }
            );

            modelBuilder.Entity<Inventory>().HasKey(iv => new { iv.ItemExternalId, iv.WarehouseExternalId });
            modelBuilder.Entity<Inventory>().ToTable("Inventory").HasData(
                new Inventory() { ItemExternalId = "Item1", WarehouseExternalId = "AhaWarehouse", OnHandQty = 1 }
            );

            modelBuilder.Entity<Lots>().HasKey(l => new { l.ExternalId, l.ItemExternalId, l.Qty, l.WarehouseExternalId });
            modelBuilder.Entity<Lots>().ToTable("Lots").HasData(
                new Lots() { ExternalId = "lot1", ItemExternalId = "Item1",  Qty = 999999, WarehouseExternalId = "AhaWarehouse" }
            );

            modelBuilder.Entity<PTProduct>().HasKey(i => new { i.ExternalId });
            modelBuilder.Entity<PTProduct>().ToTable("Products").HasData(
                new PTProduct() { ExternalId = "tempProduct", JobExternalId= "-11111", MoExternalId = "tempMO", OpExternalId = "tempMO", InventoryAvailableTiming = "AtOperationRunStart", WarehouseExternalId = "AhaWarehouse", TotalOutputQty = 1}
            );

            modelBuilder.Entity<Material>().ToTable("Material");


            modelBuilder.Entity<ManufacturingOrder>().ToTable("ManufacturingOrders");
            
            modelBuilder.Entity<ResourceOperation>().HasKey(rc => new {rc.JobExternalId, rc.ManufacturingOrderExternalId, rc.ExternalId });
            modelBuilder.Entity<ResourceOperation>().ToTable("ResourceOperations")
                .Property(ro => ro.RequiredFinishQty).HasColumnType("DECIMAL(10,4)");

            modelBuilder.Entity<InternalActivity>().ToTable("InternalActivities");

            modelBuilder.Entity<ResourceRequirement>().HasKey(rr => new { rr.JobExternalId, rr.MoExternalId, rr.OpExternalId, rr.ExternalId });
            modelBuilder.Entity<ResourceRequirement>().ToTable("ResourceRequirements");
            
            modelBuilder.Entity<RequiredCapability>().HasKey(rc => new { rc.JobExternalId, rc.MoExternalId, rc.OpExternalId, rc.CapabilityExternalId, rc.ResourceRequirementExternalId });
            modelBuilder.Entity<RequiredCapability>().ToTable("RequiredCapabilities");
            modelBuilder.Entity<OperationAttribute>().ToTable("OperationAttributes");
        }

        public async Task Init()
        {
            //Create database if it doesn't exist
            try
            {
                await Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't create the database: " + ex.Message);
                throw;
            }

            try
            {

                //Clear all existing data
                if (Plants.Any())
                {
                    Plants.RemoveRange(Plants);
                }
                if (Departments.Any())
                {
                    Departments.RemoveRange(Departments);
                }
                if (Resources.Any())
                {
                    Resources.RemoveRange(Resources);
                }
                if (Capabilities.Any())
                {
                    Capabilities.RemoveRange(Capabilities);
                }
                if (ResourceCapabilities.Any())
                {
                    ResourceCapabilities.RemoveRange(ResourceCapabilities);
                }
                if (Jobs.Any())
                {
                    Jobs.RemoveRange(Jobs);
                }
                if (ManufacturingOrders.Any())
                {
                    ManufacturingOrders.RemoveRange(ManufacturingOrders);
                }
                if (ResourceOperations.Any())
                {
                    ResourceOperations.RemoveRange(ResourceOperations);
                }
                if (ResourceRequirements.Any())
                {
                    ResourceRequirements.RemoveRange(ResourceRequirements);
                }
                if (InternalActivities.Any())
                {
                    InternalActivities.RemoveRange(InternalActivities);
                }
                if (OperationAttributes.Any())
                {
                    OperationAttributes.RemoveRange(OperationAttributes);
                }
                if (SalesOrders.Any())
                {
                    SalesOrders.RemoveRange(SalesOrders);
                }
                if (SalesOrderLines.Any())
                {
                    SalesOrderLines.RemoveRange(SalesOrderLines);
                }
                if (SalesOrderLineDistributions.Any())
                {
                    SalesOrderLineDistributions.RemoveRange(SalesOrderLineDistributions);
                }
                if (Material.Any())
                {
                    Material.RemoveRange(Material);
                }
                if (PTProducts.Any())
                {
                    PTProducts.RemoveRange(PTProducts);
                }
                if (Inventories.Any())
                {
                    Inventories.RemoveRange(Inventories);
                }
                if (Items.Any())
                {
                    Items.RemoveRange(Items);
                }
                if (Lots.Any())
                {
                    Lots.RemoveRange(Lots);
                }
                if (PlantWarehouses.Any())
                {
                    PlantWarehouses.RemoveRange(PlantWarehouses);
                }
                if (RequiredCapabilities.Any())
                {
                    RequiredCapabilities.RemoveRange(RequiredCapabilities);
                }
                //Save clear
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Deleting Table Data: " + ex.Message);
                //throw;
            }

            await SaveChangesAsync();
        }
    }
}
