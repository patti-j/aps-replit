using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Prod.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CFGroupId",
                table: "PlanningAreas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FavoriteSettingScenarioIds",
                table: "PlanningAreas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CFGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CFGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CFGroups_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Object = table.Column<int>(type: "int", nullable: false),
                    ShowInGantt = table.Column<bool>(type: "bit", nullable: false),
                    ShowInGrids = table.Column<bool>(type: "bit", nullable: false),
                    CanPublish = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFields_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningAreaTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningAreaTags_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldCFGroup",
                columns: table => new
                {
                    CustomFieldId = table.Column<int>(type: "int", nullable: false),
                    CFGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldCFGroup", x => new { x.CFGroupId, x.CustomFieldId });
                    table.ForeignKey(
                        name: "FK_CustomFieldCFGroup_CFGroups_CFGroupId",
                        column: x => x.CFGroupId,
                        principalTable: "CFGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomFieldCFGroup_CustomFields_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomFields",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanningAreaPATag",
                columns: table => new
                {
                    PlanningAreaId = table.Column<int>(type: "int", nullable: false),
                    PAGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaPATag", x => new { x.PAGroupId, x.PlanningAreaId });
                    table.ForeignKey(
                        name: "FK_PlanningAreaPATag_PlanningAreaTags_PAGroupId",
                        column: x => x.PAGroupId,
                        principalTable: "PlanningAreaTags",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanningAreaPATag_PlanningAreas_PlanningAreaId",
                        column: x => x.PlanningAreaId,
                        principalTable: "PlanningAreas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_CFGroupId",
                table: "PlanningAreas",
                column: "CFGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CFGroups_CompanyId",
                table: "CFGroups",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldCFGroup_CustomFieldId",
                table: "CustomFieldCFGroup",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFields_CompanyId",
                table: "CustomFields",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaPATag_PlanningAreaId",
                table: "PlanningAreaPATag",
                column: "PlanningAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaTags_CompanyId",
                table: "PlanningAreaTags",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreas_CFGroups_CFGroupId",
                table: "PlanningAreas",
                column: "CFGroupId",
                principalTable: "CFGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreas_CFGroups_CFGroupId",
                table: "PlanningAreas");

            migrationBuilder.DropTable(
                name: "CustomFieldCFGroup");

            migrationBuilder.DropTable(
                name: "PlanningAreaPATag");

            migrationBuilder.DropTable(
                name: "CFGroups");

            migrationBuilder.DropTable(
                name: "CustomFields");

            migrationBuilder.DropTable(
                name: "PlanningAreaTags");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_CFGroupId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "CFGroupId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "FavoriteSettingScenarioIds",
                table: "PlanningAreas");
        }
    }
}
