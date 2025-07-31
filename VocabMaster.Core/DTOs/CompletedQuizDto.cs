namespace VocabMaster.Core.DTOs
{
    public class CompletedQuizDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool WasCorrect { get; set; }
    }

    public class MarkQuizCompletedDto
    {
        public int QuizQuestionId { get; set; }
        public bool WasCorrect { get; set; }
    }

    public class QuizCompletionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CompletedQuizDto Data { get; set; }
    }
}