namespace OnlineStoreWeb.API.DTO.OrderItem;

public record UpdateOrderItemDto
{
    public int Id {get;set;}
    public required int Quantity {get;set;}
    public required int ProductId {get;set;}
    public required int UserId {get;set;}
}