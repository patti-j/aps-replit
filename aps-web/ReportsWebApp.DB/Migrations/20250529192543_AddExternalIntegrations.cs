using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalIntegrationId",
                table: "PlanningAreas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExternalIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalIntegrations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreas_ExternalIntegrationId",
                table: "PlanningAreas",
                column: "ExternalIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIntegrations_CompanyId",
                table: "ExternalIntegrations",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreas_ExternalIntegrations_ExternalIntegrationId",
                table: "PlanningAreas",
                column: "ExternalIntegrationId",
                principalTable: "ExternalIntegrations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreas_ExternalIntegrations_ExternalIntegrationId",
                table: "PlanningAreas");

            migrationBuilder.DropTable(
                name: "ExternalIntegrations");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreas_ExternalIntegrationId",
                table: "PlanningAreas");

            migrationBuilder.DropColumn(
                name: "ExternalIntegrationId",
                table: "PlanningAreas");
        }
    }
}
