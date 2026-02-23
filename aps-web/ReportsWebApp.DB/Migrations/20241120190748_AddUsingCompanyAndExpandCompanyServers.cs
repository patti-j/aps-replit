using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddUsingCompanyAndExpandCompanyServers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyServers_Companies_CompanyId",
                table: "CompanyServers");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "CompanyServers",
                newName: "ManagingCompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyServers_CompanyId",
                table: "CompanyServers",
                newName: "IX_CompanyServers_ManagingCompanyId");

            migrationBuilder.AddColumn<int>(
                name: "UsedByCompanyId",
                table: "PlanningAreas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                table: "CompanyServers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServerName",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ServerCompany",
                columns: table => new
                {
                    CompanyServerId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerCompany", x => new { x.CompanyServerId, x.CompanyId });
                    table.ForeignKey(
                        name: "FK_ServerCompany_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ServerCompany_CompanyServers_CompanyServerId",
                        column: x => x.CompanyServerId,
                        principalTable: "CompanyServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_UsedByCompanyId",
                table: "PlanningAreas",
                column: "UsedByCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerCompany_CompanyId",
                table: "ServerCompany",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyServers_Companies_ManagingCompanyId",
                table: "CompanyServers",
                column: "ManagingCompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreas_Companies_UsedByCompanyId",
                table: "PlanningAreas",
                column: "UsedByCompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyServers_Companies_ManagingCompanyId",
                table: "CompanyServers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreas_Companies_UsedByCompanyId",
                table: "PlanningAreas");

            migrationBuilder.DropTable(
                name: "ServerCompany");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_UsedByCompanyId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "UsedByCompanyId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "ServerName",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CompanyServers");

            migrationBuilder.RenameColumn(
                name: "ManagingCompanyId",
                table: "CompanyServers",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyServers_ManagingCompanyId",
                table: "CompanyServers",
                newName: "IX_CompanyServers_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyServers_Companies_CompanyId",
                table: "CompanyServers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
