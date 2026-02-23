using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddImportAndPublishFieldsToDataConnectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIntegrationV2Enabled",
                table: "DataConnectors",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostExportURL",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostImportURL",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreExportURL",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreImportProgramArgs",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreImportProgramPath",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreImportURL",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RunPreImportSQL",
                table: "DataConnectors",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIntegrationV2Enabled",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "PostExportURL",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "PostImportURL",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "PreExportURL",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "PreImportProgramArgs",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "PreImportProgramPath",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "PreImportURL",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "RunPreImportSQL",
                table: "DataConnectors");
        }
    }
}
