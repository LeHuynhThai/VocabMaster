using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationJsonToDictionaryDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranslationsJson",
                table: "DictionaryDetails");

            migrationBuilder.AddColumn<string>(
                name: "Vietnamese",
                table: "Vocabularies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vietnamese",
                table: "Vocabularies");

            migrationBuilder.AddColumn<string>(
                name: "TranslationsJson",
                table: "DictionaryDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
