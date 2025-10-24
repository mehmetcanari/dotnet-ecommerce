using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service;

public interface IProductService
{
    Task<Result> CreateProductAsync(ProductCreateRequestDto productCreateRequest);
    Task<Result> UpdateProductAsync(Guid id, ProductUpdateRequestDto productUpdateRequest);
    Task<Result> UpdateProductStockAsync(List<Domain.Model.BasketItem> basketItems);
    Task<Result> DeleteProductAsync(Guid id);
}