using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPAApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PlanningAreaKey",
                table: "PlanningAreas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "PlanningAreas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_ApiKey",
                table: "PlanningAreas",
                column: "ApiKey");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_PlanningAreaKey",
                table: "PlanningAreas",
                column: "PlanningAreaKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_ApiKey",
                table: "PlanningAreas");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_PlanningAreaKey",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "PlanningAreas");

            migrationBuilder.AlterColumn<string>(
                name: "PlanningAreaKey",
                table: "PlanningAreas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
