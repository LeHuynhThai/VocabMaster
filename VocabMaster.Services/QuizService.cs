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
        private readonly IMapper _mapper;
        private readonly ILogger<QuizService> _logger;
        private readonly Random _random = new Random();

        public QuizService(
            IQuizQuestionRepo quizQuestionRepo,
            IVocabularyRepo vocabularyRepo,
            IMapper mapper,
            ILogger<QuizService> logger)
        {
            _quizQuestionRepo = quizQuestionRepo;
            _vocabularyRepo = vocabularyRepo;
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
    }
} 