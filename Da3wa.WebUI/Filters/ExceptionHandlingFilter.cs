using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Da3wa.WebUI.Filters
{
    /// <summary>
    /// Global exception filter that handles all exceptions in controller actions.
    /// Automatically logs exceptions, sets TempData error messages, and redirects appropriately.
    /// </summary>
    public class ExceptionHandlingFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<ExceptionHandlingFilter> _logger;

        public ExceptionHandlingFilter(ILogger<ExceptionHandlingFilter> logger)
        {
            _logger = logger;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;
            var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";

            // Log the exception
            _logger.LogError(exception,
                "Exception in {Controller}.{Action}: {Message}",
                controllerName,
                actionName,
                exception.Message);

            // Mark exception as handled
            context.ExceptionHandled = true;

            // Store error message in HttpContext.Items for later retrieval
            var errorMessage = exception switch
            {
                KeyNotFoundException notFound => notFound.Message,
                InvalidOperationException invalidOp => invalidOp.Message,
                UnauthorizedAccessException => "You do not have permission to perform this action.",
                _ => "An unexpected error occurred. Please try again later."
            };

            context.HttpContext.Items["ExceptionErrorMessage"] = errorMessage;

            // Handle based on exception type
            context.Result = exception switch
            {
                KeyNotFoundException notFound => HandleNotFound(context, notFound),
                InvalidOperationException invalidOp => HandleInvalidOperation(context, invalidOp),
                UnauthorizedAccessException => HandleUnauthorized(context),
                _ => HandleGeneric(context, exception)
            };

            return Task.CompletedTask;
        }

        private IActionResult HandleNotFound(ExceptionContext context, KeyNotFoundException exception)
        {
            if (IsAjaxRequest(context))
            {
                return new JsonResult(new { error = exception.Message }) { StatusCode = 404 };
            }

            return RedirectToIndex(context);
        }

        private IActionResult HandleInvalidOperation(ExceptionContext context, InvalidOperationException exception)
        {
            if (IsAjaxRequest(context))
            {
                return new JsonResult(new { error = exception.Message }) { StatusCode = 400 };
            }

            // For POST requests, redirect back to the same action (which will show the error)
            if (context.HttpContext.Request.Method == "POST")
            {
                var controllerName = context.RouteData.Values["controller"]?.ToString();
                var actionName = context.RouteData.Values["action"]?.ToString();
                
                if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName))
                {
                    return new RedirectToActionResult(actionName, controllerName, null);
                }
            }

            return RedirectToIndex(context);
        }

        private IActionResult HandleUnauthorized(ExceptionContext context)
        {
            if (IsAjaxRequest(context))
            {
                return new JsonResult(new { error = "You do not have permission to perform this action." }) { StatusCode = 403 };
            }

            return new RedirectToActionResult("Index", "Home", null);
        }

        private IActionResult HandleGeneric(ExceptionContext context, Exception exception)
        {
            if (IsAjaxRequest(context))
            {
                return new JsonResult(new { error = "An unexpected error occurred. Please try again later." }) { StatusCode = 500 };
            }

            var controllerName = context.RouteData.Values["controller"]?.ToString();
            if (!string.IsNullOrEmpty(controllerName))
            {
                return RedirectToIndex(context);
            }

            return new RedirectToActionResult("Error", "Home", null);
        }

        private static IActionResult RedirectToIndex(ExceptionContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            if (!string.IsNullOrEmpty(controllerName))
            {
                return new RedirectToActionResult("Index", controllerName, null);
            }

            return new RedirectToActionResult("Index", "Home", null);
        }

        private static bool IsAjaxRequest(ExceptionContext context)
        {
            var request = context.HttpContext.Request;
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
