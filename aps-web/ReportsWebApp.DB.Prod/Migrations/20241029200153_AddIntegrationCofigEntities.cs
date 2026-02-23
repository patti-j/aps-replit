using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Prod.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationCofigEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntegrationConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    LastEditedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditingUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationConfigs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Distinct = table.Column<bool>(type: "bit", nullable: true),
                    IntegrationConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Features_IntegrationConfigs_IntegrationConfigId",
                        column: x => x.IntegrationConfigId,
                        principalTable: "IntegrationConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningAreaIntegrationConfig",
                columns: table => new
                {
                    PlanningAreaId = table.Column<int>(type: "int", nullable: false),
                    IntegrationConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaIntegrationConfig", x => new { x.IntegrationConfigId, x.PlanningAreaId });
                    table.ForeignKey(
                        name: "FK_PlanningAreaIntegrationConfig_IntegrationConfigs_IntegrationConfigId",
                        column: x => x.IntegrationConfigId,
                        principalTable: "IntegrationConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanningAreaIntegrationConfig_PlanningAreas_PlanningAreaId",
                        column: x => x.PlanningAreaId,
                        principalTable: "PlanningAreas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    SourceOption = table.Column<int>(type: "int", nullable: false),
                    FixedValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntegrationConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Properties_IntegrationConfigs_IntegrationConfigId",
                        column: x => x.IntegrationConfigId,
                        principalTable: "IntegrationConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Features_IntegrationConfigId",
                table: "Features",
                column: "IntegrationConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConfigs_CompanyId",
                table: "IntegrationConfigs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaIntegrationConfig_PlanningAreaId",
                table: "PlanningAreaIntegrationConfig",
                column: "PlanningAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_IntegrationConfigId",
                table: "Properties",
                column: "IntegrationConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "PlanningAreaIntegrationConfig");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "IntegrationConfigs");
        }
    }
}
