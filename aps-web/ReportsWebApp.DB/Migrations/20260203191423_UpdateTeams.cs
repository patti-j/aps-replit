using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScopedRole",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    ScopeId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScopedRole", x => new { x.TeamId, x.ScopeId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ScopedRole_PlanningAreaScopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "PlanningAreaScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScopedRole_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScopedRole_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTeam",
                columns: table => new
                {
                    UsersId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTeam", x => new { x.TeamId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserTeam_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserTeam_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScopedRole_RoleId",
                table: "ScopedRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScopedRole_ScopeId",
                table: "ScopedRole",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTeam_UsersId",
                table: "UserTeam",
                column: "UsersId");
            
            migrationBuilder.AddColumn<string>(
                name: "ComputedRolesAndScopes",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComputedRolesAndScopes",
                table: "Teams");
            
            migrationBuilder.DropTable(
                name: "ScopedRole");

            migrationBuilder.DropTable(
                name: "UserTeam");
        }
    }
}
