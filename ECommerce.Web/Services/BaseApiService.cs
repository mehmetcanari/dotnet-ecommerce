using ECommerce.Shared.Wrappers;
using System.Text.Json;

namespace ECommerce.Web.Services;

public abstract class BaseApiService(HttpClient httpClient)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected async Task<Result<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                return Result<T>.Failure($"API Hatası: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<Result<T>>(_jsonOptions);

            return result ?? Result<T>.Failure("Veri boş geldi.");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Sistem Hatası: {ex.Message}");
        }
    }

    protected async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);

            if (!response.IsSuccessStatusCode)
                return Result<TResponse>.Failure($"API Hatası: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<Result<TResponse>>(_jsonOptions);
            return result ?? Result<TResponse>.Failure("Veri boş geldi.");
        }
        catch (Exception ex)
        {
            return Result<TResponse>.Failure($"Sistem Hatası: {ex.Message}");
        }
    }
}