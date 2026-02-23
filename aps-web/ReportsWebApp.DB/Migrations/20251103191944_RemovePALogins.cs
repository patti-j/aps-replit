using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class RemovePALogins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningAreaAuthorizations");

            migrationBuilder.DropTable(
                name: "PlanningAreaLogins");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanningAreaLogins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PAPermissionGroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningAreaLogins_PlanningAreaPermissionGroups_PAPermissionGroupId",
                        column: x => x.PAPermissionGroupId,
                        principalTable: "PlanningAreaPermissionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanningAreaLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanningAreaAuthorizations",
                columns: table => new
                {
                    PlanningAreaLoginId = table.Column<int>(type: "int", nullable: false),
                    PlanningAreaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaAuthorizations", x => new { x.PlanningAreaLoginId, x.PlanningAreaKey });
                    table.ForeignKey(
                        name: "FK_PlanningAreaAuthorizations_PlanningAreaLogins_PlanningAreaLoginId",
                        column: x => x.PlanningAreaLoginId,
                        principalTable: "PlanningAreaLogins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaLogins_PAPermissionGroupId",
                table: "PlanningAreaLogins",
                column: "PAPermissionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaLogins_UserId",
                table: "PlanningAreaLogins",
                column: "UserId");
        }
    }
}
