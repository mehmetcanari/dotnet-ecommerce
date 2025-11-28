using ECommerce.Web.Filters;
using ECommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

[AuthenticatedUser]
public class AccountController(AccountApiService accountApiService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await accountApiService.GetProfileAsync();
        if (result.IsSuccess)
        {
            return View(result.Data);
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToAction("Index", "Home");
    }
}