using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningAreaManagingServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Server",
                table: "PlanningAreas");

            migrationBuilder.AddColumn<int>(
                name: "ServerId",
                table: "PlanningAreas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_ServerId",
                table: "PlanningAreas",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreas_CompanyServers_ServerId",
                table: "PlanningAreas",
                column: "ServerId",
                principalTable: "CompanyServers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreas_CompanyServers_ServerId",
                table: "PlanningAreas");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_ServerId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "PlanningAreas");

            migrationBuilder.AddColumn<string>(
                name: "Server",
                table: "PlanningAreas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
