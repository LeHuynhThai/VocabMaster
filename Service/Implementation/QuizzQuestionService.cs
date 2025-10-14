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
    }
}
