using ECommerce.Application.Abstract;
using ECommerce.Application.Commands.Product;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Events;
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

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IBasketItemRepository basketItemRepository, IStoreUnitOfWork unitOfWork, IMediator mediator, IAccountRepository accountRepository, 
    ILogService logger, IPaymentService paymentService, ICurrentUserService currentUserService, IMessageBroker messageBroker) : IRequestHandler<CreateOrderCommand, Result>
{
    public async Task<Result> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userInfoResult = await ValidateAndGetUserInfoAsync(request.Model);
            if (userInfoResult is { IsFailure: true, Message: not null })
                return Result.Failure(userInfoResult.Message);

            var (account, basketItems) = userInfoResult.Data;
            var order = CreateOrder(account.Id, account.Address, basketItems);

            string ipAddress = currentUserService.GetIpAddress();
            Buyer buyer = CreateBuyer(account, ipAddress);
            PaymentCard paymentCard = CreatePaymentCard(request.Model);
            Address shippingAddress = CreateAddress(account);
            Address billingAddress = CreateAddress(account);

            await unitOfWork.BeginTransactionAsync();

            var paymentResult = await paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems);
            if (paymentResult is { IsFailure: true, Message: not null })
            {
                logger.LogWarning(ErrorMessages.PaymentFailed, paymentResult.Message);
                await unitOfWork.RollbackTransaction();
                return Result.Failure($"{ErrorMessages.PaymentFailed}: {paymentResult.Message}");
            }

            await orderRepository.Create(order, cancellationToken);
            await mediator.Send(new UpdateProductStockCommand(basketItems), cancellationToken);
            await messageBroker.Publish(new OrderCreatedEvent
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalPrice = order.BasketItems.Sum(bi => bi.Quantity * bi.UnitPrice),
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                CreatedOn = DateTime.UtcNow,
                Status = order.Status
            }, "order_exchange", "order.created");

            await unitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransaction();
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<(User Account, List<BasketItem> BasketItems)>> ValidateAndGetUserInfoAsync(CreateOrderRequestDto request)
    {
        var accountResult = await GetCurrentUserAccountAsync();
        if (accountResult is { IsSuccess: false, Message: not null })
            return Result<(User, List<BasketItem>)>.Failure(accountResult.Message);

        if (accountResult.Data?.Email is null)
            return Result<(User, List<BasketItem>)>.Failure(ErrorMessages.AccountNotFound);

        var basketItems = await GetUserBasketItemsAsync(accountResult.Data);
        if (basketItems is { IsSuccess: false, Message: not null })
            return Result<(User, List<BasketItem>)>.Failure(basketItems.Message);

        if (basketItems.Data == null || basketItems.Data.Count == 0)
            return Result<(User, List<BasketItem>)>.Failure(ErrorMessages.BasketItemNotFound);

        return Result<(User, List<BasketItem>)>.Success((accountResult.Data, basketItems.Data));
    }

    private async Task<Result<List<BasketItem>>> GetUserBasketItemsAsync(User user)
    {
        var userBasketItems = await basketItemRepository.GetActiveItems(user);
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

    private Domain.Model.Order CreateOrder(Guid userId, string address, List<BasketItem> basketItems) => new Domain.Model.Order
    {
        UserId = userId,
        ShippingAddress = address,
        BillingAddress = address,
        BasketItems = basketItems
    };

    private Buyer CreateBuyer(User account, string ipAddress) => new Buyer
    {
        Id = account.Id,
        Name = account.Name,
        Surname = account.Surname,
        Email = account.Email ?? string.Empty,
        GsmNumber = account.PhoneNumber ?? string.Empty,
        IdentityNumber = account.IdentityNumber,
        RegistrationAddress = account.Address,
        Ip = ipAddress,
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

    private async Task<Result<User>> GetCurrentUserAccountAsync()
    {
        var email = currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);

        var account = await accountRepository.GetByEmail(email);
        if (account == null)
            return Result<User>.Failure(ErrorMessages.AccountNotFound);

        return Result<User>.Success(account);
    }
}
