using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace ECommerce.Web.Filters;

public class AuthenticatedUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var accessToken = httpContext.Session.GetString("AccessToken");

        if (string.IsNullOrEmpty(accessToken))
        {
            RedirectToLogin(context);
            return;
        }

        if (IsTokenExpired(accessToken))
        {
            httpContext.Session.Clear();
            RedirectToLogin(context);
            return;
        }

        base.OnActionExecuting(context);
    }

    private bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var expClaim = jwt.Claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim == null)
                return true;

            var expUnix = long.Parse(expClaim.Value);
            var expTime = DateTimeOffset.FromUnixTimeSeconds(expUnix);

            return expTime < DateTimeOffset.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    private static void RedirectToLogin(ActionExecutingContext context)
    {
        context.Result = new RedirectToActionResult("Login", "Auth", null);
    }
}
