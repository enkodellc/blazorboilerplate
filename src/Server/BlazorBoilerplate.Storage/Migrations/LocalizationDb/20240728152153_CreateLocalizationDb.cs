using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorBoilerplate.Storage.Migrations.LocalizationDb
{
    /// <inheritdoc />
    public partial class CreateLocalizationDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalizationRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MsgId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MsgIdPlural = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Translation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Culture = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ContextId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluralFormRules",
                columns: table => new
                {
                    Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Selector = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluralFormRules", x => x.Language);
                });

            migrationBuilder.CreateTable(
                name: "PluralTranslations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Translation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalizationRecordId = table.Column<long>(type: "bigint", nullable: false)
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

        /// <inheritdoc />
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
