using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlanningAreaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAUsers_Users_UserId",
                table: "PAUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaUsers_PAUsers_PlanningAreaLoginId",
                table: "PlanningAreaUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanningAreaUsers",
                table: "PlanningAreaUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PAUsers",
                table: "PAUsers");

            migrationBuilder.RenameTable(
                name: "PlanningAreaUsers",
                newName: "PlanningAreaAuthorizations");

            migrationBuilder.RenameTable(
                name: "PAUsers",
                newName: "PlanningAreaLogins");

            migrationBuilder.RenameIndex(
                name: "IX_PAUsers_UserId",
                table: "PlanningAreaLogins",
                newName: "IX_PlanningAreaLogins_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanningAreaAuthorizations",
                table: "PlanningAreaAuthorizations",
                columns: new[] { "PlanningAreaLoginId", "PlanningAreaKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanningAreaLogins",
                table: "PlanningAreaLogins",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaAuthorizations_PlanningAreaLogins_PlanningAreaLoginId",
                table: "PlanningAreaAuthorizations",
                column: "PlanningAreaLoginId",
                principalTable: "PlanningAreaLogins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaLogins_Users_UserId",
                table: "PlanningAreaLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaAuthorizations_PlanningAreaLogins_PlanningAreaLoginId",
                table: "PlanningAreaAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaLogins_Users_UserId",
                table: "PlanningAreaLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanningAreaLogins",
                table: "PlanningAreaLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanningAreaAuthorizations",
                table: "PlanningAreaAuthorizations");

            migrationBuilder.RenameTable(
                name: "PlanningAreaLogins",
                newName: "PAUsers");

            migrationBuilder.RenameTable(
                name: "PlanningAreaAuthorizations",
                newName: "PlanningAreaUsers");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningAreaLogins_UserId",
                table: "PAUsers",
                newName: "IX_PAUsers_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PAUsers",
                table: "PAUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanningAreaUsers",
                table: "PlanningAreaUsers",
                columns: new[] { "PlanningAreaLoginId", "PlanningAreaKey" });

            migrationBuilder.AddForeignKey(
                name: "FK_PAUsers_Users_UserId",
                table: "PAUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaUsers_PAUsers_PlanningAreaLoginId",
                table: "PlanningAreaUsers",
                column: "PlanningAreaLoginId",
                principalTable: "PAUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
