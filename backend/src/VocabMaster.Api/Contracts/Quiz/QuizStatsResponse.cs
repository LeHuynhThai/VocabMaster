namespace VocabMaster.Api.Contracts.Quiz;

public sealed class QuizStatsResponse
{
    public int TotalQuestions { get; init; }

    public int CompletedQuestions { get; init; }

    public int CorrectAnswers { get; init; }

    public double AccuracyRate { get; init; }
}