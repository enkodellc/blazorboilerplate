using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace BlazorBoilerplate.Server.Migrations
{
    public partial class ApiLogUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "ApiLogs",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ApiLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "ApiLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApiLogs");
        }
    }
}
