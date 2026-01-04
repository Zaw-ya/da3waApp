using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Da3wa.WebUI.Filters
{
    /// <summary>
    /// Action filter that handles exceptions and sets TempData messages automatically.
    /// Use this attribute on controller actions to remove try-catch blocks.
    /// Note: This is optional since ExceptionHandlingFilter is registered globally.
    /// </summary>
    public class HandleExceptionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<HandleExceptionAttribute>>();

                var exception = context.Exception;
                var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
                var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";

                // Log the exception
                logger.LogError(exception,
                    "Exception in {Controller}.{Action}: {Message}",
                    controllerName,
                    actionName,
                    exception.Message);

                // Handle exception based on type
                string errorMessage;
                bool shouldRedirect = true;

                switch (exception)
                {
                    case KeyNotFoundException keyNotFound:
                        errorMessage = keyNotFound.Message;
                        context.HttpContext.Response.StatusCode = 404;
                        break;

                    case InvalidOperationException invalidOp:
                        errorMessage = invalidOp.Message;
                        context.HttpContext.Response.StatusCode = 400;
                        // For POST requests, don't redirect - return the view instead
                        if (context.HttpContext.Request.Method == "POST")
                        {
                            shouldRedirect = false;
                            context.ExceptionHandled = true;
                            
                            if (context.Controller is Controller controller)
                            {
                                controller.TempData["Error"] = errorMessage;
                                context.Result = new ViewResult
                                {
                                    ViewName = actionName,
                                    ViewData = controller.ViewData,
                                    TempData = controller.TempData
                                };
                            }
                            else
                            {
                                // Fallback if not a Controller
                                context.Result = new RedirectToActionResult("Index", controllerName, null);
                            }
                            return;
                        }
                        break;

                    case UnauthorizedAccessException:
                        errorMessage = "You do not have permission to perform this action.";
                        context.HttpContext.Response.StatusCode = 403;
                        break;

                    default:
                        errorMessage = "An unexpected error occurred. Please try again later.";
                        context.HttpContext.Response.StatusCode = 500;
                        break;
                }

                // Set error message in TempData
                if (context.Controller is Controller ctrl)
                {
                    ctrl.TempData["Error"] = errorMessage;
                }

                // Mark exception as handled
                context.ExceptionHandled = true;

                // Redirect if needed
                if (shouldRedirect)
                {
                    var redirectController = context.RouteData.Values["controller"]?.ToString();
                    if (!string.IsNullOrEmpty(redirectController))
                    {
                        context.Result = new RedirectToActionResult("Index", redirectController, null);
                    }
                    else
                    {
                        context.Result = new RedirectToActionResult("Index", "Home", null);
                    }
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
