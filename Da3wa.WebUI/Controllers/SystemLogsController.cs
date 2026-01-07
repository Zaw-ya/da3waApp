using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Da3wa.WebUI.Models;
using Da3wa.Domain;

namespace Da3wa.WebUI.Controllers
{
    [Authorize]
    public class SystemLogsController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SystemLogsController> _logger;

        public SystemLogsController(IWebHostEnvironment environment, ILogger<SystemLogsController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.IsInRole(AppRoles.Client))
            {
                return Forbid();
            }

            var logsPath = Path.Combine(_environment.ContentRootPath, "logs");
            var model = new SystemLogsViewModel
            {
                LogFiles = new List<LogFileInfo>()
            };

            if (Directory.Exists(logsPath))
            {
                var logFiles = Directory.GetFiles(logsPath, "*.log")
                    .Select(filePath => new FileInfo(filePath))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Take(50)
                    .Select(f => new LogFileInfo
                    {
                        FileName = f.Name,
                        FilePath = f.FullName,
                        FileSize = FormatFileSize(f.Length),
                        LastModified = f.LastWriteTime,
                        CreatedDate = f.CreationTime
                    })
                    .ToList();

                model.LogFiles = logFiles;
            }

            return View(model);
        }

        public IActionResult ViewLog(string fileName)
        {
            if (User.IsInRole(AppRoles.Client))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required");
            }

            var logsPath = Path.Combine(_environment.ContentRootPath, "logs");
            var filePath = Path.Combine(logsPath, fileName);

            // Security check: ensure the file is within the logs directory
            var fullLogsPath = Path.GetFullPath(logsPath);
            var fullFilePath = Path.GetFullPath(filePath);

            if (!fullFilePath.StartsWith(fullLogsPath))
            {
                return BadRequest("Invalid file path");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Log file not found");
            }

            try
            {
                var lines = System.IO.File.ReadAllLines(filePath);
                var model = new LogFileContentViewModel
                {
                    FileName = fileName,
                    Lines = lines.Reverse().Take(1000).Reverse().ToList(), // Last 1000 lines
                    TotalLines = lines.Length,
                    FileSize = FormatFileSize(new FileInfo(filePath).Length),
                    LastModified = System.IO.File.GetLastWriteTime(filePath)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading log file: {FileName}", fileName);
                return StatusCode(500, "Error reading log file");
            }
        }

        public IActionResult DownloadLog(string fileName)
        {
            if (User.IsInRole(AppRoles.Client))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required");
            }

            var logsPath = Path.Combine(_environment.ContentRootPath, "logs");
            var filePath = Path.Combine(logsPath, fileName);

            // Security check: ensure the file is within the logs directory
            var fullLogsPath = Path.GetFullPath(logsPath);
            var fullFilePath = Path.GetFullPath(filePath);

            if (!fullFilePath.StartsWith(fullLogsPath))
            {
                return BadRequest("Invalid file path");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Log file not found");
            }

            try
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading log file: {FileName}", fileName);
                return StatusCode(500, "Error downloading log file");
            }
        }

        [HttpPost]
        public IActionResult DeleteLog(string fileName)
        {
            if (User.IsInRole(AppRoles.Client))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required");
            }

            var logsPath = Path.Combine(_environment.ContentRootPath, "logs");
            var filePath = Path.Combine(logsPath, fileName);

            // Security check: ensure the file is within the logs directory
            var fullLogsPath = Path.GetFullPath(logsPath);
            var fullFilePath = Path.GetFullPath(filePath);

            if (!fullFilePath.StartsWith(fullLogsPath))
            {
                return BadRequest("Invalid file path");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Log file not found");
            }

            try
            {
                System.IO.File.Delete(filePath);
                TempData["SuccessMessage"] = $"Log file '{fileName}' deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting log file: {FileName}", fileName);
                TempData["ErrorMessage"] = "Error deleting log file";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult ClearAllLogs()
        {
            if (User.IsInRole(AppRoles.Client))
            {
                return Forbid();
            }

            var logsPath = Path.Combine(_environment.ContentRootPath, "logs");

            if (Directory.Exists(logsPath))
            {
                try
                {
                    var logFiles = Directory.GetFiles(logsPath, "*.log");
                    foreach (var file in logFiles)
                    {
                        System.IO.File.Delete(file);
                    }

                    TempData["SuccessMessage"] = $"{logFiles.Length} log file(s) deleted successfully";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing all logs");
                    TempData["ErrorMessage"] = "Error clearing log files";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
