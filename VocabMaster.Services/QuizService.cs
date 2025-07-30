using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;

namespace VocabMaster.Services
{
    /// <summary>
    /// Service for quiz operations
    /// </summary>
    public class QuizService : IQuizService
    {
        private readonly IQuizQuestionRepo _quizQuestionRepo;
        private readonly IVocabularyRepo _vocabularyRepo;
        private readonly ICompletedQuizRepo _completedQuizRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizService> _logger;
        private readonly Random _random = new Random();

        public QuizService(
            IQuizQuestionRepo quizQuestionRepo,
            IVocabularyRepo vocabularyRepo,
            ICompletedQuizRepo completedQuizRepo,
            IMapper mapper,
            ILogger<QuizService> logger)
        {
            _quizQuestionRepo = quizQuestionRepo;
            _vocabularyRepo = vocabularyRepo;
            _completedQuizRepo = completedQuizRepo;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gets a random quiz question
        /// </summary>
        /// <returns>A random quiz question DTO</returns>
        public async Task<QuizQuestionDto> GetRandomQuestion()
        {
            try
            {
                // Check if we have any quiz questions
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();
                
                // If not, create one from vocabulary
                if (!hasQuestions)
                {
                    await CreateQuizQuestionFromVocabulary();
                }
                
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

        /// <summary>
        /// Gets a random quiz question that hasn't been completed by the user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>A random uncompleted quiz question DTO</returns>
        public async Task<QuizQuestionDto> GetRandomUncompletedQuestion(int userId)
        {
            try
            {
                // Check if we have any quiz questions
                bool hasQuestions = await _quizQuestionRepo.AnyQuizQuestions();
                
                // If not, create one from vocabulary
                if (!hasQuestions)
                {
                    await CreateQuizQuestionFromVocabulary();
                }
                
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

        /// <summary>
        /// Creates a quiz question with random answers from vocabulary
        /// </summary>
        /// <returns>The created quiz question</returns>
        public async Task<QuizQuestion> CreateQuizQuestionFromVocabulary()
        {
            try
            {
                // Get random vocabulary for the question
                var vocabularies = await _vocabularyRepo.GetRandomVocabularies(4);
                
                if (vocabularies.Count < 4)
                {
                    _logger.LogWarning("Not enough vocabulary items to create a quiz question");
                    return null;
                }

                // Select one vocabulary item for the question
                var questionVocab = vocabularies[0];
                
                // Create quiz question
                var quizQuestion = new QuizQuestion
                {
                    Word = questionVocab.Word,
                    CorrectAnswer = questionVocab.Vietnamese,
                    WrongAnswer1 = vocabularies[1].Vietnamese,
                    WrongAnswer2 = vocabularies[2].Vietnamese,
                    WrongAnswer3 = vocabularies[3].Vietnamese
                };

                // Log the created question
                _logger.LogInformation("Created quiz question: Word={Word}, CorrectAnswer={CorrectAnswer}, WrongAnswer1={WrongAnswer1}, WrongAnswer2={WrongAnswer2}, WrongAnswer3={WrongAnswer3}",
                    quizQuestion.Word, quizQuestion.CorrectAnswer, quizQuestion.WrongAnswer1, quizQuestion.WrongAnswer2, quizQuestion.WrongAnswer3);

                // Save the quiz question
                return await _quizQuestionRepo.CreateQuizQuestion(quizQuestion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz question from vocabulary");
                throw;
            }
        }

        /// <summary>
        /// Verifies an answer to a quiz question
        /// </summary>
        /// <param name="questionId">The ID of the question</param>
        /// <param name="answer">The selected answer</param>
        /// <returns>Quiz result with information about correct/incorrect answer</returns>
        public async Task<QuizResultDto> CheckAnswer(int questionId, string answer)
        {
            try
            {
                // Get the quiz question
                var question = await _quizQuestionRepo.GetQuizQuestionById(questionId);
                
                if (question == null)
                {
                    _logger.LogWarning("Quiz question not found: {QuestionId}", questionId);
                    return new QuizResultDto
                    {
                        IsCorrect = false,
                        CorrectAnswer = null,
                        Message = "Câu hỏi không tồn tại"
                    };
                }

                // Check if the answer is correct
                bool isCorrect = question.CorrectAnswer == answer;
                
                return new QuizResultDto
                {
                    IsCorrect = isCorrect,
                    CorrectAnswer = question.CorrectAnswer,
                    Message = isCorrect ? "Chính xác!" : "Không chính xác. Hãy thử lại!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiz answer");
                throw;
            }
        }

        /// <summary>
        /// Verifies an answer to a quiz question and marks it as completed if correct
        /// </summary>
        /// <param name="questionId">The ID of the question</param>
        /// <param name="answer">The selected answer</param>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Quiz result with information about correct/incorrect answer</returns>
        public async Task<QuizResultDto> CheckAnswerAndMarkCompleted(int questionId, string answer, int userId)
        {
            try
            {
                _logger.LogInformation("CheckAnswerAndMarkCompleted called with QuestionId={QuestionId}, Answer={Answer}, UserId={UserId}",
                    questionId, answer, userId);
                
                // Get the result first
                var result = await CheckAnswer(questionId, answer);
                _logger.LogInformation("CheckAnswer result: IsCorrect={IsCorrect}, CorrectAnswer={CorrectAnswer}",
                    result.IsCorrect, result.CorrectAnswer);
                
                // If answer is correct, mark it as completed
                if (result.IsCorrect)
                {
                    _logger.LogInformation("Answer is correct, creating CompletedQuiz record");
                    var completedQuiz = new CompletedQuiz
                    {
                        UserId = userId,
                        QuizQuestionId = questionId,
                        WasCorrect = true
                    };
                    
                    _logger.LogInformation("Calling MarkAsCompleted with UserId={UserId}, QuizQuestionId={QuizQuestionId}, WasCorrect=true",
                        completedQuiz.UserId, completedQuiz.QuizQuestionId);
                    
                    try
                    {
                        var markedResult = await _completedQuizRepo.MarkAsCompleted(completedQuiz);
                        _logger.LogInformation("MarkAsCompleted successful. Result Id={ResultId}", markedResult.Id);
                    }
                    catch (Exception markEx)
                    {
                        _logger.LogError(markEx, "Error in MarkAsCompleted for correct answer");
                        throw;
                    }
                }
                else
                {
                    // Even if answer is wrong, we mark it as attempted but with wasCorrect = false
                    _logger.LogInformation("Answer is incorrect, creating CompletedQuiz record with WasCorrect=false");
                    var completedQuiz = new CompletedQuiz
                    {
                        UserId = userId,
                        QuizQuestionId = questionId,
                        WasCorrect = false
                    };
                    
                    _logger.LogInformation("Calling MarkAsCompleted with UserId={UserId}, QuizQuestionId={QuizQuestionId}, WasCorrect=false",
                        completedQuiz.UserId, completedQuiz.QuizQuestionId);
                    
                    try
                    {
                        var markedResult = await _completedQuizRepo.MarkAsCompleted(completedQuiz);
                        _logger.LogInformation("MarkAsCompleted successful. Result Id={ResultId}", markedResult.Id);
                    }
                    catch (Exception markEx)
                    {
                        _logger.LogError(markEx, "Error in MarkAsCompleted for incorrect answer");
                        throw;
                    }
                }
                
                _logger.LogInformation("CheckAnswerAndMarkCompleted completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and marking quiz answer for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets all completed quiz questions for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of completed quiz questions</returns>
        public async Task<List<CompletedQuizDto>> GetCompletedQuizzes(int userId)
        {
            try
            {
                var completedQuizzes = await _completedQuizRepo.GetByUserId(userId);
                return _mapper.Map<List<CompletedQuizDto>>(completedQuizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed quizzes for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets statistics about a user's quiz progress
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Quiz statistics</returns>
        public async Task<QuizStatsDto> GetQuizStatistics(int userId)
        {
            try
            {
                // Get total number of quiz questions
                int totalQuestions = await _quizQuestionRepo.CountQuizQuestions();
                
                // Get user's completed quizzes
                var completedQuizzes = await _completedQuizRepo.GetByUserId(userId);
                int completedCount = completedQuizzes.Count;
                int correctCount = completedQuizzes.Count(cq => cq.WasCorrect);
                
                return new QuizStatsDto
                {
                    TotalQuestions = totalQuestions,
                    CompletedQuestions = completedCount,
                    CorrectAnswers = correctCount,
                    CompletionPercentage = totalQuestions > 0 ? (double)completedCount / totalQuestions * 100 : 0,
                    CorrectPercentage = completedCount > 0 ? (double)correctCount / completedCount * 100 : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz statistics for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets the total number of quiz questions in the system
        /// </summary>
        /// <returns>Total number of questions</returns>
        public async Task<int> CountTotalQuestions()
        {
            try
            {
                return await _quizQuestionRepo.CountQuizQuestions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting total quiz questions");
                return 0;
            }
        }
    }
} 