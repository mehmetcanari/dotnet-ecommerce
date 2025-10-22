using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserEmail();
        string GetIpAdress();
    }
}