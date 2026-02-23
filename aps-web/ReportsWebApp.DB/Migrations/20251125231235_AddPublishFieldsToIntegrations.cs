using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishFieldsToIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnalyticsURL",
                table: "ExternalIntegrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostExportURL",
                table: "ExternalIntegrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreExportURL",
                table: "ExternalIntegrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostExportSQL",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalyticsURL",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PostExportURL",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PreExportURL",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PostExportSQL",
                table: "DataConnectors");
        }
    }
}
