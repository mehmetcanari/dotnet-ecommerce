using System.Security.Claims;
using ECommerce.Domain.Model;
namespace ECommerce.Application.Interfaces.Service
{
    public interface ITokenUserClaimsService
    {
        public Task<ClaimsPrincipal> GetClaimsPrincipalFromToken(RefreshToken refreshToken);
    }
}