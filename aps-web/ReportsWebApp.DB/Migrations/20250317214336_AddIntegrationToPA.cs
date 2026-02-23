using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationToPA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DBIntegrationId",
                table: "PlanningAreas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DBIntegrationLastAppliedTime",
                table: "PlanningAreas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_DBIntegrationId",
                table: "PlanningAreas",
                column: "DBIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_DBIntegrations_CreatedBy",
                table: "DBIntegrations",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_DBIntegrations_Users_CreatedBy",
                table: "DBIntegrations",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreas_DBIntegrations_DBIntegrationId",
                table: "PlanningAreas",
                column: "DBIntegrationId",
                principalTable: "DBIntegrations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DBIntegrations_Users_CreatedBy",
                table: "DBIntegrations");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreas_DBIntegrations_DBIntegrationId",
                table: "PlanningAreas");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_DBIntegrationId",
                table: "PlanningAreas");

            migrationBuilder.DropIndex(
                name: "IX_DBIntegrations_CreatedBy",
                table: "DBIntegrations");

            migrationBuilder.DropColumn(
                name: "DBIntegrationId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "DBIntegrationLastAppliedTime",
                table: "PlanningAreas");
        }
    }
}
