using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddServerOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwningUserId",
                table: "CompanyServers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyServers_OwningUserId",
                table: "CompanyServers",
                column: "OwningUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyServers_Users_OwningUserId",
                table: "CompanyServers",
                column: "OwningUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyServers_Users_OwningUserId",
                table: "CompanyServers");

            migrationBuilder.DropIndex(
                name: "IX_CompanyServers_OwningUserId",
                table: "CompanyServers");

            migrationBuilder.DropColumn(
                name: "OwningUserId",
                table: "CompanyServers");
        }
    }
}
