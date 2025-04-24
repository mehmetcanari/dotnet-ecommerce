namespace ECommerce.Application.DTO.Response.BasketItem;

public record BasketItemResponseDto
{
    public int AccountId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
}