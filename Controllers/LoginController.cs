using Microsoft.AspNetCore.Mvc;
using RealTimeProApp.Models;

namespace RealTimeProApp.Controllers;

public class LoginController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Simple practicum demo login. Replace with ASP.NET Identity only if the exam requires real authentication.
        HttpContext.Session.SetString("FarmerEmail", model.Email);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }
}
