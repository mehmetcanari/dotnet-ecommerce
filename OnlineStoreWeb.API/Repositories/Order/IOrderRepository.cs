interface IOrderRepository
{
    List<Order> GetAllOrders();
    Order GetOrderWithId(int id);
    void AddOrder(CreateOrderDto createOrderDto);
    void UpdateOrder(UpdateOrderDto updateOrderDto);
    void DeleteOrder(int id);
}