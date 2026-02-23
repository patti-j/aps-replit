using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Prod.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [dbo].[PlanningAreaLogins]
");

            migrationBuilder.DropColumn(
                name: "PlantPermissionsId",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "UserPermissionSetId",
                table: "PlanningAreaLogins");

            migrationBuilder.AddColumn<int>(
                name: "PAPermissionGroupId",
                table: "PlanningAreaLogins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PAUserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    PackageObjectId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Allowed = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAUserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAUserPermissions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanningAreaPermissionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningAreaPermissionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningAreaPermissionGroups_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAPermissionGroupPAUserPermission",
                columns: table => new
                {
                    PAPermissionGroupId = table.Column<int>(type: "int", nullable: false),
                    PAUserPermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAPermissionGroupPAUserPermission", x => new { x.PAPermissionGroupId, x.PAUserPermissionId });
                    table.ForeignKey(
                        name: "FK_PAPermissionGroupPAUserPermission_PAUserPermissions_PAUserPermissionId",
                        column: x => x.PAUserPermissionId,
                        principalTable: "PAUserPermissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PAPermissionGroupPAUserPermission_PlanningAreaPermissionGroups_PAPermissionGroupId",
                        column: x => x.PAPermissionGroupId,
                        principalTable: "PlanningAreaPermissionGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaLogins_PAPermissionGroupId",
                table: "PlanningAreaLogins",
                column: "PAPermissionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PAPermissionGroupPAUserPermission_PAUserPermissionId",
                table: "PAPermissionGroupPAUserPermission",
                column: "PAUserPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PAUserPermissions_CompanyId",
                table: "PAUserPermissions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningAreaPermissionGroups_CompanyId",
                table: "PlanningAreaPermissionGroups",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningAreaLogins_PlanningAreaPermissionGroups_PAPermissionGroupId",
                table: "PlanningAreaLogins",
                column: "PAPermissionGroupId",
                principalTable: "PlanningAreaPermissionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningAreaLogins_PlanningAreaPermissionGroups_PAPermissionGroupId",
                table: "PlanningAreaLogins");

            migrationBuilder.DropTable(
                name: "PAPermissionGroupPAUserPermission");

            migrationBuilder.DropTable(
                name: "PAUserPermissions");

            migrationBuilder.DropTable(
                name: "PlanningAreaPermissionGroups");

            migrationBuilder.DropIndex(
                name: "IX_PlanningAreaLogins_PAPermissionGroupId",
                table: "PlanningAreaLogins");

            migrationBuilder.DropColumn(
                name: "PAPermissionGroupId",
                table: "PlanningAreaLogins");

            migrationBuilder.AddColumn<long>(
                name: "PlantPermissionsId",
                table: "PlanningAreaLogins",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserPermissionSetId",
                table: "PlanningAreaLogins",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
