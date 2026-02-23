using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Prod.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedDomainsToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedDomains",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedDomains",
                table: "Companies");
        }
    }
}
