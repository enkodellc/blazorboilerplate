using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations
{
    public partial class Multitenancy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AspNetRoles",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: false),
                    Author = table.Column<string>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_TenantId",
                table: "AspNetRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_TenantId",
                table: "Books",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Title",
                table: "Tenants",
                column: "Title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_Tenants_TenantId",
                table: "AspNetRoles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_Tenants_TenantId",
                table: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_TenantId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetRoles");
        }
    }
}
