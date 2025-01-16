
public class OrderRepository : IOrderRepository
{
    public readonly StoreDbContext _context;

    public OrderRepository(StoreDbContext context)
    {
        _context = context;
    }

    public void AddOrder(CreateOrderDto createOrderDto)
    {
        Order order = new Order();
        {
            order.Id = _context.Orders.Max(o => o.Id) + 1;

            
        }
    }

    public void DeleteOrder(int id)
    {
        throw new NotImplementedException();
    }

    public List<Order> GetAllOrders()
    {
        throw new NotImplementedException();
    }

    public Order GetOrderWithId(int id)
    {
        throw new NotImplementedException();
    }

    public void UpdateOrder(UpdateOrderDto updateOrderDto)
    {
        throw new NotImplementedException();
    }
}