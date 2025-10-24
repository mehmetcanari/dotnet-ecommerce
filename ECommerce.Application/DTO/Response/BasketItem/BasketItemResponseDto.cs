namespace ECommerce.Application.DTO.Response.BasketItem;

public record BasketItemResponseDto
{
    public required Guid UserId { get; set; }
    public required Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}