using Microsoft.EntityFrameworkCore.Migrations;

namespace BlazorBoilerplate.Storage.Migrations.LocalizationDb
{
    public partial class CreateLocalizationDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalizationRecords",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    LocalizationCulture = table.Column<string>(nullable: false),
                    ResourceKey = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationRecords", x => x.Id);
                    table.UniqueConstraint("AK_LocalizationRecords_Key_LocalizationCulture_ResourceKey", x => new { x.Key, x.LocalizationCulture, x.ResourceKey });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalizationRecords");
        }
    }
}
