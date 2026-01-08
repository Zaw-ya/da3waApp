using Da3wa.Application.DTOs;
using Da3wa.Application.Interfaces;
using Da3wa.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Da3wa.WebUI.Controllers
{
    [Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.Supervisor}")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICityService _cityService;

        public AuthController(IAuthService authService, ICityService cityService)
        {
            _authService = authService;
            _cityService = cityService;
        }

        // GET: Auth
        public async Task<IActionResult> Index()
        {
            var users = await _authService.GetAllUsersAsync();
            return View(users);
        }

        // GET: Auth/Create
        public async Task<IActionResult> Create()
        {
            await SetupViewBagForCreate(null);
            return View();
        }

        // POST: Auth/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                await SetupViewBagForCreate(createUserDto);
                return View(createUserDto);
            }

            var createdById = User.Identity?.Name;
            var userDto = await _authService.CreateUserAsync(createUserDto, createdById);
            TempData["Success"] = $"User '{userDto.Email}' created successfully.";
            return RedirectToAction("Dashboard","Home");
        }

        // GET: Auth/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            await SetupViewBagForEdit(user);

            var updateDto = new UpdateUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Address = user.Address,
                PrimaryContactNo = user.PrimaryContactNo,
                SecondaryContactNo = user.SecondaryContactNo,
                CityId = user.CityId,
                Role = user.Role,
                IsActive = user.IsActive,
                Approved = user.Approved
            };

            return View(updateDto);
        }

        // POST: Auth/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                await SetupViewBagForEdit(updateUserDto);
                return View(updateUserDto);
            }

            var userDto = await _authService.UpdateUserAsync(updateUserDto);
            TempData["Success"] = $"User '{userDto.Email}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Auth/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: Auth/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Administrator)]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _authService.DeleteUserAsync(id);
            if (result)
            {
                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user or user not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Auth/ChangePassword/5
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var changePasswordDto = new ChangePasswordDto
            {
                UserId = id
            };

            return View(changePasswordDto);
        }

        // POST: Auth/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return View(changePasswordDto);
            }

            var result = await _authService.ChangeUserPasswordAsync(changePasswordDto.UserId, changePasswordDto.NewPassword);
            if (result)
            {
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = "Failed to change password or user not found.";
                return View(changePasswordDto);
            }
        }

        // POST: Auth/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _authService.ToggleUserActiveStatusAsync(id);
            if (result)
            {
                TempData["Success"] = "User active status toggled successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to toggle user active status or user not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Auth/ToggleApproval/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApproval(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _authService.ToggleUserApprovalStatusAsync(id);
            if (result)
            {
                TempData["Success"] = "User approval status toggled successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to toggle user approval status or user not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Auth/GetUsersByRole
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction(nameof(Index));
            }

            var users = await _authService.GetUsersByRoleAsync(role);
            ViewBag.SelectedRole = role;
            return View("Index", users);
        }

        // Helper methods for ViewBag setup
        private async Task SetupViewBagForCreate(object? model)
        {
            var roles = await _authService.GetAllRolesAsync();
            var selectedRole = model is CreateUserDto dto ? dto.Role : roles.FirstOrDefault();
            ViewBag.Roles = new SelectList(roles, selectedRole);
            
            var cities = await _cityService.GetAllAsync();
            var selectedCityId = model is CreateUserDto dto2 ? (int?)dto2.CityId : null;
            ViewBag.Cities = new SelectList(cities, "Id", "CityName", selectedCityId);
        }

        private async Task SetupViewBagForEdit(object? model)
        {
            var roles = await _authService.GetAllRolesAsync();
            string? selectedRole = null;
            
            if (model is UpdateUserDto updateDto)
                selectedRole = updateDto.Role;
            else if (model is UserDto userDto)
                selectedRole = userDto.Role;
                
            ViewBag.Roles = new SelectList(roles, selectedRole);
            
            var cities = await _cityService.GetAllAsync();
            int? selectedCityId = null;
            
            if (model is UpdateUserDto updateDto2)
                selectedCityId = updateDto2.CityId;
            else if (model is UserDto userDto2)
                selectedCityId = userDto2.CityId;
                
            ViewBag.Cities = new SelectList(cities, "Id", "CityName", selectedCityId);
        }
    }
}
