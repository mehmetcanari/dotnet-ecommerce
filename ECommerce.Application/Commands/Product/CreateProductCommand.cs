using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Product;

public class CreateProductCommand : IRequest<Result>
{
    public required ProductCreateRequestDto ProductCreateRequest { get; set; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;

    public CreateProductCommandHandler(IProductRepository productRepository , ICategoryRepository categoryRepository , ILoggingService logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingProduct = await _productRepository.CheckProductExistsWithName(request.ProductCreateRequest.Name);
            if (existingProduct)
            {
                return Result.Failure("Product with this name already exists");
            }

            var category = await _categoryRepository.GetCategoryById(request.ProductCreateRequest.CategoryId);
            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            var product = new Domain.Model.Product
            {
                Name = request.ProductCreateRequest.Name,
                Description = request.ProductCreateRequest.Description,
                Price = request.ProductCreateRequest.Price,
                DiscountRate = request.ProductCreateRequest.DiscountRate,
                ImageUrl = request.ProductCreateRequest.ImageUrl,
                StockQuantity = request.ProductCreateRequest.StockQuantity,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow,
                CategoryId = category.CategoryId
            };

            product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);

            await _productRepository.Create(product);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"An error occurred while creating the product", ex.Message);
            return Result.Failure("An error occurred while creating the product");
        }
    }
}