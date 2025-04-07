using System.Security.Claims;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IAccessTokenService
{
    Task<AccessToken> GenerateAccessTokenAsync(string email, IList<string> roles);
    Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
} 