using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class RenameGroupsTableToRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupCategory_Groups_GroupsId",
                table: "GroupCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Companies_CompanyId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupSchedulerFavorite_Groups_GroupsId",
                table: "GroupSchedulerFavorite");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroup_Groups_GroupsId",
                table: "UserGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Roles");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_CompanyId",
                table: "Roles",
                newName: "IX_Roles_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");
            
            migrationBuilder.AddForeignKey(
                name: "FK_GroupCategory_Roles_GroupsId",
                table: "GroupCategory",
                column: "GroupsId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSchedulerFavorite_Roles_GroupsId",
                table: "GroupSchedulerFavorite",
                column: "GroupsId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Companies_CompanyId",
                table: "Roles",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroup_Roles_GroupsId",
                table: "UserGroup",
                column: "GroupsId",
                principalTable: "Roles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupCategory_Roles_GroupsId",
                table: "GroupCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupSchedulerFavorite_Roles_GroupsId",
                table: "GroupSchedulerFavorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Companies_CompanyId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroup_Roles_GroupsId",
                table: "UserGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Groups");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_CompanyId",
                table: "Groups",
                newName: "IX_Groups_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupCategory_Groups_GroupsId",
                table: "GroupCategory",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Companies_CompanyId",
                table: "Groups",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSchedulerFavorite_Groups_GroupsId",
                table: "GroupSchedulerFavorite",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroup_Groups_GroupsId",
                table: "UserGroup",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
