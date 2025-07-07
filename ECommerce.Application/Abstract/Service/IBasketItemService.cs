using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Utility;
namespace ECommerce.Application.Abstract.Service;

public interface IBasketItemService
{
    Task<Result> CreateBasketItemAsync(CreateBasketItemRequestDto createBasketItemRequestDto);
    Task<Result> UpdateBasketItemAsync(UpdateBasketItemRequestDto updateBasketItemRequestDto);
    Task<Result> DeleteAllNonOrderedBasketItemsAsync();
    Task ClearBasketItemsIncludeOrderedProductAsync(Domain.Model.Product updatedProduct);
    Task ClearBasketItemsCacheAsync();
}