namespace Repository.DTOs
{
    public class CompletedQuizDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public bool WasCorrect { get; set; }
    }
}