using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class RenameOtherGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroup", 
                table: "UserGroup");
            
            migrationBuilder.DropForeignKey(
                name: "FK_UserGroup_Roles_GroupsId", 
                table: "UserGroup");
            
            migrationBuilder.DropForeignKey(
                name: "FK_UserGroup_Users_UsersId", 
                table: "UserGroup");
            
            migrationBuilder.DropIndex(
                name: "IX_UserGroup_UsersId",
                table: "UserGroup");
            
            migrationBuilder.RenameTable(
                name: "UserGroup", 
                newName: "UserRole");
            
            migrationBuilder.RenameColumn(
                name: "GroupsId", 
                newName: "RoleId", 
                table: "UserRole");
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRole", 
                table: "UserRole", 
                columns: new string[] { "UsersId",  "RoleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Roles_RoleId", 
                table: "UserRole", 
                column: "RoleId", 
                principalTable: "Roles", 
                principalColumn: "Id");
            
            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Users_UsersId", 
                table: "UserRole", 
                column: "UsersId", 
                principalTable: "Users", 
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UsersId",
                table: "UserRole",
                column: "UsersId");
            
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupSchedulerFavorite", 
                table: "GroupSchedulerFavorite");
            
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSchedulerFavorite_GanttFavorites_GanttFavoritesId", 
                table: "GroupSchedulerFavorite");
            
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSchedulerFavorite_Roles_GroupsId", 
                table: "GroupSchedulerFavorite");
            
            migrationBuilder.DropIndex(
                name: "IX_GroupSchedulerFavorite_GroupsId", 
                table: "GroupSchedulerFavorite");
            
            migrationBuilder.RenameTable(
                name: "GroupSchedulerFavorite",
                newName: "SchedulerFavorite");
            
            migrationBuilder.RenameColumn(
                name: "GroupsId", 
                table: "SchedulerFavorite", 
                newName: "RolesId");
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_SchedulerFavorite", 
                table: "SchedulerFavorite", 
                columns: new string[]{ "GanttFavoritesId", "RolesId" });
            
            migrationBuilder.AddForeignKey(
                name: "FK_SchedulerFavorite_GanttFavorites_GanttFavoritesId",
                table: "SchedulerFavorite",
                column: "GanttFavoritesId",
                principalTable: "GanttFavorites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.AddForeignKey(
                name: "FK_SchedulerFavorite_Roles_RolesId",
                table: "SchedulerFavorite",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerFavorite_RolesId",
                table: "SchedulerFavorite",
                column: "RolesId");
            
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupCategory",
                table: "GroupCategory");
            
            migrationBuilder.DropForeignKey(
                name: "FK_GroupCategory_Categories_CategoriesId",
                table: "GroupCategory");
            
            migrationBuilder.DropForeignKey(
                name: "FK_GroupCategory_Roles_GroupsId",
                table: "GroupCategory");
            
            migrationBuilder.DropIndex(
                name: "IX_GroupCategory_GroupsId",
                table: "GroupCategory");
            
            migrationBuilder.RenameTable(
                name: "GroupCategory",
                newName: "RoleCategory");

            migrationBuilder.RenameColumn(
                name: "GroupsId",
                newName: "RolesId", 
                table: "RoleCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleCategory",
                table: "RoleCategory",
                columns: new string[] { "CategoriesId", "RolesId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RoleCategory_Categories_CategoriesId",
                table: "RoleCategory",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id");
            
            migrationBuilder.AddForeignKey(
                name: "FK_RoleCategory_Roles_RolesId",
                table: "RoleCategory",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RoleCategory_RolesId",
                table: "RoleCategory",
                column: "RolesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleCategory", 
                table: "RoleCategory");
            
            migrationBuilder.DropForeignKey(
                name: "FK_RoleCategory_Categories_CategoriesId", 
                table: "RoleCategory");
            
            migrationBuilder.DropForeignKey(
                name: "FK_RoleCategory_Roles_RolesId", 
                table: "RoleCategory");
            
            migrationBuilder.DropIndex(
                name: "IX_RoleCategory_RolesId", 
                table: "RoleCategory");
            
            migrationBuilder.RenameTable(
                name: "RoleCategory",
                newName: "GroupCategory");

            migrationBuilder.RenameColumn(
                name: "RolesId",
                newName: "GroupsId", 
                table: "GroupCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupCategory", 
                table: "GroupCategory",
                columns: new string[] { "CategoriesId", "GroupsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GroupCategory_Categories_CategoriesId", 
                table: "GroupCategory",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id");
            
            migrationBuilder.AddForeignKey(
                name: "FK_GroupCategory_Roles_GroupsId", 
                table: "GroupCategory",
                column: "GroupsId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCategory_GroupsId",
                table: "GroupCategory",
                column: "GroupsId");
            
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_SchedulerFavorite", 
                table: "SchedulerFavorite");
            
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulerFavorite_GanttFavorites_GanttFavoritesId", 
                table: "SchedulerFavorite");
            
            migrationBuilder.DropForeignKey(
                name: "FK_SchedulerFavorite_Roles_RolesId", 
                table: "SchedulerFavorite");
            
            migrationBuilder.DropIndex(
                name: "IX_SchedulerFavorite_RolesId", 
                table: "SchedulerFavorite");
            
            migrationBuilder.RenameTable(
                name: "SchedulerFavorite",
                newName: "GroupSchedulerFavorite");
            
            migrationBuilder.RenameColumn(
                name: "RolesId", 
                table: "GroupSchedulerFavorite", 
                newName: "GroupsId");
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupSchedulerFavorite", 
                table: "GroupSchedulerFavorite", 
                columns: new string[]{ "GanttFavoritesId", "GroupsId" });
            
            migrationBuilder.AddForeignKey(
                name: "FK_GroupSchedulerFavorite_GanttFavorites_GanttFavoritesId", 
                table: "GroupSchedulerFavorite",
                column: "GanttFavoritesId",
                principalTable: "GanttFavorites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            
            migrationBuilder.AddForeignKey(
                name: "FK_GroupSchedulerFavorite_Roles_GroupsId", 
                table: "GroupSchedulerFavorite",
                column: "GroupsId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_GroupSchedulerFavorite_GroupsId",
                table: "GroupSchedulerFavorite",
                column: "GroupsId");
            
            
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRole", 
                table: "UserRole");
            
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Roles_RoleId", 
                table: "UserRole");
            
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Users_UsersId", 
                table: "UserRole");
            
            migrationBuilder.DropIndex(
                name: "IX_UserRole_UsersId", 
                table: "UserRole");
            
            migrationBuilder.RenameTable(
                name: "UserRole", 
                newName: "UserGroup"); 
            
            migrationBuilder.RenameColumn(
                name: "RoleId", 
                newName: "GroupsId", 
                table: "UserGroup"); 
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroup", 
                table: "UserGroup", 
                columns: new string[] { "UsersId",  "GroupsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroup_Roles_GroupsId", 
                table: "UserGroup", 
                column: "GroupsId", 
                principalTable: "Roles", 
                principalColumn: "Id");
            
            migrationBuilder.AddForeignKey(
                name: "FK_UserGroup_Users_UsersId", 
                table: "UserGroup", 
                column: "UsersId", 
                principalTable: "Users", 
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_UsersId", 
                table: "UserGroup",
                column: "UsersId");
        }
    }
}
