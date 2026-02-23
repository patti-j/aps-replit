using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateComplicatedRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscribedNotifications",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
            
            //ideally these migrations would use JSON agg functions but those are only in Azure SQL (and preview 2025) so it wouldn't work in local dev environments
            migrationBuilder.Sql(@"
WITH UserNotiMap(UserId, NotificationType) AS (
	SELECT UserId, T.Type
	FROM [UserNotification]
	RIGHT JOIN (
		SELECT Id, t.Type
		FROM NotificationType AS t
	) AS [T] ON [T].[Id] = NotificationId
), UserTypeMap(UserId, Types) AS (
	SELECT UserId, '[' + STRING_AGG(NotificationType, ',') + ']'  FROM UserNotiMap
	GROUP BY UserId
)

MERGE INTO Users U
	USING UserTypeMap M
		ON U.Id = M.UserId
WHEN MATCHED THEN
	UPDATE
		SET U.SubscribedNotifications = M.Types;");
            
            migrationBuilder.AddColumn<string>(
                name: "Permissions",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
            
            migrationBuilder.Sql(@"
WITH UserNotiMap(GroupId, PKey) AS (
	SELECT GroupId, s1.[Key]
	FROM [Groups]
	LEFT JOIN (
		SELECT [g1].[GroupId], [g1].[PermissionId], [p].[Id], [p].[Key]
		FROM [GroupPermission] AS [g1]
		INNER JOIN [PermissionKeys] AS [p] ON [g1].[PermissionId] = [p].[Id]
	) AS [s1] ON [Groups].[Id] = [s1].[GroupId]
), UserTypeMap(GroupId, Types) AS (
	SELECT GroupId, '[""' + STRING_AGG(PKey, '"", ""') + '""]'  FROM UserNotiMap
	GROUP BY GroupId
), GroupPermissionMap(GroupId, Permissions) AS (
	SELECT GroupId, Types
	FROM UserTypeMap
	WHERE GroupId IS NOT NULL
)

MERGE INTO Groups G
	USING GroupPermissionMap M
		ON G.Id = M.GroupId
WHEN MATCHED THEN
	UPDATE
		SET G.Permissions = M.Permissions;");
            
            migrationBuilder.DropTable(
                name: "GroupPermission");

            migrationBuilder.DropTable(
                name: "UserNotification");

            migrationBuilder.DropTable(
                name: "PermissionKeys");

            migrationBuilder.DropTable(
                name: "NotificationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscribedNotifications",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "Groups");

            migrationBuilder.CreateTable(
                name: "NotificationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserNotification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "GroupPermission",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermission", x => new { x.GroupId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_GroupPermission_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupPermission_PermissionKeys_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "PermissionKeys",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermission_PermissionId",
                table: "GroupPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotification_UserId",
                table: "UserNotification",
                column: "UserId");
        }
    }
}
