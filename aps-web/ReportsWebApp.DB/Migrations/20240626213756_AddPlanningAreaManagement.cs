using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningAreaManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyServers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TaskNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserPermissionSetId = table.Column<long>(type: "bigint", nullable: false),
                    PlantPermissionsId = table.Column<long>(type: "bigint", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompressionType = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ServerConnectionCount = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAUsers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PAUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanningAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyServerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    PlanningAreaId = table.Column<int>(type: "int", nullable: false),
                    PAUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaUser", x => new { x.PAUserId, x.PlanningAreaId });
                    table.ForeignKey(
                        name: "FK_PlanningAreaUser_PAUsers_PAUserId",
                        column: x => x.PAUserId,
                        principalTable: "PAUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanningAreaUser_PlanningAreas_PlanningAreaId",
                        column: x => x.PlanningAreaId,
                        principalTable: "PlanningAreas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyServers_CompanyId",
                table: "CompanyServers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PAUsers_CompanyId",
                table: "PAUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PAUsers_UserId",
                table: "PAUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_CompanyServerId",
                table: "PlanningAreas",
                column: "CompanyServerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaUser_PlanningAreaId",
                table: "PlanningAreaUser",
                column: "PlanningAreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningAreaUser");

            migrationBuilder.DropTable(
                name: "PAUsers");

            migrationBuilder.DropTable(
                name: "PlanningAreas");

            migrationBuilder.DropTable(
                name: "CompanyServers");
        }
    }
}
