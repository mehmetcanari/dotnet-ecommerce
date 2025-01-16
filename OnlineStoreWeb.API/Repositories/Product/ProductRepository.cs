
public class ProductRepository : IProductRepository
{
    public readonly StoreDbContext _context;

    public ProductRepository(StoreDbContext context)
    {
        _context = context;
    }

    public void AddProduct(CreateProductDto createProductDto)
    {
        throw new NotImplementedException();
    }

    public void UpdateProduct(UpdateProductDto updateProductDto)
    {
        throw new NotImplementedException();
    }

    public void DeleteProduct(int id)
    {
        throw new NotImplementedException();
    }

    public List<Product> GetAllProducts()
    {
        throw new NotImplementedException();
    }

    public Product GetProductWithId(int id)
    {
        throw new NotImplementedException();
    }
}