using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Product;
using OnlineStoreWeb.API.Services.Product;

namespace OnlineStoreWeb.API.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
public class AdminProductController(
    IProductService productService,
    IValidator<ProductCreateDto> createProductValidator,
    IValidator<ProductUpdateDto> updateProductValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while fetching products");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            var product = await productService.GetProductWithIdAsync(id);
            return Ok(product);
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while fetching the product");
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct(ProductCreateDto productCreateRequest)
    {
        ValidationResult result = await createProductValidator.ValidateAsync(productCreateRequest);
        if (!result.IsValid)
        {
            result.AddToModelState(this.ModelState, null);
            return BadRequest(this.ModelState);
        }
        
        try
        {
            await productService.AddProductAsync(productCreateRequest);
            return Created($"products/{productCreateRequest.Name}", new { message = "Product created successfully" });
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while creating the product");
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto productUpdateRequest)
    {
        ValidationResult result = await updateProductValidator.ValidateAsync(productUpdateRequest);
        
        try
        {
            if (!result.IsValid)
            {
                result.AddToModelState(this.ModelState, null);
                return BadRequest(this.ModelState);
            }
            
            await productService.UpdateProductAsync(id, productUpdateRequest);
            return Ok(new { message = "Product updated successfully" });
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while updating the product");
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await productService.DeleteProductAsync(id);
            return Ok(new { message = "Product deleted successfully" });
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while deleting the product");
        }
    }
}