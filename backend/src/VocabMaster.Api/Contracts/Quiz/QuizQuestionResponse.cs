namespace VocabMaster.Api.Contracts.Quiz;

public sealed class QuizQuestionResponse
{
    public int Id { get; init; }

    public string Word { get; init; } = string.Empty;

    public string CorrectAnswer { get; init; } = string.Empty;

    public string WrongAnswer1 { get; init; } = string.Empty;

    public string WrongAnswer2 { get; init; } = string.Empty;

    public string WrongAnswer3 { get; init; } = string.Empty;
}