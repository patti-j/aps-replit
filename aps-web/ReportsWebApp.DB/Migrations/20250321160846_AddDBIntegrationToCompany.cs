using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddDBIntegrationToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "DBIntegrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DBIntegrations_CompanyId",
                table: "DBIntegrations",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_DBIntegrations_Companies_CompanyId",
                table: "DBIntegrations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DBIntegrations_Companies_CompanyId",
                table: "DBIntegrations");

            migrationBuilder.DropIndex(
                name: "IX_DBIntegrations_CompanyId",
                table: "DBIntegrations");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "DBIntegrations");
        }
    }
}
