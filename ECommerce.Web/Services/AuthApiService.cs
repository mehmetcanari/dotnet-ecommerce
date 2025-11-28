using ECommerce.Shared.DTO.Request.Account;
using ECommerce.Shared.DTO.Response.Auth;
using ECommerce.Shared.Wrappers;
using ECommerce.Web.Models;

namespace ECommerce.Web.Services;

public class AuthApiService(HttpClient httpClient) : BaseApiService(httpClient)
{
    public async Task<Result<AuthResponseDto>> LoginAsync(LoginViewModel model)
    {
        var request = new AccountLoginRequestDto()
        {
            Email = model.Email,
            Password = model.Password
        };

        return await PostAsync<AccountLoginRequestDto, AuthResponseDto>("/api/authentication/login", request);
    }

    public async Task<Result<object>> RegisterAsync(RegisterViewModel model)
    {
        var registerRequest = new AccountRegisterRequestDto
        {
            Name = model.Name,
            Surname = model.Surname,
            IdentityNumber = model.IdentityNumber,
            Email = model.Email,
            Password = model.Password,
            Phone = model.Phone,
            PhoneCode = model.PhoneCode,
            DateOfBirth = model.DateOfBirth ?? DateTime.MinValue,
            ElectronicConsent = model.ElectronicConsent,
            PrivacyPolicyConsent = model.PrivacyPolicyConsent,
            MembershipAgreement = model.MembershipAgreement,
            Country = model.Country,
            City = model.City,
            ZipCode = model.ZipCode,
            Address = model.Address
        };

        return await PostAsync<AccountRegisterRequestDto, object>("/api/authentication/register", registerRequest);
    }

    public async Task<Result<object>> ForgotPasswordAsync(ForgotPasswordViewModel model)
    {
        var request = new AccountForgotPasswordRequestDto
        {
            Email = model.Email
        };

        return await PostAsync<AccountForgotPasswordRequestDto, object>("/api/authentication/forgot-password", request);
    }

    public async Task LogoutAsync()
    {
        await PostAsync<object, object>("/api/authentication/logout", new { });
    }
}