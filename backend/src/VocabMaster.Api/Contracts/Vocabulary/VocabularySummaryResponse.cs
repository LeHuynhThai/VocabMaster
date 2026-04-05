namespace VocabMaster.Api.Contracts.Vocabulary;

public sealed class VocabularySummaryResponse
{
    public int Id { get; init; }

    public string Word { get; init; } = string.Empty;

    public string? Vietnamese { get; init; }
}