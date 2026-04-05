namespace VocabMaster.Api.Contracts.Common;

public sealed class DataResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T Data { get; init; } = default!;
}