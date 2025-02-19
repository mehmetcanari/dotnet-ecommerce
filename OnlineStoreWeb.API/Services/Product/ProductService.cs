using OnlineStoreWeb.API.DTO.Product;
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
    
    public async Task<List<Model.Product>> GetAllProductsAsync()
    {
        try
        {
            List<Model.Product> products = await _productRepository.Read();
            if (products.Count <= 0)
            {
                throw new Exception("No products found.");
            }

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all products");
            throw new Exception(ex.Message);
        }
    }

    public async Task<Model.Product> GetProductWithIdAsync(int requestId)
    {
        try
        {
            List<Model.Product> products = await _productRepository.Read();
            Model.Product product = products.FirstOrDefault(p => p.Id == requestId) ?? throw new Exception("Product not found");
            return product;
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
            List<Model.Product> products = await _productRepository.Read();
            if (products.Any(p => p.Name == productCreateRequest.Name)) //Duplicate product name check
            {
                throw new Exception("Product already exists in the database");
            }

            Model.Product product = new Model.Product
            {
                Name = productCreateRequest.Name,
                Description = productCreateRequest.Description,
                Price = productCreateRequest.Price,
                ImageUrl = productCreateRequest.ImageUrl,
                StockQuantity = productCreateRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            };
            
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
            List<Model.Product> products = await _productRepository.Read();
            Model.Product product = products.FirstOrDefault(p => p.Id == id) ?? throw new Exception("Product not found");

            product.Name = productUpdateRequest.Name;
            product.Description = productUpdateRequest.Description;
            product.Price = productUpdateRequest.Price;
            product.ImageUrl = productUpdateRequest.ImageUrl;
            product.StockQuantity = productUpdateRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;

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
            List<Model.Product> products = await _productRepository.Read();
            Model.Product product = products.FirstOrDefault(p => p.Id == id) ?? throw new Exception("Product not found");

            await _productRepository.Delete(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            throw new Exception(ex.Message);
        }
    }
}