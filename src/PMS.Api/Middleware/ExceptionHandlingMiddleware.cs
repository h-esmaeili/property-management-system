using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace PMS.Api.Middleware;

/// <summary>
/// Maps known exceptions to HTTP status codes and RFC 7807 <see cref="ProblemDetails"/> responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Unhandled exception after response started; rethrowing.");
                throw;
            }

            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title, detail, clientError) = MapException(ex);

        if (clientError)
            _logger.LogWarning(ex, "Request failed with {StatusCode}: {Message}", statusCode, ex.Message);
        else
            _logger.LogError(ex, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path.Value
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, JsonOptions);
    }

    private (int StatusCode, string Title, string? Detail, bool ClientError) MapException(Exception ex)
    {
        var dev = _environment.IsDevelopment();

        return ex switch
        {
            UnauthorizedAccessException e => (StatusCodes.Status401Unauthorized, "Unauthorized", e.Message, true),
            InvalidOperationException e => (StatusCodes.Status400BadRequest, "Bad Request", e.Message, true),
            ArgumentOutOfRangeException e => (StatusCodes.Status400BadRequest, "Bad Request", e.Message, true),
            ArgumentException e => (StatusCodes.Status400BadRequest, "Bad Request", e.Message, true),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server Error",
                dev
                    ? ex.Message
                    : "An unexpected error occurred. Include the trace id when contacting support.",
                false)
        };
    }
}
