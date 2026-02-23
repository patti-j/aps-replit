using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class FolderToLocationAndAddPAsToScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PlanningAreaFolders",
                newName: "PlanningAreaLocations");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningAreas_FolderId",
                table: "PlanningAreas",
                newName: "IX_PlanningAreas_LocationId");

            migrationBuilder.RenameColumn(
                name: "FolderId",
                table: "PlanningAreas",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningAreaFolders_ParentId",
                newName: "IX_PlanningAreaLocations_ParentId",
                table: "PlanningAreaLocations");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningAreaFolders_ServerId",
                newName: "IX_PlanningAreaLocations_ServerId",
                table: "PlanningAreaLocations");

            migrationBuilder.CreateTable(
                name: "PAPlanningAreaScope",
                columns: table => new
                {
                    PlanningAreaId = table.Column<int>(type: "int", nullable: false),
                    PlanningAreaScopeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAPlanningAreaScope", x => new { x.PlanningAreaId, x.PlanningAreaScopeId });
                    table.ForeignKey(
                        name: "FK_PAPlanningAreaScope_PlanningAreaScopes_PlanningAreaScopeId",
                        column: x => x.PlanningAreaScopeId,
                        principalTable: "PlanningAreaScopes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PAPlanningAreaScope_PlanningAreas_PlanningAreaId",
                        column: x => x.PlanningAreaId,
                        principalTable: "PlanningAreas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PAPlanningAreaScope_PlanningAreaScopeId",
                table: "PAPlanningAreaScope",
                column: "PlanningAreaScopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreas_PlanningAreaLocations_LocationId",
                table: "PlanningAreas");

            migrationBuilder.DropTable(
                name: "PAPlanningAreaScope");

            migrationBuilder.DropTable(
                name: "PlanningAreaLocations");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "PlanningAreas",
                newName: "FolderId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningAreas_LocationId",
                table: "PlanningAreas",
                newName: "IX_PlanningAreas_FolderId");

            migrationBuilder.CreateTable(
                name: "PlanningAreaFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    ServerId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningAreaFolders_CompanyServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "CompanyServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanningAreaFolders_PlanningAreaFolders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "PlanningAreaFolders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaFolders_ParentId",
                table: "PlanningAreaFolders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaFolders_ServerId",
                table: "PlanningAreaFolders",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreas_PlanningAreaFolders_FolderId",
                table: "PlanningAreas",
                column: "FolderId",
                principalTable: "PlanningAreaFolders",
                principalColumn: "Id");
        }
    }
}
