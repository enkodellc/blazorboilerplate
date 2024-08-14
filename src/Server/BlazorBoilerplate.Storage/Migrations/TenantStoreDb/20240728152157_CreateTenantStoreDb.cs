using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorBoilerplate.Storage.Migrations.TenantStoreDb
{
    /// <inheritdoc />
    public partial class CreateTenantStoreDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantInfo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantInfo");
        }
    }
}
