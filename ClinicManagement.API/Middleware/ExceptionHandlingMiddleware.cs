using System.Net;
using System.Text.Json;
using ClinicManagement.Application.Common.Exceptions;

namespace ClinicManagement.API.Middleware;

public class ExceptionHandlingMiddleware
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException valEx => (
                (int)HttpStatusCode.BadRequest,
                valEx.Message,
                valEx.Errors.SelectMany(kvp => kvp.Value).ToArray()
            ),
            UnauthorizedException unauthEx => (
                (int)HttpStatusCode.Unauthorized,
                unauthEx.Message,
                Array.Empty<string>()
            ),
            ForbiddenException forbidEx => (
                (int)HttpStatusCode.Forbidden,
                forbidEx.Message,
                Array.Empty<string>()
            ),
            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message,
                Array.Empty<string>()
            ),
            ConflictException conflictEx => (
                (int)HttpStatusCode.Conflict,
                conflictEx.Message,
                Array.Empty<string>()
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.",
                Array.Empty<string>()
            )
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled application exception: {Message}", exception.Message);
        }

        context.Response.StatusCode = statusCode;

        var body = new
        {
            message,
            errors
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body, jsonOptions));
    }
}
