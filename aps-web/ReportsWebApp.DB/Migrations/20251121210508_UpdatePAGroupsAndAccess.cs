using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePAGroupsAndAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "PlanningAreaAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanningAreaId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PermissionGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningAreaAccesses_PlanningAreaPermissionGroups_PermissionGroupId",
                        column: x => x.PermissionGroupId,
                        principalTable: "PlanningAreaPermissionGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanningAreaAccesses_PlanningAreas_PlanningAreaId",
                        column: x => x.PlanningAreaId,
                        principalTable: "PlanningAreas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanningAreaAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaAccesses_PermissionGroupId",
                table: "PlanningAreaAccesses",
                column: "PermissionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaAccesses_PlanningAreaId",
                table: "PlanningAreaAccesses",
                column: "PlanningAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaAccesses_UserId",
                table: "PlanningAreaAccesses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningAreaAccesses");

            migrationBuilder.AddColumn<int>(
                name: "PermissionGroupId",
                table: "PlanningAreaTags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserPATag",
                columns: table => new
                {
                    PAGroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
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
    }
}
