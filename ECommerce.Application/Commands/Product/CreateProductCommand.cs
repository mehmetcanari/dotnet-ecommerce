using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using Result = ECommerce.Application.Utility.Result;

namespace ECommerce.Application.Commands.Product;

public class CreateProductCommand : IRequest<Result>
{
    public required ProductCreateRequestDto ProductCreateRequest { get; set; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductSearchService _productSearchService;
    private readonly ILoggingService _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ILoggingService logger,
        IProductSearchService productSearchService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _productSearchService = productSearchService;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await ValidateRequest(request.ProductCreateRequest);
            if (!validationResult.IsSuccess)
            {
                return Result.Failure(validationResult.Error);
            }

            await _productSearchService.CreateProductIndexWithNGramAsync();

            var product = CreateProductEntity(request.ProductCreateRequest, validationResult.Data);
            await _productRepository.Create(product);
            await _productSearchService.IndexProductAsync(product);

            _logger.LogInformation("Product created successfully. ProductId: {ProductId}, Name: {Name}", 
                product.ProductId, product.Name);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the product: {Message}", ex.Message);
            return Result.Failure("An error occurred while creating the product");
        }
    }

    private async Task<Result<Domain.Model.Category>> ValidateRequest(ProductCreateRequestDto request)
    {
        var existingProduct = await _productRepository.CheckProductExistsWithName(request.Name);
        if (existingProduct)
        {
            _logger.LogWarning("Product creation failed. Product with name '{Name}' already exists", request.Name);
            return Result<Domain.Model.Category>.Failure("Product with this name already exists");
        }

        var category = await _categoryRepository.GetCategoryById(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Product creation failed. Category with ID {CategoryId} not found", request.CategoryId);
            return Result<Domain.Model.Category>.Failure("Category not found");
        }

        return Result<Domain.Model.Category>.Success(category);
    }

    private static Domain.Model.Product CreateProductEntity(
        ProductCreateRequestDto request,
        Domain.Model.Category category)
    {
        var product = new Domain.Model.Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountRate = request.DiscountRate,
            ImageUrl = request.ImageUrl,
            StockQuantity = request.StockQuantity,
            ProductCreated = DateTime.UtcNow,
            ProductUpdated = DateTime.UtcNow,
            CategoryId = category.CategoryId
        };

        product.Price = MathService.CalculateDiscount(product.Price, product.DiscountRate);
        return product;
    }
}