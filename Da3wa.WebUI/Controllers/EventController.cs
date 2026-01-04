using Da3wa.Application.Interfaces;
using Da3wa.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Da3wa.WebUI.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ICityService _cityService;
        private readonly ICategoryService _categoryService;

        public EventController(IEventService eventService, ICityService cityService, ICategoryService categoryService)
        {
            _eventService = eventService;
            _cityService = cityService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllAsync();
            return View(events);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            if (ModelState.IsValid)
            {
                await _eventService.CreateAsync(@event);
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropDowns(@event);
            return View(@event);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var @event = await _eventService.GetByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            await PopulateDropDowns(@event);
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _eventService.UpdateAsync(@event);
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropDowns(@event);
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _eventService.GetByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            await _eventService.ToggleDeleteAsync(id);
            TempData["SuccessMessage"] = @event.IsDeleted ? "Event restored successfully!" : "Event deleted successfully!";
            
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDowns(Event? @event = null)
        {
            var cities = await _cityService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            
            ViewData["Cities"] = new SelectList(cities, "Id", "CityName", @event?.CityId);
            ViewData["Categories"] = new SelectList(categories, "Id", "Name", @event?.CategoryId);
        }
    }
}
