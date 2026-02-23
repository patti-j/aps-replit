using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class MovePASettingsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "CompressionType",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "DisplayLanguage",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "ServerConnectionCount",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "TaskNotes",
                table: "PlanningAreaLogins");

            migrationBuilder.AddColumn<int>(
                name: "CompressionType",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayLanguage",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaskNotes",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompressionType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TaskNotes",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "PlanningAreaLogins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CompressionType",
                table: "PlanningAreaLogins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayLanguage",
                table: "PlanningAreaLogins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "PlanningAreaLogins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "PlanningAreaLogins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ServerConnectionCount",
                table: "PlanningAreaLogins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TaskNotes",
                table: "PlanningAreaLogins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
