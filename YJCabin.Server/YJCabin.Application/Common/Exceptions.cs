namespace YJCabin.Application.Common;

public class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public object? Details { get; }

    public AppException(string code, string message, int statusCode, object? details = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Details = details;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, string key)
        : base("not_found", $"{resource} '{key}' was not found.", 404)
    {
    }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base("conflict", message, 409)
    {
    }
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Invalid credentials.")
        : base("unauthorized", message, 401)
    {
    }
}

public sealed class ValidationException : AppException
{
    public ValidationException(string field, string message)
        : base("validation_error", message, 400, new { field })
    {
    }
}
