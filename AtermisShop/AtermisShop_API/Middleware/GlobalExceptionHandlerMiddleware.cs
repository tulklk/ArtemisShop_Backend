using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AtermisShop_API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Handle database connection errors
        string errorMessage = exception.Message;
        if (exception is PostgresException pgEx && (pgEx.SqlState == "XX000" || exception.Message.Contains("Circuit breaker")))
        {
            errorMessage = "Database connection temporarily unavailable. Please try again in a few moments.";
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
        }
        else if (exception is DbUpdateException dbEx && dbEx.InnerException is PostgresException innerPgEx && 
                 (innerPgEx.SqlState == "XX000" || innerPgEx.Message.Contains("Circuit breaker")))
        {
            errorMessage = "Database connection temporarily unavailable. Please try again in a few moments.";
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
        }
        else
        {
            context.Response.StatusCode = exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        // Return consistent error format that matches controller responses
        var response = new
        {
            message = errorMessage,
            statusCode = context.Response.StatusCode
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

