namespace YJCabin.Application.Common;

public sealed class ApiError
{
    public string Code { get; init; } = "error";
    public string Message { get; init; } = string.Empty;
    public object? Details { get; init; }
}
