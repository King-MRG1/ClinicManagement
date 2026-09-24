using System.Net;
using System.Text.Json;
using FluentValidation;

namespace ClinicManagement.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException valEx => (
                (int)HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                valEx.Errors.Select(e => e.ErrorMessage).ToArray()
            ),
            ArgumentException argEx => (
                (int)HttpStatusCode.BadRequest,
                argEx.Message,
                Array.Empty<string>()
            ),
            KeyNotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message,
                Array.Empty<string>()
            ),
            InvalidOperationException conflictEx => (
                (int)HttpStatusCode.Conflict,
                conflictEx.Message,
                Array.Empty<string>()
            ),
            UnauthorizedAccessException unauthEx => (
                unauthEx.Message.StartsWith("Forbidden", StringComparison.OrdinalIgnoreCase)
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized,
                unauthEx.Message,
                Array.Empty<string>()
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.",
                Array.Empty<string>()
            )
        };

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
