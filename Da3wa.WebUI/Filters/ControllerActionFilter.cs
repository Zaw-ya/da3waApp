using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Da3wa.WebUI.Filters
{
    /// <summary>
    /// Action filter that stores the controller instance in HttpContext.Items
    /// so it can be accessed by exception filters.
    /// </summary>
    public class ControllerActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Store controller instance in HttpContext.Items for exception filter access
            if (context.Controller is Controller controller)
            {
                context.HttpContext.Items["__Controller"] = controller;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Transfer error message from Items to TempData if it exists
            if (context.HttpContext.Items.TryGetValue("ExceptionErrorMessage", out var errorMessage) &&
                errorMessage is string message &&
                context.Controller is Controller controller)
            {
                controller.TempData["Error"] = message;
            }
        }
    }
}

