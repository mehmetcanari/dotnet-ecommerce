using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface IAccessTokenService
{
    Task<AccessToken> GenerateAccessTokenAsync(string email, IList<string> roles);
} 