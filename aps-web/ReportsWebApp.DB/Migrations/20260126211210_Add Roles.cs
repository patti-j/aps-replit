using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DesktopPermissions",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Groups",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SoftMigrationStatus",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable("BackupGroupPermissions", columns: table => new
            {
                GroupId = table.Column<int>(type: "int", nullable: false),
                Permissions = table.Column<string>(type: "nvarchar(max)", nullable: true)
            });

            migrationBuilder.Sql("INSERT INTO dbo.BackupGroupPermissions (GroupId, Permissions)\nSELECT Id, Permissions\nFROM dbo.Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Groups SET Groups.Permissions = BackupGroupPermissions.Permissions\nFROM dbo.Groups AS Groups\nINNER JOIN dbo.BackupGroupPermissions AS BackupGroupPermissions ON Groups.Id = BackupGroupPermissions.GroupId");
            
            migrationBuilder.DropTable("BackupGroupPermissions");
            
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "DesktopPermissions",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "SoftMigrationStatus",
                table: "Companies");
        }
    }
}
