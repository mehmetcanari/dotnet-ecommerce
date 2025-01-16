namespace OnlineStoreWeb.API.Controllers.Product;

public class ProductController
{
    private readonly IProductRepository _productRepository;
    private readonly WebApplication _app;

    public ProductController(IProductRepository productRepository, WebApplication app)
    {
        _productRepository = productRepository;
        _app = app;
    }

    public void InitializeController()
    {
    }

    public void PostProduct()
    {
        _app.MapPost("/api/product", async (CreateProductDto productDto) =>
        {
            try
            {
                if (productDto == null)
                    return Results.BadRequest("Product data is required");

                await _productRepository.AddProductAsync(productDto);
                return Results.Created($"/api/product", productDto + "Product created successfully");
            }
            catch (Exception ex)
            {
                return Results.StatusCode(500);
            }
        });
    }

    public void GetAllProducts()
    {
        _app.MapGet("/api/product", async () =>
        {
            try
            {
                var products = await _productRepository.GetAllProductsAsync();
                if (products == null)
                    return Results.NotFound("No products found");

                return Results.Ok($"Products fetched successfully: {products}");
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message + "Error fetching products");
            }
        });
    }

    public void GetProductWithId()
    {
        _app.MapGet("/api/product/{id}", async (int id) =>
        {
            return await _productRepository.GetProductWithIdAsync(id);
        });
    }
}