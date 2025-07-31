namespace VocabMaster.Core.DTOs
{
    public class QuizStatsDto
    {
        public int TotalQuestions { get; set; }
        public int CompletedQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double CompletionPercentage { get; set; }
        public double CorrectPercentage { get; set; }
    }
} 