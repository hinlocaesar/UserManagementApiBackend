
using System.Net;
using System.Text.Json;

namespace UserManagementApi.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Public endpoints that don't require authentication
        if (IsPublicEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Check for the authentication token in the request headers
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            _logger.LogWarning("Authentication failed: No Authorization header present");
            await SendUnauthorizedResponseAsync(context, "Authentication token is required");
            return;
        }

        string token = authHeader.ToString();

        // If the token is in the format "Bearer {token}", extract the token part
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(7).Trim();
        }

        // Validate the token
        if (!IsValidToken(token))
        {
            _logger.LogWarning("Authentication failed: Invalid token");
            await SendUnauthorizedResponseAsync(context, "Invalid authentication token");
            return;
        }

        // Token is valid, continue with the request
        await _next(context);
    }

    private bool IsPublicEndpoint(PathString path)
    {
        // List of endpoints that don't require authentication
        var publicEndpoints = new[]
        {
            "/",                // Root endpoint
            "/swagger",         // Swagger endpoints
            "/api/auth/login",  // Login endpoint (if you have one)
        };

        return publicEndpoints.Any(endpoint => 
            path.StartsWithSegments(endpoint, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsValidToken(string token)
    {
        // TODO: Implement actual token validation logic here
        // This could involve JWT validation, checking against a database, etc.
        
        // For demonstration purposes, we'll use a simple check
        // In a real application, use a proper authentication mechanism
        return !string.IsNullOrEmpty(token) && token != "invalid";
    }

    private async Task SendUnauthorizedResponseAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401 Unauthorized
        
        var response = new 
        {
            status = 401,
            message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
