namespace VocabMaster.Application.Models.Quiz;

public sealed class QuizStatsSummary
{
    public int TotalQuestions { get; init; }

    public int CompletedQuestions { get; init; }

    public int CorrectAnswers { get; init; }

    public double AccuracyRate { get; init; }
}