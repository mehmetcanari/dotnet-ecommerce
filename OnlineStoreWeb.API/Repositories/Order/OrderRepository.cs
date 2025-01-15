
public class OrderRepository : IOrderRepository
{
    public readonly StoreDbContext _context;

    public OrderRepository(StoreDbContext context)
    {
        _context = context;
    }

    public void AddOrder(CreateUserDto createUserDto)
    {
        throw new NotImplementedException();
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

    public void UpdateOrder(UpdateUserDto updateUserDto)
    {
        throw new NotImplementedException();
    }
}