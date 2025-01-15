interface IOrderItemRepository
{
    List<OrderItem> GetAllOrderItems();
    OrderItem GetOrderItemWithId(int id);
    void AddOrderItem(CreateOrderItemDto createOrderItemDto);
    void UpdateOrderItem(UpdateOrderItemDto updateOrderItemDto);
    void DeleteOrderItem(int id);
}