using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalServerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "CompanyServers",
                newName: "SystemId");

            migrationBuilder.AddColumn<string>(
                name: "AdminMessage",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApiPort",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificateName",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComputerNameOrIP",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServerManagerPath",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SsoClientId",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SsoDomain",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminMessage",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "ApiPort",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "CertificateName",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "ComputerNameOrIP",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "ServerManagerPath",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "SsoClientId",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "SsoDomain",
                table: "CompanyServers");

            migrationBuilder.RenameColumn(
                name: "SystemId",
                table: "CompanyServers",
                newName: "Status");
        }
    }
}
