using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service
{
    public interface ICurrentUserService
    {
        Result<string> GetCurrentUserEmail();
        Result<bool> IsAuthenticated();
    }
}