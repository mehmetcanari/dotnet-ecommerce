namespace OnlineStoreWeb.API.DTO.Response.OrderItem;

public class OrderItemResponseDto
{
    public int AccountId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int ProductId { get; set; }
}