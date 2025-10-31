using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Product;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class CreateOrderCommand(CreateOrderRequestDto request) : IRequest<Result>
{
    public readonly CreateOrderRequestDto Model = request;
}

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IBasketItemRepository basketItemRepository, IStoreUnitOfWork unitOfWork, IMediator mediator, IUserRepository userRepository, 
    ILogService logger, IPaymentService paymentService, ICurrentUserService currentUserService, ICacheService cache) : IRequestHandler<CreateOrderCommand, Result>
{
    public async Task<Result> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.UnauthorizedAction);

            var basketItemsResult = await GetUserBasketItemsAsync(userId);
            if (basketItemsResult is { IsFailure: true, Message: not null })
                return Result.Failure(basketItemsResult.Message);

            var basketItems = basketItemsResult.Data;
            if (basketItems == null || basketItems.Count == 0)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            var user = await userRepository.GetById(Guid.Parse(userId), cancellationToken);
            if (user == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var order = CreateOrder(userId, user.Address, basketItems);
            var buyer = CreateBuyer(user);
            var paymentCard = CreatePaymentCard(request.Model);
            var shippingAddress = CreateAddress(user);
            var billingAddress = CreateAddress(user);

            await unitOfWork.BeginTransactionAsync();

            var paymentResult = await paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems);
            if (paymentResult is { IsFailure: true, Message: not null })
            {
                logger.LogWarning(ErrorMessages.PaymentFailed, paymentResult.Message);
                await unitOfWork.RollbackTransaction();
                return Result.Failure($"{ErrorMessages.PaymentFailed}: {paymentResult.Message}");
            }

            await mediator.Send(new UpdateProductStockCommand(basketItems), cancellationToken);
            await orderRepository.Create(order, cancellationToken);

            await unitOfWork.CommitTransactionAsync();
            await cache.RemoveAsync($"{CacheKeys.UserOrders}_{userId}", cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransaction();
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<List<BasketItem>>> GetUserBasketItemsAsync(string userId)
    {
        var userBasketItems = await basketItemRepository.GetActiveItems(Guid.Parse(userId));
        if (userBasketItems.Count == 0)
            return Result<List<BasketItem>>.Failure(ErrorMessages.BasketItemNotFound);

        var items = userBasketItems.Select(basketItem => new BasketItem
        {
            UserId = basketItem.UserId,
            ProductId = basketItem.ProductId,
            ExternalId = basketItem.ExternalId,
            Quantity = basketItem.Quantity,
            UnitPrice = basketItem.UnitPrice,
            ProductName = basketItem.ProductName,
            IsPurchased = true
        }).ToList();

        return Result<List<BasketItem>>.Success(items);
    }

    private PaymentCard CreatePaymentCard(CreateOrderRequestDto request) => new PaymentCard
    {
        CardHolderName = request.PaymentCard.CardHolderName,
        CardNumber = request.PaymentCard.CardNumber,
        ExpirationMonth = request.PaymentCard.ExpirationMonth,
        ExpirationYear = request.PaymentCard.ExpirationYear,
        Cvc = request.PaymentCard.Cvc,
        RegisterCard = request.PaymentCard.RegisterCard
    };

    private Domain.Model.Order CreateOrder(string userId, string address, List<BasketItem> basketItems) => new Domain.Model.Order
    {
        UserId = Guid.Parse(userId),
        ShippingAddress = address,
        BillingAddress = address,
        BasketItems = basketItems
    };

    private Buyer CreateBuyer(User account) => new Buyer
    {
        Id = account.Id,
        Name = account.Name,
        Surname = account.Surname,
        Email = account.Email ?? string.Empty,
        GsmNumber = account.PhoneNumber ?? string.Empty,
        IdentityNumber = account.IdentityNumber,
        RegistrationAddress = account.Address,
        Ip = currentUserService.GetIpAddress(),
        City = account.City,
        Country = account.Country,
        ZipCode = account.ZipCode
    };

    private Address CreateAddress(User account) => new Address
    {
        ContactName = $"{account.Name} {account.Surname}",
        Description = account.Address,
        City = account.City,
        Country = account.Country,
        ZipCode = account.ZipCode
    };
}
