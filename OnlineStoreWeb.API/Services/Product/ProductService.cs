public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        try
        {
            List<Product> products = await _productRepository.Get();
            return products;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Product> GetProductWithIdAsync(int id)
    {
        try
        {
            List<Product> products = await _productRepository.Get();
            Product product = products.FirstOrDefault(p => p.Id == id) ?? throw new Exception("Product not found");
            return product;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddProductAsync(CreateProductDto createProductRequest)
    {
        try
        {
            List<Product> products = await _productRepository.Get();
            if (products.Any(p => p.Name == createProductRequest.Name)) //Duplicate product name check
            {
                throw new Exception("Product already exists in the database");
            }

            Product product = new Product
            {
                Name = createProductRequest.Name,
                Description = createProductRequest.Description,
                Price = createProductRequest.Price,
                ImageUrl = createProductRequest.ImageUrl,
                StockQuantity = createProductRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            };

            await _productRepository.Add(product);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateProductAsync(UpdateProductDto updateProductRequest)
    {
        try
        {
            List<Product> products = await _productRepository.Get();
            Product product = products.FirstOrDefault(p => p.Id == updateProductRequest.Id) ?? throw new Exception("Product not found");

            product.Name = updateProductRequest.Name;
            product.Description = updateProductRequest.Description;
            product.Price = updateProductRequest.Price;
            product.ImageUrl = updateProductRequest.ImageUrl;
            product.StockQuantity = updateProductRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;

            await _productRepository.Update(product);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            List<Product> products = await _productRepository.Get();
            Product product = products.FirstOrDefault(p => p.Id == id) ?? throw new Exception("Product not found");

            await _productRepository.Delete(product);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}