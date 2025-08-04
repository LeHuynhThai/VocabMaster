using AutoMapper;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services.Quiz;

namespace VocabMaster.Services.Quiz
{
    public class QuizQuestionService : IQuizQuestionService
    {
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly ICompletedQuizRepo _completedQuizRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizQuestionService> _logger;

        public QuizQuestionService(
            IQuizQuestionRepo quizQuestionRepo,
            ICompletedQuizRepo completedQuizRepo,
            IMapper mapper,
            ILogger<QuizQuestionService> logger)
        {
            _quizQuestionRepo = quizQuestionRepo ?? throw new ArgumentNullException(nameof(quizQuestionRepo));
            _completedQuizRepo = completedQuizRepo ?? throw new ArgumentNullException(nameof(completedQuizRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<QuizQuestionDto> GetRandomQuestion()
        {
            try
            {
                // Check if we have any quiz questions
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                // Get a random quiz question
                var question = await _quizQuestionRepo.GetRandomQuizQuestion();

                if (question == null)
                {
                    _logger.LogWarning("No quiz questions available");
                    return null;
                }

                // Log the question details for debugging
                _logger.LogInformation("Retrieved quiz question: Word={Word}, CorrectAnswer={CorrectAnswer}, WrongAnswer1={WrongAnswer1}, WrongAnswer2={WrongAnswer2}, WrongAnswer3={WrongAnswer3}",
                    question.Word, question.CorrectAnswer, question.WrongAnswer1, question.WrongAnswer2, question.WrongAnswer3);

                // Map to DTO
                var questionDto = _mapper.Map<QuizQuestionDto>(question);

                // Log the DTO details to verify mapping
                _logger.LogInformation("Mapped to DTO: Word={Word}, CorrectAnswer={CorrectAnswer}, WrongAnswer1={WrongAnswer1}, WrongAnswer2={WrongAnswer2}, WrongAnswer3={WrongAnswer3}",
                    questionDto.Word, questionDto.CorrectAnswer, questionDto.WrongAnswer1, questionDto.WrongAnswer2, questionDto.WrongAnswer3);

                return questionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random quiz question");
                throw;
            }
        }

        public async Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId)
        {
            try
            {
                // Check if we have any quiz questions
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();

                // Get IDs of completed questions for this user
                var completedQuestionIds = await _completedQuizRepo.GetCompletedQuizQuestionIdsByUserId(userId);

                // Get a random quiz question that hasn't been completed by this user
                var question = await _quizQuestionRepo.GetRandomUnansweredQuizQuestion(completedQuestionIds);

                // If all questions have been answered, get a random question
                if (question == null)
                {
                    _logger.LogInformation("User {UserId} has completed all quiz questions, returning a random one", userId);
                    question = await _quizQuestionRepo.GetRandomQuizQuestion();
                }

                if (question == null)
                {
                    _logger.LogWarning("No quiz questions available for user {UserId}", userId);
                    return null;
                }

                // Map to DTO
                var questionDto = _mapper.Map<QuizQuestionDto>(question);

                return questionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random uncompleted quiz question for user {UserId}", userId);
                throw;
            }
        }
    }
}