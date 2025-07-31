using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletedQuizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    QuizQuestionId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WasCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedQuizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedQuizzes_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedQuizzes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedQuizzes_QuizQuestionId",
                table: "CompletedQuizzes",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedQuizzes_UserId_QuizQuestionId",
                table: "CompletedQuizzes",
                columns: new[] { "UserId", "QuizQuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedQuizzes");
        }
    }
}
