using Microsoft.EntityFrameworkCore;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly StoreDbContext _context;
    private readonly IProductRepository _productRepository;

    public OrderItemRepository(StoreDbContext context, IProductRepository productRepository)
    {
        _context = context;
        _productRepository = productRepository;
    }

    public async Task<List<OrderItem>> GetAllOrderItemsAsync()
    {
        try
        {
            return await _context.OrderItems.ToListAsync(); 
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch order items", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<OrderItem?> GetOrderItemWithIdAsync(int id)
    {
        try
        {
            return await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id);
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddOrderItemAsync(CreateOrderItemDto createOrderItemRequest)
    {
        try
        {
            Product fetchedProduct = await _productRepository.GetProductWithIdAsync(createOrderItemRequest.ProductId)
                ?? throw new Exception("Product not found");

            OrderItem orderItem = new OrderItem
            {
                Quantity = createOrderItemRequest.Quantity,
                UserId = createOrderItemRequest.UserId,
                ProductId = createOrderItemRequest.ProductId,
                Price = fetchedProduct.Price,
                OrderItemUpdated = DateTime.UtcNow
            };

            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to save order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderItemAsync(int id, UpdateOrderItemDto updateOrderItemRequest)
    {
        try
        {
            OrderItem orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new Exception("OrderItem not found");

            Product fetchedProduct = await _productRepository.GetProductWithIdAsync(updateOrderItemRequest.ProductId)
                ?? throw new Exception("Product not found");

            orderItem.Quantity = updateOrderItemRequest.Quantity;
            orderItem.ProductId = updateOrderItemRequest.ProductId;
            orderItem.UserId = updateOrderItemRequest.UserId;
            orderItem.Price = fetchedProduct.Price;
            orderItem.OrderItemUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderItemAsync(int id)
    {
        try
        {
            OrderItem orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new Exception("OrderItem not found");

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}