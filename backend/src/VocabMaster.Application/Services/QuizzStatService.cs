using VocabMaster.Application.Interfaces;
using VocabMaster.Application.Models.Quiz;
using VocabMaster.Domain.Interfaces;

namespace VocabMaster.Application.Services
{
    public class QuizzStatService : IQuizzStatService
    {
        private readonly IQuizzStatRepo _quizzStatRepo;

        public QuizzStatService(IQuizzStatRepo quizzStatRepo)
        {
            _quizzStatRepo = quizzStatRepo;
        }

        public async Task<QuizStatsSummary> GetQuizStats(int userId)
        {
            var totalQuestions = await _quizzStatRepo.GetTotalQuestions();
            var completedQuizzes = await _quizzStatRepo.GetCompletedQuizzes(userId);

            var completedQuestions = completedQuizzes.Count;
            var correctAnswers = completedQuizzes.Count(cq => cq.WasCorrect);
            var accuracyRate = completedQuestions == 0
                ? 0
                : (double)correctAnswers / completedQuestions * 100;

            return new QuizStatsSummary
            {
                TotalQuestions = totalQuestions,
                CompletedQuestions = completedQuestions,
                CorrectAnswers = correctAnswers,
                AccuracyRate = accuracyRate
            };
        }

        public async Task<List<CompletedQuizAnswerSummary>> GetCompletedAnswers(int userId)
        {
            var completedQuizzes = await _quizzStatRepo.GetCompletedQuizzes(userId);

            return completedQuizzes.Select(completedQuiz => new CompletedQuizAnswerSummary
            {
                Id = completedQuiz.Id,
                QuizQuestionId = completedQuiz.QuizQuestionId,
                Word = completedQuiz.QuizQuestion.Word,
                CorrectAnswer = completedQuiz.QuizQuestion.CorrectAnswer,
                CompletedAt = completedQuiz.CompletedAt,
                WasCorrect = completedQuiz.WasCorrect
            }).ToList();
        }
    }
}
