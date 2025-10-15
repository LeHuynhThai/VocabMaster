using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementation
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
            return await _quizzQuestionRepo.GetRandomUncompletedQuestion(userId);
        }

        public async Task<bool> SubmitQuizAnswer(int userId, int quizQuestionId, string selectedAnswer)
        {
            var question = await _quizzQuestionRepo.GetQuestionById(quizQuestionId);
            if (question == null)
            {
                return false;
            }
            bool isCorrect = selectedAnswer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            await _quizzQuestionRepo.SaveCompletedQuiz(userId, quizQuestionId, isCorrect);
            return isCorrect;
        }
    }
}
