using ECommerce.Shared.Wrappers;
using ECommerce.Web.Models;

namespace ECommerce.Web.Services;

public class AccountApiService(HttpClient httpClient) : BaseApiService(httpClient)
{
    public async Task<Result<ProfileViewModel>> GetProfileAsync()
    {
        return await GetAsync<ProfileViewModel>("/api/account/profile");
    }
}