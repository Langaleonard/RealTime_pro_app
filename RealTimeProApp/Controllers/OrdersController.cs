using Microsoft.AspNetCore.Mvc;
using RealTimeProApp.Models;
using RealTimeProApp.Services;

namespace RealTimeProApp.Controllers;

public class OrdersController : Controller
{
    private readonly EventHubPublisherService _publisher;

    public OrdersController(EventHubPublisherService publisher)
    {
        _publisher = publisher;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.EventHubConfigured = _publisher.IsConfigured;
        return View(new OrderEvent());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(OrderEvent order)
    {
        ViewBag.EventHubConfigured = _publisher.IsConfigured;

        if (!ModelState.IsValid)
        {
            return View(order);
        }

        if (!_publisher.IsConfigured)
        {
            TempData["ErrorMessage"] = "Event Hubs is not configured yet. Add the Azure:EventHubConnectionString setting first.";
            return View(order);
        }

        await _publisher.PublishOrderAsync(order);
        TempData["SuccessMessage"] = $"Order {order.OrderId} was sent to Azure Event Hubs.";
        return RedirectToAction(nameof(Index));
    }
}
