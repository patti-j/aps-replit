using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddServerUpdateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutomaticUpdateDay",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutomaticUpdateFrequency",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutomaticUpdateHour",
                table: "CompanyServers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateCheck",
                table: "CompanyServers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomaticUpdateDay",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "AutomaticUpdateFrequency",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "AutomaticUpdateHour",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "LastUpdateCheck",
                table: "CompanyServers");
        }
    }
}
