using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnedAtToLearnedWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LearnedAt",
                table: "LearnedVocabularies",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
                
            // Atualizar registros existentes com a data atual
            migrationBuilder.Sql("UPDATE LearnedVocabularies SET LearnedAt = GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LearnedAt",
                table: "LearnedVocabularies");
        }
    }
}
