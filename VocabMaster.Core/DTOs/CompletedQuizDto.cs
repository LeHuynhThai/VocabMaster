namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// DTO for completed quiz questions
    /// </summary>
    public class CompletedQuizDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool WasCorrect { get; set; }
    }

    /// <summary>
    /// DTO for marking a quiz question as completed
    /// </summary>
    public class MarkQuizCompletedDto
    {
        public int QuizQuestionId { get; set; }
        public bool WasCorrect { get; set; }
    }

    /// <summary>
    /// Response DTO for quiz completion operation
    /// </summary>
    public class QuizCompletionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CompletedQuizDto Data { get; set; }
    }
}