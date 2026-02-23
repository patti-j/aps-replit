using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DBIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    VersionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DBIntegrationObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateCommand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DBIntegrationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBIntegrationObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DBIntegrationObjects_DBIntegrations_DBIntegrationId",
                        column: x => x.DBIntegrationId,
                        principalTable: "DBIntegrations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DBIntegrationObjects_DBIntegrationId",
                table: "DBIntegrationObjects",
                column: "DBIntegrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DBIntegrationObjects");

            migrationBuilder.DropTable(
                name: "DBIntegrations");
        }
    }
}
