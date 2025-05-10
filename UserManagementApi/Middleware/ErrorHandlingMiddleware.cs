// filepath: c:\Users\caesar.hinlo\Documents\selfStudy\UserManagementApiBackend\UserManagementApiBackend\UserManagementApi\Middleware\ErrorHandlingMiddleware.cs

using System.Net;
using System.Text.Json;

namespace UserManagementApi.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception has occurred while executing the request.");

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Internal server error.";

        // You can add more specific exception handling here
        if (exception is InvalidOperationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = exception.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = HttpStatusCode.Unauthorized;
            message = exception.Message;
        }
        else if (exception is KeyNotFoundException)
        {
            statusCode = HttpStatusCode.NotFound;
            message = "Resource not found.";
        }

        var response = JsonSerializer.Serialize(new { error = message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(response);
    }
}