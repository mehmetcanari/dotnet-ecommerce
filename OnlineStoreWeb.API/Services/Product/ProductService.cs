using OnlineStoreWeb.API.DTO.Product;
using OnlineStoreWeb.API.Repositories.Product;

namespace OnlineStoreWeb.API.Services.Product;

public class ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    : IProductService
{
    public async Task<List<Model.Product>> GetAllProductsAsync()
    {
        try
        {
            List<Model.Product> products = await productRepository.Get();
            return products;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching all products");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Model.Product> GetProductWithIdAsync(int requestId)
    {
        try
        {
            List<Model.Product> products = await productRepository.Get();
            Model.Product product = products.FirstOrDefault(p => p.Id == requestId) ?? throw new Exception("Product not found");
            return product;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching product with id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddProductAsync(CreateProductDto createProductRequest)
    {
        try
        {
            List<Model.Product> products = await productRepository.Get();
            if (products.Any(p => p.Name == createProductRequest.Name)) //Duplicate product name check
            {
                throw new Exception("Product already exists in the database");
            }

            Model.Product product = new Model.Product
            {
                Name = createProductRequest.Name,
                Description = createProductRequest.Description,
                Price = createProductRequest.Price,
                ImageUrl = createProductRequest.ImageUrl,
                StockQuantity = createProductRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            };

            await productRepository.Add(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while adding product: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateProductAsync(int id, UpdateProductDto updateProductRequest)
    {
        try
        {
            List<Model.Product> products = await productRepository.Get();
            Model.Product product = products.FirstOrDefault(p => p.Id == id) ?? throw new Exception("Product not found");

            product.Name = updateProductRequest.Name;
            product.Description = updateProductRequest.Description;
            product.Price = updateProductRequest.Price;
            product.ImageUrl = updateProductRequest.ImageUrl;
            product.StockQuantity = updateProductRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;

            await productRepository.Update(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            List<Model.Product> products = await productRepository.Get();
            Model.Product product = products.FirstOrDefault(p => p.Id == id) ?? throw new Exception("Product not found");

            await productRepository.Delete(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}