using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class UpdateBasketItemCommand(UpdateBasketItemRequestDto request) : IRequest<Result>
{
    public readonly UpdateBasketItemRequestDto Model = request;
}

public class UpdateBasketItemCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILoggingService logger, IAccountRepository accountRepository, 
    IProductRepository productRepository, IBasketItemService basketItemService) : IRequestHandler<UpdateBasketItemCommand, Result>
{
    public async Task<Result> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = GetValidatedUserEmail();

            var accountResult = await ValidateAndGetAccount(emailResult);
            if (accountResult is { IsFailure: true, Message: not null })
                return Result.Failure(accountResult.Message);

            if (accountResult.Data == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var basketItemResult = await ValidateAndGetBasketItem(request, accountResult.Data);
            if (basketItemResult is { IsFailure: true, Message: not null })
                return Result.Failure(basketItemResult.Message);

            var productResult = await ValidateAndGetProduct(request);
            if (productResult is { IsFailure: true, Message: not null })
                return Result.Failure(productResult.Message);

            if (productResult.Data == null)
                return Result.Failure(ErrorMessages.ProductNotFound);

            var stockValidationResult = ValidateStock(request, productResult.Data);
            if (stockValidationResult.IsFailure)
                return Result.Failure(ErrorMessages.StockNotAvailable);

            if (basketItemResult.Data == null)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            await UpdateBasketItem(basketItemResult.Data, productResult.Data, request);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private string GetValidatedUserEmail()
    {
        var email = currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        return email;
    }

    private async Task<Result<Domain.Model.User>> ValidateAndGetAccount(string email)
    {
        var account = await accountRepository.GetByEmail(email);
        if (account == null)
            return Result<Domain.Model.User>.Failure(ErrorMessages.AccountNotFound);

        return Result<Domain.Model.User>.Success(account);
    }

    private async Task<Result<Domain.Model.BasketItem>> ValidateAndGetBasketItem(UpdateBasketItemCommand request, Domain.Model.User account)
    {
        var basketItem = await basketItemRepository.GetUserCart(request.Model.Id, account);

        if (basketItem == null)
            return Result<Domain.Model.BasketItem>.Failure(ErrorMessages.BasketItemNotFound);

        return Result<Domain.Model.BasketItem>.Success(basketItem);
    }

    private async Task<Result<Domain.Model.Product>> ValidateAndGetProduct(UpdateBasketItemCommand request)
    {
        var product = await productRepository.GetById(request.Model.ProductId);
        if (product == null)
            return Result<Domain.Model.Product>.Failure(ErrorMessages.ProductNotFound);

        return Result<Domain.Model.Product>.Success(product);
    }

    private Result ValidateStock(UpdateBasketItemCommand request, Domain.Model.Product product)
    {
        if (request.Model.Quantity > product.StockQuantity)
            return Result.Failure(ErrorMessages.StockNotAvailable);

        return Result.Success();
    }

    private async Task UpdateBasketItem(Domain.Model.BasketItem basketItem, Domain.Model.Product product, UpdateBasketItemCommand request)
    {
        basketItem.Quantity = request.Model.Quantity;
        basketItem.ProductId = request.Model.ProductId;
        basketItem.ProductName = product.Name;
        basketItem.UnitPrice = product.Price;
        basketItem.IsOrdered = false;

        basketItemRepository.Update(basketItem);
        await basketItemService.ClearBasketItemsCacheAsync();
    }
}