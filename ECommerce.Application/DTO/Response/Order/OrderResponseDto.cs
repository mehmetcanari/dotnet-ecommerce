using ECommerce.Application.DTO.Response.OrderItem;

namespace ECommerce.Application.DTO.Response.Order;

public class OrderResponseDto
{
    public int AccountId { get; init; }
    public ICollection<OrderItemResponseDto> OrderItems { get; set; } = new List<OrderItemResponseDto>();
    public decimal TotalPrice => OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
    public DateTime OrderDate { get; init; }
    public string ShippingAddress { get; init; }
    public string BillingAddress { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public OrderStatus Status { get; init; }
}