using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service
{
    public interface ICurrentUserService
    {
        Task<Result<string>> GetCurrentUserId();
        Result<string> GetCurrentUserEmail();
        Result<bool> IsAuthenticated();
    }
}