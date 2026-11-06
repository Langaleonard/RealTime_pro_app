using Microsoft.AspNetCore.Mvc;
using RealTimeProApp.Services;

namespace RealTimeProApp.Controllers;

public class ProcessedOrdersController : Controller
{
    private readonly ProcessedOrderService _service;

    public ProcessedOrdersController(ProcessedOrderService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.SqlConfigured = _service.IsConfigured;
        var orders = await _service.GetRecentAsync();
        return View(orders);
    }
}
