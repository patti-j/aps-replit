using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Prod.Migrations
{
    /// <inheritdoc />
    public partial class CompanyDBConnectionStringChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GanttFavoriteGroup");

            migrationBuilder.RenameColumn(
                name: "PasswordKey",
                table: "CompanyDbs",
                newName: "ImportUserPasswordKey");

            migrationBuilder.AddColumn<string>(
                name: "DBName",
                table: "CompanyDbs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DBPasswordKey",
                table: "CompanyDbs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DBServerName",
                table: "CompanyDbs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DBUserName",
                table: "CompanyDbs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "GroupSchedulerFavorite",
                columns: table => new
                {
                    GanttFavoritesId = table.Column<int>(type: "int", nullable: false),
                    GroupsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSchedulerFavorite", x => new { x.GanttFavoritesId, x.GroupsId });
                    table.ForeignKey(
                        name: "FK_GroupSchedulerFavorite_GanttFavorites_GanttFavoritesId",
                        column: x => x.GanttFavoritesId,
                        principalTable: "GanttFavorites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupSchedulerFavorite_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupSchedulerFavorite_GroupsId",
                table: "GroupSchedulerFavorite",
                column: "GroupsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupSchedulerFavorite");

            migrationBuilder.DropColumn(
                name: "DBName",
                table: "CompanyDbs");

            migrationBuilder.DropColumn(
                name: "DBPasswordKey",
                table: "CompanyDbs");

            migrationBuilder.DropColumn(
                name: "DBServerName",
                table: "CompanyDbs");

            migrationBuilder.DropColumn(
                name: "DBUserName",
                table: "CompanyDbs");

            migrationBuilder.RenameColumn(
                name: "ImportUserPasswordKey",
                table: "CompanyDbs",
                newName: "PasswordKey");

            migrationBuilder.CreateTable(
                name: "GanttFavoriteGroup",
                columns: table => new
                {
                    GanttFavoritesId = table.Column<int>(type: "int", nullable: false),
                    GroupsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GanttFavoriteGroup", x => new { x.GanttFavoritesId, x.GroupsId });
                    table.ForeignKey(
                        name: "FK_GanttFavoriteGroup_GanttFavorites_GanttFavoritesId",
                        column: x => x.GanttFavoritesId,
                        principalTable: "GanttFavorites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GanttFavoriteGroup_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GanttFavoriteGroup_GroupsId",
                table: "GanttFavoriteGroup",
                column: "GroupsId");
        }
    }
}
