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
                    MsgId = table.Column<string>(nullable: true),
                    MsgIdPlural = table.Column<string>(nullable: true),
                    Translation = table.Column<string>(nullable: true),
                    Culture = table.Column<string>(nullable: true),
                    ContextId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluralFormRules",
                columns: table => new
                {
                    Language = table.Column<string>(maxLength: 5, nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Selector = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluralFormRules", x => x.Language);
                });

            migrationBuilder.CreateTable(
                name: "PluralTranslations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Index = table.Column<int>(nullable: false),
                    Translation = table.Column<string>(nullable: false),
                    LocalizationRecordId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluralTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluralTranslations_LocalizationRecords_LocalizationRecordId",
                        column: x => x.LocalizationRecordId,
                        principalTable: "LocalizationRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationRecords_MsgId_Culture_ContextId",
                table: "LocalizationRecords",
                columns: new[] { "MsgId", "Culture", "ContextId" },
                unique: true,
                filter: "[MsgId] IS NOT NULL AND [Culture] IS NOT NULL AND [ContextId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PluralTranslations_LocalizationRecordId",
                table: "PluralTranslations",
                column: "LocalizationRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PluralFormRules");

            migrationBuilder.DropTable(
                name: "PluralTranslations");

            migrationBuilder.DropTable(
                name: "LocalizationRecords");
        }
    }
}
