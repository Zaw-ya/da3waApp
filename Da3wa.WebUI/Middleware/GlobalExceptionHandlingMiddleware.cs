using System.Net;
using System.Text.Json;
using Da3wa.WebUI.Models;

namespace Da3wa.WebUI.Middleware
{
    /// <summary>
    /// Global exception handling middleware for unhandled exceptions.
    /// This catches exceptions that weren't handled by the ExceptionHandlingFilter.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
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
                _logger.LogError(ex, "An exception occurred. Request Path: {Path}", context.Request.Path);

                if (IsApiRequest(context))
                {
                    await HandleExceptionAsync(context, ex);
                }
                else
                {
                    // Rethrow for UI requests so UseExceptionHandler or UseDeveloperExceptionPage
                    // can handle them correctly and populate IExceptionHandlerPathFeature.
                    throw;
                }
            }
        }

        private bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api") ||
                   context.Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Only handle if response hasn't started
            if (context.Response.HasStarted) return;

            // Return JSON for API requests
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ErrorViewModel
            {
                RequestId = context.TraceIdentifier,
            };

            // Add exception details in development
            if (_environment.IsDevelopment())
            {
                response.ErrorMessage = exception.Message;
                response.StackTrace = exception.StackTrace;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}

