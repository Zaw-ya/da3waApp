using System.Diagnostics;
using Da3wa.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Da3wa.Domain;
using Da3wa.Application.Interfaces;

namespace Da3wa.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IGuestService _guestService;

        public HomeController(IEventService eventService, IGuestService guestService)
        {
            _eventService = eventService;
            _guestService = guestService;
        }

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
        public async Task<IActionResult> Dashboard()
        {
            if (User.IsInRole(AppRoles.Client))
            {
                return Forbid();
            }

            var model = new DashboardViewModel();

            // Get all events and guests
            var allEvents = (await _eventService.GetAllAsync()).Where(e => !e.IsDeleted).ToList();
            var allGuests = (await _guestService.GetAllAsync()).Where(g => !g.IsDeleted).ToList();

            // Calculate statistics
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            var startOfToday = now.Date;

            model.TotalEvents = allEvents.Count;
            model.EventsThisMonth = allEvents.Count(e => e.CreatedOn >= startOfMonth);

            model.TotalGuests = allGuests.Count;
            model.GuestsThisWeek = allGuests.Count(g => g.CreatedOn >= startOfWeek);

            model.PendingRSVPs = allGuests.Count(g => !g.IsAttending);

            model.TotalAttendance = allGuests.Count(g => g.IsAttending);
            model.AttendanceToday = allGuests.Count(g => g.IsAttending && g.LastUpdatedOn >= startOfToday);

            return View(model);
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
