namespace VocabMaster.Api.Contracts.Vocabulary;

public sealed class LearnedWordResponse
{
    public int Id { get; init; }

    public string Word { get; init; } = string.Empty;

    public DateTime LearnedAt { get; init; }
}