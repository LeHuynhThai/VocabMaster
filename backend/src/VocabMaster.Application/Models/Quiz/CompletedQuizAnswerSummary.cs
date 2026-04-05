namespace VocabMaster.Application.Models.Quiz;

public sealed class CompletedQuizAnswerSummary
{
    public int Id { get; init; }

    public int QuizQuestionId { get; init; }

    public string Word { get; init; } = string.Empty;

    public string CorrectAnswer { get; init; } = string.Empty;

    public DateTime CompletedAt { get; init; }

    public bool WasCorrect { get; init; }
}