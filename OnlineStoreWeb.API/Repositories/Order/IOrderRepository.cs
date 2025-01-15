interface IOrderRepository
{
    List<Order> GetAllOrders();
    Order GetOrderWithId(int id);
    void AddOrder(CreateUserDto createUserDto);
    void UpdateOrder(UpdateUserDto updateUserDto);
    void DeleteOrder(int id);
}