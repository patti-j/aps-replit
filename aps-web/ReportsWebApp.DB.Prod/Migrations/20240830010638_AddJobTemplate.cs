using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Prod.Migrations
{
    /// <inheritdoc />
    public partial class AddJobTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobTemplateId",
                table: "CtpRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JobTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarehouseExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NeedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredPathId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Throughput = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsHot = table.Column<bool>(type: "bit", nullable: false),
                    HotReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Warehouses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BottleneckConstraints = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemsWithStockMaterialConstraints = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CopyRoutingFromTemplate = table.Column<bool>(type: "bit", nullable: false),
                    DBRShippingBufferOverrideDays = table.Column<double>(type: "float", nullable: false),
                    DefaultPathExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTemplates_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CtpRequests_JobTemplateId",
                table: "CtpRequests",
                column: "JobTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTemplates_CompanyId",
                table: "JobTemplates",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CtpRequests_JobTemplates_JobTemplateId",
                table: "CtpRequests");

            migrationBuilder.DropTable(
                name: "JobTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CtpRequests_JobTemplateId",
                table: "CtpRequests");

            migrationBuilder.DropColumn(
                name: "JobTemplateId",
                table: "CtpRequests");
        }
    }
}
