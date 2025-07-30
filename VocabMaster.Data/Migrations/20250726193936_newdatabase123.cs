using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabMaster.Migrations
{
    /// <inheritdoc />
    public partial class newdatabase123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictionaryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phonetic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneticsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeaningsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TranslationsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryDetails_Word",
                table: "DictionaryDetails",
                column: "Word",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictionaryDetails");
        }
    }
}
