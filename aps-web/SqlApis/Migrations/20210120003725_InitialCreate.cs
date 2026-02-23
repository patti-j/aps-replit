using System;
using Microsoft.EntityFrameworkCore.Migrations;
using PlanetTogetherContext.Contexts;

namespace PlanetTogetherContext.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Capabilities",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capabilities", x => x.ExternalId);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    PlantExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => new { x.PlantExternalId, x.ExternalId });
                });

            migrationBuilder.CreateTable(
                name: "InternalActivities",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    JobExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    MoExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    OpExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    RequiredFinishQty = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    ProductionStatus = table.Column<string>(type: "NVARCHAR(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalActivities", x => x.ExternalId);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    NeedDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Commitment = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    Customer = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    OrderNo = table.Column<string>(type: "VARCHAR(250)", nullable: true),
                    Hot = table.Column<bool>(type: "BIT", nullable: false),
                    ColorCodeAlpha = table.Column<int>(type: "INT", nullable: false),
                    ColorCodeBlue = table.Column<int>(type: "INT", nullable: false),
                    ColorCodeGreen = table.Column<int>(type: "INT", nullable: false),
                    ColorCodeRed = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.ExternalId);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingOrders",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    JobExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    NeedDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    ReleaseDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingOrders", x => x.ExternalId);
                });

            migrationBuilder.CreateTable(
                name: "OperationAttributes",
                columns: table => new
                {
                    OpExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    JobExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    MOExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    SetupHrs = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationAttributes", x => x.OpExternalId);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.ExternalId);
                });

            migrationBuilder.CreateTable(
                name: "RequiredCapabilities",
                columns: table => new
                {
                    CapabilityExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    JobExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    MoExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    OpExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    ResourceRequirementExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredCapabilities", x => new { x.JobExternalId, x.MoExternalId, x.OpExternalId, x.CapabilityExternalId, x.ResourceRequirementExternalId });
                });

            migrationBuilder.CreateTable(
                name: "ResourceCapabilities",
                columns: table => new
                {
                    CapabilityExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    PlantExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    DepartmentExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    ResourceExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceCapabilities", x => new { x.PlantExternalId, x.DepartmentExternalId, x.CapabilityExternalId, x.ResourceExternalId });
                });

            migrationBuilder.CreateTable(
                name: "ResourceOperations",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    JobExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    ManufacturingOrderExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    NeedDateTime = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Qty = table.Column<decimal>(type: "DECIMAL(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceOperations", x => new { x.JobExternalId, x.ManufacturingOrderExternalId, x.ExternalId });
                });

            migrationBuilder.CreateTable(
                name: "ResourceRequirements",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    JobExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    MoExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    OpExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    DefaultResourcePlantExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    DefaultResourceDepartmentExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    DefaultResourceExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceRequirements", x => new { x.JobExternalId, x.MoExternalId, x.OpExternalId, x.ExternalId });
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    PlantExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    DepartmentExternalId = table.Column<string>(type: "NVARCHAR(250)", nullable: true),
                    CapacityType = table.Column<string>(type: "NVARCHAR(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ExternalId);
                });

            migrationBuilder.InsertData(
                table: "Capabilities",
                columns: new[] { "ExternalId", "Name" },
                values: new object[,]
                {
                    { "Architect", "Architect" },
                    { "CoreDevelopment", "CoreDevelopment" },
                    { "Simulation", "Simulation" },
                    { "Customization", "Customization" }
                });

            migrationBuilder.InsertData(
                table: "ResourceCapabilities",
                columns: new[] { "CapabilityExternalId", "DepartmentExternalId", "PlantExternalId", "ResourceExternalId" },
                values: new object[,]
                {
                    { "Customization", "6786756851059831831", "Aha", "Andre Bruno" },
                    { "Customization", "6786756851059831831", "Aha", "Cavan Crawford" },
                    { "CoreDevelopment", "6786756851059831831", "Aha", "Brian Wrestler" },
                    { "CoreDevelopment", "6786756851059831831", "Aha", "Corey Nelson" },
                    { "CoreDevelopment", "6786756851059831831", "Aha", "Cavan Crawford" },
                    { "CoreDevelopment", "6786756851059831831", "Aha", "Andre Bruno" },
                    { "Architect", "6786756851059831831", "Aha", "Cavan Crawford" },
                    { "Simulation", "6786756851059831831", "Aha", "Larry Hargis" },
                    { "Customization", "6786756851059831831", "Aha", "Corey Orndoff" }
                });            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Capabilities");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "InternalActivities");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "ManufacturingOrders");

            migrationBuilder.DropTable(
                name: "OperationAttributes");

            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropTable(
                name: "RequiredCapabilities");

            migrationBuilder.DropTable(
                name: "ResourceCapabilities");

            migrationBuilder.DropTable(
                name: "ResourceOperations");

            migrationBuilder.DropTable(
                name: "ResourceRequirements");

            migrationBuilder.DropTable(
                name: "Resources");
        }

        public void ClearDatabase(PTContext context)
        {            
            context.SalesOrderLineDistributions.RemoveRange(context.SalesOrderLineDistributions);
            context.SalesOrderLines.RemoveRange(context.SalesOrderLines);
            context.SalesOrders.RemoveRange(context.SalesOrders);

            context.Lots.RemoveRange(context.Lots);
            context.Inventories.RemoveRange(context.Inventories);
            context.PlantWarehouses.RemoveRange(context.PlantWarehouses);
            context.Warehouses.RemoveRange(context.Warehouses);

            context.Items.RemoveRange(context.Items);
            context.Jobs.RemoveRange(context.Jobs);
            context.Resources.RemoveRange(context.Resources);
            context.Departments.RemoveRange(context.Departments);
            context.Plants.RemoveRange(context.Plants);

            context.SaveChanges();
        }

    }
}
