namespace ECommerce.Application.DTO.Response.BasketItem;

public record BasketItemResponseDto
{
    public required string UserId { get; set; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
}