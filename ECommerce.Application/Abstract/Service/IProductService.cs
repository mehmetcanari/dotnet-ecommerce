using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;

namespace ECommerce.Application.Abstract.Service;

public interface IProductService
{
    Task<List<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto> GetProductWithIdAsync(int requestId);
    Task CreateProductAsync(ProductCreateRequestDto productCreateRequest);
    Task UpdateProductAsync(int id, ProductUpdateRequestDto productUpdateRequest);
    Task UpdateProductStockAsync(List<Domain.Model.BasketItem> basketItems);
    Task DeleteProductAsync(int id);
}