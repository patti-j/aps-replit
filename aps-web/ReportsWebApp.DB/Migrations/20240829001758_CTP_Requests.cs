using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class CTP_Requests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CtpRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PADetailsId = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ItemExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarehouseExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NeedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredPathId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledFinish = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InventoryInquiry = table.Column<bool>(type: "bit", nullable: false),
                    HotOff = table.Column<bool>(type: "bit", nullable: false),
                    ReserveCapacityAndMaterialsUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SchedulingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Throughput = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsHot = table.Column<bool>(type: "bit", nullable: false),
                    HotReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CtpRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CtpRequests_PlanningAreas_PADetailsId",
                        column: x => x.PADetailsId,
                        principalTable: "PlanningAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CtpRequests_PADetailsId",
                table: "CtpRequests",
                column: "PADetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CtpRequests");
        }
    }
}
