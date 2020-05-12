using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations.TenantStoreDb
{
    public partial class DefaultTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TenantInfo",
                keyColumn: "Id",
                keyValue: "BlazorBoilerplate");

            migrationBuilder.InsertData(
                table: "TenantInfo",
                columns: new[] { "Id", "ConnectionString", "Identifier", "Name" },
                values: new object[] { "Master", null, "Master", "Master" });

            migrationBuilder.Sql("UPDATE AspNetUserTokens SET TenantId='Master' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql("UPDATE AspNetUserRoles SET TenantId='Master' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql("UPDATE AspNetUserLogins SET TenantId='Master' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql("UPDATE AspNetUserClaims SET TenantId='Master' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql("UPDATE AspNetRoles SET TenantId='Master' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql("UPDATE AspNetRoleClaims SET TenantId='Master' WHERE TenantId='BlazorBoilerplate'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TenantInfo",
                keyColumn: "Id",
                keyValue: "Master");

            migrationBuilder.InsertData(
                table: "TenantInfo",
                columns: new[] { "Id", "ConnectionString", "Identifier", "Name" },
                values: new object[] { "BlazorBoilerplate", null, "BlazorBoilerplate", "BlazorBoilerplate" });

            migrationBuilder.Sql("UPDATE AspNetUserTokens SET TenantId='BlazorBoilerplate' WHERE TenantId='Master'");
            migrationBuilder.Sql("UPDATE AspNetUserRoles SET TenantId='BlazorBoilerplate' WHERE TenantId='Master'");
            migrationBuilder.Sql("UPDATE AspNetUserLogins SET TenantId='BlazorBoilerplate' WHERE TenantId='Master'");
            migrationBuilder.Sql("UPDATE AspNetUserClaims SET TenantId='BlazorBoilerplate' WHERE TenantId='Master'");
            migrationBuilder.Sql("UPDATE AspNetRoles SET TenantId='BlazorBoilerplate' WHERE TenantId='Master'");
            migrationBuilder.Sql("UPDATE AspNetRoleClaims SET TenantId='BlazorBoilerplate' WHERE TenantId='Master'");
        }
    }
}
