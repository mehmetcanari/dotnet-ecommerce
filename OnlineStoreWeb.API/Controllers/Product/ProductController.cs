using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Product
{
    [ApiController]
    [Route("products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductRepository productRepository, ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto productDto)
        {
            try
            {
                if (productDto == null)
                    return BadRequest(new { message = "Product data is required" });

                await _productRepository.AddProductAsync(productDto);
                return Created($"products", new { message = "Product created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productRepository.GetAllProductsAsync();
                if (!products.Any())
                    return NotFound(new { message = "No products found" });

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching products: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while fetching products");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductWithId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var product = await _productRepository.GetProductWithIdAsync(id);
                if (product == null)
                    return NotFound(new { message = $"with id: {id} product not found" });

                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product fetching process error: {Message}", ex.Message);
                return BadRequest(new { message = "Product fetching process error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching product: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while fetching the product");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                if (updateProductDto == null)
                    return BadRequest(new { message = "Product update data is required" });

                await _productRepository.UpdateProductAsync(id, updateProductDto);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating product: {Message}", ex.Message);
                return BadRequest(new { message = "Invalid product data provided" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for update: {Message}", ex.Message);
                return NotFound(new { message = $"Product with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while updating the product");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                await _productRepository.DeleteProductAsync(id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for deletion: {Message}", ex.Message);
                return NotFound(new { message = $"Product with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while deleting the product");
            }
        }
    }
}