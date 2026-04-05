namespace VocabMaster.Api.Contracts.Common;

public sealed class OperationResultResponse
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;
}