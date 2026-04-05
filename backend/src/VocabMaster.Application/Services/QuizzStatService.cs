using VocabMaster.Application.Interfaces;
using VocabMaster.Domain.Entities;
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

        public async Task<int> GetTotalQuestions()
        {
            return await _quizzStatRepo.GetTotalQuestions();
        }

        public async Task<List<CompletedQuiz>> GetCompletedQuizzes(int userId)
        {
            return await _quizzStatRepo.GetCompletedQuizzes(userId);
        }
    }
}
