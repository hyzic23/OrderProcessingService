using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace OrderProcessing.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";

            var response = new ProblemDetails
            {
                Instance = context.Request.Path
            };

            // Handle known business exceptions
            if (ex is AppExceptionMiddleware appEx)
            {
                context.Response.StatusCode = (int)appEx.StatusCode;
                response.Status = context.Response.StatusCode;
                response.Title = appEx.Message;
                response.Detail = appEx.Message;
            }
            // Handle unknown exceptions
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Status = context.Response.StatusCode;
                response.Title = "An internal server error occurred.";
                response.Detail = _env.IsDevelopment() ? ex.StackTrace?.ToString() : "An unexpected error occurred.";
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}