using System.Text.Json;
using TaskFlow.Core;

namespace TaskFlow.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception)
        {
            // Only return JSON error for API requests (not Razor Pages)
            if (IsApiRequest(context))
            {
                await WriteErrorResponse(context, StatusCodes.Status500InternalServerError, "Ocorreu um erro interno.");
            }
            else
            {
                // Let Razor Pages handle their own exceptions
                throw;
            }
        }
    }

    private static bool IsApiRequest(HttpContext context)
    {
        // Check if the request is for an API endpoint (starts with /api/)
        // or if the Accept header prefers JSON
        var path = context.Request.Path.Value ?? string.Empty;
        var acceptsJson = context.Request.Headers.Accept.ToString().Contains("application/json");
        
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) || 
               (acceptsJson && !path.Contains(".cshtml"));
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(body);
    }
}
