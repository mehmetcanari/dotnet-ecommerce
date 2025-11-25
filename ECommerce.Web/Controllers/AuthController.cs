using ECommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;

public class AuthController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        return View(model);
    }
}