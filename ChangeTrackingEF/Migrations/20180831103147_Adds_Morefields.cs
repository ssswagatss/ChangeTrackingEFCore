using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChangeTrackingEF.Migrations
{
    public partial class Adds_Morefields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AuthorDOB",
                table: "Posts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedMonth",
                table: "Posts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedYear",
                table: "Posts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorDOB",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatedMonth",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatedYear",
                table: "Posts");
        }
    }
}
