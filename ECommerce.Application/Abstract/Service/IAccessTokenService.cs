using ECommerce.Domain.Model;
using ECommerce.Application.Utility;
namespace ECommerce.Application.Abstract.Service;

public interface IAccessTokenService
{
    Result<AccessToken> GenerateAccessTokenAsync(Guid userId, string email, IList<string> roles);
} 