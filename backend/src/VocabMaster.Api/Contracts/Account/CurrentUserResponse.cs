namespace VocabMaster.Api.Contracts.Account;

public sealed class CurrentUserResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int LearnedWordsCount { get; init; }
}