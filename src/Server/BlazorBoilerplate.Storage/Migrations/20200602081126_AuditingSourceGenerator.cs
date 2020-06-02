using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations
{
    public partial class AuditingSourceGenerator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Todos",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Todos",
                type: "datetime2(7)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedById",
                table: "Todos",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "Todos",
                type: "datetime2(7)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "IX_Todos_CreatedById",
                table: "Todos",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Todos_ModifiedById",
                table: "Todos",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_AspNetUsers_CreatedById",
                table: "Todos",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_AspNetUsers_ModifiedById",
                table: "Todos",
                column: "ModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_AspNetUsers_CreatedById",
                table: "Todos");

            migrationBuilder.DropForeignKey(
                name: "FK_Todos_AspNetUsers_ModifiedById",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_CreatedById",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_ModifiedById",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "Todos");
        }
    }
}
