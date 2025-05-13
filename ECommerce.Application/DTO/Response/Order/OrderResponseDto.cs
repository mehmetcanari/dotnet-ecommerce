using ECommerce.Application.DTO.Response.BasketItem;

namespace ECommerce.Application.DTO.Response.Order;

public record OrderResponseDto
{
    public int AccountId { get; init; }
    public ICollection<BasketItemResponseDto> BasketItems { get; init; } = new List<BasketItemResponseDto>();
    public decimal TotalPrice => BasketItems.Sum(oi => oi.UnitPrice * oi.Quantity);
    public DateTime OrderDate { get; init; }
    public required string ShippingAddress { get; init; }
    public required string BillingAddress { get; init; }
    public required OrderStatus Status { get; init; }
}