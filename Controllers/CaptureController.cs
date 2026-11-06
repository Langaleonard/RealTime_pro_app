using Microsoft.AspNetCore.Mvc;
using RealTimeProApp.Models;
using RealTimeProApp.Services;

namespace RealTimeProApp.Controllers;

public class CaptureController : Controller
{
    private readonly AgricultureStorageService _storage;

    public CaptureController(AgricultureStorageService storage)
    {
        _storage = storage;
    }

    [HttpGet]
    public IActionResult Index() => View(new CaptureViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CaptureViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var record = await _storage.CaptureAsync(model);
        TempData["SuccessMessage"] = record.Status == "Irrigation Required"
            ? "Telemetry processed. Low moisture detected and an irrigation command was created."
            : "Telemetry processed successfully. Field status is normal.";

        return RedirectToAction("Index", "ProcessedData");
    }
}
