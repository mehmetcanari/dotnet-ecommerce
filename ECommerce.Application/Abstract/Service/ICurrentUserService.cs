using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service
{
    public interface ICurrentUserService
    {
        Task<Result<string>> GetUserId();
        Result<string> GetUserEmail();
        Result<bool> IsAuthenticated();
    }
}