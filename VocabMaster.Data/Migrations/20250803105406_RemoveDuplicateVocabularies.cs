using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabMaster.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateVocabularies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa dữ liệu trùng lặp trước khi tạo unique index
            migrationBuilder.Sql(@"
                WITH DuplicateWords AS (
                    SELECT Word, 
                           ROW_NUMBER() OVER (PARTITION BY Word ORDER BY Id) AS RowNum
                    FROM Vocabularies
                )
                DELETE FROM Vocabularies
                WHERE Id IN (
                    SELECT v.Id
                    FROM Vocabularies v
                    JOIN DuplicateWords d ON v.Word = d.Word
                    WHERE d.RowNum > 1
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_Word",
                table: "Vocabularies",
                column: "Word",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vocabularies_Word",
                table: "Vocabularies");
        }
    }
}
