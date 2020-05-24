using BlazorBoilerplate.Shared;
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

            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserTokens')) UPDATE AspNetUserTokens SET TenantId='{Settings.DefaultTenantId}' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserRoles')) UPDATE AspNetUserRoles SET TenantId='{Settings.DefaultTenantId}' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserLogins')) UPDATE AspNetUserLogins SET TenantId='{Settings.DefaultTenantId}' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserClaims')) UPDATE AspNetUserClaims SET TenantId='{Settings.DefaultTenantId}' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetRoles')) UPDATE AspNetRoles SET TenantId='{Settings.DefaultTenantId}' WHERE TenantId='BlazorBoilerplate'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetRoleClaims')) UPDATE AspNetRoleClaims SET TenantId='{Settings.DefaultTenantId}' WHERE TenantId='BlazorBoilerplate'");
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

            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserTokens')) UPDATE AspNetUserTokens SET TenantId='BlazorBoilerplate' WHERE TenantId='{Settings.DefaultTenantId}'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserRoles')) UPDATE AspNetUserRoles SET TenantId='BlazorBoilerplate' WHERE TenantId='{Settings.DefaultTenantId}'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserLogins')) UPDATE AspNetUserLogins SET TenantId='BlazorBoilerplate' WHERE TenantId='{Settings.DefaultTenantId}'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetUserClaims')) UPDATE AspNetUserClaims SET TenantId='BlazorBoilerplate' WHERE TenantId='{Settings.DefaultTenantId}'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetRoles')) UPDATE AspNetRoles SET TenantId='BlazorBoilerplate' WHERE TenantId='{Settings.DefaultTenantId}'");
            migrationBuilder.Sql($"IF (EXISTS (SELECT * FROM sys.tables WHERE [name] = 'AspNetRoleClaims')) UPDATE AspNetRoleClaims SET TenantId='BlazorBoilerplate' WHERE TenantId='{Settings.DefaultTenantId}'");
        }
    }
}
