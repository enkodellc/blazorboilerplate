using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations
{
    public partial class SQLDbLoggingMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(nullable: true),
                    MessageTemplate = table.Column<string>(nullable: true),
                    Level = table.Column<string>(nullable: true),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Exception = table.Column<string>(nullable: true),
                    Properties = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_TimeStamp",
                table: "Logs",
                column: "TimeStamp")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");
        }
    }
}
