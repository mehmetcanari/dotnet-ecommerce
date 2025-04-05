namespace ECommerce.API.DTO.Response.OrderItem;

public class OrderItemResponseDto
{
    public int AccountId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
}