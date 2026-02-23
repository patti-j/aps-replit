using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddDataConnectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImportDataConnector",
                table: "ExternalIntegrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishDataConnector",
                table: "ExternalIntegrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DataConnectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataConnectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataConnectors_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataConnectors_CompanyId",
                table: "DataConnectors",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataConnectors");

            migrationBuilder.DropColumn(
                name: "ImportDataConnector",
                table: "ExternalIntegrations");

            migrationBuilder.DropColumn(
                name: "PublishDataConnector",
                table: "ExternalIntegrations");
        }
    }
}
