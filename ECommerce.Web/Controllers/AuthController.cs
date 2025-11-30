using ECommerce.Web.Models;
using ECommerce.Web.Services;
using ECommerce.Web.Utility;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class AuthController(AuthApiService authService, AccountApiService accountApiService) : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await authService.LoginAsync(model);

        if (result is { IsSuccess: true, Data: not null })
        {
            HttpContext.Session.SetString("AccessToken", result.Data.AccessToken);
            HttpContext.Session.SetString("UserEmail", model.Email);

            var profileResult = await accountApiService.GetProfileAsync();
            if (profileResult is { IsSuccess: true, Data: not null })
            {
                HttpContext.Session.SetString("UserName", profileResult.Data.Name);
                HttpContext.Session.SetString("UserSurname", profileResult.Data.Surname);
            }

            return RedirectToAction("Index", "Home");
        }

        var userMessage = ClientErrorMessageTranslator.GetUserFriendlyMessage(result.Message);
        ModelState.AddModelError(string.Empty, userMessage);

        return View(model);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await authService.RegisterAsync(model);

        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Login));
        }

        var userMessage = ClientErrorMessageTranslator.GetUserFriendlyMessage(result.Message);
        ModelState.AddModelError(string.Empty, userMessage);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.ForgotPasswordAsync(model);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            return RedirectToAction(nameof(Login));
        }

        var userMessage = ClientErrorMessageTranslator.GetUserFriendlyMessage(result.Message);
        ModelState.AddModelError(string.Empty, userMessage);

        return View(model);
    }
}