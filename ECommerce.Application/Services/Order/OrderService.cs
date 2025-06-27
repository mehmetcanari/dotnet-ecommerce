using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Events;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Order;

namespace ECommerce.Application.Services.Order;

public class OrderService : BaseValidator, IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBasketItemService _basketItemService;
    private readonly IProductService _productService;
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPaymentService _paymentService;
    private readonly IMessageBroker _messageBroker;
    private ICurrentUserService _currentUserService;
    private IMediator _mediator;

    public OrderService(
        IOrderRepository orderRepository,
        IBasketItemService basketItemService,
        IBasketItemRepository basketItemRepository,
        IUnitOfWork unitOfWork,
        IProductService productService,
        IAccountRepository accountRepository,
        ILoggingService logger,
        IHttpContextAccessor httpContextAccessor,
        IPaymentService paymentService,
        IServiceProvider serviceProvider, 
        ICurrentUserService currentUserService,
        IMediator mediator,
        IMessageBroker messageBroker) : base(serviceProvider)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _basketItemRepository = basketItemRepository;
        _unitOfWork = unitOfWork;
        _basketItemService = basketItemService;
        _productService = productService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _paymentService = paymentService;
        _currentUserService = currentUserService;
        _mediator = mediator;
        _messageBroker = messageBroker;
    }

    public async Task<Result> CreateOrderAsync(OrderCreateRequestDto orderCreateRequestDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var userInfoResult = await ValidateAndGetUserInfoAsync(orderCreateRequestDto);
            if (userInfoResult.IsFailure)
            {
                return Result.Failure(userInfoResult.Error);
            }

            var (account, basketItems) = userInfoResult.Data;
            var order = CreateOrder(account.Id, account.Address, basketItems);

            string ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            Buyer buyer = CreateBuyer(account, ipAddress);
            PaymentCard paymentCard = CreatePaymentCard(orderCreateRequestDto);
            Address shippingAddress = CreateAddress(account);
            Address billingAddress = CreateAddress(account);

            var paymentResult = await _paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems);

            if (paymentResult.Data != null && paymentResult.Data.Status != "success")
            {
                _logger.LogWarning("Payment failed: {ErrorCode} - {ErrorMessage}",
                    paymentResult.Data.ErrorCode, paymentResult.Data.ErrorMessage);
                await _unitOfWork.RollbackTransaction();
                return Result.Failure($"Payment failed: {paymentResult.Data.ErrorMessage}");
            }

            await _orderRepository.Create(order);
            await _basketItemService.DeleteAllNonOrderedBasketItemsAsync();
            await _productService.UpdateProductStockAsync(basketItems);
            await _messageBroker.PublishAsync(new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                AccountId = order.AccountId,
                TotalPrice = order.BasketItems.Sum(bi => bi.Quantity * bi.UnitPrice),
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                OrderDate = DateTime.UtcNow,
                Status = order.Status
            }, "order_exchange", "order.created");

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Order added successfully: {Order}", order);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransaction();
            _logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<(Domain.Model.Account Account, List<Domain.Model.BasketItem> BasketItems)>> ValidateAndGetUserInfoAsync(OrderCreateRequestDto orderCreateRequestDto)
    {
        var validationResult = await ValidateAsync(orderCreateRequestDto);
        if (validationResult is { IsSuccess: false, Error: not null }) 
            return Result<(Domain.Model.Account, List<Domain.Model.BasketItem>)>.Failure(validationResult.Error);

        var accountResult = await GetCurrentUserAccountAsync();
        if (accountResult.IsFailure)
            return Result<(Domain.Model.Account, List<Domain.Model.BasketItem>)>.Failure(accountResult.Error);

        var basketItems = await GetUserBasketItemsAsync(accountResult.Data.Email);
        if (basketItems.IsFailure)
        {
            _logger.LogWarning("Failed to get basket items: {ErrorMessage}", basketItems.Error);
            return Result<(Domain.Model.Account, List<Domain.Model.BasketItem>)>.Failure(basketItems.Error);
        }

        return Result<(Domain.Model.Account, List<Domain.Model.BasketItem>)>.Success((accountResult.Data, basketItems.Data));
    }

    private async Task<Result<List<Domain.Model.BasketItem>>> GetUserBasketItemsAsync(string email)
    {
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
        {
            _logger.LogWarning("Account not found: {Email}", email);
            return Result<List<Domain.Model.BasketItem>>.Failure("Account not found");
        }
        
        var userBasketItems = await _basketItemRepository.GetNonOrderedBasketItems(account);
        if (userBasketItems.Count == 0)
        {
            _logger.LogWarning("No basket items found for this user: {Email}", email);
            return Result<List<Domain.Model.BasketItem>>.Failure("No basket items found for this user");
        }

        var items = userBasketItems.Select(basketItem => new Domain.Model.BasketItem
        {
            AccountId = basketItem.AccountId,
            ProductId = basketItem.ProductId,
            ExternalId = basketItem.ExternalId,
            Quantity = basketItem.Quantity,
            UnitPrice = basketItem.UnitPrice,
            ProductName = basketItem.ProductName,
            IsOrdered = true
        }).ToList();

        return Result<List<Domain.Model.BasketItem>>.Success(items);
    }

    private PaymentCard CreatePaymentCard(OrderCreateRequestDto orderCreateRequestDto)
    {
        return new PaymentCard
        {
            CardHolderName = orderCreateRequestDto.PaymentCard.CardHolderName,
            CardNumber = orderCreateRequestDto.PaymentCard.CardNumber,
            ExpirationMonth = orderCreateRequestDto.PaymentCard.ExpirationMonth,
            ExpirationYear = orderCreateRequestDto.PaymentCard.ExpirationYear,
            CVC = orderCreateRequestDto.PaymentCard.CVC,
            RegisterCard = orderCreateRequestDto.PaymentCard.RegisterCard
        };
    }

    private Domain.Model.Order CreateOrder(int accountId, string address, List<Domain.Model.BasketItem> basketItems)
    {
        return new Domain.Model.Order
        {
            AccountId = accountId,
            ShippingAddress = address,
            BillingAddress = address,
            BasketItems = basketItems
        };
    }

    private Buyer CreateBuyer(Domain.Model.Account account, string ipAddress)
    {
        return new Buyer
        {
            Id = account.Id.ToString(),
            Name = account.Name,
            Surname = account.Surname,
            Email = account.Email,
            GsmNumber = account.PhoneNumber,
            IdentityNumber = account.IdentityNumber,
            RegistrationAddress = account.Address,
            Ip = ipAddress,
            City = account.City,
            Country = account.Country,
            ZipCode = account.ZipCode
        };
    }

    private Address CreateAddress(Domain.Model.Account account)
    {
        return new Address
        {
            ContactName = $"{account.Name} {account.Surname}",
            Description = account.Address,
            City = account.City,
            Country = account.Country,
            ZipCode = account.ZipCode
        };
    }

    public async Task<Result> UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto)
    {
        try
        {
            var validationResult = await ValidateAsync(orderUpdateRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);

            var result = await _mediator.Send(new UpdateOrderStatusByAccountIdCommand 
            {
                AccountId = accountId,
                OrderUpdateRequestDto = orderUpdateRequestDto
            });
            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to update order status: {ErrorMessage}", result.Error);
                return Result.Failure(result.Error);
            }
            
            await _unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<Domain.Model.Account>> GetCurrentUserAccountAsync()
    {
        var emailResult = _currentUserService.GetCurrentUserEmail();
        if (emailResult is { IsSuccess: false, Error: not null })
        {
            _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
            return Result<Domain.Model.Account>.Failure(emailResult.Error);
        }
        
        if (emailResult.Data == null)
        {
            _logger.LogWarning("User email is null or empty");
            return Result<Domain.Model.Account>.Failure("User email is null or empty");
        }
        
        var account = await _accountRepository.GetAccountByEmail(emailResult.Data);
        if (account == null)
        {
            _logger.LogWarning("Account not found: {Email}", emailResult.Data);
            return Result<Domain.Model.Account>.Failure("Account not found");
        }

        return Result<Domain.Model.Account>.Success(account);
    }
}