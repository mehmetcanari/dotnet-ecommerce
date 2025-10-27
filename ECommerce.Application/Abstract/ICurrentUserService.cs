using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserEmail();
        string GetIpAddress();
    }
}