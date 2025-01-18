using Microsoft.EntityFrameworkCore;


public class ProductRepository : IProductRepository
{
    private readonly StoreDbContext _context;

    public ProductRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        try
        {
            return await _context.Products.ToListAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch products", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Product?> GetProductWithIdAsync(int id)
    {
        try
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id) 
                ?? throw new Exception("Product not found");
            if (product == null)
                throw new Exception("Product not found");

            return product;
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch product", ex);
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
            var product = new Product
            {
                Name = createProductRequest.Name,
                Description = createProductRequest.Description,
                Price = createProductRequest.Price,
                ImageUrl = createProductRequest.ImageUrl,
                StockQuantity = createProductRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to save product", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateProductAsync(int id, UpdateProductDto updateProductRequest)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new Exception("Product not found");

            product.Name = updateProductRequest.Name;
            product.Description = updateProductRequest.Description;
            product.Price = updateProductRequest.Price;
            product.ImageUrl = updateProductRequest.ImageUrl;
            product.StockQuantity = updateProductRequest.StockQuantity;
            product.ProductUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update product", ex);
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
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new Exception("Product not found");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete product", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}