using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.DTO.Response.BasketItem;

namespace ECommerce.Application.Abstract.Service;

public interface IBasketItemService
{
    Task CreateBasketItemAsync(CreateBasketItemRequestDto createBasketItemRequestDto, string email);
    Task<List<BasketItemResponseDto>> GetAllBasketItemsAsync(string email);
    Task UpdateBasketItemAsync(UpdateBasketItemRequestDto updateBasketItemRequestDto, string email);
    Task DeleteAllBasketItemsAsync(string email);
    Task ClearBasketItemsIncludeOrderedProductAsync(Domain.Model.Product updatedProduct);
}