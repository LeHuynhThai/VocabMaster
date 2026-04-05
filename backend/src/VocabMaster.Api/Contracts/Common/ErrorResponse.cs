namespace VocabMaster.Api.Contracts.Common;

public sealed class ErrorResponse
{
    public string Error { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}