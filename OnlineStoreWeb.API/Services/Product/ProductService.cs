using OnlineStoreWeb.API.DTO.Request.Product;
using OnlineStoreWeb.API.DTO.Response.Product;
using OnlineStoreWeb.API.Repositories.Product;

namespace OnlineStoreWeb.API.Services.Product;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }
    
    public async Task<List<ProductResponseDto>> GetAllProductsAsync()
    {
        try
        {
            var products = await _productRepository.Read();
            if (products.Count <= 0)
            {
                throw new Exception("No products found.");
            }

            return products.Select(p => new ProductResponseDto
            {
                ProductName = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountRate = p.DiscountRate,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all products");
            throw new Exception(ex.Message);
        }
    }

    public async Task<ProductResponseDto> GetProductWithIdAsync(int requestId)
    {
        try
        {
            var products = await _productRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == requestId) ?? throw new Exception("Product not found");
            
            var productResponse = new ProductResponseDto
            {
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountRate = product.DiscountRate,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity
            };

            return productResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching product with id: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task AddProductAsync(ProductCreateDto productCreateRequest)
    {
        try
        {
            var products = await _productRepository.Read();
            if (products.Any(p => p.Name == productCreateRequest.Name)) //Duplicate product name check
            {
                throw new Exception("Product already exists in the database");
            }

            var product = new Model.Product
            {
                Name = productCreateRequest.Name,
                Description = productCreateRequest.Description,
                Price = productCreateRequest.Price,
                DiscountRate = productCreateRequest.DiscountRate,
                ImageUrl = productCreateRequest.ImageUrl,
                StockQuantity = productCreateRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            };
            
            if (product.DiscountRate > 0)
                product.Price -= (product.Price * product.DiscountRate / 100); //Calculate discounted price
            
            await _productRepository.Create(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding product: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task UpdateProductAsync(int id, ProductUpdateDto productUpdateRequest)
    {
        try
        {
            var products = await _productRepository.Read();
            var product = products.FirstOrDefault(p => p.ProductId == id) ?? throw new Exception("Product not found");

            product.Name = productUpdateRequest.Name;
            product.Description = productUpdateRequest.Description;
            product.Price = productUpdateRequest.Price;
            product.DiscountRate = productUpdateRequest.DiscountRate;
            product.ImageUrl = productUpdateRequest.ImageUrl;
            product.StockQuantity = productUpdateRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;
            
            if (product.DiscountRate > 0)
                product.Price -= (product.Price * product.DiscountRate / 100); //Calculate discounted price

            await _productRepository.Update(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            List<Model.Product?> products = await _productRepository.Read();
            Model.Product? product = products.FirstOrDefault(p => p.ProductId == id) ?? throw new Exception("Product not found");

            await _productRepository.Delete(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }
}