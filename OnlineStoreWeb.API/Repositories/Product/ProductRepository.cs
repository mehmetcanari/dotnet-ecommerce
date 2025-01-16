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
        catch (Exception ex)
        {
            throw new Exception("Error fetching products", ex);
        }
    }

    public async Task<Product?> GetProductWithIdAsync(int id)
    {
        try
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching product", ex);
        }
    }

    public async Task AddProductAsync(CreateProductDto createProductDto)
    {
        try
        {
            var product = new Product
            {
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Description = createProductDto.Description,
                ProductCreated = createProductDto.ProductCreated,
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding product", ex);
        }
    }

    public async Task UpdateProductAsync(UpdateProductDto updateProductDto)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == updateProductDto.Id)
                ?? throw new Exception("Product not found");

            product.Name = updateProductDto.Name;
            product.Price = updateProductDto.Price;
            product.Description = updateProductDto.Description;
            product.ProductUpdated = updateProductDto.ProductUpdated;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating product", ex);
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
        catch (Exception ex)
        {
            throw new Exception("Error deleting product", ex);
        }
    }
}