using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;

namespace VocabMaster.Application.Services
{
    public class QuizzQuestionService : IQuizzQuestionService
    {
        private readonly IQuizzQuestionRepo _quizzQuestionRepo;

        public QuizzQuestionService(IQuizzQuestionRepo quizzQuestionRepo)
        {
            _quizzQuestionRepo = quizzQuestionRepo;
        }

        public async Task<QuizQuestion?> GetRandomUncompletedQuestion(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            return await _quizzQuestionRepo.GetRandomUncompletedQuestion(userId);
        }

        public async Task<bool> SubmitQuizAnswer(int userId, int quizQuestionId, string selectedAnswer)
        {
            if (userId <= 0)
            {
                return false;
            }

            var question = await _quizzQuestionRepo.GetQuestionById(quizQuestionId);
            if (question == null)
            {
                return false;
            }

            var existingCompletion = await _quizzQuestionRepo.GetCompletedQuiz(userId, quizQuestionId);
            if (existingCompletion != null)
            {
                return existingCompletion.WasCorrect;
            }

            var isCorrect = selectedAnswer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            var completedQuiz = await _quizzQuestionRepo.SaveCompletedQuiz(userId, quizQuestionId, isCorrect);
            return completedQuiz.WasCorrect;
        }
    }
}
