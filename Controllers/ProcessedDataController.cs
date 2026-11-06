using Microsoft.AspNetCore.Mvc;
using RealTimeProApp.Services;

namespace RealTimeProApp.Controllers;

public class ProcessedDataController : Controller
{
    private readonly AgricultureStorageService _storage;

    public ProcessedDataController(AgricultureStorageService storage)
    {
        _storage = storage;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UsingAzure = _storage.UsingAzure;
        return View(await _storage.GetTelemetryAsync());
    }

    public async Task<IActionResult> Image(string imageName)
    {
        var image = await _storage.GetDroneImageAsync(imageName);
        if (image == null) return NotFound();
        return File(image.Value.Content, image.Value.ContentType);
    }
}
