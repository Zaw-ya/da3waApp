using Da3wa.Application.Interfaces;
using Da3wa.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Da3wa.WebUI.Controllers
{
    [Authorize]
    public class GuestController : Controller
    {
        private readonly IGuestService _guestService;
        private readonly IEventService _eventService;

        public GuestController(IGuestService guestService, IEventService eventService)
        {
            _guestService = guestService;
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var guests = await _guestService.GetAllAsync();
            return View(guests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
            {
                return NotFound();
            }
            return View(guest);
        }

        public async Task<IActionResult> Create()
        {
            var events = await _eventService.GetAllAsync();
            if (events != null && events.Any())
            {
                ViewData["Events"] = new SelectList(events, "Id", "Name");
            }
            else
            {
                ViewData["Events"] = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            return View(new Guest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guest guest, List<string> phoneNumbers)
        {
            if (guest == null)
            {
                guest = new Guest();
            }

            if (phoneNumbers != null && phoneNumbers.Any())
            {
                guest.Tel = phoneNumbers.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            }

            // Set ExpireAt to a default value (e.g., 30 days from now)
            guest.ExpireAt = DateTime.Now.AddDays(30);

            if (ModelState.IsValid)
            {
                await _guestService.CreateAsync(guest);
                TempData["SuccessMessage"] = "Guest created successfully!";
                return RedirectToAction(nameof(Index));
            }

            var events = await _eventService.GetAllAsync();
            if (events != null && events.Any())
            {
                ViewData["Events"] = new SelectList(events, "Id", "Name");
            }
            else
            {
                ViewData["Events"] = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            return View(guest);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
            {
                return NotFound();
            }
            var events = await _eventService.GetAllAsync();


            if (events != null && events.Any())
            {
                ViewData["Events"] = new SelectList(events, "Id", "Name", guest.EventId);
            }
            else
            {
                ViewData["Events"] = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            return View(guest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Guest guest, List<string> phoneNumbers)
        {
            if (id != guest.Id)
            {
                return NotFound();
            }

            if (phoneNumbers != null && phoneNumbers.Any())
            {
                guest.Tel = phoneNumbers.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            }

            if (ModelState.IsValid)
            {
                await _guestService.UpdateAsync(guest);
                TempData["SuccessMessage"] = "Guest updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            var events = await _eventService.GetAllAsync();
            ViewData["Events"] = new SelectList(events, "Id", "Name", guest.EventId);
            return View(guest);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
            {
                return NotFound();
            }
            return View(guest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest == null)
                return NotFound();

            if (guest.IsDeleted)
            {
                await _guestService.ToggleDeleteAsync(id);
                TempData["SuccessMessage"] = "Guest restored successfully!";
            }
            else
            {
                await _guestService.ToggleDeleteAsync(id);
                TempData["SuccessMessage"] = "Guest deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ImportVcf()
        {
            var events = await _eventService.GetAllAsync();
            if (events != null && events.Any())
            {
                ViewData["Events"] = new SelectList(events, "Id", "Name");
            }
            else
            {
                ViewData["Events"] = new SelectList(Enumerable.Empty<SelectListItem>());
                TempData["WarningMessage"] = "No events found. Please create an event first before importing guests.";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportVcf(IFormFile vcfFile, int eventId)
        {
            if (vcfFile == null || vcfFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid VCF file.";
                return RedirectToAction(nameof(ImportVcf));
            }

            if (eventId == 0)
            {
                TempData["ErrorMessage"] = "Please select an event.";
                return RedirectToAction(nameof(ImportVcf));
            }

            try
            {
                using var stream = vcfFile.OpenReadStream();
                var parsedGuests = await _guestService.ParseVcfAsync(stream, eventId);

                if (parsedGuests == null || !parsedGuests.Any())
                {
                    TempData["ErrorMessage"] = "No valid contacts found in the VCF file.";
                    return RedirectToAction(nameof(ImportVcf));
                }

                // Store guests in Session for preview
                var guestsJson = System.Text.Json.JsonSerializer.Serialize(parsedGuests);
                HttpContext.Session.SetString("ParsedGuests", guestsJson);
                HttpContext.Session.SetInt32("EventId", eventId);

                return RedirectToAction(nameof(PreviewImport));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing VCF file: {ex.Message}";
                return RedirectToAction(nameof(ImportVcf));
            }
        }

        public IActionResult PreviewImport()
        {
            var guestsJson = HttpContext.Session.GetString("ParsedGuests");

            if (string.IsNullOrEmpty(guestsJson))
            {
                TempData["ErrorMessage"] = "No data to preview. Please upload a VCF file first.";
                return RedirectToAction(nameof(ImportVcf));
            }

            var guests = System.Text.Json.JsonSerializer.Deserialize<List<Guest>>(guestsJson);
            return View(guests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(string selectedGuestsJson)
        {
            if (string.IsNullOrEmpty(selectedGuestsJson))
            {
                TempData["ErrorMessage"] = "No guests selected for import.";
                return RedirectToAction(nameof(ImportVcf));
            }

            var selectedGuests = System.Text.Json.JsonSerializer.Deserialize<List<Guest>>(selectedGuestsJson);

            if (selectedGuests == null || !selectedGuests.Any())
            {
                TempData["ErrorMessage"] = "No guests selected for import.";
                return RedirectToAction(nameof(ImportVcf));
            }

            var importedCount = await _guestService.CreateManyAsync(selectedGuests);

            // Clear session data after successful import
            HttpContext.Session.Remove("ParsedGuests");
            HttpContext.Session.Remove("EventId");

            TempData["SuccessMessage"] = $"Successfully imported {importedCount} guest(s) to the database.";
            return RedirectToAction(nameof(Index));
        }
    }
}
