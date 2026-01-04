using System.Diagnostics;
using Da3wa.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Da3wa.Domain;

namespace Da3wa.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated && !User.IsInRole(AppRoles.Client))
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            if (User.IsInRole(AppRoles.Client))
            {
                return Forbid();
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            
            var model = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            };

            if (exceptionHandlerPathFeature != null)
            {
                model.ErrorMessage = exceptionHandlerPathFeature.Error.Message;
                // Only show stack trace in development or based on policy
                model.StackTrace = exceptionHandlerPathFeature.Error.StackTrace;
            }

            return View(model);
        }
    }
}
