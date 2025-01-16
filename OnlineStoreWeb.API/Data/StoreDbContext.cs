public class StoreDbContext
{
    public required List<Product> Products {get; set;}
    public required List<Order> Orders {get; set;}
    public required List<OrderItem> OrderItems {get; set;}
    public required List<User> Users {get; set;}
}