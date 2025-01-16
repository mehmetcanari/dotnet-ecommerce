
public class OrderItemRepository : IOrderItemRepository
{
    public readonly StoreDbContext _context;

    public OrderItemRepository(StoreDbContext context)
    {
        _context = context;
    }

    public void AddOrderItem(CreateOrderItemDto createOrderItemDto)
    {
        throw new NotImplementedException();
    }

    public void UpdateOrderItem(UpdateOrderItemDto updateOrderItemDto)
    {
        throw new NotImplementedException();
    }

    public void DeleteOrderItem(int id)
    {
        throw new NotImplementedException();
    }

    public List<OrderItem> GetAllOrderItems()
    {
        throw new NotImplementedException();
    }

    public OrderItem GetOrderItemWithId(int id)
    {
        throw new NotImplementedException();
    }
}