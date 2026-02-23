using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Prod.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationConfigAddVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UpgradedFromConfigId",
                table: "IntegrationConfigs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionNumber",
                table: "IntegrationConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpgradedFromConfigId",
                table: "IntegrationConfigs");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "IntegrationConfigs");
        }
    }
}
