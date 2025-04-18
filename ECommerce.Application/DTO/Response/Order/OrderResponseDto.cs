using ECommerce.Application.DTO.Response.OrderItem;

namespace ECommerce.Application.DTO.Response.Order;

public record OrderResponseDto
{
    public int AccountId { get; init; }
    public ICollection<OrderItemResponseDto> OrderItems { get; set; } = new List<OrderItemResponseDto>();
    public decimal TotalPrice => OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
    public DateTime OrderDate { get; init; }
    public required string ShippingAddress { get; init; }
    public required string BillingAddress { get; init; }
    public required PaymentMethod PaymentMethod { get; init; }
    public required OrderStatus Status { get; init; }
}