using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueStorageService _queueStorageService;

        public OrdersController(
            QueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var messages =
                await _queueStorageService.GetMessagesAsync();

            return View(messages);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            OrderMessage order)
        {
            if (!ModelState.IsValid)
            {
                return View(order);
            }

            string message =
                $"Order ID: {order.OrderId} | " +
                $"Customer: {order.CustomerName} | " +
                $"Product: {order.ProductName} | " +
                $"Quantity: {order.Quantity} | " +
                $"Status: {order.Status}";

            await _queueStorageService.SendMessageAsync(message);

            return RedirectToAction(nameof(Index));
        }
    }
}