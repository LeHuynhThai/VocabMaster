namespace VocabMaster.Api.Contracts.Quiz;

public sealed class QuizCompletionStatusResponse
{
    public bool Completed { get; init; }

    public bool AllCompleted { get; init; }

    public string Message { get; init; } = string.Empty;
}