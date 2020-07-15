using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations.TenantStoreDb
{
    public partial class CreateTenantStoreDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantInfo",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    Identifier = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    ConnectionString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantInfo", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TenantInfo",
                columns: new[] { "Id", "ConnectionString", "Identifier", "Name" },
                values: new object[] { "Master", null, "Master", "Master" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantInfo_Identifier",
                table: "TenantInfo",
                column: "Identifier",
                unique: true,
                filter: "[Identifier] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantInfo");
        }
    }
}
