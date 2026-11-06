using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class LogsController : Controller
    {
        private readonly FileStorageService _fileStorageService;

        public LogsController(
            FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var files =
                await _fileStorageService.GetFilesAsync();

            return View(files);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? logFile)
        {
            if (logFile == null || logFile.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a file to upload.";

                return RedirectToAction(nameof(Index));
            }

            await _fileStorageService.UploadFileAsync(logFile);

            TempData["SuccessMessage"] =
                "Log file uploaded successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            try
            {
                var file =
                    await _fileStorageService
                        .DownloadFileAsync(fileName);

                return File(
                    file.Content,
                    file.ContentType,
                    fileName);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}