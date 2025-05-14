using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Utility;
namespace ECommerce.Application.Abstract.Service;

public interface IBasketItemService
{
    Task<Result> CreateBasketItemAsync(CreateBasketItemRequestDto createBasketItemRequestDto, string email);
    Task<Result<List<BasketItemResponseDto>>> GetAllBasketItemsAsync(string email);
    Task<Result> UpdateBasketItemAsync(UpdateBasketItemRequestDto updateBasketItemRequestDto, string email);
    Task<Result> DeleteAllBasketItemsAsync(string email);
    Task ClearBasketItemsIncludeOrderedProductAsync(Domain.Model.Product updatedProduct);
}