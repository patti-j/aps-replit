using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationConfigFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VersionNumber",
                table: "IntegrationConfigs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IntegrationConfigs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "LastEditingUserId",
                table: "IntegrationConfigs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConfigs_LastEditingUserId",
                table: "IntegrationConfigs",
                column: "LastEditingUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationConfigs_Users_LastEditingUserId",
                table: "IntegrationConfigs",
                column: "LastEditingUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationConfigs_Users_LastEditingUserId",
                table: "IntegrationConfigs");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationConfigs_LastEditingUserId",
                table: "IntegrationConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "VersionNumber",
                table: "IntegrationConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IntegrationConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "LastEditingUserId",
                table: "IntegrationConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
