using Da3wa.Application.Interfaces;
using Da3wa.Domain;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDocumentProcessingService _documentProcessingService;
        private readonly IAuthService _authService;

        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private readonly string[] _allowedTemplateExtensions = { ".pptx", ".docx", ".pdf" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly long _maxTemplateFileSize = 10 * 1024 * 1024; // 10MB

        public EventController(IEventService eventService, ICityService cityService, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment, IDocumentProcessingService documentProcessingService, IAuthService authService)
        {
            _eventService = eventService;
            _cityService = cityService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
            _documentProcessingService = documentProcessingService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var @event = await _eventService.GetByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event, IFormFile? invitationImage, IFormFile? templateFile)
        {
            if (ModelState.IsValid)
            {
                // Handle template file upload and processing
                if (templateFile != null && templateFile.Length > 0)
                {
                    if (!ValidateTemplateFile(templateFile, out string templateErrorMessage))
                    {
                        ModelState.AddModelError("templateFile", templateErrorMessage);
                    }
                    else if (string.IsNullOrEmpty(@event.Bridegroom) || string.IsNullOrEmpty(@event.Bride))
                    {
                        ModelState.AddModelError("templateFile", "Bridegroom and Bride names are required when uploading a template file.");
                    }
                    else
                    {
                        try
                        {
                            // Save template file temporarily
                            var tempFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "temp");
                            if (!Directory.Exists(tempFolder))
                            {
                                Directory.CreateDirectory(tempFolder);
                            }

                            var tempFileName = $"{Guid.NewGuid()}_{Path.GetFileName(templateFile.FileName)}";
                            var tempFilePath = Path.Combine(tempFolder, tempFileName);

                            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                            {
                                await templateFile.CopyToAsync(fileStream);
                            }

                            // Save template file permanently
                            var templateFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "templates");
                            if (!Directory.Exists(templateFolder))
                            {
                                Directory.CreateDirectory(templateFolder);
                            }

                            var templateUniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(templateFile.FileName)}";
                            var templatePermanentPath = Path.Combine(templateFolder, templateUniqueFileName);
                            System.IO.File.Copy(tempFilePath, templatePermanentPath, true);

                            @event.TemplateFilePath = $"~/uploads/templates/{templateUniqueFileName}";

                            // Process template and convert to PNG
                            var outputFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
                            if (!Directory.Exists(outputFolder))
                            {
                                Directory.CreateDirectory(outputFolder);
                            }

                            var processedImagePath = await _documentProcessingService.ProcessTemplateAndConvertToPngAsync(
                                tempFilePath,
                                @event.Bridegroom,
                                @event.Bride,
                                @event.Name ?? "event",
                                outputFolder);

                            // Clean up temp file
                            if (System.IO.File.Exists(tempFilePath))
                            {
                                System.IO.File.Delete(tempFilePath);
                            }

                            // Delete template file after successful conversion
                            if (System.IO.File.Exists(templatePermanentPath))
                            {
                                System.IO.File.Delete(templatePermanentPath);
                            }

                            // Set image path to processed image
                            var fileName = Path.GetFileName(processedImagePath);
                            @event.ImagePath = $"~/uploads/events/{fileName}";

                            // Clear template file path since we deleted it
                            @event.TemplateFilePath = null;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("templateFile", $"Error processing template file: {ex.Message}");
                        }
                    }
                }
                // Handle invitation image upload (if no template file)
                else if (invitationImage != null && invitationImage.Length > 0)
                {
                    if (!ValidateImage(invitationImage, out string errorMessage))
                    {
                        ModelState.AddModelError("invitationImage", errorMessage);
                    }
                    else
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(invitationImage.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await invitationImage.CopyToAsync(fileStream);
                        }

                        @event.ImagePath = $"~/uploads/events/{uniqueFileName}";
                    }
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        await _eventService.CreateAsync(@event);
                        TempData["SuccessMessage"] = "Event created successfully!";
                        return RedirectToAction("Dashboard", "Home");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "An error occurred while saving the event: " + ex.Message);
                    }
                }
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
        public async Task<IActionResult> Edit(int id, Event @event, IFormFile? invitationImage, IFormFile? templateFile)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Handle template file upload and processing
                if (templateFile != null && templateFile.Length > 0)
                {
                    if (!ValidateTemplateFile(templateFile, out string templateErrorMessage))
                    {
                        ModelState.AddModelError("templateFile", templateErrorMessage);
                    }
                    else if (string.IsNullOrEmpty(@event.Bridegroom) || string.IsNullOrEmpty(@event.Bride))
                    {
                        ModelState.AddModelError("templateFile", "Bridegroom and Bride names are required when uploading a template file.");
                    }
                    else
                    {
                        try
                        {
                            // Delete old files if exist
                            if (!string.IsNullOrEmpty(@event.ImagePath))
                            {
                                var relativePath = @event.ImagePath.StartsWith("~/") ? @event.ImagePath.Substring(2) : @event.ImagePath.TrimStart('/');
                                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            if (!string.IsNullOrEmpty(@event.TemplateFilePath))
                            {
                                var relativePath = @event.TemplateFilePath.StartsWith("~/") ? @event.TemplateFilePath.Substring(2) : @event.TemplateFilePath.TrimStart('/');
                                var oldTemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
                                if (System.IO.File.Exists(oldTemplatePath))
                                {
                                    System.IO.File.Delete(oldTemplatePath);
                                }
                            }

                            // Save template file temporarily
                            var tempFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "temp");
                            if (!Directory.Exists(tempFolder))
                            {
                                Directory.CreateDirectory(tempFolder);
                            }

                            var tempFileName = $"{Guid.NewGuid()}_{Path.GetFileName(templateFile.FileName)}";
                            var tempFilePath = Path.Combine(tempFolder, tempFileName);

                            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                            {
                                await templateFile.CopyToAsync(fileStream);
                            }

                            // Save template file permanently
                            var templateFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "templates");
                            if (!Directory.Exists(templateFolder))
                            {
                                Directory.CreateDirectory(templateFolder);
                            }

                            var templateUniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(templateFile.FileName)}";
                            var templatePermanentPath = Path.Combine(templateFolder, templateUniqueFileName);
                            System.IO.File.Copy(tempFilePath, templatePermanentPath, true);

                            @event.TemplateFilePath = $"~/uploads/templates/{templateUniqueFileName}";

                            // Process template and convert to PNG
                            var outputFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
                            if (!Directory.Exists(outputFolder))
                            {
                                Directory.CreateDirectory(outputFolder);
                            }

                            var processedImagePath = await _documentProcessingService.ProcessTemplateAndConvertToPngAsync(
                                tempFilePath,
                                @event.Bridegroom,
                                @event.Bride,
                                @event.Name ?? "event",
                                outputFolder);

                            // Clean up temp file
                            if (System.IO.File.Exists(tempFilePath))
                            {
                                System.IO.File.Delete(tempFilePath);
                            }

                            // Delete template file after successful conversion
                            if (System.IO.File.Exists(templatePermanentPath))
                            {
                                System.IO.File.Delete(templatePermanentPath);
                            }

                            // Set image path to processed image
                            var fileName = Path.GetFileName(processedImagePath);
                            @event.ImagePath = $"~/uploads/events/{fileName}";

                            // Clear template file path since we deleted it
                            @event.TemplateFilePath = null;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("templateFile", $"Error processing template file: {ex.Message}");
                        }
                    }
                }
                // Handle invitation image upload (if no template file)
                else if (invitationImage != null && invitationImage.Length > 0)
                {
                    if (!ValidateImage(invitationImage, out string errorMessage))
                    {
                        ModelState.AddModelError("invitationImage", errorMessage);
                    }
                    else
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(@event.ImagePath))
                        {
                            var relativePath = @event.ImagePath.StartsWith("~/") ? @event.ImagePath.Substring(2) : @event.ImagePath.TrimStart('/');
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(invitationImage.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await invitationImage.CopyToAsync(fileStream);
                        }

                        @event.ImagePath = $"~/uploads/events/{uniqueFileName}";
                    }
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        await _eventService.UpdateAsync(@event);
                        TempData["SuccessMessage"] = "Event updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "An error occurred while updating the event: " + ex.Message);
                    }
                }
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

        private bool ValidateImage(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedExtensions.Contains(extension))
            {
                errorMessage = "Invalid file type. Allowed types: " + string.Join(", ", _allowedExtensions);
                return false;
            }

            if (file.Length > _maxFileSize)
            {
                errorMessage = $"File size exceeds the limit of {_maxFileSize / (1024 * 1024)}MB.";
                return false;
            }

            return true;
        }

        private bool ValidateTemplateFile(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedTemplateExtensions.Contains(extension))
            {
                errorMessage = "Invalid file type. Allowed types: " + string.Join(", ", _allowedTemplateExtensions);
                return false;
            }

            if (file.Length > _maxTemplateFileSize)
            {
                errorMessage = $"File size exceeds the limit of {_maxTemplateFileSize / (1024 * 1024)}MB.";
                return false;
            }

            return true;
        }

        private async Task PopulateDropDowns(Event? @event = null)
        {
            var cities = await _cityService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            var clientUsers = await _authService.GetUsersByRoleAsync(AppRoles.Client);

            ViewData["Cities"] = new SelectList(cities, "Id", "CityName", @event?.CityId);
            ViewData["Categories"] = new SelectList(categories, "Id", "Name", @event?.CategoryId);
            ViewData["Users"] = new SelectList(clientUsers, "Id", "UserName", @event?.UserId);
        }
    }
}
