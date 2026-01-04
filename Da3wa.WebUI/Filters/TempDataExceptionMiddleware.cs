using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Da3wa.WebUI.Filters
{
    /// <summary>
    /// Middleware that transfers error messages from HttpContext.Items to TempData
    /// after exception handling filter has set them.
    /// </summary>
    public class TempDataExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public TempDataExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // After the action has executed, check if there's an error message in Items
            if (context.Items.TryGetValue("ExceptionErrorMessage", out var errorMessage) && 
                errorMessage is string message)
            {
                // Try to get the controller and set TempData
                var controller = context.Items["__Controller"] as Controller;
                if (controller != null)
                {
                    controller.TempData["Error"] = message;
                }
            }
        }
    }
}

