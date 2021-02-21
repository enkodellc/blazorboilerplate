using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations.ApiContentDb
{
    public partial class CreateApiContentDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WikiPages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pageid = table.Column<int>(nullable: false),
                    ns = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Extract = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiPages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WikiPages_pageid",
                table: "WikiPages",
                column: "pageid",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WikiPages");
        }
    }
}
