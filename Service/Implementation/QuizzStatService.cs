using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementation
{
    public class QuizzStatService : IQuizzStatService
    {
        private readonly IQuizzStatRepo _quizzStatRepo;

        public QuizzStatService(IQuizzStatRepo quizzStatRepo)
        {
            _quizzStatRepo = quizzStatRepo;
        }

        public async Task<int> GetTotalQuestions()
        {
            return await _quizzStatRepo.GetTotalQuestions();
        }

        public async Task<List<CompletedQuiz>> GetCompletedQuizzes(int userId)
        {
            return await _quizzStatRepo.GetCompletedQuizzes(userId);
        }

        public async Task<List<CompletedQuiz>> GetCorrectAnswers(int userId)
        {
            return await _quizzStatRepo.GetCorrectAnswers(userId);
        }
    }
}
