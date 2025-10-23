using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Events;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Domain.Model;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Order;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.Order;

public class OrderService : BaseValidator, IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBasketItemService _basketItemService;
    private readonly IProductService _productService;
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;
    private readonly IStoreUnitOfWork _storeUnitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly IMessageBroker _messageBroker;
    private ICurrentUserService _currentUserService;
    private IMediator _mediator;

    public OrderService(
        IOrderRepository orderRepository,
        IBasketItemService basketItemService,
        IBasketItemRepository basketItemRepository,
        IStoreUnitOfWork unitOfWork,
        IProductService productService,
        IAccountRepository accountRepository,
        ILoggingService logger,
        IPaymentService paymentService,
        IServiceProvider serviceProvider, 
        ICurrentUserService currentUserService,
        IMediator mediator,
        IMessageBroker messageBroker) : base(serviceProvider)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _basketItemRepository = basketItemRepository;
        _storeUnitOfWork = unitOfWork;
        _basketItemService = basketItemService;
        _productService = productService;
        _logger = logger;
        _paymentService = paymentService;
        _currentUserService = currentUserService;
        _mediator = mediator;
        _messageBroker = messageBroker;
    }

    public async Task<Result> CreateOrderAsync(OrderCreateRequestDto orderCreateRequestDto)
    {
        try
        {
            var userInfoResult = await ValidateAndGetUserInfoAsync(orderCreateRequestDto);
            if (userInfoResult.IsFailure && userInfoResult.Message is not null)
                return Result.Failure(userInfoResult.Message);

            var (account, basketItems) = userInfoResult.Data;
            var order = CreateOrder(account.Id, account.Address, basketItems);

            string ipAddress = _currentUserService.GetIpAdress();
            Buyer buyer = CreateBuyer(account, ipAddress);
            PaymentCard paymentCard = CreatePaymentCard(orderCreateRequestDto);
            Address shippingAddress = CreateAddress(account);
            Address billingAddress = CreateAddress(account);

            await _storeUnitOfWork.BeginTransactionAsync();
            
            var paymentResult = await _paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems);
            if (paymentResult.Data != null && paymentResult.Data.Status != "success")
            {
                _logger.LogWarning(ErrorMessages.PaymentFailed, paymentResult.Data.ErrorCode, paymentResult.Data.ErrorMessage);
                await _storeUnitOfWork.RollbackTransaction();
                return Result.Failure($"{ErrorMessages.PaymentFailed}: {paymentResult.Data.ErrorMessage}");
            }

            await _orderRepository.Create(order);
            await _basketItemService.DeleteAllNonOrderedBasketItemsAsync();
            await _basketItemService.ClearBasketItemsCacheAsync();
            await _productService.UpdateProductStockAsync(basketItems);
            await _messageBroker.PublishAsync(new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                TotalPrice = order.BasketItems.Sum(bi => bi.Quantity * bi.UnitPrice),
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                OrderDate = DateTime.UtcNow,
                Status = order.Status
            }, "order_exchange", "order.created");

            await _storeUnitOfWork.CommitTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _storeUnitOfWork.RollbackTransaction();
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<(User Account, List<Domain.Model.BasketItem> BasketItems)>> ValidateAndGetUserInfoAsync(OrderCreateRequestDto orderCreateRequestDto)
    {
        var validationResult = await ValidateAsync(orderCreateRequestDto);
        if (validationResult is { IsSuccess: false, Message: not null }) 
            return Result<(User, List<Domain.Model.BasketItem>)>.Failure(validationResult.Message);

        var accountResult = await GetCurrentUserAccountAsync();
        if (accountResult is { IsSuccess: false, Message: not null }) 
            return Result<(User, List<Domain.Model.BasketItem>)>.Failure(accountResult.Message);

        if(accountResult.Data is null || accountResult.Data.Email is null)
            return Result<(User, List<Domain.Model.BasketItem>)>.Failure(ErrorMessages.AccountNotFound);

        var basketItems = await GetUserBasketItemsAsync(accountResult.Data.Email);
        if (basketItems is { IsSuccess: false, Message: not null})
            return Result<(User, List<Domain.Model.BasketItem>)>.Failure(basketItems.Message);

        if(basketItems.Data == null || basketItems.Data.Count == 0)
            return Result<(User, List<Domain.Model.BasketItem>)>.Failure(ErrorMessages.BasketItemNotFound);

        return Result<(User, List<Domain.Model.BasketItem>)>.Success((accountResult.Data, basketItems.Data));
    }

    private async Task<Result<List<Domain.Model.BasketItem>>> GetUserBasketItemsAsync(string email)
    {
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
            return Result<List<Domain.Model.BasketItem>>.Failure(ErrorMessages.AccountNotFound);
        
        var userBasketItems = await _basketItemRepository.GetNonOrderedBasketItems(account);
        if (userBasketItems.Count == 0)
            return Result<List<Domain.Model.BasketItem>>.Failure(ErrorMessages.BasketItemNotFound);

        var items = userBasketItems.Select(basketItem => new Domain.Model.BasketItem
        {
            UserId = basketItem.UserId,
            ProductId = basketItem.ProductId,
            ExternalId = basketItem.ExternalId,
            Quantity = basketItem.Quantity,
            UnitPrice = basketItem.UnitPrice,
            ProductName = basketItem.ProductName,
            IsOrdered = true
        }).ToList();

        return Result<List<Domain.Model.BasketItem>>.Success(items);
    }

    private PaymentCard CreatePaymentCard(OrderCreateRequestDto orderCreateRequestDto) => new PaymentCard
    {
        CardHolderName = orderCreateRequestDto.PaymentCard.CardHolderName,
        CardNumber = orderCreateRequestDto.PaymentCard.CardNumber,
        ExpirationMonth = orderCreateRequestDto.PaymentCard.ExpirationMonth,
        ExpirationYear = orderCreateRequestDto.PaymentCard.ExpirationYear,
        CVC = orderCreateRequestDto.PaymentCard.CVC,
        RegisterCard = orderCreateRequestDto.PaymentCard.RegisterCard
    };

    private Domain.Model.Order CreateOrder(string userId, string address, List<Domain.Model.BasketItem> basketItems) => new Domain.Model.Order
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

    public async Task<Result> UpdateOrderStatus(string userId, UpdateOrderStatusRequestDto orderUpdateRequestDto)
    {
        try
        {
            var validationResult = await ValidateAsync(orderUpdateRequestDto);
            if (validationResult is { IsSuccess: false, Message: not null }) 
                return Result.Failure(validationResult.Message);

            var result = await _mediator.Send(new UpdateOrderStatusCommand 
            {
                UserId = userId,
                Request = orderUpdateRequestDto
            });

            if (result is { IsFailure: true, Message: not null})
                return Result.Failure(result.Message);

            await _storeUnitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<User>> GetCurrentUserAccountAsync()
    {
        var email = _currentUserService.GetUserEmail();
        if(string.IsNullOrEmpty(email))
            return Result<User>.Failure(ErrorMessages.AccountEmailNotFound);
        
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
            return Result<User>.Failure(ErrorMessages.AccountNotFound);

        return Result<User>.Success(account);
    }
}