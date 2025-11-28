using ECommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ECommerce.Web.Controllers;

public class AuthController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = httpClientFactory.CreateClient("ECommerceAPI");
            
            var loginRequest = new
            {
                email = model.Email,
                password = model.Password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/Authentication/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result is { IsSuccess: true, Data.AccessToken: not null })
                {
                    HttpContext.Session.SetString("AccessToken", result.Data.AccessToken);
                    HttpContext.Session.SetString("UserEmail", model.Email);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var userFriendlyMessage = GetUserFriendlyErrorMessage(result?.Message);
                    ModelState.AddModelError(string.Empty, userFriendlyMessage);
                    return View(model);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var userFriendlyMessage = GetUserFriendlyErrorMessage(errorResult?.Message);
                ModelState.AddModelError(string.Empty, userFriendlyMessage);
                return View(model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = httpClientFactory.CreateClient("ECommerceAPI");
            
            var registerRequest = new
            {
                name = model.Name,
                surname = model.Surname,
                email = model.Email,
                identityNumber = model.IdentityNumber,
                password = model.Password,
                phone = model.Phone,
                phoneCode = model.PhoneCode,
                dateOfBirth = model.DateOfBirth,
                electronicConsent = model.ElectronicConsent,
                membershipAgreement = model.MembershipAgreement,
                kvkkConsent = model.PrivacyPolicyConsent,
                country = model.Country,
                city = model.City,
                zipCode = model.ZipCode,
                address = model.Address
            };

            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/Authentication/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var userFriendlyMessage = GetUserFriendlyErrorMessage(errorResult?.Message);
                ModelState.AddModelError(string.Empty, userFriendlyMessage);
                return View(model);
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    private static string GetUserFriendlyErrorMessage(string? apiErrorMessage)
    {
        if (string.IsNullOrEmpty(apiErrorMessage))
            return "İşlem başarısız oldu. Lütfen tekrar deneyiniz.";

        return apiErrorMessage switch
        {
            // Authentication errors
            "authentication.invalid.credentials" => "E-posta veya şifre hatalı.",
            "authentication.error.logging.in" => "Giriş yapılırken bir hata oluştu.",
            "authentication.account.not.authorized" => "Bu hesap yetkili değil.",
            "authentication.error.validating.login" => "Giriş bilgileri doğrulanamadı.",
            
            // Account errors
            "account.not.found" => "Kullanıcı bulunamadı.",
            "account.email.not.found" => "E-posta adresi bulunamadı.",
            "account.banned" => "Bu hesap engellenmiştir.",
            "account.invalid.email.or.password" => "E-posta veya şifre hatalı.",
            "account.email.already.in.use" => "Bu e-posta adresi zaten kullanılıyor.",
            "account.identity.number.already.exists" => "Bu T.C. kimlik numarası zaten kayıtlı.",
            "account.creation.failed" => "Hesap oluşturulamadı.",
            "account.deleted" => "Bu hesap silinmiştir.",
            
            // Identity errors
            "account.identity.user.not.found" => "Kullanıcı bulunamadı.",
            
            // Token errors
            "authentication.error.generating.tokens" => "Oturum oluşturulamadı.",
            "authentication.failed.to.generate.access.token" => "Giriş işlemi tamamlanamadı.",
            
            // Default fallback
            _ when apiErrorMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase) => "Geçersiz bilgi girdiniz.",
            _ when apiErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase) => "Bilgi bulunamadı.",
            _ when apiErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase) => "Bu bilgi zaten kayıtlı.",
            _ when apiErrorMessage.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) => "Bu işlem için yetkiniz yok.",
            _ => "İşlem başarısız oldu. Lütfen tekrar deneyiniz."
        };
    }

    private class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    private class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}