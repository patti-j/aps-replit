using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlanningAreaMappingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAUsers_Companies_CompanyId",
                table: "PAUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaUsers_PAUsers_PAUserId",
                table: "PlanningAreaUsers");

            migrationBuilder.DropIndex(
                name: "IX_PAUsers_CompanyId",
                table: "PAUsers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PAUsers");

            migrationBuilder.RenameColumn(
                name: "PAUserId",
                table: "PlanningAreaUsers",
                newName: "PlanningAreaLoginId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaUsers_PAUsers_PlanningAreaLoginId",
                table: "PlanningAreaUsers",
                column: "PlanningAreaLoginId",
                principalTable: "PAUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaUsers_PAUsers_PlanningAreaLoginId",
                table: "PlanningAreaUsers");

            migrationBuilder.RenameColumn(
                name: "PlanningAreaLoginId",
                table: "PlanningAreaUsers",
                newName: "PAUserId");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "PAUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PAUsers_CompanyId",
                table: "PAUsers",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PAUsers_Companies_CompanyId",
                table: "PAUsers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaUsers_PAUsers_PAUserId",
                table: "PlanningAreaUsers",
                column: "PAUserId",
                principalTable: "PAUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
