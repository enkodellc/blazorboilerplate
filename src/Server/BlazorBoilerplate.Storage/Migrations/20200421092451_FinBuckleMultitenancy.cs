using BlazorBoilerplate.Shared;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations
{
    public partial class FinBuckleMultitenancy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetUserTokens",
                maxLength: 64,
                nullable: false,
                defaultValue: Settings.DefaultTenantId);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetUserRoles",
                maxLength: 64,
                nullable: false,
                defaultValue: Settings.DefaultTenantId);

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "AspNetUserLogins",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE AspNetUserLogins SET Id=NEWID() WHERE Id=''");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetUserLogins",
                maxLength: 64,
                nullable: false,
                defaultValue: Settings.DefaultTenantId);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetUserClaims",
                maxLength: 64,
                nullable: false,
                defaultValue: Settings.DefaultTenantId);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetRoles",
                maxLength: 64,
                nullable: false,
                defaultValue: Settings.DefaultTenantId);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "AspNetRoleClaims",
                maxLength: 64,
                nullable: false,
                defaultValue: Settings.DefaultTenantId);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_LoginProvider_ProviderKey_TenantId",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                columns: new[] { "NormalizedName", "TenantId" },
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserLogins_LoginProvider_ProviderKey_TenantId",
                table: "AspNetUserLogins");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AspNetUserLogins");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUserLogins");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetRoleClaims");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
        }
    }
}
