using ECommerce.Shared.Wrappers;
using ECommerce.Web.Filters;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace ECommerce.Web.Controllers;

[AuthenticatedUser]
public class AccountController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient("ECommerceAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

            var response = await client.GetAsync("/api/account/profile");

            if (response.IsSuccessStatusCode)
            {
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                var responseContent = Encoding.UTF8.GetString(responseBytes);

                var result = JsonSerializer.Deserialize<Result<ProfileViewModel>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                if (result is { IsSuccess: true, Data: not null })
                {
                    return View(result.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = "Profil bilgileri alınamadı.";
                    return RedirectToAction("Index", "Home");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                TempData["ErrorMessage"] = "Oturumunuz sona erdi. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                TempData["ErrorMessage"] = "Profil bilgileri alınırken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "API'ye bağlanılamadı. Lütfen daha sonra tekrar deneyiniz.";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
            return RedirectToAction("Index", "Home");
        }
    }
}
