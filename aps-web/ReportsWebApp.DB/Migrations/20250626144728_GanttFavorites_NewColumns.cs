using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class GanttFavorites_NewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanningArea",
                table: "GanttFavorites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScenarioName",
                table: "GanttFavorites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanningArea",
                table: "GanttFavorites");

            migrationBuilder.DropColumn(
                name: "ScenarioName",
                table: "GanttFavorites");
        }
    }
}
