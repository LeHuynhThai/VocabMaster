namespace VocabMaster.Api.Contracts.Vocabulary;

public sealed class AllLearnedVocabularyResponse
{
    public bool AllLearned { get; init; }

    public string Message { get; init; } = string.Empty;
}