using Da3wa.Application.Interfaces;
using Da3wa.Domain;
using Da3wa.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Da3wa.WebUI.Controllers
{
    [Authorize]
    public class CountryController : Controller
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        public async Task<IActionResult> Index()
        {
            var countries = await _countryService.GetAllAsync();
            return View(countries);
        }

        public async Task<IActionResult> Details(int id)
        {
            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Country country)
        {
            if (ModelState.IsValid)
            {
                await _countryService.CreateAsync(country);
                TempData["SuccessMessage"] = "Country created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        public async Task<IActionResult> Edit(int id)
        {

            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Country country)
        {
            if (id != country.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _countryService.UpdateAsync(country);
                TempData["SuccessMessage"] = "Country updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            if (country.IsDeleted)
            {
                // Restore the country
                await _countryService.ToggleDeleteAsync(id);
                TempData["SuccessMessage"] = "Country restored successfully!";
            }
            else
            {
                // Soft delete the country
                await _countryService.ToggleDeleteAsync(id);
                TempData["SuccessMessage"] = "Country deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
