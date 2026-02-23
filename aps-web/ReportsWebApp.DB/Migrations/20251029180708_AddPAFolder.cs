using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPAFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "PlanningAreas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlanningAreaFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<int>(type: "int", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "IX_PlanningAreas_FolderId",
                table: "PlanningAreas",
                column: "FolderId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreas_PlanningAreaFolders_FolderId",
                table: "PlanningAreas");

            migrationBuilder.DropTable(
                name: "PlanningAreaFolders");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_FolderId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "PlanningAreas");
        }
    }
}
