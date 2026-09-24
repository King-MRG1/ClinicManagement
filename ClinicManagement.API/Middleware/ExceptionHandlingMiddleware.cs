using System.Net;
using System.Text.Json;
using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Common.Models;

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

        var response = exception switch
        {
            ValidationException valEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = new ApiResponse<object>
                {
                    Success = false,
                    Message = valEx.Message,
                    Errors = valEx.Errors.SelectMany(kvp => kvp.Value).ToList()
                }
            },
            UnauthorizedException unauthEx => new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Body = ApiResponse<object>.Failed(unauthEx.Message)
            },
            ForbiddenException forbidEx => new
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Body = ApiResponse<object>.Failed(forbidEx.Message)
            },
            NotFoundException notFoundEx => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = ApiResponse<object>.Failed(notFoundEx.Message)
            },
            ConflictException conflictEx => new
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Body = ApiResponse<object>.Failed(conflictEx.Message)
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = ApiResponse<object>.Failed("An unexpected error occurred. Please try again later.")
            }
        };

        if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled application exception: {Message}", exception.Message);
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Body, jsonOptions));
    }
}
