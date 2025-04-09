using System.Security.Claims;

public interface ITokenUserClaimsService
{
    public Task<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token);
}