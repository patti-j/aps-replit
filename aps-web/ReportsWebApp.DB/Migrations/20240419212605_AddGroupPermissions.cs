using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Readonly",
                table: "Groups",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            // Create default Groups, and assign each User to the appropriate default groups based on their role
            migrationBuilder.Sql("INSERT INTO [dbo].PermissionKeys ([Key]) VALUES ('viewDashboard'), ('viewReports'), ('manageReports'), ('manageIntegration'), ('manageUsers'), ('managePermissionGroups'), ('accessGantt'), ('ptAdmin')");
            migrationBuilder.Sql(@"INSERT INTO [dbo].Groups (CompanyId, [Name], CreationDate, [Readonly]) VALUES (1, 'PTAdmin', GETDATE(), 1)
INSERT INTO [dbo].Groups (CompanyId, [Name], CreationDate, [Readonly])  (SELECT Id, 'Default Administrator', GETDATE(), 1 from Companies)
INSERT INTO [dbo].Groups (CompanyId, [Name], CreationDate, [Readonly])  (SELECT Id, 'Default User', GETDATE(), 1 from Companies)
INSERT INTO [dbo].Groups (CompanyId, [Name], CreationDate, [Readonly])  (SELECT Id, 'Default Reports Admin', GETDATE(), 1 from Companies)");
            migrationBuilder.Sql(@"DECLARE @viewDashboard int
SELECT @viewDashboard = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'viewDashboard'
DECLARE @viewReports int
SELECT @viewReports = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'viewReports'
DECLARE @manageReports int
SELECT @manageReports = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'manageReports'
DECLARE @manageIntegration int
SELECT @manageIntegration = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'manageIntegration'
DECLARE @manageUsers int
SELECT @manageUsers = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'manageUsers'
DECLARE @managePermissionGroups int
SELECT @managePermissionGroups = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'managePermissionGroups'
DECLARE @accessGantt int
SELECT @accessGantt = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'accessGantt'
DECLARE @ptAdmin int
SELECT @ptAdmin = Id FROM [dbo].PermissionKeys p WHERE p.[Key] = 'ptAdmin'

INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewDashboard FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewReports FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @manageReports FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @manageIntegration FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @manageUsers FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @managePermissionGroups FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @accessGantt FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @ptAdmin FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin'

INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewDashboard FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewReports FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @manageReports FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @manageIntegration FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @manageUsers FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @managePermissionGroups FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @accessGantt FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator'

INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewDashboard FROM [dbo].Groups g WHERE g.[Name] = 'Default User'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewReports FROM [dbo].Groups g WHERE g.[Name] = 'Default User'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @accessGantt FROM [dbo].Groups g WHERE g.[Name] = 'Default User'

INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewDashboard FROM [dbo].Groups g WHERE g.[Name] = 'Default Reports Admin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @manageReports FROM [dbo].Groups g WHERE g.[Name] = 'Default Reports Admin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @viewReports FROM [dbo].Groups g WHERE g.[Name] = 'Default Reports Admin'
INSERT INTO [dbo].GroupPermission (GroupId, PermissionId) SELECT Id, @accessGantt FROM [dbo].Groups g WHERE g.[Name] = 'Default Reports Admin'");
            migrationBuilder.Sql(@"INSERT INTO [dbo].UserGroup (UsersId, GroupsId) SELECT u.Id, (SELECT Id FROM [dbo].Groups g WHERE g.[Name] = 'PTAdmin' AND g.CompanyId = 1) FROM [dbo].Users u JOIN [dbo].UserCompany uc ON uc.UserId = u.Id WHERE u.Role = 0
INSERT INTO [dbo].UserGroup (UsersId, GroupsId) SELECT u.Id, (SELECT Id FROM [dbo].Groups g WHERE g.[Name] = 'Default Administrator' AND g.CompanyId = uc.CompanyId) FROM [dbo].Users u JOIN [dbo].UserCompany uc ON uc.UserId = u.Id WHERE u.Role = 1
INSERT INTO [dbo].UserGroup (UsersId, GroupsId) SELECT u.Id, (SELECT Id FROM [dbo].Groups g WHERE g.[Name] = 'Default User' AND g.CompanyId = uc.CompanyId) FROM [dbo].Users u JOIN [dbo].UserCompany uc ON uc.UserId = u.Id WHERE u.Role = 3
INSERT INTO [dbo].UserGroup (UsersId, GroupsId) SELECT u.Id, (SELECT Id FROM [dbo].Groups g WHERE g.[Name] = 'Default Reports Admin' AND g.CompanyId = uc.CompanyId) FROM [dbo].Users u JOIN [dbo].UserCompany uc ON uc.UserId = u.Id WHERE u.Role = 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupPermission");

            migrationBuilder.DropTable(
                name: "PermissionKeys");

            migrationBuilder.DropColumn(
                name: "Readonly",
                table: "Groups");
        }
    }
}
