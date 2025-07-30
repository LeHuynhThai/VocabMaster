namespace VocabMaster.Core.DTOs
{
    /// <summary>
    /// DTO for quiz questions
    /// </summary>
    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string CorrectAnswer { get; set; }
        public string WrongAnswer1 { get; set; }
        public string WrongAnswer2 { get; set; }
        public string WrongAnswer3 { get; set; }
    }

    /// <summary>
    /// DTO for quiz question responses from clients
    /// </summary>
    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }

    /// <summary>
    /// DTO for quiz answer results
    /// </summary>
    public class QuizResultDto
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; }
        public string Message { get; set; }
    }
} 