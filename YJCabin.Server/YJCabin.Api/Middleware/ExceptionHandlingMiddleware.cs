using System.Net;
using System.Text.Json;
using YJCabin.Application.Common;

namespace YJCabin.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Code, ex.Message, ex.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteAsync(context, (int)HttpStatusCode.InternalServerError, "server_error", "An unexpected error occurred.", null);
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string code, string message, object? details)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new ApiError
        {
            Code = code,
            Message = message,
            Details = details
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await context.Response.WriteAsync(payload);
    }
}
