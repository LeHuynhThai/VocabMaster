namespace VocabMaster.Api.Contracts.Quiz;

public sealed class SubmitQuizAnswerResponse
{
    public bool IsCorrect { get; init; }

    public string Message { get; init; } = string.Empty;
}