using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationAndGantt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroup",
                table: "UserGroup");

            migrationBuilder.DropIndex(
                name: "IX_UserGroup_GroupsId",
                table: "UserGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupCategory",
                table: "GroupCategory");

            migrationBuilder.DropIndex(
                name: "IX_GroupCategory_CategoriesId",
                table: "GroupCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroup",
                table: "UserGroup",
                columns: new[] { "GroupsId", "UsersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupCategory",
                table: "GroupCategory",
                columns: new[] { "CategoriesId", "GroupsId" });

            migrationBuilder.CreateTable(
                name: "GanttFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CosmosDbRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GanttFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GanttFavorites_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GanttFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationType", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "UserNotification",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotification", x => new { x.NotificationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserNotification_NotificationType_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "NotificationType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserNotification_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_UsersId",
                table: "UserGroup",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCategory_GroupsId",
                table: "GroupCategory",
                column: "GroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_GanttFavoriteGroup_GroupsId",
                table: "GanttFavoriteGroup",
                column: "GroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_GanttFavorites_CompanyId",
                table: "GanttFavorites",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GanttFavorites_UserId",
                table: "GanttFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotification_UserId",
                table: "UserNotification",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GanttFavoriteGroup");

            migrationBuilder.DropTable(
                name: "UserNotification");

            migrationBuilder.DropTable(
                name: "GanttFavorites");

            migrationBuilder.DropTable(
                name: "NotificationType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroup",
                table: "UserGroup");

            migrationBuilder.DropIndex(
                name: "IX_UserGroup_UsersId",
                table: "UserGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupCategory",
                table: "GroupCategory");

            migrationBuilder.DropIndex(
                name: "IX_GroupCategory_GroupsId",
                table: "GroupCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroup",
                table: "UserGroup",
                columns: new[] { "UsersId", "GroupsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupCategory",
                table: "GroupCategory",
                columns: new[] { "GroupsId", "CategoriesId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_GroupsId",
                table: "UserGroup",
                column: "GroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCategory_CategoriesId",
                table: "GroupCategory",
                column: "CategoriesId");
        }
    }
}
