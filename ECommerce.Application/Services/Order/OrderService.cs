using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Services.Base;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;

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
    private ICurrentUserService _currentUserService;

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
        ICurrentUserService currentUserService) : base(serviceProvider)
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
    }

    public async Task<Result> CreateOrderAsync(OrderCreateRequestDto orderCreateRequestDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result.Failure(emailResult.Error);
            }
            
            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null or empty");
                return Result.Failure("User email is null or empty");
            }
            
            var validationResult = await ValidateAsync(orderCreateRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);
            
            var account = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (account == null)
            {
                _logger.LogWarning("Account not found: {Email}", emailResult.Data);
                return Result.Failure("Account not found");
            }

            var basketItems = await GetUserBasketItemsAsync(emailResult.Data);
            if (basketItems.IsFailure)
            {
                _logger.LogWarning("Failed to get basket items: {ErrorMessage}", basketItems.Error);
                return Result.Failure(basketItems.Error);
            }
            var order = CreateOrder(account.Id, account.Address, basketItems.Data);

            string ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            Buyer buyer = CreateBuyer(account, ipAddress);
            PaymentCard paymentCard = CreatePaymentCard(orderCreateRequestDto);
            Address shippingAddress = CreateAddress(account);
            Address billingAddress = CreateAddress(account);

            var paymentResult = await _paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems.Data);

            if (paymentResult.Data != null && paymentResult.Data.Status != "success")
            {
                _logger.LogWarning("Payment failed: {ErrorCode} - {ErrorMessage}",
                    paymentResult.Data.ErrorCode, paymentResult.Data.ErrorMessage);
                await _unitOfWork.RollbackTransaction();
                return Result.Failure($"Payment failed: {paymentResult.Data.ErrorMessage}");
            }

            await _orderRepository.Create(order);
            await _basketItemService.DeleteAllNonOrderedBasketItemsAsync();
            await _productService.UpdateProductStockAsync(basketItems.Data);
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

    public async Task<Result> CancelOrderAsync()
    {
        try
        {
            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result.Failure(emailResult.Error);
            }
            
            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null or empty");
                return Result.Failure("User email is null or empty");
            }
            
            var tokenAccount = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (tokenAccount == null)
            {
                _logger.LogWarning("Account not found: {Email}", emailResult.Data);
                return Result.Failure("Account not found");
            }
            
            var pendingOrders = await _orderRepository.GetAccountPendingOrders(tokenAccount.Id);

            if (pendingOrders.Count == 0)
            {
                return Result.Failure("No pending orders found");
            }

            foreach (var order in pendingOrders)
            {
                order.Status = OrderStatus.Cancelled;
                _orderRepository.Update(order);
            }

            await _unitOfWork.Commit();

            _logger.LogInformation("Orders cancelled successfully. Count: {Count}", pendingOrders.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while cancelling orders: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteOrderByIdAsync(int id)
    {
        try
        {
            var activeOrder = await _orderRepository.GetOrderById(id);
            if (activeOrder == null)
            {
                _logger.LogWarning("Order not found: {Id}", id);
                return Result.Failure("Order not found");
            }
            
            _orderRepository.Delete(activeOrder);

            await _unitOfWork.Commit();
            _logger.LogInformation("Order deleted successfully: {Order}", activeOrder);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<List<OrderResponseDto>>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.Read();
            if (orders.Count == 0)
            {
                return Result<List<OrderResponseDto>>.Failure("No orders found");
            }

            var items = orders.Select(o => new OrderResponseDto
            {
                AccountId = o.AccountId,
                BasketItems = o.BasketItems.Select(oi => new BasketItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = o.OrderDate,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                Status = o.Status
            }).ToList();

            return Result<List<OrderResponseDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            return Result<List<OrderResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result<List<OrderResponseDto>>> GetUserOrdersAsync()
    {
        try
        {
            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result<List<OrderResponseDto>>.Failure(emailResult.Error);
            }
            
            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null or empty");
                return Result<List<OrderResponseDto>>.Failure("User email is null or empty");
            }
            
            var tokenAccount = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (tokenAccount == null)
            {
                _logger.LogWarning("Account not found: {Email}", emailResult.Data);
                return Result<List<OrderResponseDto>>.Failure("Account not found");
            }

            var userOrders = await _orderRepository.GetAccountOrders(tokenAccount.Id);
            if (userOrders.Count == 0)
            {
                _logger.LogWarning("No orders found for this user: {Email}", emailResult.Data);
                return Result<List<OrderResponseDto>>.Failure("No orders found for this user");
            }
            var purchasedOrders = userOrders.Where(o => o.BasketItems.Any(oi => oi.IsOrdered)).ToList();

            var items = purchasedOrders.Select(order => new OrderResponseDto
            {
                AccountId = order.AccountId,
                BasketItems = order.BasketItems.Select(oi => new BasketItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                Status = order.Status
            }).ToList();

            return Result<List<OrderResponseDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching user orders: {Message}", ex.Message);
            return Result<List<OrderResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result<OrderResponseDto>> GetOrderByIdAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderById(id);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {Id}", id);
                return Result<OrderResponseDto>.Failure("Order not found");
            }

            OrderResponseDto orderResponseDto = new()
            {
                AccountId = order.AccountId,
                BasketItems = order.BasketItems.Select(oi => new BasketItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                Status = order.Status
            };

            return Result<OrderResponseDto>.Success(orderResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order with id: {Message}", ex.Message);
            return Result<OrderResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<Result> UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto)
    {
        try
        {
            var validationResult = await ValidateAsync(orderUpdateRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);
            
            var order = await _orderRepository.GetOrderByAccountId(accountId);
            if (order == null)
            {
                _logger.LogWarning("Order not found for account id: {AccountId}", accountId);
                return Result.Failure("Order not found");
            }
            
            order.Status = orderUpdateRequestDto.Status;
            _orderRepository.Update(order);

            await _unitOfWork.Commit();
            _logger.LogInformation("Order status updated successfully: {Order}", order);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}