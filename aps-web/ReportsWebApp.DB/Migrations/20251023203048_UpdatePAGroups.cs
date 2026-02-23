using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePAGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PermissionGroupId",
                table: "PlanningAreaTags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserPATag",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PAGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPATag", x => new { x.PAGroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserPATag_PlanningAreaTags_PAGroupId",
                        column: x => x.PAGroupId,
                        principalTable: "PlanningAreaTags",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPATag_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaTags_PermissionGroupId",
                table: "PlanningAreaTags",
                column: "PermissionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPATag_UserId",
                table: "UserPATag",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaTags_PlanningAreaPermissionGroups_PermissionGroupId",
                table: "PlanningAreaTags",
                column: "PermissionGroupId",
                principalTable: "PlanningAreaPermissionGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaTags_PlanningAreaPermissionGroups_PermissionGroupId",
                table: "PlanningAreaTags");

            migrationBuilder.DropTable(
                name: "UserPATag");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreaTags_PermissionGroupId",
                table: "PlanningAreaTags");

            migrationBuilder.DropColumn(
                name: "PermissionGroupId",
                table: "PlanningAreaTags");
        }
    }
}
