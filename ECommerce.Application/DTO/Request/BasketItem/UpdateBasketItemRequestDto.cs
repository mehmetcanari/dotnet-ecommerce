namespace ECommerce.Application.DTO.Request.BasketItem;

public record UpdateBasketItemRequestDto
{
    public required Guid Id { get; set; }
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
}