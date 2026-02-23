using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddInstanceInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningAreaUser");

            migrationBuilder.DropTable(
                name: "PlanningAreas");

            migrationBuilder.CreateTable(
                name: "PlanningAreaUsers",
                columns: table => new
                {
                    PlanningAreaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PAUserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaUsers", x => new { x.PAUserId, x.PlanningAreaKey });
                    table.ForeignKey(
                        name: "FK_PlanningAreaUsers_PAUsers_PAUserId",
                        column: x => x.PAUserId,
                        principalTable: "PAUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningAreaUsers");

            migrationBuilder.CreateTable(
                name: "PlanningAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyServerId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstanceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningAreas_CompanyServers_CompanyServerId",
                        column: x => x.CompanyServerId,
                        principalTable: "CompanyServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningAreaUser",
                columns: table => new
                {
                    PAUserId = table.Column<int>(type: "int", nullable: false),
                    PlanningAreaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaUser", x => new { x.PAUserId, x.PlanningAreaId });
                    table.ForeignKey(
                        name: "FK_PlanningAreaUser_PAUsers_PAUserId",
                        column: x => x.PAUserId,
                        principalTable: "PAUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanningAreaUser_PlanningAreas_PlanningAreaId",
                        column: x => x.PlanningAreaId,
                        principalTable: "PlanningAreas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_CompanyServerId",
                table: "PlanningAreas",
                column: "CompanyServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaUser_PlanningAreaId",
                table: "PlanningAreaUser",
                column: "PlanningAreaId");
        }
    }
}
