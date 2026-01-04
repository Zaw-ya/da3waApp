using Da3wa.Application.Interfaces;
using Da3wa.Domain;
using Da3wa.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Da3wa.WebUI.Controllers
{
    [Authorize]
    public class CityController : Controller
    {
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;

        public CityController(ICityService cityService, ICountryService countryService)
        {
            _cityService = cityService;
            _countryService = countryService;
        }

        public async Task<IActionResult> Index()
        {
            var cities = await _cityService.GetAllAsync();
            return View(cities);
        }

        public async Task<IActionResult> Details(int id)
        {
            var city = await _cityService.GetByIdAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            return View(city);
        }

        public async Task<IActionResult> Create()
        {
            var countries = await _countryService.GetAllAsync();
            ViewData["Countries"] = new SelectList(countries, "Id", "CountryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(City city)
        {
            if (ModelState.IsValid)
            {
                await _cityService.CreateAsync(city);
                TempData["SuccessMessage"] = "City created successfully!";
                return RedirectToAction(nameof(Index));
            }
            var countries = await _countryService.GetAllAsync();
            ViewData["Countries"] = new SelectList(countries, "Id", "CountryName");
            return View(city);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var city = await _cityService.GetByIdAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            var countries = await _countryService.GetAllAsync();
            ViewData["Countries"] = new SelectList(countries, "Id", "CountryName", city.CountryId);
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, City city)
        {
            if (id != city.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _cityService.UpdateAsync(city);
                TempData["SuccessMessage"] = "City updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            var countries = await _countryService.GetAllAsync();
            ViewData["Countries"] = new SelectList(countries, "Id", "CountryName", city.CountryId);
            return View(city);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var city = await _cityService.GetByIdAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var City = await _cityService.GetByIdAsync(id);
            if (City == null)
                return NotFound();

            if (City.IsDeleted)
            {
                await _countryService.ToggleDeleteAsync(id);
                TempData["SuccessMessage"] = "City restored successfully!";
            }
            else
            {
                await _countryService.ToggleDeleteAsync(id);
                TempData["SuccessMessage"] = "City deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
