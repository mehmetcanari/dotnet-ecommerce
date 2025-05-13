using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IAccessTokenService
{
    Task<AccessToken> GenerateAccessTokenAsync(string email, IList<string> roles);
} 