using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishDbSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportConnectionString",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImportIntegrationUserAndPass",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishConnectionString",
                table: "DataConnectors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportConnectionString",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "ImportIntegrationUserAndPass",
                table: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "PublishConnectionString",
                table: "DataConnectors");
        }
    }
}
