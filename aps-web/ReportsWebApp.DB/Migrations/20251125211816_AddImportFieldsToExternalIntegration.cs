using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddImportFieldsToExternalIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIntegrationV2Enabled",
                table: "ExternalIntegrations",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostImportURL",
                table: "ExternalIntegrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreImportProgramArgs",
                table: "ExternalIntegrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreImportProgramPath",
                table: "ExternalIntegrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreImportURL",
                table: "ExternalIntegrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RunPreImportSQL",
                table: "ExternalIntegrations",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIntegrationV2Enabled",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PostImportURL",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PreImportProgramArgs",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PreImportProgramPath",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PreImportURL",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "RunPreImportSQL",
                table: "ExternalIntegrations");
        }
    }
}
