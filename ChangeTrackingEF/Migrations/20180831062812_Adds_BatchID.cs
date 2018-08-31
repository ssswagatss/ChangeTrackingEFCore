using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChangeTrackingEF.Migrations
{
    public partial class Adds_BatchID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "ChangeLogs",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "ChangeLogs");
        }
    }
}
