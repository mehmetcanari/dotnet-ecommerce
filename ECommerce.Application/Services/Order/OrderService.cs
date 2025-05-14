using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Services.Order;

public class OrderService : IOrderService
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

    public OrderService(
        IOrderRepository orderRepository,
        IBasketItemService basketItemService,
        IBasketItemRepository basketItemRepository,
        IUnitOfWork unitOfWork,
        IProductService productService,
        IAccountRepository accountRepository,
        ILoggingService logger,
        IHttpContextAccessor httpContextAccessor,
        IPaymentService paymentService)
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
    }

    public async Task AddOrderAsync(OrderCreateRequestDto orderCreateRequestDto, string email)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var accounts = await _accountRepository.Read();
            var account = accounts.FirstOrDefault(a => a.Email == email)
                ?? throw new Exception("User not found");

            var basketItems = await GetUserBasketItemsAsync(email, account.Id);
            var order = CreateOrder(account.Id, account.Address, basketItems.Data);

            string ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            Buyer buyer = CreateBuyer(account, ipAddress);
            PaymentCard paymentCard = CreatePaymentCard(orderCreateRequestDto);
            Address shippingAddress = CreateAddress(account);
            Address billingAddress = CreateAddress(account);

            Iyzipay.Model.Payment paymentResult = await _paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems.Data);

            if (paymentResult.Status != "success")
            {
                _logger.LogWarning("Payment failed: {ErrorCode} - {ErrorMessage}",
                    paymentResult.ErrorCode, paymentResult.ErrorMessage);
                throw new Exception($"Payment failed: {paymentResult.ErrorMessage}");
            }

            await _orderRepository.Create(order);
            await _basketItemService.DeleteAllBasketItemsAsync(email);
            await _productService.UpdateProductStockAsync(basketItems.Data);
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Order added successfully: {Order}", order);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransaction();
            _logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw;
        }
    }

    private async Task<Result<List<Domain.Model.BasketItem>>> GetUserBasketItemsAsync(string email, int accountId)
    {
        var basketItems = await _basketItemRepository.Read();
        var userBasketItems = basketItems
            .Where(oi => oi.AccountId == accountId)
            .Where(oi => oi.IsOrdered == false)
            .ToList();

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

    public async Task CancelOrderAsync(string email)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ??
                throw new Exception("Account not found");

            var pendingOrders = orders
                .Where(o => o.AccountId == tokenAccount.Id && o.Status == OrderStatus.Pending)
                .ToList();

            if (pendingOrders.Count == 0)
            {
                throw new Exception("No pending orders found");
            }

            foreach (var order in pendingOrders)
            {
                order.Status = OrderStatus.Cancelled;
                _orderRepository.Update(order);
            }

            await _unitOfWork.Commit();

            _logger.LogInformation("Orders cancelled successfully. Count: {Count}", pendingOrders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while cancelling orders: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteOrderByIdAsync(int id)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var orderToDelete = orders.FirstOrDefault(o => o.OrderId == id) ?? throw new Exception("Order not found");
            _orderRepository.Delete(orderToDelete);

            await _unitOfWork.Commit();
            _logger.LogInformation("Order deleted successfully: {Order}", orderToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            throw;
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

    public async Task<Result<List<OrderResponseDto>>> GetUserOrdersAsync(string email)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var accounts = await _accountRepository.Read();

            var tokenAccount = accounts.FirstOrDefault(a => a.Email == email) ??
                               throw new Exception("Account not found");

            var userOrders = orders.Where(o => o.AccountId == tokenAccount.Id).ToList();
            var orderedItems = userOrders.Where(o => o.BasketItems.Any(oi => oi.IsOrdered)).ToList();

            if (orderedItems.Count == 0)
                return Result<List<OrderResponseDto>>.Failure("No orders found for this user");

            var items = orderedItems.Select(order => new OrderResponseDto
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
            var orders = await _orderRepository.Read();
            var order = orders.FirstOrDefault(o => o.OrderId == id) ??
                        throw new Exception("Order not found");

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

    public async Task UpdateOrderStatusByAccountIdAsync(int accountId, OrderUpdateRequestDto orderUpdateRequestDto)
    {
        try
        {
            var orders = await _orderRepository.Read();
            var order = orders.FirstOrDefault(o => o.AccountId == accountId) ?? throw new Exception("Order not found");
            order.Status = orderUpdateRequestDto.Status;
            _orderRepository.Update(order);

            await _unitOfWork.Commit();
            _logger.LogInformation("Order status updated successfully: {Order}", order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw;
        }
    }
}