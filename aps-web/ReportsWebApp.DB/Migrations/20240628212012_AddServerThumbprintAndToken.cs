using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddServerThumbprintAndToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaUser_PAUsers_PAUserId",
                table: "PlanningAreaUser");

            migrationBuilder.AddColumn<string>(
                name: "AuthToken",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Thumbprint",
                table: "CompanyServers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaUser_PAUsers_PAUserId",
                table: "PlanningAreaUser",
                column: "PAUserId",
                principalTable: "PAUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaUser_PAUsers_PAUserId",
                table: "PlanningAreaUser");

            migrationBuilder.DropColumn(
                name: "AuthToken",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "Thumbprint",
                table: "CompanyServers");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaUser_PAUsers_PAUserId",
                table: "PlanningAreaUser",
                column: "PAUserId",
                principalTable: "PAUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
