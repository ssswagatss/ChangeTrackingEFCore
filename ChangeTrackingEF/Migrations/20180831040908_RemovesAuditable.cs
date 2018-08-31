using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChangeTrackingEF.Migrations
{
    public partial class RemovesAuditable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ChangeLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ChangeLogs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChangeLogs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ChangeLogs",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ChangeLogs",
                nullable: true);
        }
    }
}
