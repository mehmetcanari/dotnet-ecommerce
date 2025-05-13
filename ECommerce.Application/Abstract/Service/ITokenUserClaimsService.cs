using System.Security.Claims;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface ITokenUserClaimsService
{
    public ClaimsPrincipal GetClaimsPrincipalFromToken(RefreshToken refreshToken);
}