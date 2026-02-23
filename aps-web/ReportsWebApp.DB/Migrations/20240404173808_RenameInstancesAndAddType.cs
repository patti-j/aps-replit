using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ReportsWebApp.DB.Models;

#nullable disable

namespace ReportsWebApp.DB.Migrations
{
    /// <inheritdoc />
    public partial class RenameInstancesAndAddType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("Instances", "dbo", "CompanyDbs");
            migrationBuilder.AddColumn<EDbType>("DbType", "CompanyDbs", defaultValue: EDbType.Import);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("DbType", "CompanyDbs");
            migrationBuilder.RenameTable("CompanyDbs", "dbo", "Instances");
        }
    }
}
