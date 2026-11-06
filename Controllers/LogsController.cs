using Microsoft.AspNetCore.Mvc;
using RealTimeProApp.Models;
using RealTimeProApp.Services;

namespace RealTimeProApp.Controllers;

public class LogsController : Controller
{
    private readonly AgricultureStorageService _storage;

    public LogsController(AgricultureStorageService storage)
    {
        _storage = storage;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UsingAzure = _storage.UsingAzure;
        return View(new OperationsViewModel
        {
            IrrigationCommands = await _storage.GetIrrigationCommandsAsync(),
            OperationalLogs = await _storage.GetOperationalLogsAsync()
        });
    }
}
